using System.Text.Json.Serialization; // Required for strict JSON binding

namespace SmartInventorySys.Models
{
    // The main container sent from JavaScript
    public class CheckoutRequest
    {
        public List<TransactionItem> CartItems { get; set; } = new List<TransactionItem>();
        public decimal DiscountPercent { get; set; }
        public string Comments { get; set; } = string.Empty; // New Field
    }

    // The individual items inside the cart
    public class TransactionItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        // Note: We don't need Name or Price here, the server will look them up from DB
    }
}