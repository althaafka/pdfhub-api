namespace PDFHub.API.Models.DTOs;

public class TokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiration { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
    public string TokenType { get; set; } = "Bearer";
}
