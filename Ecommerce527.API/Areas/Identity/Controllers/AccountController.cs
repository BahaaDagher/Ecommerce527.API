
using Ecommerce510.Api.JwtFeatures;
using ECommerce527.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Ecommerce527.API.Areas.Identity.Controllers
{
    [Area(CD.IDENTITY_AREA)]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IRepository<ApplicationUserOtp> _applicationUserOtpRepository;
        private readonly IJwtHandler _jwtHandler;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, IRepository<ApplicationUserOtp> applicationUserOtpRepository, IJwtHandler jwtHandler)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _applicationUserOtpRepository = applicationUserOtpRepository;
            _jwtHandler = jwtHandler;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            ApplicationUser user = new ApplicationUser()
            {
                Name = registerRequest.Name,
                Address = registerRequest.Address,
                Email = registerRequest.Email,
                UserName = registerRequest.UserName,
            };
            var result = await _userManager.CreateAsync(user, registerRequest.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse<object>()
                {
                    IsSuccess = false  ,
                    Message = "Invalid Data" , 
                    Errors = result.Errors.Select(e=>e.Description)
                });
            }
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail), "Account", new { area = "Identity", userId = user.Id, token }, Request.Scheme);
            await _emailSender.SendEmailAsync(
                registerRequest.Email,
                "Ecommerce527 Confirm Email",
                $"<h1>click <a href={link}> here </a> to confirm Your Email </h1>");
            await _userManager.AddToRoleAsync(user, CD.CUSTOMER_ROLE);
            return CreatedAtAction(nameof(Login) , new ApiResponse<object>()
            {
                IsSuccess = true  ,
                Message = "User Registered Successfully" , 
            });
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            var user = await _userManager.FindByEmailAsync(loginRequest.UserNameOrEmail)
                ?? await _userManager.FindByNameAsync(loginRequest.UserNameOrEmail);

            if (user is null)
            {
                return NotFound(new ApiResponse<object>()
                {
                    IsSuccess = false  , 
                    Message = "invalid userName Or Password" 
                }); 
            }
            var result = await _signInManager.PasswordSignInAsync(user, loginRequest.Password, loginRequest.RememberMe, true);
            if (!result.Succeeded)
            {
                List<string> errors = new List<string>(); 
                if (result.IsLockedOut)
                {
                    errors.Add("you are Locked Now try again Later"); 
                }
                else if (result.IsNotAllowed)
                {
                    errors.Add("please Confirm Your Email First");
                }
                else
                {
                    errors.Add("invalid userName Or Password");
                }
                return BadRequest(new ApiResponse<object>()
                {
                    IsSuccess = false  , 
                    Message = "Invalid Data" , 
                    Errors = errors
                });
            }
            var token = await _jwtHandler.GenerateAccessTokenAsync(user);
            return Ok(new AuthenticatedResponse()
            {
                AccessToken = token
            });
           
        }
        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return NotFound(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "invalid user"
                });
            }
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "Cant confirm Email" , 
                    Errors = result.Errors.Select(e=>e.Description) 
                });
            }
            return Ok(new ApiResponse<object>()
            {
                IsSuccess = true,
                Message = "Confirmed Email Successfully"
            });
        }
        [HttpPost("ResendEmailConfirmation")]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationRequest resendEmailConfirmationRequest)
        {
            var user = await _userManager.FindByEmailAsync(resendEmailConfirmationRequest.UserNameOrEmail)
               ?? await _userManager.FindByNameAsync(resendEmailConfirmationRequest.UserNameOrEmail);

            if (user is null)
            {
                return NotFound(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "invalid user"
                });
            }
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail), "Account", new { area = "Identity", userId = user.Id, token }, Request.Scheme);
            await _emailSender.SendEmailAsync(
                user.Email,
                "Ecommerce527 Confirm Email",
                $"<h1>click <a href={link}> here </a> to confirm Your Email </h1>");
            return Ok(new ApiResponse<object>()
            {
                IsSuccess = true,
                Message = "Resend Email ConfirmationSuccessfully"
            });
        }
        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest forgetPasswordRequest)
        {
            var user = await _userManager.FindByEmailAsync(forgetPasswordRequest.UserNameOrEmail)
             ?? await _userManager.FindByNameAsync(forgetPasswordRequest.UserNameOrEmail);

            if (user is null)
            {
                return NotFound(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "invalid user"
                });
            }
            var applicationUserotps = await _applicationUserOtpRepository.GetAsync(e => e.ApplicationUserId == user.Id);
            var count = applicationUserotps.Count(e => (DateTime.UtcNow - e.CreatedAt).TotalHours <= 24);
            if (count >= 5)
            {
                return BadRequest(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "To many Attempts please try again Later"
                });
            }
            var otp = new Random().Next(1000, 9999).ToString();
            var applicationUserOtp = new ApplicationUserOtp(user.Id, otp);
            await _applicationUserOtpRepository.AddAsync(applicationUserOtp);
            await _applicationUserOtpRepository.CommitAsync();
            await _emailSender.SendEmailAsync(
               user.Email,
               "Ecommerce527 Forget Password",
               $"<h1> use this otp <span style=\"color: red\">{otp}</span> to Reset your password</h1>");
            return Ok(new ApiResponse<object>()
            {
                IsSuccess = true,
                Message = "Otp Send Successfully"
            });
        }
        [HttpPost("ValidateOTP")]
        public async Task<IActionResult> ValidateOTP(ValidateOTPRequest validateOTPRequest)
        {
            var user = await _userManager.FindByIdAsync(validateOTPRequest.UserId);

            if (user is null)
            {
                return NotFound(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "invalid user"
                });
            }

            var otps = await _applicationUserOtpRepository.GetAsync(e =>
                e.ApplicationUserId == user.Id &&
                e.IsValid == true &&
                e.ValidTo >= DateTime.UtcNow
            );
            var otp = otps.OrderByDescending(e => e.CreatedAt).FirstOrDefault();
            if (otp is null || otp.OTP != validateOTPRequest.OTP)
            {
                return BadRequest(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "invalid / Expired OTP"
                });
            }
            otp.IsValid = false;
            await _applicationUserOtpRepository.CommitAsync();
            return Ok(new ApiResponse<object>()
            {
                IsSuccess = true,
                Message = "Otp Confirmed"
            });
        }
        [HttpPost("NewPassword")]
        public async Task<IActionResult> NewPassword(NewPasswordRequest newPasswordRequest)
        {
           
            var user = await _userManager.FindByIdAsync(newPasswordRequest.UserId);
            if (user is null)
            {
                return NotFound(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "invalid user"
                });
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPasswordRequest.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "invalid user" , 
                    Errors = result.Errors.Select(e=>e.Description)
                });
            }
            return Ok(new ApiResponse<object>()
            {
                IsSuccess = true,
                Message = "Reset Password Successfully"
            });
        }

    }
}
