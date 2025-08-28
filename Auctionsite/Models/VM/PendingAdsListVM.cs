namespace Auctionsite.Models.VM
{
    public class PendingAdsListVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string UserName { get; set; }
        public DateTime AddedAt { get; set; }
        public bool IsSeenByAdmin { get; set; }
    }
}
