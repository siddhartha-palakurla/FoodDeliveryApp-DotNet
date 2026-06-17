using FoodDelivery.API.Models;
using MongoDB.Driver;

namespace FoodDelivery.API.Services
{
    public class FoodService
    {
        private readonly IMongoCollection<Food> _foods;
        private readonly IWebHostEnvironment _env;

        public FoodService(IMongoClient client, IConfiguration config, IWebHostEnvironment env)
        {
            var database = client.GetDatabase(
                config["MongoDb:DatabaseName"] ?? "food-delivery"
            );

            _foods = database.GetCollection<Food>("foods");
            _env = env;
        }

        // Add food
        public async Task AddFood(Food food)
        {
            await _foods.InsertOneAsync(food);
        }

        // List food
        public async Task<List<Food>> ListFood()
        {
            return await _foods.Find(_ => true).ToListAsync();
        }

        // Remove food
        public async Task RemoveFood(string id)
        {
            var food = await _foods.Find(f => f.Id == id).FirstOrDefaultAsync();
            if (food == null) return;

            // delete image file
            var imagePath = Path.Combine(_env.ContentRootPath, "uploads", food.Image);
            if (File.Exists(imagePath))
                File.Delete(imagePath);

            await _foods.DeleteOneAsync(f => f.Id == id);
        }

          // ✅ REQUIRED FOR ORDER PLACEMENT
        public async Task<Food?> GetById(string foodId)
        {
            return await _foods
                .Find(f => f.Id == foodId)
                .FirstOrDefaultAsync();
        }
    }
}
