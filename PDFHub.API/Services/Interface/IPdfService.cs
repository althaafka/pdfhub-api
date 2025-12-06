using PDFHub.API.Models;
using PDFHub.API.Models.DTOs;

namespace PDFHub.API.Services;

public interface IPdfService
{
    Task<ServiceResult<UploadPdfResponse>> UploadPdfAsync(IFormFile file, string userId);
    Task<ServiceResult<EditPdfResponse>> EditPdfAsync(int id, EditPdfRequestDto editDto, string userId);
}