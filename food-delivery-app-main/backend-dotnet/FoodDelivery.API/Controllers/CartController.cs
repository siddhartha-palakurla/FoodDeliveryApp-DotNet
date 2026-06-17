using FoodDelivery.API.DTOs;
using FoodDelivery.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodDelivery.API.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [Authorize] // replaces authMiddleware
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        private string UserId =>
            User.FindFirstValue("id")!;

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(CartRequest request)
        {
            await _cartService.AddToCart(UserId, request.ItemId);
            return Ok(new { success = true, message = "Added to Cart" });
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveFromCart(CartRequest request)
        {
            await _cartService.RemoveFromCart(UserId, request.ItemId);
            return Ok(new { success = true, message = "Removed From Cart" });
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetCart()
        {
            var cartData = await _cartService.GetCart(UserId);
            return Ok(new { success = true, cartData });
        }
    }
}
