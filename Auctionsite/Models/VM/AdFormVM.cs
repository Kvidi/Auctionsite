using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Auctionsite.Models.Database;

namespace Auctionsite.Models.VM
{
    public class AdFormVM : IValidatableObject
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        [Display(Name = "Bilder (max 10)")]
        public List<string> ImageUrls { get; set; } = new();
        public List<string> ImageOrder { get; set; } = new();
        public List<string> DeletedImageUrls { get; set; } = new(); // For Editing an Ad, to keep track of deleted images
        public List<ImageDto> Images { get; set; } = new(); // For the Ad Preview
        public string? VideoURL { get; set; }

        [Required(ErrorMessage = "Titel är obligatorisk.")]
        [StringLength(100)]
        public string? Title { get; set; }
        [Required(ErrorMessage = "Skick måste anges.")]
        public Condition? Condition { get; set; }

        [Required(ErrorMessage = "Beskrivning är obligatorisk.")]
        [StringLength(5000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Annons-typ måste väljas.")]
        public AdType? AdType { get; set; }
        public decimal? StartingPrice { get; set; }
        public decimal? BuyNowPrice { get; set; }
        public decimal? MinimumEndPrice { get; set; }

        public bool AvailableForPickup { get; set; }        
        [StringLength(100)]
        public string? PickupLocation { get; set; }
        
        public string? ShippingMethod { get; set; }
        public List<string> PaymentMethods { get; set; } = new();
        public DateTime? AuctionEndDate { get; set; }
        public bool IsCompanySeller { get; set; }

        public bool IsRejected { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime AddedAt { get; set; }
        public bool IsSeenByAdmin { get; set; }

        public List<SelectListItem> ConditionOptions { get; set; } = new();
        public List<SelectListItem> AdTypeOptions { get; set; } = new();
        public List<CategoryForAdvertisement> Breadcrumbs { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AvailableForPickup && string.IsNullOrWhiteSpace(PickupLocation))
            {
                yield return new ValidationResult(
                    "Upphämtningsplats krävs om upphämtning är valt.",
                    new[] { nameof(PickupLocation) });
            }                

            if (BuyNowPrice.HasValue && StartingPrice.HasValue && BuyNowPrice < StartingPrice)
            {
                yield return new ValidationResult(
                    "Köp nu-priset får inte vara lägre än utropspriset.",
                    new[] { nameof(BuyNowPrice) });
            }                

            if ((AdType == Auctionsite.Models.Database.AdType.Auction || AdType == Auctionsite.Models.Database.AdType.Both) && !StartingPrice.HasValue)
            {
                yield return new ValidationResult(
                    "Du måste ange ett startpris för auktionen.",
                    new[] { nameof(StartingPrice) });
            }

            if ((AdType == Auctionsite.Models.Database.AdType.BuyNow || AdType == Auctionsite.Models.Database.AdType.Both) && !BuyNowPrice.HasValue)
            {
                yield return new ValidationResult(
                    "Du måste ange ett köp nu-pris för annonsen.",
                    new[] { nameof(BuyNowPrice) });
            }

            if ((AdType == Auctionsite.Models.Database.AdType.Auction || AdType == Auctionsite.Models.Database.AdType.Both) && StartingPrice.HasValue && !AuctionEndDate.HasValue)
            {
                yield return new ValidationResult(
                    "Du måste ange ett slutdatum för auktionen.",
                    new[] { nameof(AuctionEndDate) });
            }            

            if (AuctionEndDate.HasValue && AuctionEndDate <= DateTime.Now)
            {
                yield return new ValidationResult(
                    "Slutdatumet måste vara i framtiden.",
                    new[] { nameof(AuctionEndDate) });
            }
        }
    }
    public class ImageDto
    {
        public int? Id { get; set; }
        [Required]
        public string Url { get; set; } = null!;
    }
}
