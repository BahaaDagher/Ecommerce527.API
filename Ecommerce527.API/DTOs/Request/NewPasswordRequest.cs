using System.ComponentModel.DataAnnotations;

namespace Ecommerce527.API.DTOs.Request
{
    public class NewPasswordRequest
    {
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password) , Compare(nameof(Password))]
        public string ConfirmedPassword { get; set; }
        public string UserId { get; set; }
    }
}
