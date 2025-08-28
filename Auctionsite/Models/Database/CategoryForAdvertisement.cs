using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auctionsite.Models.Database
{
    public class CategoryForAdvertisement
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        // Specifies if the category has a parent category and if so, which one
        public int? ParentCategoryId { get; set; }
        public CategoryForAdvertisement? ParentCategory { get; set; }

        // Navigation to subcategories
        public ICollection<CategoryForAdvertisement> Subcategories { get; set; } = new List<CategoryForAdvertisement>();

        // Positions for sibling categories
        public int DisplayOrder { get; set; }

        // Navigation to advertisements
        public ICollection<Advertisement> Advertisements { get; set; } = new List<Advertisement>();
    }
}


