using ECommerce527.API.Repositories;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce527.API.Areas.Admin.Controllers
{
    [Area(CD.ADMIN_AREA)]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE} , {CD.ADMIN_ROLE} , {CD.EMPLOYEE_ROLE}")]
    public class BrandsController : ControllerBase
    {
        private IRepository<Brand> _brandRepository;// = new Repository<Brand>();

        public BrandsController(IRepository<Brand> brandRepository)
        {
            _brandRepository = brandRepository;
        }
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            //var brands = _context.Brands.AsQueryable();
            var brands = await _brandRepository.GetAsync();

            return Ok(new ApiResponse<IEnumerable<Brand>>()
            {
                IsSuccess = true,
                Message = "Brands returned Successfully",
                Data = brands.AsEnumerable()
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            //var brands = _context.Brands.AsQueryable();
            var brand = await _brandRepository.GetOneAsync(b => b.Id == id);
            if (brand is null)
            {
                return NotFound(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "Invalid Brand Id"
                });
            }

            return Ok(new ApiResponse<Brand>()
            {
                IsSuccess = true,
                Message = "Categories returned Successfully",
                Data = brand
            });
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateUpdateBrandRequest createUpdateBrandRequest)
        {
            Brand brand = createUpdateBrandRequest.Adapt<Brand>();
            if (createUpdateBrandRequest.ImgFile is not null && createUpdateBrandRequest.ImgFile.Length > 0)
            {
                //var fileName = Guid.NewGuid().ToString()  +  Path.GetExtension(ImgFile.FileName) ; 
                var fileName = Guid.NewGuid().ToString() + "-" + createUpdateBrandRequest.ImgFile.FileName;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\brand_images", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    createUpdateBrandRequest.ImgFile.CopyTo(stream);
                }
                brand.Img = fileName;
            }
            //_context.Brands.Add(brand);
            var createdBrand = await _brandRepository.AddAsync(brand);
            await _brandRepository.CommitAsync();
            return CreatedAtAction(nameof(GetOne), new { id = createdBrand.Entity.Id }, new ApiResponse<Brand>()
            {
                IsSuccess = true,
                Message = "brand Created Successfully",
            });
        }
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE} , {CD.ADMIN_ROLE} ")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromForm] CreateUpdateBrandRequest createUpdateBrandRequest)
        {
            //var brandInDb = _context.Brands.AsNoTracking().FirstOrDefault(b => b.Id == brand.Id);
            var brandInDb = await _brandRepository.GetOneAsync(b => b.Id == id, tracked: false);
            brandInDb = createUpdateBrandRequest.Adapt(brandInDb);
            brandInDb.Id = id;
            if (createUpdateBrandRequest.ImgFile is not null && createUpdateBrandRequest.ImgFile.Length > 0)
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\brand_images", brandInDb.Img);
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }

                //var fileName = Guid.NewGuid().ToString()  +  Path.GetExtension(ImgFile.FileName) ; 
                var fileName = Guid.NewGuid().ToString() + "-" + createUpdateBrandRequest.ImgFile.FileName;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\brand_images", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    createUpdateBrandRequest.ImgFile.CopyTo(stream);
                }
                brandInDb.Img = fileName;
            }
            //_context.Brands.Update(brand);
            _brandRepository.Update(brandInDb);
            await _brandRepository.CommitAsync();
            return Ok(new ApiResponse<object>()
            {
                IsSuccess = true,
                Message = "brand Updated Successfully",
            });
        }
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE} , {CD.ADMIN_ROLE} ")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            //var brand = _context.Brands.FirstOrDefault(c => c.Id == id);
            var brand = await _brandRepository.GetOneAsync(c => c.Id == id);

            if (brand == null)
                return NotFound(new ApiResponse<Brand>()
                {
                    IsSuccess = true,
                    Message = "Invalid Brand Id"
                });

            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\brand_images", brand.Img);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            //_context.Remove(brand);
            //_context.SaveChanges();
            _brandRepository.Delete(brand);
            await _brandRepository.CommitAsync();
            return NoContent();
        }
    }
}
