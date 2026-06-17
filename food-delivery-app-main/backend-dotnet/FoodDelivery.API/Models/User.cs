using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FoodDelivery.API.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;

        public string? Image { get; set; }

        public Dictionary<string, int> CartData { get; set; } = new();
    }
}
