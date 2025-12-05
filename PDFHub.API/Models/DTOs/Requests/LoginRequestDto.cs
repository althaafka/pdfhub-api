namespace PDFHub.API.Models.DTOs;

public class LoginRequestDto
{
    public string EmailOrUsername { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
