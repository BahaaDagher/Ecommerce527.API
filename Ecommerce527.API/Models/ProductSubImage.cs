using Microsoft.EntityFrameworkCore;

namespace Ecommerce527.API.Models
{
    [PrimaryKey(nameof(ProductId) , nameof(Img))]
    public class ProductSubImage
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string Img { get; set; }
    }
}
