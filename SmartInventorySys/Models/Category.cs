using System.ComponentModel.DataAnnotations;

namespace SmartInventorySys.Models // <--- This MUST match your project name
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        // Navigation Property: One Category has many Products
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}