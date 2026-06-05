using ECommerce527.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace Ecommerce527.API.Areas.Customer.Controllers
{
    [Area(CD.CUSTOMER_AREA)]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class CheckoutController : ControllerBase
    {
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<Models.Product> _productRepository;
        private readonly IRepository<Cart> _cartRepository;

        public CheckoutController(IEmailSender emailSender, UserManager<ApplicationUser> userManager, IRepository<Order> orderRepository, IRepository<OrderItem> orderItemRepository, IRepository<Models.Product> productRepository, IRepository<Cart> cartRepository)
        {
            _emailSender = emailSender;
            _userManager = userManager;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _cartRepository = cartRepository;
        }

        [HttpPost("Success")]
        public async Task<IActionResult> Success(int orderId)
        {
            var order = await _orderRepository.GetOneAsync(o => o.Id == orderId);
            if (order is null) return NotFound();
            var user = await _userManager.FindByIdAsync(order.ApplicationUserId);
            // Send Email 
            await _emailSender.SendEmailAsync(
                user.Email,
                "Payment Successfull",
                $"Your payment is Successfull"
                );

            // Change Order status to InProgress 

            order.OrderStatus = OrderStatus.InProgress;
            var service = new SessionService();
            var session = service.Get(order.SessionId);
            order.TransactionId = session.PaymentIntentId;

            // add orderItem From cart
            // decrease the product Quantity 
            // delete the data from Cart 
            var carts = await _cartRepository.GetAsync();
            foreach (var item in carts)
            {
                var orderItem = new OrderItem()
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Count = item.Count,
                    Price = item.Price,
                };
                await _orderItemRepository.AddAsync(orderItem);
                var product = await _productRepository.GetOneAsync(p => p.Id == item.ProductId);
                product.Quantity -= item.Count;
                await _productRepository.CommitAsync();
                _cartRepository.Delete(item);

            }
            await _orderRepository.CommitAsync();


            return Ok(new ApiResponse<object>()
            {
                IsSuccess = true,
                Message = "Successfull Payment"
            });
        }
        [HttpPost("Cancel")]
        public IActionResult Cancel(int orderId)
        {
            return Ok(new ApiResponse<object>()
            {
                IsSuccess = true,
                Message = "Successfull Cancel"
            });
        }
    }

}
