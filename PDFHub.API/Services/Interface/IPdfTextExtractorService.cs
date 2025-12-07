using PDFHub.API.Models;

namespace PDFHub.API.Services;

public interface IPdfTextExtractorService
{
    Task<ServiceResult<string>> ExtractTextFromPdfAsync(string filePath);
}
