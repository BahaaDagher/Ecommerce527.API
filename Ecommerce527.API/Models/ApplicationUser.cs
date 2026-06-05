using Microsoft.AspNetCore.Identity;

namespace Ecommerce527.API.Models
{
    public class ApplicationUser :IdentityUser
    {
        public string Name {get ; set; }
        public string Address {get ; set; }
    } 
}
