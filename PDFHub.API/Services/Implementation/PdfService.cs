using AutoMapper;
using PDFHub.API.Models;
using PDFHub.API.Models.Domains;
using PDFHub.API.Models.DTOs;
using PDFHub.API.Repositories;

namespace PDFHub.API.Services;

public class PdfService : IPdfService
{
    private readonly IPdfRepository _pdfRepository;
    private readonly IWebHostEnvironment _environment;
    private readonly IMapper _mapper;

    public PdfService(IPdfRepository pdfRepository, IWebHostEnvironment environment, IMapper mapper)
    {
        _pdfRepository = pdfRepository;
        _environment = environment;
        _mapper = mapper;
    }

    public async Task<ServiceResult<UploadPdfResponse>> UploadPdfAsync(IFormFile file, string userId)
    {
        try
        {
            // Validate file
            if (file == null || file.Length == 0)
            {
                return ServiceResult<UploadPdfResponse>.FailureResult("No file uploaded");
            }

            // Validate PDF file type
            if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResult<UploadPdfResponse>.FailureResult("Only PDF files are allowed");
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

            var response = _mapper.Map<UploadPdfResponse>(savedPdf);

            return ServiceResult<UploadPdfResponse>.SuccessResult(response, "PDF uploaded successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<UploadPdfResponse>.FailureResult("An error occurred while uploading the file");
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
}