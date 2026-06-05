using ECommerce527.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace Ecommerce527.API.Areas.Customer.Controllers
{
    [Area(CD.CUSTOMER_AREA)]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CartsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<Promotion> _promotionRepository;
        private readonly IRepository<Order> _orderRepository;

        public CartsController(UserManager<ApplicationUser> userManager, IRepository<Product> productRepository, IRepository<Cart> cartRepository, IRepository<Promotion> promotionRepository, IRepository<Models.Order> orderRepository)
        {
            _userManager = userManager;
            _productRepository = productRepository;
            _cartRepository = cartRepository;
            _promotionRepository = promotionRepository;
            _orderRepository = orderRepository;
        }
        [HttpGet]
        public async Task<IActionResult> GetList(string? code)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound();
            }
            var carts = await _cartRepository.GetAsync(c => c.ApplicationUserId == user.Id, includes: [p => p.Product]);
            if (code != null)
            {
                var promotion = await _promotionRepository.GetOneAsync(p =>
                                        p.Code == code &&
                                        p.IsValid &&
                                        p.ValidTo > DateTime.UtcNow &&
                                        p.MaxUsage > 0
                                        );
                if (promotion != null)
                {
                    var cart = carts.FirstOrDefault(c => c.ProductId == promotion.ProductId);
                    if (cart != null)
                    {
                        cart.Price -= cart.Price * (promotion.Discount / 100);
                        promotion.MaxUsage--;
                        await _cartRepository.CommitAsync();
                    }
                    else
                    {
                        return BadRequest(new ApiResponse<object>()
                        {
                            IsSuccess = false,
                            Message = "there is product can apply this promotion on it",
                        });
                    }
                }
                else
                {
                    return BadRequest(new ApiResponse<object>()
                    {
                        IsSuccess = false,
                        Message = "Invalid / Expired Promotion",
                    });
                }
            }
            var totalPrice = carts.Sum(c => c.Price * c.Count);
            return Ok(new ApiResponse<IEnumerable<Cart>>()
            {
                IsSuccess = true,
                Message = "data returned successfully",
                Data = carts
            });
        }
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int count)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound(new ApiResponse<object>
            {
                IsSuccess = false,
                Message = "user not found "
            });
            var product = await _productRepository.GetOneAsync(p => p.Id == productId);
            if (product is null) return NotFound(new ApiResponse<object>
            {
                IsSuccess = false,
                Message = "product not found "
            });
            var cart = new Cart()
            {
                ApplicationUserId = user.Id,
                ProductId = productId,
                Count = count,
                Price = product.Price - (product.Price * (product.Discount / 100)),
            };
            await _cartRepository.AddAsync(cart);
            await _cartRepository.CommitAsync();
            return Ok(new ApiResponse<object>()
            {
                IsSuccess = true,
                Message = "product added  Successfully"
            });
            //return RedirectToAction("Index" , "Home");
        }
        [HttpPut("{productId}/Increment")]
        public async Task<IActionResult> IncrementCount(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();
            var cart = await _cartRepository.GetOneAsync(
                c => c.ApplicationUserId == user.Id && c.ProductId == productId, includes: [p => p.Product]);
            if (cart is null)
            {
                return NotFound();
            }
            if (cart.Count < cart.Product.Quantity)
            {
                cart.Count++;
                await _cartRepository.CommitAsync();
            }
            return Ok(new ApiResponse<object>()
            {
                IsSuccess = true,
                Message = "product Incremented Successfully"
            });
        }
        [HttpPut("{productId}/Decrement")]

        public async Task<IActionResult> DecrementCount(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();
            var cart = await _cartRepository.GetOneAsync(
                c => c.ApplicationUserId == user.Id && c.ProductId == productId, includes: [p => p.Product]);
            if (cart is null)
            {
                return NotFound();
            }
            if (cart.Count > 1)
            {
                cart.Count--;
                await _cartRepository.CommitAsync();
            }
            return Ok(new ApiResponse<object>()
            {
                IsSuccess = true,
                Message = "product Decremented Successfully"
            });
        }
        [HttpPut("{productId}/DeleteProduct")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();
            var cart = await _cartRepository.GetOneAsync(
                c => c.ApplicationUserId == user.Id && c.ProductId == productId, includes: [p => p.Product]);
            if (cart is null)
            {
                return NotFound();
            }
            _cartRepository.Delete(cart);
            await _cartRepository.CommitAsync();
            return Ok(new ApiResponse<object>()
            {
                IsSuccess = true,
                Message = "product Deleteed Successfully"
            });
        }
        [HttpPost("Pay")]
        public async Task<IActionResult> Pay()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();
            var carts = await _cartRepository.GetAsync(c => c.ApplicationUserId == user.Id, includes: [p => p.Product]);
            if (carts is null) return NotFound();
            var order = new Order()
            {
                ApplicationUserId = user.Id,
                TotalPrice = carts.Sum(c => c.Price * c.Count),
            };
            await _orderRepository.AddAsync(order);
            await _orderRepository.CommitAsync();
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Customer/Checkout/Success/?orderId={order.Id}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Customer/Checkout/Cancel/?orderId={order.Id}",
            };

            foreach (var item in carts)
            {
                var sessionLineItemOptions = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                            Description = item.Product.Description,
                        },
                        UnitAmount = (long)item.Price * 100,
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLineItemOptions);
            }
            var service = new SessionService();
            var session = service.Create(options);
            order.SessionId = session.Id;

            await _orderRepository.CommitAsync();
            return Ok(new ApiResponse<string>()
            {
                IsSuccess = true,
                Message = "Url returned successfully",
                Data = session.Url
            });
        }
    }
}
