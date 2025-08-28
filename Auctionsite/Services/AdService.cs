using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Threading.Tasks;
using System.IO;
using Auctionsite.Data;
using Auctionsite.Helpers;
using Auctionsite.Models;
using Auctionsite.Models.Bids;
using Auctionsite.Models.Database;
using Auctionsite.Models.VM;
using Auctionsite.Services.Interfaces;

namespace Auctionsite.Services
{
    public class AdService : IAdService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICategoryService _categoryService;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AdService> _logger;
        public AdService(ApplicationDbContext db, ICategoryService categoryService, UserManager<User> userManager, IWebHostEnvironment env, ILogger<AdService> logger)
        {
            _db = db;
            _categoryService = categoryService;
            _userManager = userManager;
            _env = env;
            _logger = logger;
        }

        #region CRUD

        public async Task<PagedResult<Advertisement>> GetAllAdsAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _db.Advertisements
                .Include(ad => ad.Advertiser)
                .Include(ad => ad.Category);

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        // Gets all advertisements that are expiring within a specified time span.
        public async Task<List<Advertisement>> GetAdsExpiringWithinAsync(TimeSpan timeSpan)
        {
            var now = DateTime.Now;
            var threshold = now.Add(timeSpan);

            return await _db.Advertisements
                .Include(ad => ad.UsersWhoFavourited)
                .Where(ad => ad.PurchasedAt == null &&
                            ad.AuctionEndDate >= now &&
                            ad.AuctionEndDate <= threshold) 
                .ToListAsync();
        }

        // Gets the advertisement by ID, including all related data (except for Chats, not needed).
        public async Task<Advertisement?> GetAdByIdAsync(int id)
        {
            return await _db.Advertisements
                .Include(ad => ad.Advertiser)
                .Include(ad => ad.Category)
                .Include(ad => ad.Images)
                .Include(ad => ad.Bids)
                .Include(ad => ad.MaxBids)
                .Include(ad => ad.UsersWhoFavourited)
                .FirstOrDefaultAsync(ad => ad.Id == id);
        }
        public async Task<Advertisement> CreateAdAsync(Advertisement ad)
        {
            _db.Advertisements.Add(ad);
            await _db.SaveChangesAsync();
            return ad;
        }

        public async Task<bool> UpdateAdAsync(Advertisement ad)
        {
            if (ad == null) 
            {
                return false;
            }

            _db.Advertisements.Update(ad);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAdAsync(int id)
        {            
            var ad = await _db.Advertisements
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (ad == null)
                return false;

            // Delete each image file from disk
            foreach (var img in ad.Images)
            {
                // Convert URL ("/Images/UserImages/xxx.jpg") to physical path
                var relative = img.Url.StartsWith("~")
                    ? img.Url.Substring(1)
                    : img.Url.TrimStart('/');
                var fullPath = Path.GetFullPath(Path.Combine(_env.WebRootPath, relative));

                if (File.Exists(fullPath))
                {
                    try
                    {
                        File.Delete(fullPath);
                    }
                    catch (Exception ex)
                    {
                        // Log warning but continue with deletion
                        _logger.LogWarning(ex, "Could not delete image file {FilePath}", fullPath);
                    }
                }
            }
                        
            _db.Advertisements.Remove(ad);            
            await _db.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Specific actions

        // Retrieves all advertisements that are pending approval
        public async Task<List<Advertisement>> GetPendingAdvertisementsAsync()
        {
            return await _db.Advertisements
                .Include(ad => ad.Advertiser)
                .Where(ad => ad.ApprovedAt == null && !ad.IsRejected)
                .ToListAsync();
        }

        public async Task MarkAsSeenByAdminAsync(int adId)
        {
            var ad = await _db.Advertisements.FindAsync(adId);
            if (ad == null) throw new KeyNotFoundException($"Ad with id {adId} not found.");

            if (!ad.IsSeenByAdmin)
            {
                ad.IsSeenByAdmin = true;
                await _db.SaveChangesAsync();
            }
        }

        // Approves an advertisement by setting its ApprovedAt property to the current time.
        public async Task<bool> ApproveAdAsync(int id)
        {
            var ad = await _db.Advertisements.FindAsync(id);
            if (ad == null) return false;

            ad.ApprovedAt = DateTime.Now;
            _db.Advertisements.Update(ad);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleFavouriteAsync(int adId, string userId)
        {
            var ad = await _db.Advertisements.FindAsync(adId);
            if (ad == null) return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;


            // Check if the user already favourited this ad
            var alreadyFavourited = user.FavouriteAds.Any(a => a.Id == adId);

            if (alreadyFavourited)
            {
                user.FavouriteAds.Remove(ad);
            }
            else
            {
                user.FavouriteAds.Add(ad);
            }

            await _db.SaveChangesAsync();
            return true;
        }

        public Task<bool> DeleteImageAsync(string imageUrl)
        {           
            var relativeUrl = imageUrl.StartsWith("~")
            ? imageUrl.Substring(1)
            : imageUrl.TrimStart('/');
            var imagePath = Path.GetFullPath(Path.Combine(_env.WebRootPath, relativeUrl));

            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        #endregion

        #region Filtering and searching

        /// Retrieves advertisements based on various filters
        public async Task<PagedResult<Advertisement>> GetAdsByFilterAsync(AdSearchFilterViewModel filter, int pageNumber = 1, int pageSize = 10)
        {
            var query = _db.Advertisements
                .Where(ad => ad.ApprovedAt != null) // Only include approved ads
                .Include(ad => ad.Category)
                .Include(ad => ad.Advertiser)
                .Include(ad => ad.Bids)
                .AsQueryable();

            // Improved search logic based on SearchType
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.Trim();

                switch (filter.SearchType)
                {
                    case SearchType.ExactPhrase:
                        // Search for exact phrase (consecutive characters) in title or description
                        query = query.Where(ad =>
                            ad.Title.Contains(searchTerm) ||
                            ad.Description.Contains(searchTerm));
                        break;

                    case SearchType.StartsWith:
                        // Search for titles that start with the search term
                        query = query.Where(ad =>
                            ad.Title.StartsWith(searchTerm) ||
                            ad.Description.StartsWith(searchTerm));
                        break;

                    case SearchType.Contains:
                    default:
                        // Default behavior: search for individual words anywhere
                        var words = searchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var word in words)
                        {
                            var currentWord = word; // Capture for closure
                            query = query.Where(ad =>
                                ad.Title.Contains(currentWord) ||
                                ad.Description.Contains(currentWord));
                        }
                        break;
                }
            }

            if (filter.CategoryId.HasValue)
            {
                // Get the selected category and all its descendants
                var allCatIds = await _categoryService.GetDescendantCategoryIdsAsync(filter.CategoryId.Value);
                query = query.Where(ad => allCatIds.Contains(ad.CategoryId));
            }

            if (filter.SelectedLocations?.Any() == true)
            {
                query = query.Where(ad => filter.SelectedLocations.Contains(ad.PickupLocation));
            }

            if (filter.SelectedCategoryIds?.Any() == true)
            {
                query = query.Where(ad => filter.SelectedCategoryIds.Contains(ad.CategoryId));
            }

            if (filter.SelectedConditions?.Any() == true)
            {
                query = query.Where(ad => filter.SelectedConditions.Contains(ad.Condition));
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(ad => ad.CurrentHighestBid >= filter.MinPrice.Value || ad.BuyNowPrice >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(ad => ad.CurrentHighestBid <= filter.MaxPrice.Value || ad.BuyNowPrice <= filter.MaxPrice.Value);
            }

            if (filter.AdType.HasValue)
            {
                query = query.Where(ad => ad.AdType == filter.AdType.Value);
            }

            switch (filter.SellerType)
            {
                case SellerType.PrivateOnly:
                    query = query.Where(ad => !ad.IsCompanySeller);
                    break;
                case SellerType.CompanyOnly:
                    query = query.Where(ad => ad.IsCompanySeller);
                    break;
            }

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }


        /// Retrieves all advertisements that are associated with a specific category.
        public async Task<PagedResult<Advertisement>> GetAdsByCategoryIdAsync(int categoryId, int pageNumber = 1, int pageSize = 10)
        {
            var query = _db.Advertisements
                .Include(ad => ad.Category)
                .Include(ad => ad.Advertiser)
                .Where(a => a.CategoryId == categoryId);

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<PagedResult<Advertisement>> GetAdsByCategoryWithDescendantsAsync(int categoryId, int pageNumber = 1, int pageSize = 10)
        {
            // Get all descendant category IDs for the given category
            var allCatIds = await _categoryService.GetDescendantCategoryIdsAsync(categoryId);

            // Filter advertisements by the descendant category IDs
            var query = _db.Advertisements
                .Include(ad => ad.Category)
                .Include(ad => ad.Advertiser)
                .Where(ad => allCatIds.Contains(ad.CategoryId));

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }



        /// Retrieves all advertisements that have been created by a specific user.
        public async Task<PagedResult<Advertisement>> GetAdsByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 10)
        {
            var query = _db.Advertisements
                .Include(ad => ad.Category)
                .Include(ad => ad.Advertiser)
                .Where(ad => ad.Advertiser.Id == userId);

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        /// Retrieves all advertisements that have been favourited by a specific user.
        public async Task<PagedResult<Advertisement>> GetFavourites(string userId, int pageNumber = 1, int pageSize = 10)
        {
            var query = _db.Advertisements
                .Include(ad => ad.Category)
                .Include(ad => ad.Advertiser)
                .Where(ad => ad.UsersWhoFavourited.Any(u => u.Id == userId));
            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }
        // Retrieves the IDs of all advertisements that have been favourited by a specific user.
        public async Task<HashSet<int>> GetFavouriteAdIdsAsync(string userId)
        {
            return await _db.Advertisements
                .Where(a => a.UsersWhoFavourited.Any(u => u.Id == userId))
                .Select(a => a.Id)
                .ToHashSetAsync();
        }

        #endregion

        #region Bid-related methods

        public async Task<PlaceMaxBidResult> PlaceMaxBidAsync(int advertisementId, string userId, decimal maxBidAmount)
        {
            // --- Add max bid ---

            var advertisement = await GetAdByIdAsync(advertisementId);
            if (advertisement == null || advertisement.StartingPrice == null)
            {
                return new PlaceMaxBidResult { Success = false, Error = PlaceBidError.BiddingNotAvailable };
            }

            var leadingMaxBid = await GetLeadingMaxBidAsync(advertisementId);
            var isLeading = leadingMaxBid != null && leadingMaxBid.UserId == userId;
            var leadingVisibleBid = await GetLeadingVisibleBidAsync(advertisementId);
            var currentUserMaxBid = await GetUserMaxBidAsync(advertisementId, userId);

            // Run validation to see if the max bid can be added
            var validationResult = ValidateBeforePlacingMaxBid(advertisement, leadingVisibleBid, currentUserMaxBid, maxBidAmount, isLeading);
            // If the validation returns an error, return it and don't proceed
            if (validationResult != null)
            {
                return validationResult;
            }

            // Save or update the user's max bid
            await SaveOrUpdateMaxBidAsync(currentUserMaxBid, advertisementId, userId, maxBidAmount);
            // Favourite the ad if not already favourited
            await AddToFavouritesIfNotAlreadyAsync(userId, advertisementId);

            // --- Add visible bid ---

            // If no max bid exist, user places first ever bid (the starting price)
            if (leadingMaxBid == null)
            {
                await AddVisibleBidAsync(advertisementId, userId, advertisement.StartingPrice.Value, BidEventType.None);
                await _db.SaveChangesAsync();
                return new PlaceMaxBidResult { Success = true, Error = PlaceBidError.None };
            }

            // If user is already the leading max bidder no visible bid is added
            var userIsLeading = leadingMaxBid.UserId == userId;
            if (userIsLeading)
            {
                return new PlaceMaxBidResult { Success = true, Error = PlaceBidError.AlreadyLeading }; // Success = true because the max bid was still updated
            }

            // Compare new max bid to current leading max bid
            if (maxBidAmount > leadingMaxBid.Amount)
            {
                // User becomes new leader:
                // User places visible bid as either leading max bid + increment if it's within the user's max bid,
                // or the user's max bid itself if the increment would exceed it.

                // Get increment based on leading max bid
                var updatedIncrement = GetIncrement(leadingMaxBid.Amount);

                decimal counterAmount = leadingMaxBid.Amount + updatedIncrement;

                // The new leading bid is either the incremented leading max bid or the user's max bid, whichever is lower.
                decimal newLeadingBid = Math.Min(counterAmount, maxBidAmount);

                // If the former leading bidder's max bid is higher than the visible bid, place a final counter bid from the former leading max bidder and show it as MaxBidReached.
                if (leadingMaxBid.Amount > leadingVisibleBid.Amount)
                    await AddVisibleBidAsync(advertisementId, leadingMaxBid.UserId, leadingMaxBid.Amount, BidEventType.MaxBidReached);

                // Place User's new leading bid
                await AddVisibleBidAsync(advertisementId, userId, newLeadingBid, BidEventType.None);
            }
            else if (maxBidAmount < leadingMaxBid.Amount)
            {
                // Leading bidder responds with a counter just above user's max

                // Get increment based on user's max bid
                var updatedIncrement = GetIncrement(maxBidAmount);

                decimal counterAmount = maxBidAmount + updatedIncrement; // Always beat by full increment, not rounded up to nearest increment. This is how Tradera works.

                // Place user's visible bid (which is also user's max bid)
                await AddVisibleBidAsync(advertisementId, userId, maxBidAmount, BidEventType.None);

                if (counterAmount > leadingMaxBid.Amount)
                {
                    // Cannot beat by full increment, show their max as final visible, but the BidEventType is ViaMaxBid because it's still the leading bid
                    await AddVisibleBidAsync(advertisementId, leadingMaxBid.UserId, leadingMaxBid.Amount, BidEventType.ViaMaxBid);     
                }
                else
                {
                    // Can beat by full increment
                    await AddVisibleBidAsync(advertisementId, leadingMaxBid.UserId, counterAmount, BidEventType.ViaMaxBid);
                }

                await _db.SaveChangesAsync();
                return new PlaceMaxBidResult { Success = true, Error = PlaceBidError.CounteredViaMaxBid };
            }
            else // maxBidAmount == leadingMaxBid.Amount
            {
                // User placed same max as existing leading bid, but leading bidder placed it first
                await AddVisibleBidAsync(advertisementId, userId, maxBidAmount, BidEventType.None);
                await AddVisibleBidAsync(advertisementId, leadingMaxBid.UserId, leadingMaxBid.Amount, BidEventType.MaxBidPlacedFirst);

                await _db.SaveChangesAsync();
                return new PlaceMaxBidResult { Success = true, Error = PlaceBidError.MaxBidPlacedFirst };
            }

            await _db.SaveChangesAsync();
            return new PlaceMaxBidResult { Success = true, Error = PlaceBidError.None};
        }

        // Gets the max bid amount for a user
        public async Task<MaxBid?> GetUserMaxBidAsync(int advertisementId, string userId)
        {
            return await _db.MaxBids
                .Where(mb => mb.AdvertisementId == advertisementId && mb.UserId == userId)
                .FirstOrDefaultAsync();
        }

        // Gets the current visible leading bid on an ad.
        public async Task<Bid?> GetLeadingVisibleBidAsync(int advertisementId)
        {
            return await _db.Bids
                .Where(b => b.AdvertisementId == advertisementId)
                .OrderByDescending(b => b.Amount)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal?> GetLeadingVisibleBidAmountAsync(int advertisementId)
        {
            return await _db.Bids
                .Where(b => b.AdvertisementId == advertisementId)
                .OrderByDescending(b => b.Amount)
                .Select(b => b.Amount)
                .FirstOrDefaultAsync();
        }

        public async Task<string?> GetLeadingBidderIdAsync(int advertisementId)
        {
            return await _db.Bids
                .Where(b => b.AdvertisementId == advertisementId)
                .OrderByDescending(b => b.Amount)
                .ThenByDescending(b => b.EventType == BidEventType.MaxBidPlacedFirst ? 1 : 0) 
                .Select(b => b.User.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetMinimumBidAmountAsync(int advertisementId)
        {
            // Get the leading visible bid
            var leadingVisibleBid = await GetLeadingVisibleBidAsync(advertisementId);
            if (leadingVisibleBid == null)
            {
                // If no bids exist, return the starting price as the minimum bid amount
                var ad = await _db.Advertisements.FindAsync(advertisementId);
                return ad?.StartingPrice ?? 0m; // Return 0 if no starting price is set
            }
            // Get the increment based on the leading visible bid
            var increment = GetIncrement(leadingVisibleBid.Amount);

            return leadingVisibleBid.Amount + increment;
        }

        // Returns the full (visible) bid history, newest first. For display in the UI.
        public async Task<List<BidHistoryVM>> GetBidHistoryAsync(int advertisementId)
        {
            var advertisement = await _db.Advertisements
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == advertisementId);

            if (advertisement == null)
                return new List<BidHistoryVM>();

            var bids = await _db.Bids
                .Include(b => b.User)
                .Where(b => b.AdvertisementId == advertisementId)
                .OrderByDescending(b => b.PlacedAt)
                .ThenByDescending(b => b.Id)
                .ToListAsync();

            if (bids == null || !bids.Any())
            {
                // If no bids exist, return an empty list
                return new List<BidHistoryVM>();
            }

            var bidVMs = bids.Select(b => new BidHistoryVM
            {
                UserName = b.User.UserName,
                Amount = b.Amount,
                PlacedAt = b.PlacedAt,
                EventType = b.EventType
            }).ToList();

            // Add the starting price entry as the last item
            bidVMs.Add(new BidHistoryVM
            {
                UserName = "Startar",
                Amount = (decimal)advertisement.StartingPrice,
                PlacedAt = advertisement.ApprovedAt ?? advertisement.AddedAt, 
                EventType = BidEventType.None
            });

            return bidVMs;
        }

        // Get the bid count on an ad
        public async Task<int> GetBidCountAsync(int advertisementId)
        {
            return await _db.Bids
                .Where(b => b.AdvertisementId == advertisementId)
                .CountAsync();
        }

        // Get the id's of users who are outbid on an ad 
        public async Task<List<string>> GetOutbidUserIdsAsync(int advertisementId)
        {
            var leadingBidderId = await GetLeadingBidderIdAsync(advertisementId);

            var outbidUserIds = new List<string>();

            if (leadingBidderId != null)
            {
                outbidUserIds = await _db.MaxBids
                    .Where(mb => mb.AdvertisementId == advertisementId && mb.UserId != leadingBidderId)
                    .Select(mb => mb.UserId)
                    .ToListAsync();
            }  

            return outbidUserIds;
        }

        // Checks if a user is outbid on an ad.
        public async Task<bool> IsOutBid (int adId, string userId)
        {
            var leadingBidderId = await GetLeadingBidderIdAsync(adId);
            if (leadingBidderId == null) return false; // No bids placed yet

            // Check if user has placed a bid
            bool hasUserBid = await _db.Bids
                .AnyAsync(b => b.AdvertisementId == adId && b.UserId == userId);

            if (!hasUserBid) return false; // User has not participated in bidding

            // User has bid, but is not the leader -> they are outbid 
            return leadingBidderId != userId;
        }

        #endregion

        #region Helper methods

        // Checks if the max bid can be placed
        private PlaceMaxBidResult? ValidateBeforePlacingMaxBid(Advertisement ad, Bid? leadingVisibleBid, MaxBid? currentUserMaxBid, decimal maxBidAmount, bool isLeading)
        {
            // Check if the ad is available for bidding
            if (ad.StartingPrice == null)
            {
                return new PlaceMaxBidResult { Success = false, Error = PlaceBidError.BiddingNotAvailable };
            }

            // Check if the user's max bid amount is the same as the already existing one (if it exists)
            if (currentUserMaxBid != null && maxBidAmount == currentUserMaxBid.Amount)
            {
                return new PlaceMaxBidResult { Success = false, Error = PlaceBidError.SameAsPrevious };
            }

            // If no leading visible bid exists, check if the max bid amount is less than the starting price
            if (leadingVisibleBid == null)
            {
                if (maxBidAmount < ad.StartingPrice.Value)
                {
                    return new PlaceMaxBidResult { Success = false, Error = PlaceBidError.BidTooLow };
                }
            }
            else // If a leading visible bid exists, check if the max bid amount is less than the minimum allowed bid
            {
                // Calculate the increment and minimum allowed bid based on the leading visible bid and whether the user is leading
                var increment = GetIncrement(leadingVisibleBid.Amount);
                var minimumAllowedBid = isLeading ? leadingVisibleBid.Amount : (leadingVisibleBid.Amount + increment);

                if (maxBidAmount < minimumAllowedBid)
                {
                    return new PlaceMaxBidResult { Success = false, Error = PlaceBidError.BidTooLow };
                }
            }

            return null; // All good
        }

        // Gets current leading max bid
        private async Task<MaxBid?> GetLeadingMaxBidAsync(int advertisementId)
        {
            return await _db.MaxBids
                .Where(mb => mb.AdvertisementId == advertisementId)
                .OrderByDescending(mb => mb.Amount)
                .ThenBy(mb => mb.PlacedAt)
                .FirstOrDefaultAsync();
        }

        // Calculates the increment based on the leading bid or max bid
        private decimal GetIncrement(decimal bidAmount)
        {
            switch (bidAmount)
            {
                case < 20m:
                    return 1m; // For bids less than 20, increment is 1
                case < 100m:
                    return 5m; // For bids 20 - 99, increment is 5
                case < 200m:
                    return 10m; // For bids 100 - 199, increment is 10
                case < 400m:
                    return 15m; // For bids 200 - 399, increment is 15
                case < 1000m:
                    return 20m; // For bids 400 - 999, increment is 20
                case < 2500m:
                    return 30m; // For bids 1000 - 2499, increment is 30
                case < 4000m:
                    return 50m; // For bids 2500 - 3999, increment is 50
                case < 6000m:
                    return 100m; // For bids 4000 - 5999, increment is 100
                default:
                    return 200m; // For bids 6000 and above, increment is 200
            }
        }

        // Saves or updates the max bid
        private async Task SaveOrUpdateMaxBidAsync(MaxBid? existing, int adId, string userId, decimal amount)
        {
            if (existing != null)
            {
                existing.Amount = amount;
                existing.PlacedAt = DateTime.Now;
                _db.Update(existing);
            }
            else // If no existing max bid, create a new one
            {
                var newMaxBid = new MaxBid
                {
                    AdvertisementId = adId,
                    UserId = userId,
                    Amount = amount,
                    PlacedAt = DateTime.Now
                };
                await _db.MaxBids.AddAsync(newMaxBid);
            }
            await _db.SaveChangesAsync();
        }

        // Adds visible bid and updates the current highest bid, if necessary.
        private async Task AddVisibleBidAsync(int adId, string userId, decimal amount, BidEventType eventType)
        {
            var bid = new Bid
            {
                AdvertisementId = adId,
                UserId = userId,
                Amount = amount,
                EventType = eventType
            };
            await _db.Bids.AddAsync(bid);

            await UpdateCurrentHighestBid(adId, amount);
        }

        // Updates the current highest bid on an advertisement if the new bid is higher than the existing one.
        private async Task UpdateCurrentHighestBid(int advertisementId, decimal newHighestBid)
        {
            var ad = await _db.Advertisements.FindAsync(advertisementId);
            if (ad != null)
            {
                if (ad.CurrentHighestBid < newHighestBid)
                {
                    ad.CurrentHighestBid = newHighestBid;
                    _db.Advertisements.Update(ad);
                }
                else if (ad.CurrentHighestBid > newHighestBid)
                {
                    // If the new highest bid is lower than the current, we don't update it.
                    return;
                } 
            }
        }

        private async Task AddToFavouritesIfNotAlreadyAsync(string userId, int adId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new InvalidOperationException("User not found.");

            // Only add if the ad isn't already favourited
            if (!user.FavouriteAds.Any(a => a.Id == adId))
            {
                var ad = await _db.Advertisements.FindAsync(adId);
                if (ad != null)
                {
                    user.FavouriteAds.Add(ad);
                    await _db.SaveChangesAsync();
                }
            }
        }

        #endregion
    }
}
