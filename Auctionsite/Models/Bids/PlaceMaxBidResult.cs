namespace Auctionsite.Models.Bids
{
    public class PlaceMaxBidResult
    {
        public bool Success { get; set; } // Indicates if the max bid was placed successfully
        public PlaceBidError? Error { get; set; } // Error code indicating if something went wrong
    }

    public enum PlaceBidError
    {
        None, 
        BiddingNotAvailable, 
        BidTooLow,
        AlreadyLeading,/// The user is already leading with their max bid — not an error, just informational
        CounteredViaMaxBid, // The user has been outbid by a max bid from the leading bidder
        MaxBidPlacedFirst, // The leading bidder placed the same max bid first, so the user is not leading
        SameAsPrevious,
        NotAuthenticated,
        UnknownError
    }
}
