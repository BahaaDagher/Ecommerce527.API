using System.ComponentModel.DataAnnotations;

namespace Ecommerce527.API.DTOs.Request
{
    public class LoginRequest
    {
        public string UserNameOrEmail { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool  RememberMe { get; set; }

    }
}
