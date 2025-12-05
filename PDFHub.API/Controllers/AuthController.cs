using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PDFHub.API.Models.DTOs;
using PDFHub.API.Services;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var deviceInfo = Request.Headers.UserAgent.ToString();

        var result = await _authService.LoginAsync(loginDto, ipAddress, deviceInfo);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto refreshTokenDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var result = await _authService.LogoutAsync(userId, refreshTokenDto.RefreshToken);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto refreshTokenDto)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var deviceInfo = Request.Headers.UserAgent.ToString();

        var result = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken, ipAddress, deviceInfo);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }
}