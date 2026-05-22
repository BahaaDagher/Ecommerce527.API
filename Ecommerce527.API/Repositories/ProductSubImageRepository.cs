
using Microsoft.EntityFrameworkCore;

namespace ECommerce527.API.Repositories
{
    public class ProductSubImageRepository : Repository<ProductSubImage> , IProductSubImageRepository
    {
        public ProductSubImageRepository(ApplicationDbContext context) : base(context)
        {
        }

        public void RemoveRange(IEnumerable<ProductSubImage> productSubImages)
        {
            _context.ProductSubImages.RemoveRange(productSubImages); 
        }
    }
}
