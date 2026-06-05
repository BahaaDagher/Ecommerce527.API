namespace Ecommerce527.API.DTOs.Request
{
    public class FilterProductRequest
    {
        public string? ProductName { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public bool IsHot { get; set; }
        public bool IsLowQuality { get; set; }
        public int page { get; set; } = 1;
    }
}
