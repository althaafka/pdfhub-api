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
            var model = _configuration["GeminiApi:Model"] ?? "gemini-1.5-flash";

            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                return ServiceResult<string>.FailureResult("Gemini API key not configured. Please add your API key to appsettings.json");
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
        catch (TaskCanceledException)
        {
            return ServiceResult<string>.FailureResult("Request timed out. Please try again.");
        }
        catch (HttpRequestException)
        {
            return ServiceResult<string>.FailureResult("Network error. Please check your internet connection and try again.");
        }
        catch (Exception)
        {
            return ServiceResult<string>.FailureResult("Failed to generate summary. Please try again later.");
        }
    }
}
