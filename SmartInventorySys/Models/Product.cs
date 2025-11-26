using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInventorySys.Models // <--- CHECK THIS LINE CAREFULLY
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public int LowStockThreshold { get; set; } = 5;

        public string? ImageUrl { get; set; }

        // Main Category
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        // --- NEW: Sub-Category Link ---
        public int? SubCategoryId { get; set; } // Nullable, in case no sub-category exists
        public virtual SubCategory? SubCategory { get; set; }
    }
}