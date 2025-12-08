using Mscc.GenerativeAI;
using PDFHub.API.Models;

namespace PDFHub.API.Services;

public class GeminiApiService : IGeminiApiService
{
    private readonly IConfiguration _configuration;

    public GeminiApiService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<ServiceResult<string>> GenerateSummaryAsync(string text)
    {
        try
        {
            // Get configuration
            var apiKey = _configuration["GeminiApi:ApiKey"];
            var model = _configuration["GeminiApi:Model"];

            if (string.IsNullOrEmpty(apiKey))
            {
                return ServiceResult<string>.FailureResult("Gemini API key not configured.");
            }

            // Validate input text
            if (string.IsNullOrWhiteSpace(text))
            {
                return ServiceResult<string>.FailureResult("No text provided for summarization");
            }

            // Generate summary
            var googleAI = new GoogleAI(apiKey);
            var geminiModel = googleAI.GenerativeModel(model);

            var prompt = $"Please provide a concise summary of the following document:\n\n{text}";

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var response = await geminiModel.GenerateContent(prompt);
            var summary = response.Text;

            if (string.IsNullOrWhiteSpace(summary))
            {
                return ServiceResult<string>.FailureResult("Gemini API returned empty summary");
            }

            return ServiceResult<string>.SuccessResult(summary, "Summary generated successfully");
        }
        catch (Exception)
        {
            return ServiceResult<string>.FailureResult("Failed to generate summary");
        }
    }
}
