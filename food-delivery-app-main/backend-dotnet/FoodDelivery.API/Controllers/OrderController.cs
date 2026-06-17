using FoodDelivery.API.DTOs;
using FoodDelivery.API.Models;
using FoodDelivery.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace FoodDelivery.API.Controllers
{
    [ApiController]
    [Route("api/order")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly FoodService _foodService;

        public OrderController(OrderService orderService, FoodService foodService)
        {
            _orderService = orderService;
            _foodService = foodService;
        }

        private string UserId => User.FindFirstValue("id")!;

        // PLACE ORDER
        [HttpPost("place")]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
        {
            // 🔹 Build order items from DB (NOT frontend)
            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;

            foreach (var item in request.Items)
            {
                var food = await _foodService.GetById(item.FoodId);
                if (food == null) continue;

                orderItems.Add(new OrderItem
                {
                    FoodId = food.Id,
                    Name = food.Name,
                    Price = food.Price,
                    Quantity = item.Quantity
                });

                totalAmount += food.Price * item.Quantity;
            }

            totalAmount += 2; // delivery fee

            var order = new Order
            {
                UserId = UserId,
                Items = orderItems,
                Amount = totalAmount,
                Address = request.Address,
                Payment = false,
                Status = "Pending"
            };

            await _orderService.CreateOrder(order);

            // 🔹 Stripe session
            var lineItems = orderItems.Select(i => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "inr",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = i.Name
                    },
                    UnitAmount = (long)(i.Price * 100 * 86)
                },
                Quantity = i.Quantity
            }).ToList();

            var sessionService = new SessionService();
            var session = await sessionService.CreateAsync(new SessionCreateOptions
            {
                Mode = "payment",
                LineItems = lineItems,
                SuccessUrl = $"http://localhost:5173/verify?success=true&orderId={order.Id}",
                CancelUrl = $"http://localhost:5173/verify?success=false&orderId={order.Id}"
            });

            return Ok(new { success = true, session_url = session.Url });
        }

        // VERIFY ORDER
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyOrder([FromBody] VerifyOrderRequest request)
        {
            if (request.Success == "true")
            {
                await _orderService.MarkPaid(request.OrderId);
                return Ok(new { success = true });
            }

            await _orderService.Delete(request.OrderId);
            return Ok(new { success = false });
        }
    }
}
