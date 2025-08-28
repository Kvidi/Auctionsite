using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vega.Models.Database
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        [MaxLength(5000)]
        public string Description { get; set; }

        [Required]
        [MaxLength(10)]
        [Column(TypeName = "money")]
        public decimal Price { get; set; }

        [Required]
        [MaxLength(2048)]
        public string HeadImageURL { get; set; }

        [MaxLength(10)]
        public List<string>? ImageList { get; set; }

        [MaxLength(2048)]
        public string? VideoURL { get; set; }

        [Required]
        public DateTime AddedAt { get; set; } = DateTime.Now;

        [Required]
        public decimal MomsSats { get; set; }

        public virtual ICollection<CategoryForProduct> Categories { get; set; } = new List<CategoryForProduct>();

        public virtual ICollection<OrderProduct> OrderProducts { get; set; }
    }
}
