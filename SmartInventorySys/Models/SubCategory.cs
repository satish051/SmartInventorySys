using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInventorySys.Models
{
    public class SubCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        // Link to Parent Category
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }
    }
}