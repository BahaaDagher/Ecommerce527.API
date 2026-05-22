using ECommerce527.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;

namespace Ecommerce527.API.Areas.Admin.Controllers
{
    [Area(CD.ADMIN_AREA)]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE}  , {CD.ADMIN_ROLE} , {CD.EMPLOYEE_ROLE}")]

    public class CategoriesController : ControllerBase
    {
        IRepository<Category> _categoryRepository; // = new Repository<Category>(); 

        public CategoriesController(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            //var categories = _context.Categories.AsQueryable(); 
            var categories = await _categoryRepository.GetAsync();
            // filter 
            return Ok(new ApiResponse<IEnumerable<Category>>()
            {
                IsSuccess = true,
                Message = "categories Created   Successfully",
                Data =  categories
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            //var categories = _context.Categories.AsQueryable(); 
            var category = await _categoryRepository.GetOneAsync(c=>c.Id == id);
            if (category is null )
            {
                return NotFound(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "invalid Category id",
                });
            }
                // filter 
            return Ok(new ApiResponse<Category>()
            {
                IsSuccess = true,
                Message = "categories Created   Successfully",
                Data = category
            });
        }
        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            //_context.Categories.Add(category);
            //_context.SaveChanges();
            var Savedcategory =  await _categoryRepository.AddAsync(category);
            await _categoryRepository.CommitAsync();
            return CreatedAtAction(nameof(GetOne)  , new { id = Savedcategory.Entity.Id }, new ApiResponse<object>()
            {
                IsSuccess = true,
                Message = "category Created Successfully ",
            });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update( int id , Category categoryRequest)
        {
            var category = await _categoryRepository.GetOneAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "invalid Category id",
                });

            }
            category.Name = categoryRequest.Name;
            category.Description = categoryRequest.Description;
            category.Status = categoryRequest.Status;
            //_context.Categories.Update(category);
            //_context.SaveChanges();
            _categoryRepository.Update(category);
            await _categoryRepository.CommitAsync();
            return Ok( new ApiResponse<Category>()
            {
                IsSuccess = true,
                Message = "category Updated Successfully ",
            });
        }
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE}  , {CD.ADMIN_ROLE} ")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            //var category = _context.Categories.FirstOrDefault(c => c.Id == id);
            var category = await _categoryRepository.GetOneAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "invalid Category id",
                });

            }
            //_context.Categories.Remove(category);
            //_context.SaveChanges();
            _categoryRepository.Delete(category);
            await _categoryRepository.CommitAsync();
            return NoContent(); 
        }
    }
}
