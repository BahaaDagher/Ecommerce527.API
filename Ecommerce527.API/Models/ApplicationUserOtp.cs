using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce527.API.Models
{
    public class ApplicationUserOtp
    {
        public string Id { get; set; }
        public string ApplicationUserId { get; set; }
        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser ApplicationUser { get; set; }
        public string OTP {  get; set; }
        public bool IsValid { get; set; }
        public DateTime ValidTo { get; set; }
        public DateTime CreatedAt  { get; set; }

        public ApplicationUserOtp()
        {

        }
        public ApplicationUserOtp( string ApplicationUserId , string OTP  )
        {
            this.ApplicationUserId = ApplicationUserId; 
            this.OTP = OTP;
            Id = Guid.NewGuid().ToString();
            IsValid = true;
            ValidTo = DateTime.UtcNow.AddMinutes(30);
            CreatedAt = DateTime.UtcNow;
        }
    }
}
