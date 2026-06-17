using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.DTOs
{
    public class AddFoodRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public IFormFile Image { get; set; } = null!;
    }
}
