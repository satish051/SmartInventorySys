using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Added for Column type

namespace SmartInventorySys.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        public string OrderNumber { get; set; } = string.Empty; // Fix: Initialize it

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; } // Price before Tax/Discount

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } // e.g., Rs. 500

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } // 13% VAT

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } // Final Payable

        public string PaymentMethod { get; set; } = "Cash";

        // Fix: Initialize the list so it is never null
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        public string? Comments { get; set; } // New Field

        public string? UserId { get; set; }
    }
}