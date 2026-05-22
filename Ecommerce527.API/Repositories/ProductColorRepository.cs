

namespace ECommerce527.API.Repositories
{
    public class ProductColorRepository : Repository<ProductColor> , IProductColorRepository
    {
        public ProductColorRepository(ApplicationDbContext context) : base(context)
        {
        }

        public void RemoveRange(IEnumerable<ProductColor> productColors)
        {
            _context.ProductColors.RemoveRange(productColors);
        }
    }
}
