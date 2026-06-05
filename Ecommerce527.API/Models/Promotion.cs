using Microsoft.EntityFrameworkCore;

namespace Ecommerce527.API.Models
{
    [Index(nameof(Code)  , IsUnique =true)]
    public class Promotion
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int MaxUsage { get; set; }
        public decimal Discount { get; set; }
        public bool IsValid { get; set; }
        public DateTime ValidTo { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
