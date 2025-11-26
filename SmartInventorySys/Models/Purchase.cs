using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInventorySys.Models
{
    public class Purchase
    {
        [Key]
        public int Id { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [Required]
        public string InvoiceNo { get; set; } = string.Empty; // Supplier's Invoice No

        public int SupplierId { get; set; }
        public virtual Supplier? Supplier { get; set; }

        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; } // Cost Price (Buying Price)

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }
    }
}