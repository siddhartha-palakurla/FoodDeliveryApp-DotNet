using FoodDelivery.API.Models;
namespace FoodDelivery.API.DTOs
{
    public class PlaceOrderRequest
    {
        public Address Address { get; set; } = null!;
        public List<OrderItemRequest> Items { get; set; } = new();
        public decimal Amount { get; set; }
    }

    public class OrderItemRequest
    {
        public string FoodId { get; set; } = "";
        public int Quantity { get; set; }
    }

    public class VerifyOrderRequest
    {
        public string OrderId { get; set; } = "";
        public string Success { get; set; } = "";
    }

    public class UpdateStatusRequest
    {
        public string OrderId { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
