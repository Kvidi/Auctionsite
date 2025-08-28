using Auctionsite.Models;
using Auctionsite.Models.Bids;
using Auctionsite.Models.Database;
using Auctionsite.Models.VM;

namespace Auctionsite.Services.Interfaces
{
    public interface IAdService
    {
        // CRUD
        Task<PagedResult<Advertisement>> GetAllAdsAsync(int pageNumber, int pageSize);
        Task<List<Advertisement>> GetAdsExpiringWithinAsync(TimeSpan timeSpan);
        Task<Advertisement?> GetAdByIdAsync(int id);
        Task<Advertisement> CreateAdAsync(Advertisement ad);
        Task<bool> UpdateAdAsync(Advertisement ad);
        Task<bool> DeleteAdAsync(int id);

        // Specific actions
        Task<List<Advertisement>> GetPendingAdvertisementsAsync();
        Task MarkAsSeenByAdminAsync(int adId);
        Task<bool> ApproveAdAsync(int id);
        Task<bool> ToggleFavouriteAsync(int adId, string userId);
        Task<bool> DeleteImageAsync(string imageUrl);

        // Filtering and searching
        Task<PagedResult<Advertisement>> GetAdsByFilterAsync(AdSearchFilterViewModel filter, int pageNumber, int pageSize);
        Task<PagedResult<Advertisement>> GetAdsByCategoryIdAsync(int categoryId, int pageNumber, int pageSize);
        Task<PagedResult<Advertisement>> GetAdsByCategoryWithDescendantsAsync(int categoryId, int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<Advertisement>> GetAdsByUserIdAsync(string userId, int pageNumber, int pageSize);
        Task<PagedResult<Advertisement>> GetFavourites(string userId, int pageNumber, int pageSize);
        Task<HashSet<int>> GetFavouriteAdIdsAsync(string userId);


        // Bid-related methods

        // Adds a new max bid for a user. Automatically handles bid increments and stores a new Bid if needed.
        Task<PlaceMaxBidResult> PlaceMaxBidAsync(int advertisementId, string userId, decimal maxBidAmount);

        // Returns the max bid that a user has placed on a specific ad (null if none).
        Task<MaxBid?> GetUserMaxBidAsync(int advertisementId, string userId);

        // Returns the current visible leading bid on an ad.
        Task<Bid?> GetLeadingVisibleBidAsync(int advertisementId);

        Task<decimal?> GetLeadingVisibleBidAmountAsync(int advertisementId);

        Task<string?> GetLeadingBidderIdAsync(int advertisementId);

        // Returns the minimum bid amount required to place a new max bid on an ad.
        public Task<decimal> GetMinimumBidAmountAsync(int advertisementId);

        // Returns the full bid history (including both user and system-generated automatic bids), newest first.
        Task<List<BidHistoryVM>> GetBidHistoryAsync(int advertisementId);

        Task<int> GetBidCountAsync(int advertisementId);
        Task<List<string>> GetOutbidUserIdsAsync(int advertisementId);

        Task<bool> IsOutBid(int adId, string userId);
    }

}
