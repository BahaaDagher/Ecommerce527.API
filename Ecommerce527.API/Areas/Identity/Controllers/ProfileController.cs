using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce527.API.Areas.Identity.Controllers
{
    [Area(CD.IDENTITY_AREA)]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> GetInfo()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "invalid user"
                });
            }
            //var applicationUserVM = new ApplicationUserVM();

            //applicationUserVM.Name = user.Name; 
            //applicationUserVM.Email = user.Email;
            //applicationUserVM.Address= user.Address;
            //applicationUserVM.PhoneNumber= user.PhoneNumber;

            var applicationUserVM = user.Adapt<ApplicationUserResponse>();

            return Ok(new ApiResponse<ApplicationUserResponse>()
            {
                IsSuccess = true,
                Message = "user Returned Successfully" ,
                Data = applicationUserVM
            });
        }
        [HttpPost("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile(ApplicationUserRequest applicationUserRequest)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "invalid user"
                });
            }
            //var userDb = applicationUserVM.Adapt<ApplicationUser>();
            user.Name = applicationUserRequest.Name;
            user.PhoneNumber = applicationUserRequest.PhoneNumber;
            user.Address = applicationUserRequest.Address;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "user updated Failed" , 
                    Errors = result.Errors.Select(e => e.Description)
                });
            }
            else
            {
                return Ok(new ApiResponse<object>()
                {
                    IsSuccess = true,
                    Message = "user Updated Successfully"
                });
            }
        }
        [HttpPost("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword(ApplicationUserRequest applicationUserRequest)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "invalid user"
                });
            }
            var result = await _userManager.ChangePasswordAsync(user, applicationUserRequest.CurrentPassword, applicationUserRequest.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "Change Password Failed",
                    Errors = result.Errors.Select(e => e.Description)
                });
            }
            else
            {
                return Ok(new ApiResponse<object>()
                {
                    IsSuccess = true,
                    Message = "password Changed Successfully"
                });
            }
        }
    }
}
