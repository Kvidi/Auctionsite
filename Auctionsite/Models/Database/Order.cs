using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vega.Models.Database
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(10)]
        [Column(TypeName = "money")]
        public decimal TotalPrice { get; set; }

        [Required]
        [MaxLength(10)]
        [Column(TypeName = "money")]
        public decimal Shipping {  get; set; }

        public string? ShippingMethod { get; set; }

        public virtual User Buyer { get; set; }

        public bool IsPlaced { get; set; } = false;

        public virtual ICollection<OrderProduct> OrderProducts { get; set; }
    }
}
