using ECommerce527.API.Repositories;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce527.API.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    //[Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE} , {CD.ADMIN_ROLE} , {CD.EMPLOYEE_ROLE}")]
    public class ProductsController : ControllerBase
    {
        //ApplicationDbContext _context = new ApplicationDbContext();

        private IRepository<Product> _productRepository;// = new Repository<Product>();
        private IRepository<Category> _categoryRepository;// = new Repository<Category>();
        private IRepository<Brand> _brandRepository;// = new Repository<Brand>();
        private IProductSubImageRepository _productSubImageRepository;// = new ProductSubImageRepository();
        private IProductColorRepository _productColorRepository;//= new ProductColorRepository();

        public ProductsController(IRepository<Product> productRepository, IRepository<Category> categoryRepository, IRepository<Brand> brandRepository, IProductSubImageRepository productSubImageRepository, IProductColorRepository productColorRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _productSubImageRepository = productSubImageRepository;
            _productColorRepository = productColorRepository;
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
            if (filter.IsLowQuality)
            {
                products = products.OrderBy(p => p.Quantity);

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
        public async Task<IActionResult> GetOne(int id)
        {
            var product = await _productRepository.GetOneAsync(p => p.Id == id);
            if (product is null)
            {
                return NotFound(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "invalid product Id"
                });
            }
            return Ok(new ApiResponse<Product>()
            {
                IsSuccess = false,
                Message = "invalid product Id",
                Data = product
            });
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateUpdateProductRequest createUpdateProductRequest)
        {
            var product = createUpdateProductRequest.Adapt<Product>();

            if (createUpdateProductRequest.ImgFile is not null && createUpdateProductRequest.ImgFile.Length > 0)
            {
                //var fileName = Guid.NewGuid().ToString()  +  Path.GetExtension(ImgFile.FileName) ; 
                var fileName = Guid.NewGuid().ToString() + "-" + createUpdateProductRequest.ImgFile.FileName;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    createUpdateProductRequest.ImgFile.CopyTo(stream);
                }
                product.MainImg = fileName;
            }
            //var SavedProduct = _context.Products.Add(product);
            var SavedProduct = await _productRepository.AddAsync(product);
            await _productRepository.CommitAsync();
            if (createUpdateProductRequest.SubImageFiles is not null && createUpdateProductRequest.SubImageFiles.Count > 0)
            {
                foreach (var image in createUpdateProductRequest.SubImageFiles)
                {
                    //var fileName = Guid.NewGuid().ToString()  +  Path.GetExtension(ImgFile.FileName) ; 
                    var fileName = Guid.NewGuid().ToString() + "-" + image.FileName;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images\\productSubImages", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        image.CopyTo(stream);
                    }
                    //_context.ProductSubImages.Add(new ProductSubImage()
                    await _productSubImageRepository.AddAsync(new ProductSubImage()
                    {
                        ProductId = SavedProduct.Entity.Id,
                        Img = fileName
                    });
                }
            }
            if (createUpdateProductRequest.Colors is not null && createUpdateProductRequest.Colors.Count > 0)
            {
                foreach (var color in createUpdateProductRequest.Colors)
                {
                    //_context.ProductColors.Add(new ProductColor()
                    await _productColorRepository.AddAsync(new ProductColor()
                    {
                        ProductId = SavedProduct.Entity.Id,
                        Color = color
                    });
                }
            }
            await _productSubImageRepository.CommitAsync();
            await _productColorRepository.CommitAsync();


            return CreatedAtAction(nameof(GetOne), new { id = SavedProduct.Entity.Id }, new ApiResponse<object>()
            {
                IsSuccess = true,
                Message = "Product created successfully"
            });
        }

        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE} , {CD.ADMIN_ROLE} ")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, CreateUpdateProductRequest createUpdateProductRequest)
        {
            //var productInDb = _context.Products.AsNoTracking().FirstOrDefault(b => b.Id == product.Id); 
            var product = await _productRepository.GetOneAsync(tracked: false, filter: b => b.Id == id);
            product = createUpdateProductRequest.Adapt(product);
            if (createUpdateProductRequest.ImgFile is not null && createUpdateProductRequest.ImgFile.Length > 0)
            {
                //var fileName = Guid.NewGuid().ToString()  +  Path.GetExtension(ImgFile.FileName) ; 
                var fileName = Guid.NewGuid().ToString() + "-" + createUpdateProductRequest.ImgFile.FileName;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    createUpdateProductRequest.ImgFile.CopyTo(stream);
                }
                product.MainImg = fileName;

                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images", product.MainImg);
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }
            //_context.Products.Update(product);
            //_context.SaveChanges();
            _productRepository.Update(product);
            await _productRepository.CommitAsync();
            if (createUpdateProductRequest.SubImageFiles != null && createUpdateProductRequest.SubImageFiles.Count() > 0)
            {
                //var productSubImages = _context.ProductSubImages.Where(p => p.ProductId == product.Id);
                var productSubImages = await _productSubImageRepository.GetAsync(filter: p => p.ProductId == product.Id);
                //_context.ProductSubImages.RemoveRange(productSubImages);
                _productSubImageRepository.RemoveRange(productSubImages);
                foreach (var image in productSubImages)
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images\\productSubImages", image.Img);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }
                foreach (var image in createUpdateProductRequest.SubImageFiles)
                {
                    //var fileName = Guid.NewGuid().ToString()  +  Path.GetExtension(ImgFile.FileName) ; 
                    var fileName = Guid.NewGuid().ToString() + "-" + image.FileName;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images\\productSubImages", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        image.CopyTo(stream);
                    }
                    //_context.ProductSubImages.Add(new ProductSubImage()
                    await _productSubImageRepository.AddAsync(new ProductSubImage()
                    {
                        ProductId = product.Id,
                        Img = fileName
                    });
                }
                await _productSubImageRepository.CommitAsync();
            }
            if (createUpdateProductRequest.Colors is not null && createUpdateProductRequest.Colors.Count > 0)
            {
                //var productColors = _context.ProductColors.Where(p => p.ProductId == product.Id);
                var productColors = await _productColorRepository.GetAsync(p => p.ProductId == product.Id);
                //_context.ProductColors.RemoveRange(productColors);
                _productColorRepository.RemoveRange(productColors);
                foreach (var color in createUpdateProductRequest.Colors)
                {
                    //_context.ProductColors.Add(new ProductColor()
                    await _productColorRepository.AddAsync(new ProductColor()
                    {
                        ProductId = product.Id,
                        Color = color
                    });
                }
                await _productColorRepository.CommitAsync();
            }

            await _productRepository.CommitAsync();
            return Ok(new ApiResponse<object>()
            {
                IsSuccess = true,
                Message = "Product Updated Successfully"
            });
        }
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE} , {CD.ADMIN_ROLE} ")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            //var product = _context.Products.FirstOrDefault(c => c.Id == id);    
            var product = await _productRepository.GetOneAsync(c => c.Id == id);
            if (product == null)
                return RedirectToAction("NotFoundPage", "Home");

            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images", product.MainImg);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }
            //var ProductSubImages = _context.ProductSubImages.Where(ps => ps.ProductId == id); 
            var ProductSubImages = await _productSubImageRepository.GetAsync(filter: ps => ps.ProductId == id);

            foreach (var item in ProductSubImages)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\productSubImages", item.Img);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }

            //_context.Remove(product);
            //_context.SaveChanges(); 
            _productRepository.Delete(product);
            await _productRepository.CommitAsync();
            return NoContent();
        }
    }
}
