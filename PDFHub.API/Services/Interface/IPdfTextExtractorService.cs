using PDFHub.API.Models;
using PDFHub.API.Services.Delegates;

namespace PDFHub.API.Services;

public interface IPdfTextExtractorService
{
    Task<ServiceResult<string>> ExtractTextFromPdfAsync(string filePath, ProgressCallback? onProgress = null);
}
