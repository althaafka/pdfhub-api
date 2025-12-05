using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PDFHub.API.Data;
using PDFHub.API.Models;
using PDFHub.API.Models.DTOs;
using PDFHub.API.Models.Domains;
using PDFHub.API.Services;

public class AuthService: IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly PDFHubDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<IdentityUser> userManager,
        ITokenService tokenService,
        PDFHubDbContext context,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _context = context;
        _configuration = configuration;
    }

    public async Task<ServiceResult> RegisterAsync(RegisterRequestDto registerDto)
    {
        // Check if email exist
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return ServiceResult.FailureResult("Email is already registered");
        }

        // Check if username exist
        var existingUsername = await _userManager.FindByNameAsync(registerDto.Username);
        if (existingUsername != null)
        {
            return ServiceResult.FailureResult("Username is already taken");
        }

        // Create new user
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

    public async Task<ServiceResult<TokenResponseDto>> LoginAsync(LoginRequestDto loginDto, string? ipAddress, string? deviceInfo)
    {
        // Email or Username validation
        var user = await _userManager.FindByEmailAsync(loginDto.EmailOrUsername)
            ?? await _userManager.FindByNameAsync(loginDto.EmailOrUsername);

        if (user == null)
        {
            return ServiceResult<TokenResponseDto>.FailureResult("Invalid credentials");
        }

        // Verify password
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!isPasswordValid)
        {
            return ServiceResult<TokenResponseDto>.FailureResult("Invalid credentials");
        }

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var refreshTokenExpirationDays = int.Parse(
            _configuration["Jwt:RefreshTokenExpirationDays"] ?? "7"
        );
        var maxRefreshTokensPerUser = int.Parse(
            _configuration["Jwt:MaxRefreshTokensPerUser"] ?? "5"
        );
        var accessTokenExpirationMinutes = int.Parse(
            _configuration["Jwt:AccessTokenExpirationMinutes"] ?? "15"
        );

        // Check token limit
        var userTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.Id)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync();

        if (userTokens.Count >= maxRefreshTokensPerUser)
        {
            var tokensToRemove = userTokens.Skip(maxRefreshTokensPerUser - 1);
            _context.RefreshTokens.RemoveRange(tokensToRemove);
        }

        // Store refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            DeviceInfo = deviceInfo,
            IsActive = true
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        var response = new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiration = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes),
            RefreshTokenExpiration = refreshTokenEntity.ExpiresAt
        };

        return ServiceResult<TokenResponseDto>.SuccessResult(response, "Login successful");
    }

    public async Task<ServiceResult> LogoutAsync(string userId, string refreshToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.UserId == userId && rt.Token == refreshToken);

        if (token == null)
        {
            return ServiceResult.FailureResult("Invalid refresh token");
        }

        token.IsActive = false;
        token.RevokedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ServiceResult.SuccessResult("Logout successful");
    }

    public async Task<ServiceResult<TokenResponseDto>> RefreshTokenAsync(string refreshToken, string? ipAddress, string? deviceInfo)
    {
        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null)
        {
            return ServiceResult<TokenResponseDto>.FailureResult("Invalid refresh token");
        }

        if (!storedToken.IsActive)
        {
            return ServiceResult<TokenResponseDto>.FailureResult("Refresh token is not active");
        }

        // Generate new tokens
        var newAccessToken = _tokenService.GenerateAccessToken(storedToken.User);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        var refreshTokenExpirationDays = int.Parse(
            _configuration["Jwt:RefreshTokenExpirationDays"] ?? "7"
        );
        var accessTokenExpirationMinutes = int.Parse(
            _configuration["Jwt:AccessTokenExpirationMinutes"] ?? "15"
        );

        // Revoke old refresh token
        storedToken.IsActive = false;
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.ReplacedByToken = newRefreshToken;

        // Create new refresh token
        var newRefreshTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            UserId = storedToken.UserId,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            DeviceInfo = deviceInfo,
            IsActive = true
        };

        _context.RefreshTokens.Add(newRefreshTokenEntity);
        await _context.SaveChangesAsync();

        var response = new TokenResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiration = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes),
            RefreshTokenExpiration = newRefreshTokenEntity.ExpiresAt
        };

        return ServiceResult<TokenResponseDto>.SuccessResult(response, "Token refreshed successfully");
    }
}