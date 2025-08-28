using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Auctionsite.Models;
using Auctionsite.Models.Database;

namespace Auctionsite.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Advertisement> Advertisements { get; set; }
    public DbSet<CategoryForAdvertisement> CategoryForAdvertisements { get;set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<MaxBid> MaxBids { get; set; }
    public DbSet<Bid> Bids { get; set; } 
    public DbSet<AdvertisementImage> AdvertisementImages { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Chat>()
            .HasOne(c => c.Customer)
            .WithMany(u => u.ChatsAsCustomer)
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Chat>()
            .HasOne(c => c.Advertiser)
            .WithMany(u => u.ChatsAsAdvertiser)
            .HasForeignKey(c => c.AdvertiserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Chat>()
            .HasOne(c => c.Advertisement)
            .WithMany(a => a.Chats)
            .HasForeignKey(c => c.AdvertisementId)
            .OnDelete(DeleteBehavior.Cascade);

        // The many-to-many relationship between User and Advertisement for favouriting ads
        modelBuilder.Entity<Advertisement>()
            .HasMany(a => a.UsersWhoFavourited)
            .WithMany(u => u.FavouriteAds)
            .UsingEntity(j => j.ToTable("UserFavouriteAdvertisements"));

        // Specifying the relationship between Advertisement and User regarding the advertiser
        modelBuilder.Entity<Advertisement>()
            .HasOne(a => a.Advertiser)
            .WithMany(u => u.Advertisements)
            .HasForeignKey("AdvertiserId")
            .OnDelete(DeleteBehavior.Restrict);

        // The relationship between Advertisement and User for purchased ads
        modelBuilder.Entity<Advertisement>()
        .HasOne(a => a.PurchasedBy)
        .WithMany()                            // or .WithMany(u => u.PurchasedAds) if we want to add such a collection
        .HasForeignKey(a => a.PurchasedByUserId)
        .OnDelete(DeleteBehavior.Restrict);

        // If a category is deleted, its advertisements won't be deleted automatically
        modelBuilder.Entity<Advertisement>()
            .HasOne(a => a.Category)
            .WithMany(c => c.Advertisements)
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure cascade delete so that when an Advertisement is removed, all related AdvertisementImage entries are automatically deleted
        modelBuilder.Entity<Advertisement>()
            .HasMany(a => a.Images)
            .WithOne(i => i.Advertisement)
            .HasForeignKey(i => i.AdvertisementId)
            .OnDelete(DeleteBehavior.Cascade);

        // If a parent category is deleted, its subcategories will have to be manually deleted or updated
        modelBuilder.Entity<CategoryForAdvertisement>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.Subcategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.TargetUser)
            .WithMany(u => u.ReviewsReceived)
            .HasForeignKey(r => r.TargetUserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Reviewer)
            .WithMany(u => u.ReviewsWritten)
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.NoAction);

    }
}
