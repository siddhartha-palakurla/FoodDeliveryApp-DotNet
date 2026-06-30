using FoodDelivery.API.Models;
using MongoDB.Driver;

namespace FoodDelivery.API.Services
{
    public class OrderService
    {
        private readonly IMongoCollection<Order> _orders;
        private readonly IMongoCollection<User> _users;

        public OrderService(IMongoClient client, IConfiguration config)
        {
            var db = client.GetDatabase(
                config["MongoDb:DatabaseName"] ?? "food-delivery"
            );

            _orders = db.GetCollection<Order>("orders");
            _users = db.GetCollection<User>("users");
        }

        public async Task<Order> CreateOrder(Order order)
        {
            await _orders.InsertOneAsync(order);
            await _users.UpdateOneAsync(
                u => u.Id == order.UserId,
                Builders<User>.Update.Set(u => u.CartData, new Dictionary<string, int>())
            );
            return order;
        }

        public async Task<Order?> GetById(string id)
            => await _orders.Find(o => o.Id == id).FirstOrDefaultAsync();

        public async Task<List<Order>> UserOrders(string userId)
            => await _orders.Find(o => o.UserId == userId).ToListAsync();

        public async Task<List<Order>> ListOrders()
            => await _orders.Find(_ => true).ToListAsync();

        public async Task UpdateStatus(string orderId, string status)
            => await _orders.UpdateOneAsync(
                o => o.Id == orderId,
                Builders<Order>.Update.Set(o => o.Status, status)
            );

        public async Task MarkPaid(string orderId)
        {
            // Get the order
            var order = await _orders
                .Find(o => o.Id == orderId)
                .FirstOrDefaultAsync();

            if (order == null)
                return;

            // Mark payment as completed
            await _orders.UpdateOneAsync(
                o => o.Id == orderId,
                Builders<Order>.Update.Set(o => o.Payment, true)
            );

            // Clear user's cart
            await _users.UpdateOneAsync(
                u => u.Id == order.UserId,
                Builders<User>.Update.Set(
                    u => u.CartData,
                    new Dictionary<string, int>()
                )
            );
        }
        public async Task Delete(string orderId)
            => await _orders.DeleteOneAsync(o => o.Id == orderId);
    }
}
