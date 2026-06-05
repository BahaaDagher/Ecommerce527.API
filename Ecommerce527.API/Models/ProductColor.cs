using Microsoft.EntityFrameworkCore;

namespace Ecommerce527.API.Models
{
    [PrimaryKey(nameof(ProductId), nameof(Color))]
    public class ProductColor
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string Color { get; set; }
    }
}
