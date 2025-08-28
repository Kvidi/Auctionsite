using Microsoft.AspNetCore.Mvc.Rendering;
using Auctionsite.Extensions;
using Auctionsite.Models.Database;

namespace Auctionsite.Helpers
{
    /// This class is used to generate select list items for dropdowns in views
    // Particularly the enums Condition and AdType
    public static class SelectListHelper
    {
        public static List<SelectListItem> GetConditionOptions()
        {
            return Enum.GetValues<Condition>()
                   .Select(c => new SelectListItem
                   {
                       Value = ((int)c).ToString(),
                       Text = c.GetDisplayName()
                   })
                   .ToList();
        }

        public static List<SelectListItem> GetAdTypeOptions()
        {
            return Enum.GetValues<AdType>()
                .Select(a => new SelectListItem
                {
                    Value = ((int)a).ToString(),
                    Text = a.GetDisplayName()
                })
                .ToList();
        }
    }

}
