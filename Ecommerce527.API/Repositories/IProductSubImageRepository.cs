namespace ECommerce527.API.Repositories
{
    public interface IProductSubImageRepository : IRepository<ProductSubImage>
    {
        public void RemoveRange(IEnumerable<ProductSubImage> productSubImages); 
    }
}
