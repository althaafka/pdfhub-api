using PDFHub.API.Models;
using PDFHub.API.Models.DTOs;

namespace PDFHub.API.Services;

public interface IPdfService
{
    Task<ServiceResult<PdfResponse>> UploadPdfAsync(IFormFile file, string userId);
    Task<ServiceResult<EditPdfResponse>> EditPdfAsync(int id, EditPdfRequestDto editDto, string userId);
    Task<ServiceResult<PdfResponse>> GetPdfByIdAsync(int id, string userId);
    Task<ServiceResult<List<PdfResponse>>> GetMyPdfsAsync(string userId);
    Task<ServiceResult> DeletePdfAsync(int id, string userId);
    Task<ServiceResult<SummaryResponse>> SummarizePdfAsync(int id, string userId);
}