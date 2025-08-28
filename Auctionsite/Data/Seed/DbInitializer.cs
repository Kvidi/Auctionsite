using Microsoft.EntityFrameworkCore;
using Auctionsite.Models.Database;

namespace Auctionsite.Data.Seed
{
    public static class DbInitializer
    {
        public static async Task SeedCategoriesAsync(ApplicationDbContext db)
        {
            if (await db.CategoryForAdvertisements.AnyAsync())
                return;

            var sport = new CategoryForAdvertisement
            {
                Name = "Sport & Fritid",
                DisplayOrder = 1,
                Subcategories = new List<CategoryForAdvertisement> {
                new CategoryForAdvertisement {
                    Name = "Utomhussporter & Friluftsliv", DisplayOrder = 1,
                    Subcategories = new List<CategoryForAdvertisement> {
                        new() { Name = "Cykel & Tillbehör",    DisplayOrder = 1 },
                        new() { Name = "Fiske & Jakt",         DisplayOrder = 2 },
                        new() { Name = "Vintersport",          DisplayOrder = 3 },
                        new() { Name = "Camping & Vandring",   DisplayOrder = 4 },
                    }
                },
                new CategoryForAdvertisement {
                    Name = "Bollsporter & Lagsporter", DisplayOrder = 2,
                    Subcategories = new List<CategoryForAdvertisement> {
                        new() { Name = "Fotboll",                       DisplayOrder = 1 },
                        new() { Name = "Tennis, Badminton, Squash",     DisplayOrder = 2 },
                        new() { Name = "Golf",                          DisplayOrder = 3 },
                    }
                },
                new CategoryForAdvertisement {
                    Name = "Träning & Fitness", DisplayOrder = 3,
                    Subcategories = new List<CategoryForAdvertisement> {
                        new() { Name = "Hantlar & Vikter",     DisplayOrder = 1 },
                        new() { Name = "Träningsmaskiner",     DisplayOrder = 2 },
                        new() { Name = "Yoga & Pilates",       DisplayOrder = 3 },
                    }
                },
                new CategoryForAdvertisement {
                    Name = "Vattensport", DisplayOrder = 4,
                    Subcategories = new List<CategoryForAdvertisement> {
                        new() { Name = "Dykning & Snorkling",   DisplayOrder = 1 },
                        new() { Name = "Kajaker & Kanoter",     DisplayOrder = 2 },
                    }
                },
                new CategoryForAdvertisement {
                    Name = "Hästsport & Ridsport", DisplayOrder = 5,
                    Subcategories = new List<CategoryForAdvertisement> {
                        new() { Name = "Ridkläder & Ridutrustning", DisplayOrder = 1 },
                        new() { Name = "Sadlar & Tillbehör",         DisplayOrder = 2 },
                        new() { Name = "Träns & Grimmor",            DisplayOrder = 3 },
                        new() { Name = "Hästtäcken",                 DisplayOrder = 4 },
                        new() { Name = "Skydd & Lindor",             DisplayOrder = 5 },
                        new() { Name = "Pälsvård",                   DisplayOrder = 6 },
                        new() { Name = "Travsport",                  DisplayOrder = 7 },
                        new() { Name = "Körutrustning",              DisplayOrder = 8 },
                        new() { Name = "Övrigt inom Ridsport",      DisplayOrder = 9 },
                    }
                },
                new CategoryForAdvertisement {
                    Name = "Övrigt inom Sport & Fritid", DisplayOrder = 6,
                    Subcategories = new List<CategoryForAdvertisement> {
                        new() { Name = "Spel & Lek",       DisplayOrder = 1 },
                        new() { Name = "Samlarprylar",     DisplayOrder = 2 },
                    }
                }
            }
            };

            var biljetter = new CategoryForAdvertisement
            {
                Name = "Biljetter & Resor",
                DisplayOrder = 2,
                Subcategories = new List<CategoryForAdvertisement> {
                new CategoryForAdvertisement {
                    Name = "Evenemangsbiljetter", DisplayOrder = 1,
                    Subcategories = new List<CategoryForAdvertisement> {
                        new() { Name = "Konserter & Musik",     DisplayOrder = 1 },
                        new() { Name = "Sportevenemang",        DisplayOrder = 2 },
                        new() { Name = "Teater & Show",         DisplayOrder = 3 },
                        new() { Name = "Festivalbiljetter",     DisplayOrder = 4 },
                    }
                },
                new CategoryForAdvertisement {
                    Name = "Resor & Upplevelser", DisplayOrder = 2,
                    Subcategories = new List<CategoryForAdvertisement> {
                        new() { Name = "Resepaket",                DisplayOrder = 1 },
                        new() { Name = "Presentkort för resor",    DisplayOrder = 2 },
                        new() { Name = "Kryssningar & Gruppresor", DisplayOrder = 3 },
                    }
                },
                new CategoryForAdvertisement {
                    Name = "Transportbiljetter", DisplayOrder = 3,
                    Subcategories = new List<CategoryForAdvertisement> {
                        new() { Name = "Tågbiljetter",   DisplayOrder = 1 },
                        new() { Name = "Flygbiljetter",  DisplayOrder = 2 },
                    }
                },
                new CategoryForAdvertisement {
                    Name = "Övrigt inom Biljetter & Resor", DisplayOrder = 4,
                    Subcategories = new List<CategoryForAdvertisement> {
                        new() { Name = "Lotter & Kuponger",          DisplayOrder = 1 },
                        new() { Name = "Gåvekort för upplevelser",   DisplayOrder = 2 },
                    }
                }
            }
            };

            var telefoni = new CategoryForAdvertisement
            {
                Name = "Telefoni, Tablets & Wearables",
                DisplayOrder = 3,
                Subcategories = new List<CategoryForAdvertisement> {
                new CategoryForAdvertisement {
                    Name = "Mobiltelefoner", DisplayOrder = 1,
                    Subcategories = new List<CategoryForAdvertisement> {
                        new() { Name = "Smartphones",           DisplayOrder = 1 },
                        new() { Name = "Klassiska telefoner",   DisplayOrder = 2 },
                        new() { Name = "Företagstelefoner",     DisplayOrder = 3 },
                    }
                },
                new CategoryForAdvertisement {
                    Name = "Tablets & Surfplattor", DisplayOrder = 2,
                    Subcategories = new List<CategoryForAdvertisement> {
                        new() { Name = "iPad & Android-tablets", DisplayOrder = 1 },
                        new() { Name = "Tillbehör",              DisplayOrder = 2 },
                    }
                },
                new CategoryForAdvertisement {
                    Name = "Wearables & Smarta Enheter", DisplayOrder = 3,
                    Subcategories = new List<CategoryForAdvertisement> {
                        new() { Name = "Smartklockor",      DisplayOrder = 1 },
                        new() { Name = "Fitnessspårrare",   DisplayOrder = 2 },
                        new() { Name = "VR-headsets",       DisplayOrder = 3 },
                    }
                },
                new CategoryForAdvertisement {
                    Name = "Tillbehör", DisplayOrder = 4,
                    Subcategories = new List<CategoryForAdvertisement> {
                        new() { Name = "Laddare & Kablar",        DisplayOrder = 1 },
                        new() { Name = "Hörlurar & Högtalare",    DisplayOrder = 2 },
                        new() { Name = "Skärmskydd & Fodral",     DisplayOrder = 3 },
                    }
                },
                new CategoryForAdvertisement {
                    Name = "Övrigt inom Telefoni", DisplayOrder = 5,
                    Subcategories = new List<CategoryForAdvertisement> {
                        new() { Name = "Mobilabonnemangskort", DisplayOrder = 1 },
                        new() { Name = "Vintage-telefoner",     DisplayOrder = 2 },
                    }
                }
            }
            };

            var ovrigt = new CategoryForAdvertisement
            {
                Name = "Övrigt",
                DisplayOrder = 4,
                Subcategories = new List<CategoryForAdvertisement> {
                new CategoryForAdvertisement {
                    Name = "Diverse Prylar", DisplayOrder = 1,
                    Subcategories = new List<CategoryForAdvertisement> {
                        new() { Name = "Hemma & Trädgård",      DisplayOrder = 1 },
                        new() { Name = "Elektronikprylar",     DisplayOrder = 2 },
                    }
                },
                new CategoryForAdvertisement {
                    Name = "Okategoriserade Varor", DisplayOrder = 2,
                    Subcategories = new List<CategoryForAdvertisement> {
                        new() { Name = "Secondhand-objekt", DisplayOrder = 1 },
                        new() { Name = "Mynt & Sedlar",     DisplayOrder = 2 },
                    }
                },
                new CategoryForAdvertisement {
                    Name = "Samlarobjekt", DisplayOrder = 3,
                    Subcategories = new List<CategoryForAdvertisement> {
                        new() { Name = "Figuriner & Modeller",  DisplayOrder = 1 },
                        new() { Name = "Vintage-reklam",        DisplayOrder = 2 },
                    }
                }
            }
            };

            await db.CategoryForAdvertisements.AddRangeAsync(
                sport, biljetter, telefoni, ovrigt
            );
            await db.SaveChangesAsync();
        }

        public static async Task SeedAdvertisementsAsync(ApplicationDbContext db, User customer)
        {
            var sport = await db.CategoryForAdvertisements
                             .Where(c => c.Name == "Sport & Fritid")
                             .Select(c => c.Id)
                             .FirstAsync();
            var resor = await db.CategoryForAdvertisements
                             .Where(c => c.Name == "Biljetter & Resor")
                             .Select(c => c.Id)
                             .FirstAsync();
            var telefoni = await db.CategoryForAdvertisements
                             .Where(c => c.Name == "Telefoni, Tablets & Wearables")
                             .Select(c => c.Id)
                             .FirstAsync();
            var ovrigt = await db.CategoryForAdvertisements
                             .Where(c => c.Name == "Övrigt")
                             .Select(c => c.Id)
                             .FirstAsync();

            var seedAds = new List<Advertisement>
            {
                new Advertisement {
                    Title = "Seed-annons 1",
                    Description = "En testannons skapad genom seedning med kategori Sport och Auktion som annonstyp.",
                    StartingPrice = 150,
                    AdType = AdType.Auction,
                    Images = new List<AdvertisementImage>
                    {
                        // First Image = head image
                        new AdvertisementImage { Url = "https://picsum.photos/600/300", IsMain = true },
                        new AdvertisementImage { Url = "https://picsum.photos/400/300", IsMain = false }
                    },
                    AddedAt = DateTime.Now.AddDays(-1),
                    AuctionEndDate = DateTime.Now.AddDays(30),
                    ApprovedAt = DateTime.Now, // Simulate approval after 1 day
                    AvailableForPickup = true,
                    PickupLocation = "Testköping",
                    Condition = Condition.Good,                  
                    Advertiser = customer,
                    CategoryId = sport
                },
                new Advertisement {
                    Title = "Seed-annons 2",
                    Description = "En testannons skapad genom seedning med kategori Resor och Köp nu som annonstyp.",
                    BuyNowPrice = 300,
                    AdType = AdType.BuyNow,
                    Images = new List<AdvertisementImage>
                    {
                        // First Image = head image
                        new AdvertisementImage { Url = "https://picsum.photos/600/300", IsMain = true },
                        new AdvertisementImage { Url = "https://picsum.photos/400/300", IsMain = false }
                    },
                    AddedAt = DateTime.Now.AddDays(-1),
                    AuctionEndDate = DateTime.Now.AddDays(30),
                    ApprovedAt = DateTime.Now, // Simulate approval after 1 day
                    AvailableForPickup = true,
                    PickupLocation = "Testköping",
                    Condition = Condition.UnUsed,
                    Advertiser = customer,
                    CategoryId = resor
                },
                new Advertisement {
                    Title = "Seed-annons 3",
                    Description = "En testannons skapad genom seedning med kategori Telefoni och både Auktion och Köp nu som annonstyp.",
                    StartingPrice = 200,
                    BuyNowPrice = 300,
                    AdType = AdType.Both,
                    Images = new List<AdvertisementImage>
                    {
                        // First Image = head image
                        new AdvertisementImage { Url = "https://picsum.photos/600/300", IsMain = true },
                        new AdvertisementImage { Url = "https://picsum.photos/400/300", IsMain = false }
                    },
                    AddedAt = DateTime.Now.AddDays(-1),
                    AuctionEndDate = DateTime.Now.AddDays(30),
                    ApprovedAt = DateTime.Now, // Simulate approval after 1 day
                    AvailableForPickup = true,
                    PickupLocation = "Testköping",
                    Condition = Condition.WellUsed,
                    Advertiser = customer,
                    CategoryId = telefoni
                },
                new Advertisement {
                    Title = "Seed-annons 4",
                    Description = "En testannons skapad genom seedning med kategori Övrigt och både Auktion och Köp nu som annonstyp.",
                    StartingPrice = 250,
                    BuyNowPrice = 400,
                    AdType = AdType.Both,
                    Images = new List<AdvertisementImage>
                    {
                        // First Image = head image
                        new AdvertisementImage { Url = "https://picsum.photos/600/300", IsMain = true },
                        new AdvertisementImage { Url = "https://picsum.photos/400/300", IsMain = false }
                    },
                    AddedAt = DateTime.Now,
                    AuctionEndDate = DateTime.Now.AddDays(30),
                    AvailableForPickup = true,
                    PickupLocation = "Testköping",
                    Condition = Condition.Defect,
                    Advertiser = customer,
                    CategoryId = ovrigt
                },
                new Advertisement {
                    Title = "Seed-annons 5",
                    Description = "En testannons skapad genom seedning med kategori Telefoni och Auktion som annonstyp.",
                    StartingPrice = 1500,
                    AdType = AdType.Auction,
                    Images = new List<AdvertisementImage>
                    {
                        // First Image = head image
                        new AdvertisementImage { Url = "https://picsum.photos/600/300", IsMain = true },
                        new AdvertisementImage { Url = "https://picsum.photos/400/300", IsMain = false }
                    },
                    AddedAt = DateTime.Now,
                    AuctionEndDate = DateTime.Now.AddDays(30),
                    AvailableForPickup = true,
                    PickupLocation = "Testköping",
                    Condition = Condition.LikeNew,
                    Advertiser = customer,
                    CategoryId = telefoni
                }
            };

            foreach (var ad in seedAds)
            {
                // Check if the advertisement already exists by title                
                if (!await db.Advertisements.AnyAsync(a => a.Title == ad.Title))
                {
                    await db.Advertisements.AddAsync(ad);
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
