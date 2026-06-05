using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ecommerce510.Api.JwtFeatures
{
    public interface IJwtHandler
    {
        Task<string> GenerateAccessTokenAsync(ApplicationUser user);
    }
}
