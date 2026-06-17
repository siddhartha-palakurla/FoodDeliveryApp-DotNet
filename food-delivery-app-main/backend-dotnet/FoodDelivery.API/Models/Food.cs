using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FoodDelivery.API.Models
{
    public class Food
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public string Category { get; set; } = null!;
        public string Image { get; set; } = null!;
    }
}
