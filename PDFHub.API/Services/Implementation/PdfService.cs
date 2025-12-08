using AutoMapper;
using PDFHub.API.Models;
using PDFHub.API.Models.Domains;
using PDFHub.API.Models.DTOs;
using PDFHub.API.Repositories;
using PDFHub.API.Services.Delegates;

namespace PDFHub.API.Services;

public class PdfService : IPdfService
{
    private readonly IPdfRepository _pdfRepository;
    private readonly IWebHostEnvironment _environment;
    private readonly IMapper _mapper;
    private readonly IPdfTextExtractorService _textExtractor;
    private readonly IGeminiApiService _geminiService;

    public PdfService(
        IPdfRepository pdfRepository,
        IWebHostEnvironment environment,
        IMapper mapper,
        IPdfTextExtractorService textExtractor,
        IGeminiApiService geminiService)
    {
        _pdfRepository = pdfRepository;
        _environment = environment;
        _mapper = mapper;
        _textExtractor = textExtractor;
        _geminiService = geminiService;
    }

    public async Task<ServiceResult<PdfResponse>> UploadPdfAsync(IFormFile file, string userId)
    {
        try
        {
            // Validate file
            if (file == null || file.Length == 0)
            {
                return ServiceResult<PdfResponse>.FailureResult("No file uploaded");
            }

            // Validate PDF file type
            if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResult<PdfResponse>.FailureResult("Only PDF files are allowed");
            }

            // Create uploads directory if not exist
            var uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads", "PDFs");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate filename
            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save file to disk
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Save file metadata to database
            var pdfFile = new PdfFiles
            {
                FileName = file.FileName,
                FilePath = filePath,
                FileSize = file.Length,
                UserId = userId
            };

            // Save to database
            var savedPdf = await _pdfRepository.AddAsync(pdfFile);

            var response = _mapper.Map<PdfResponse>(savedPdf);

            return ServiceResult<PdfResponse>.SuccessResult(response, "PDF uploaded successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<PdfResponse>.FailureResult("An error occurred while uploading the file");
        }
    }

    public async Task<ServiceResult<EditPdfResponse>> EditPdfAsync(int id, EditPdfRequestDto editDto, string userId)
    {
        try
        {
            // Get the existing PDF file
            var pdfFile = await _pdfRepository.GetByIdAsync(id);

            if (pdfFile == null)
            {
                return ServiceResult<EditPdfResponse>.FailureResult("PDF file not found");
            }

            // Verify ownership
            if (pdfFile.UserId != userId)
            {
                return ServiceResult<EditPdfResponse>.FailureResult("Unauthorized: You don't have permission to edit this PDF");
            }

            // Update
            if (!string.IsNullOrWhiteSpace(editDto.Filename))
            {
                pdfFile.FileName = editDto.Filename;
            }

            if (!string.IsNullOrWhiteSpace(editDto.Description))
            {
                pdfFile.Description = editDto.Description;
            }

            var updatedPdf = await _pdfRepository.UpdateAsync(pdfFile);

            if (updatedPdf == null)
            {
                return ServiceResult<EditPdfResponse>.FailureResult("Failed to update PDF");
            }

            var response = _mapper.Map<EditPdfResponse>(updatedPdf);

            return ServiceResult<EditPdfResponse>.SuccessResult(response, "PDF updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<EditPdfResponse>.FailureResult("An error occurred while updating the PDF");
        }
    }

    public async Task<ServiceResult<PdfResponse>> GetPdfByIdAsync(int id, string userId)
    {
        try
        {
            var pdfFile = await _pdfRepository.GetByIdAsync(id);

            if (pdfFile == null)
            {
                return ServiceResult<PdfResponse>.FailureResult("PDF file not found");
            }

            // Verify ownership
            if (pdfFile.UserId != userId)
            {
                return ServiceResult<PdfResponse>.FailureResult("Unauthorized: You don't have permission to view this PDF");
            }

            var response = _mapper.Map<PdfResponse>(pdfFile);

            return ServiceResult<PdfResponse>.SuccessResult(response, "PDF retrieved successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<PdfResponse>.FailureResult("An error occurred while retrieving the PDF");
        }
    }

    public async Task<ServiceResult<List<PdfResponse>>> GetMyPdfsAsync(string userId)
    {
        try
        {
            var pdfFiles = await _pdfRepository.GetByUserIdAsync(userId);

            var response = _mapper.Map<List<PdfResponse>>(pdfFiles);

            return ServiceResult<List<PdfResponse>>.SuccessResult(response, "PDFs retrieved successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<List<PdfResponse>>.FailureResult("An error occurred while retrieving PDFs");
        }
    }

    public async Task<ServiceResult> DeletePdfAsync(int id, string userId)
    {
        try
        {
            // Verify ownership
            var pdfFile = await _pdfRepository.GetByIdAsync(id);

            if (pdfFile == null)
            {
                return ServiceResult.FailureResult("PDF file not found");
            }

            if (pdfFile.UserId != userId)
            {
                return ServiceResult.FailureResult("Unauthorized: You don't have permission to delete this PDF");
            }

            // Soft delete
            var deleted = await _pdfRepository.DeleteAsync(id);

            if (!deleted)
            {
                return ServiceResult.FailureResult("Failed to delete PDF");
            }

            return ServiceResult.SuccessResult("PDF deleted successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult.FailureResult("An error occurred while deleting the PDF");
        }
    }

    public async Task<ServiceResult<SummaryResponse>> SummarizePdfAsync(int id, string userId, ProgressCallback? onProgress = null)
    {
        try
        {
            // Report: Initializing
            if (onProgress != null)
            {
                await onProgress(new SummarizationProgressEvent
                {
                    Progress = 0,
                    Stage = "Initializing",
                    Message = "Starting PDF summarization...",
                    IsComplete = false,
                    IsFailed = false
                });
            }

            // Get PDF and verify ownership
            var pdfFile = await _pdfRepository.GetByIdAsync(id);

            if (pdfFile == null)
            {
                if (onProgress != null)
                {
                    await onProgress(new SummarizationProgressEvent
                    {
                        Progress = 0,
                        Stage = "Failed",
                        Message = "PDF file not found",
                        IsComplete = false,
                        IsFailed = true,
                        ErrorMessage = "PDF file not found"
                    });
                }
                return ServiceResult<SummaryResponse>.FailureResult("PDF file not found");
            }

            if (pdfFile.UserId != userId)
            {
                if (onProgress != null)
                {
                    await onProgress(new SummarizationProgressEvent
                    {
                        Progress = 0,
                        Stage = "Failed",
                        Message = "Unauthorized",
                        IsComplete = false,
                        IsFailed = true,
                        ErrorMessage = "Unauthorized"
                    });
                }
                return ServiceResult<SummaryResponse>.FailureResult("Unauthorized");
            }

            // Check if summary already exists
            if (!string.IsNullOrEmpty(pdfFile.Summary))
            {
                if (onProgress != null)
                {
                    await onProgress(new SummarizationProgressEvent
                    {
                        Progress = 100,
                        Stage = "Cached",
                        Message = "Summary already exists",
                        IsComplete = true,
                        IsFailed = false,
                        Summary = pdfFile.Summary
                    });
                }
                return ServiceResult<SummaryResponse>.SuccessResult(
                    new SummaryResponse { Summary = pdfFile.Summary },
                    "Summary retrieved");
            }

            // Extract text from PDF (0-50%)
            var extractResult = await _textExtractor.ExtractTextFromPdfAsync(pdfFile.FilePath, onProgress);
            if (!extractResult.Success)
            {
                if (onProgress != null)
                {
                    await onProgress(new SummarizationProgressEvent
                    {
                        Progress = 0,
                        Stage = "Failed",
                        Message = "Text extraction failed",
                        IsComplete = false,
                        IsFailed = true,
                        ErrorMessage = extractResult.Message
                    });
                }
                return ServiceResult<SummaryResponse>.FailureResult(extractResult.Message);
            }

            // Report: Starting AI summarization (51%)
            if (onProgress != null)
            {
                await onProgress(new SummarizationProgressEvent
                {
                    Progress = 51,
                    Stage = "GeneratingSummary",
                    Message = "Calling AI to generate summary...",
                    IsComplete = false,
                    IsFailed = false
                });
            }

            // Generate summary using Gemini (51-90%)
            var summaryResult = await _geminiService.GenerateSummaryAsync(extractResult.Data);
            if (!summaryResult.Success)
            {
                if (onProgress != null)
                {
                    await onProgress(new SummarizationProgressEvent
                    {
                        Progress = 51,
                        Stage = "Failed",
                        Message = "AI summarization failed",
                        IsComplete = false,
                        IsFailed = true,
                        ErrorMessage = summaryResult.Message
                    });
                }
                return ServiceResult<SummaryResponse>.FailureResult(summaryResult.Message);
            }

            // Report: AI complete, now saving (90%)
            if (onProgress != null)
            {
                await onProgress(new SummarizationProgressEvent
                {
                    Progress = 90,
                    Stage = "SavingSummary",
                    Message = "Saving summary to database...",
                    IsComplete = false,
                    IsFailed = false
                });
            }

            // Save summary to database
            pdfFile.Summary = summaryResult.Data;
            await _pdfRepository.UpdateAsync(pdfFile);

            // Report: Complete (100%)
            if (onProgress != null)
            {
                await onProgress(new SummarizationProgressEvent
                {
                    Progress = 100,
                    Stage = "Completed",
                    Message = "PDF summarized successfully",
                    IsComplete = true,
                    IsFailed = false,
                    Summary = summaryResult.Data
                });
            }

            return ServiceResult<SummaryResponse>.SuccessResult(
                new SummaryResponse { Summary = summaryResult.Data },
                "PDF summarized successfully");
        }
        catch (Exception ex)
        {
            if (onProgress != null)
            {
                await onProgress(new SummarizationProgressEvent
                {
                    Progress = 0,
                    Stage = "Failed",
                    Message = "An unexpected error occurred",
                    IsComplete = false,
                    IsFailed = true,
                    ErrorMessage = "An error occurred while summarizing the PDF"
                });
            }
            return ServiceResult<SummaryResponse>.FailureResult("An error occurred while summarizing the PDF");
        }
    }
}