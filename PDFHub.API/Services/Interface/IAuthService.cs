using PDFHub.API.Models;
using PDFHub.API.Models.DTOs;

namespace PDFHub.API.Services;
public interface IAuthService
{
    Task<ServiceResult> RegisterAsync(RegisterRequestDto registerDto);
    Task<ServiceResult<TokenResponseDto>> LoginAsync(LoginRequestDto loginDto, string? ipAddress, string? deviceInfo);
    Task<ServiceResult> LogoutAsync(string userId, string refreshToken);
    Task<ServiceResult<TokenResponseDto>> RefreshTokenAsync(string refreshToken, string? ipAddress, string? deviceInfo);
}