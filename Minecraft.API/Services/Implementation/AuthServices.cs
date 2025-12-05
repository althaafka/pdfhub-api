using Microsoft.AspNetCore.Identity;
using Minecraft.API.Models;
using Minecraft.API.Models.DTOs;
using Minecraft.API.Services;

public class AuthService: IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;

    public AuthService(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ServiceResult> RegisterAsync(RegisterDto registerDto)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return ServiceResult.FailureResult("Email is already registered");
        }

        var existingUsername = await _userManager.FindByNameAsync(registerDto.Username);
        if (existingUsername != null)
        {
            return ServiceResult.FailureResult("Username is already taken");
        }

        var user = new IdentityUser
        {
            UserName = registerDto.Username,
            Email = registerDto.Email
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (result.Succeeded)
        {
            return ServiceResult.SuccessResult("User registered successfully");
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return ServiceResult.FailureResult($"Registration failed: {errors}");
    }
}