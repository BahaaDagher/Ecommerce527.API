namespace Ecommerce527.API.DTOs.Response
{
    public class ProductWithRelatedResponse
    {
        public IEnumerable<Product> Products { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
