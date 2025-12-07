using PDFHub.API.Models;

namespace PDFHub.API.Services;

public interface IGeminiApiService
{
    Task<ServiceResult<string>> GenerateSummaryAsync(string text);
}
