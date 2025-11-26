using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInventorySys.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        // Fix: Add '?' to allow EF Core to handle the link without complaining
        public virtual Order? Order { get; set; }

        public int ProductId { get; set; }
        // Fix: Add '?' so the compiler knows this might be loaded later
        public virtual Product? Product { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
    }
}