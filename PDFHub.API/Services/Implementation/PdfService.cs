using PDFHub.API.Data;
using PDFHub.API.Models;
using PDFHub.API.Models.Domains;
using PDFHub.API.Models.DTOs.Responses;

namespace PDFHub.API.Services;

public class PdfService : IPdfService
{
    private readonly PDFHubDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public PdfService(PDFHubDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
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

            // Create uploads directory if it doesn't exist
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

            _context.PdfFiles.Add(pdfFile);
            await _context.SaveChangesAsync();

            var response = new UploadPdfResponse
            {
                Id = pdfFile.Id,
                OriginalFileName = pdfFile.FileName,
                StoredFilePath = pdfFile.FilePath,
                FileSize = pdfFile.FileSize,
                UploadedAt = DateTime.UtcNow
            };

            return ServiceResult<UploadPdfResponse>.SuccessResult(response, "PDF uploaded successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<UploadPdfResponse>.FailureResult("An error occurred while uploading the file");
        }
    }
}