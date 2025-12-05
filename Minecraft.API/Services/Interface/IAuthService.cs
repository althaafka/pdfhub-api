using Minecraft.API.Models;
using Minecraft.API.Models.DTOs;

namespace Minecraft.API.Services;
public interface IAuthService
{
    Task<ServiceResult> RegisterAsync(RegisterDto registerDto);
}