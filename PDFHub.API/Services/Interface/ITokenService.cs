using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace PDFHub.API.Services;

public interface ITokenService
{
    string GenerateAccessToken(IdentityUser user);
    string GenerateRefreshToken();
}
