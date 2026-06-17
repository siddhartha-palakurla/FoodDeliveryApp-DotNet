using FoodDelivery.API.Models;
using MongoDB.Driver;

namespace FoodDelivery.API.Services
{
    public class CartService
    {
        private readonly IMongoCollection<User> _users;

        public CartService(IMongoClient client, IConfiguration config)
        {
            var database = client.GetDatabase(
                config["MongoDb:DatabaseName"] ?? "food-delivery"
            );

            _users = database.GetCollection<User>("users");
        }

        // Add to cart
        public async Task AddToCart(string userId, string itemId)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null) return;

            if (!user.CartData.ContainsKey(itemId))
                user.CartData[itemId] = 1;
            else
                user.CartData[itemId]++;

            await _users.ReplaceOneAsync(u => u.Id == userId, user);
        }

        // Remove from cart
        public async Task RemoveFromCart(string userId, string itemId)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null) return;

            if (user.CartData.ContainsKey(itemId) && user.CartData[itemId] > 0)
                user.CartData[itemId]--;

            await _users.ReplaceOneAsync(u => u.Id == userId, user);
        }

        // Get cart
        public async Task<Dictionary<string, int>> GetCart(string userId)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            return user?.CartData ?? new Dictionary<string, int>();
        }
    }
}
