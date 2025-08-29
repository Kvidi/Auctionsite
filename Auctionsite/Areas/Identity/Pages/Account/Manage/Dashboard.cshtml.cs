using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auctionsite.Areas.Identity.Pages.Account.Manage
{
    public class DashboardModel : PageModel
    {
        public List<DashboardCard>? Cards { get; set; }

        public void OnGet()
        {
            Cards = new List<DashboardCard>
            {
                new() { Title = "Mina annonser", Icon = "fa-star" , PageLink = "/Account/Manage/MinaAnnonser" },
                new() { Title = "Sparade annonser", Icon = "fa-heart" , PageLink = "/Account/Manage/SparadeAnnonser" },
                new() { Title = "Bevakningar", Icon = "fa-star" },                
                new() { Title = "Inställningar", Icon = "fa-gear" , PageLink = "/Account/Manage/SettingsList" }
            };
        }

        public class DashboardCard
        {
            public string Title { get; set; } = string.Empty;
            public string Icon { get; set; } = string.Empty;
            public string PageLink { get; set; } = string.Empty;
        }
    }

}
