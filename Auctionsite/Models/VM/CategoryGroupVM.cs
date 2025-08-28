using Microsoft.AspNetCore.Mvc.Rendering;

namespace Auctionsite.Models.VM
{
    public class CategoryGroupVM
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } 
        public List<SelectListItem> SubCategories { get; set; } = new();
    }
}
