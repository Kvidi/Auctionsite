using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vega.Models.Database
{
    public class OrderProduct
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        [Required]
        public int Amount { get; set; }

        [Required]
        [MaxLength(10)]
        [Column(TypeName = "money")]
        public decimal PriceOneCopy { get; set; }

        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }

    }
}
