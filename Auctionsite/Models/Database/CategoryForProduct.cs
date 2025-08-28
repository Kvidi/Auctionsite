using System.ComponentModel.DataAnnotations;

namespace Vega.Models.Database
{
    public class CategoryForProduct
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
