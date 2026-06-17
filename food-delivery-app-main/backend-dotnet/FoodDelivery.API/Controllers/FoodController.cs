using FoodDelivery.API.DTOs;
using FoodDelivery.API.Models;
using FoodDelivery.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace FoodDelivery.API.Controllers
{
    [ApiController]
    [Route("api/food")]
    public class FoodController : ControllerBase
    {
        private readonly FoodService _foodService;
        private readonly IWebHostEnvironment _env;

        public FoodController(FoodService foodService, IWebHostEnvironment env)
        {
            _foodService = foodService;
            _env = env;
        }

        // Add food (with image)
        [HttpPost("add")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddFood(
            [FromForm] string name,
            [FromForm] string description,
            [FromForm] decimal price,
            [FromForm] string category,
            [FromForm] IFormFile image
        )
        {
            if (image == null || image.Length == 0)
                return BadRequest(new { success = false, message = "Image is required" });

            var uploadsPath = Path.Combine(_env.ContentRootPath, "uploads");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}_{image.FileName}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var food = new Food
            {
                Name = name,
                Description = description,
                Price = price,
                Category = category,
                Image = fileName
            };

            await _foodService.AddFood(food);

            return Ok(new { success = true, message = "Food Added" });
        }

        // List food
        [AllowAnonymous]
        [HttpGet("list")]
        public async Task<IActionResult> ListFood()
        {
            var foods = await _foodService.ListFood();
            return Ok(new { success = true, data = foods });
        }

        // Remove food
        [HttpPost("remove")]
        public async Task<IActionResult> RemoveFood([FromBody]  RemoveFoodDto dto)
        {
            await _foodService.RemoveFood(dto.Id);
            return Ok(new { success = true, message = "Food Removed" });
        }
    }
}
