using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Auctionsite.Data;
using Auctionsite.Models;
using Auctionsite.Models.Database;
using Auctionsite.Models.VM;


namespace Auctionsite.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class SparadeAnnonserModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public SparadeAnnonserModel(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public PagedResult<AdCardVM> FavouriteAds { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public AdType? AdType { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; }
        private const int PageSize = 10;

        public async Task OnGetAsync()
        {
            // Get logged-in user and their favourite ads
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user == null)
                return;

            // Query to get favourite ads, including category and images
            var query = _context.Advertisements
                        .Include(ad => ad.Category)
                        .Include(ad => ad.Images)
                        .Where(ad => ad.UsersWhoFavourited.Any(u => u.Id == user.Id));
            
            // Apply filter if specified
            if (AdType.HasValue)            
                query = query.Where(ad => ad.AdType == AdType.Value); // Filter by ad type            

            // Count total ads for pagination
            var totalCount = await query.CountAsync();            

            // Fetch the paginated list of favourite ads
            var pageAds = await query
                .OrderBy(ad => ad.Id)  
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Map the ads to AdCardVM
            var vmList = pageAds.Select(ad => new AdCardVM
            {
                Id = ad.Id,
                Title = ad.Title,
                HeadImageUrl = ad.Images.FirstOrDefault(i => i.IsMain)?.Url,
                AuctionEndDate = ad.AuctionEndDate,
                AdType = ad.AdType,
                StartingPrice = ad.StartingPrice,
                BuyNowPrice = ad.BuyNowPrice,
                CurrentHighestBid = ad.CurrentHighestBid,
                BidCount = ad.Bids?.Count ?? 0,
                IsFavourite = true,
                ApprovedAt = ad.ApprovedAt
            }).ToList();

            FavouriteAds = new PagedResult<AdCardVM>
            {
                Items = vmList,
                PageNumber = PageNumber,
                PageSize = PageSize,
                TotalItems = totalCount
            };            
        }       
    }
}