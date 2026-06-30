using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using FoodDelivery.API.Models;


namespace FoodDelivery.API.Models
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public List<OrderItem> Items { get; set; } = new();
        public decimal Amount { get; set; }
        public Address Address { get; set; } = null!;
        public bool Payment { get; set; } = false;
        public string Status { get; set; } = "Pending";

        public DateTime Date { get; set; } = DateTime.UtcNow;
    }

    public class OrderItem
    {
        public string FoodId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    
}
