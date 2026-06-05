using ECommerce527.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce527.API.Areas.Customer.Controllers
{
    [Area(CD.CUSTOMER_AREA)]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private IRepository<Models.Product> _productRepository;

        public HomeController(IRepository<Product> productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] FilterProductRequest filter)
        {
            var products = await _productRepository.GetAsync(includes: [p => p.Category, p => p.Brand]);
            const decimal discount = 50;
            if (filter.ProductName is not null)
            {
                products = products.Where(p => p.Name.Contains(filter.ProductName));
            }
            if (filter.MinPrice > 0)
            {
                products = products.Where(p => p.Price - (p.Price * p.Discount / 100) >= filter.MinPrice);
            }
            if (filter.MaxPrice > 0)
            {
                products = products.Where(p => p.Price - (p.Price * p.Discount / 100) <= filter.MaxPrice);
            }
            if (filter.CategoryId > 0)
            {
                products = products.Where(p => p.CategoryId == filter.CategoryId);
            }
            if (filter.BrandId > 0)
            {
                products = products.Where(p => p.BrandId == filter.BrandId);
            }
            if (filter.IsHot)
            {
                products = products.Where(p => p.Discount > discount);
            }
            var ProductResponse = new ProductWithRelatedResponse();

            ProductResponse.TotalPages = (int)Math.Ceiling(products.Count() / 8.0);
            ProductResponse.CurrentPage = filter.page;
            ProductResponse.Products = products.Skip((filter.page - 1) * 8).Take(8);

            return Ok(new ApiResponse<ProductWithRelatedResponse>()
            {
                IsSuccess = true,
                Message = "Products returned successfully",
                Data = ProductResponse
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> ProductDetailes(int id)
        {
            var products = await _productRepository.GetAsync();
            //var product = products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
            var product = await _productRepository.GetOneAsync(p => p.Id == id, includes: [p => p.Category]);
            if (product is null)
            {
                return NotFound();
            }
            var relatedProducts = products.Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                                        .Skip(0)
                                        .Take(4);

            return Ok(new ApiResponse<ProductWithRelatedProductsResponse>()
            {
                IsSuccess = true,
                Message = "Data returned successfully",
                Data = new ProductWithRelatedProductsResponse()
                {
                    Product = product,
                    RelatedProducts = relatedProducts.AsEnumerable()
                }
            });
        }
    }
}
