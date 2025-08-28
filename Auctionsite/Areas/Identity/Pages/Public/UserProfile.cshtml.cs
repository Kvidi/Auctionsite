using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Auctionsite.Data;
using Auctionsite.Models;
using Auctionsite.Models.Database;
using Auctionsite.Models.VM;


public class UserProfileModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public UserProfileModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    private const int PageSize = 10;
    public UserOverviewViewModel Overview { get; set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var user = await _context.Users
            .Include(u => u.Advertisements)
                .ThenInclude(a => a.Category)
            .Include(u => u.ReviewsReceived)
                .ThenInclude(r => r.Reviewer)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound();

        var adQuery = user.Advertisements.AsQueryable();

        var totalCount = adQuery.Count();
        var pageAds = adQuery
            .OrderByDescending(ad => ad.Id)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        var adCardList = pageAds.Select(ad => new AdCardVM
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
            IsFavourite = false,
            ApprovedAt = ad.ApprovedAt
        }).ToList();


        Overview = new UserOverviewViewModel
        {
            UserId = user.Id,
            FullName = user.FirstName + " " + user.LastName,
            Annonser = new PagedResult<AdCardVM>
            {
                Items = adCardList,
                PageNumber = PageNumber,
                PageSize = PageSize,
                TotalItems = totalCount
            },
            Omdomen = user.ReviewsReceived.OrderByDescending(r => r.CreatedAt).ToList()
        };

        return Page();
    }
}
