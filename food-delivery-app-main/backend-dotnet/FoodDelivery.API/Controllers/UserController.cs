using FoodDelivery.API.DTOs;
using FoodDelivery.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoodDelivery.API.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var token = await _userService.Login(request.Email, request.Password);
            if (token == null)
                return Ok(new { success = false, message = "Invalid credentials" });

            return Ok(new { success = true, token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var token = await _userService.Register(
                request.Name, request.Email, request.Password
            );

            if (token == null)
                return Ok(new { success = false, message = "User already exists" });

            return Ok(new { success = true, token });
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin(GoogleLoginRequest request)
        {
            var token = await _userService.GoogleLogin(
                request.Name, request.Email, request.Image
            );

            return Ok(new { success = true, token });
        }
    }
}
