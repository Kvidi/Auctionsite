namespace Auctionsite.Models.VM
{
    public class ApprovePendingAdVM
    {
        public List<PendingAdsListVM> Ads { get; set; }
        public AdDetailsVM? SelectedAd { get; set; }
    }
}
