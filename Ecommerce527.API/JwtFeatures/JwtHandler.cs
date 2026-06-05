using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ecommerce510.Api.JwtFeatures
{
    public class JwtHandler : IJwtHandler
    {
        private readonly IConfiguration _configuration;
        private readonly IConfigurationSection jwtSettings;
        private readonly UserManager<ApplicationUser> _userManager;

        public JwtHandler(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            jwtSettings = _configuration.GetSection("JwtSettings");
            _userManager = userManager;
        }

        public async Task<string> GenerateAccessTokenAsync(ApplicationUser user )
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["securityKey"])); 
            var SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var Claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name ,user.Name ) , 
                new Claim(ClaimTypes.Email ,user.Email) , 
                new Claim(ClaimTypes.NameIdentifier ,user.Id ) , 
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach(var role in userRoles)
            {
                Claims.Add(new Claim(ClaimTypes.Role , role)); 
            }

            var jwtSecurityToken = new JwtSecurityToken(
                issuer : jwtSettings["validIssuer"] ,
                audience: jwtSettings["validAudience"],
                claims: Claims , 
                expires : DateTime.Now.AddMinutes( Convert.ToDouble( jwtSettings["expireTime"])),
                signingCredentials: SigningCredentials
            ); 

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken) ; 
        }
    }
}
