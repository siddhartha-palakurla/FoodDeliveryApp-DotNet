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
        [Authorize]
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
        [Authorize]
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyOrder([FromBody] VerifyOrderRequest request)
        {
            if (request.Success == "true")
            {
                await _orderService.MarkPaid(request.OrderId);

                return Ok(new
                {
                    success = true,
                    message = "Paid"
                });
            }

            await _orderService.Delete(request.OrderId);

            return Ok(new
            {
                success = false,
                message = "Not paid"
            });
        }


        //User Order
        [Authorize]
        [HttpPost("userorders")]
        public async Task<IActionResult> UserOrders()
        {
            var orders = await _orderService.UserOrders(UserId);

            return Ok(new
            {
                success = true,
                data = orders
            });
        }



        // ADMIN - List all orders
        //[AllowAnonymous]
        [HttpGet("list")]
        public async Task<IActionResult> ListOrders()
        {
            try
            {
                var orders = await _orderService.ListOrders();

                return Ok(new
                {
                    success = true,
                    data = orders
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.ToString()
                });
            }
        }



        // ADMIN - Update order status
        [HttpPost("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request)
        {
            try
            {
                await _orderService.UpdateStatus(
                    request.OrderId,
                    request.Status
                );

                return Ok(new
                {
                    success = true,
                    message = "Status Updated"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.ToString()
                });
            }
        }

    }
}
