using Microsoft.AspNetCore.Identity;

namespace PDFHub.API.Models.Domains;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }

    // Device and IP tracking
    public string? IpAddress { get; set; }
    public string? DeviceInfo { get; set; }

    // Relation
    public IdentityUser User { get; set; } = null!;
}
