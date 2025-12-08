using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using PDFHub.API.Models;
using System.Text;

namespace PDFHub.API.Services;

public class PdfTextExtractorService : IPdfTextExtractorService
{
    private const int MaxTextLength = 30000; // Limit

    public async Task<ServiceResult<string>> ExtractTextFromPdfAsync(string filePath)
    {
        try
        {
            // Validate file exists
            if (!File.Exists(filePath))
            {
                return ServiceResult<string>.FailureResult("PDF file not found");
            }

            // Extract text from PDF
            using var pdfReader = new PdfReader(filePath);
            using var pdfDocument = new PdfDocument(pdfReader);

            var text = new StringBuilder();

            // Extract text from all pages
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                var page = pdfDocument.GetPage(i);
                var strategy = new SimpleTextExtractionStrategy();
                var pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                text.Append(pageText);
                text.Append("\n");

                // Stop if reached max length
                if (text.Length > MaxTextLength)
                {
                    break;
                }
            }

            var extractedText = text.ToString().Trim();

            // Validate extracted text
            if (string.IsNullOrWhiteSpace(extractedText))
            {
                return ServiceResult<string>.FailureResult("No text could be extracted from PDF");
            }

            // Truncate if too long
            if (extractedText.Length > MaxTextLength)
            {
                extractedText = extractedText.Substring(0, MaxTextLength);
            }

            return ServiceResult<string>.SuccessResult(extractedText, "Text extracted successfully");
        }
        catch (iText.Kernel.Exceptions.BadPasswordException)
        {
            return ServiceResult<string>.FailureResult("PDF is password-protected. Please provide an unprotected PDF.");
        }
        catch (iText.IO.Exceptions.IOException)
        {
            return ServiceResult<string>.FailureResult("Failed to read PDF file. File might be corrupted.");
        }
        catch (Exception)
        {
            return ServiceResult<string>.FailureResult("An error occurred while extracting text from PDF");
        }
    }
}
