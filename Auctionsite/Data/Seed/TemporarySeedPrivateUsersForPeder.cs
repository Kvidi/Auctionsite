using Auctionsite.Models.Database;
using Microsoft.AspNetCore.Identity;

namespace Auctionsite.Data.Seed
{
    public static class TemporarySeedPrivateUsersForPeder
    {

        public static async Task Seed(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                await SeedUsers_PederSchulze(userManager);
            }




            // Seed three users with different roles for Peder Schulze
            static async Task SeedUsers_PederSchulze(UserManager<User> userManager)       // ev. private
            {

                User[] pedersUsers =
                {
                        new User { FirstName = "Peder", LastName = "Schulze", UserName = "Peder_SuperAdmin@test.com", Email = "Peder_SuperAdmin@test.com", PhoneNumber = "073-962 83 59"},
                        new User { FirstName = "Peder", LastName = "Schulze", UserName = "Peder_Auditor@test.com", Email = "Peder_Auditor@test.com", PhoneNumber = "073-962 83 59"},
                        new User { FirstName = "Peder", LastName = "Schulze", UserName = "Peder_Admin@test.com", Email = "Peder_Admin@test.com", PhoneNumber = "073-962 83 59"},
                        new User { FirstName = "Peder", LastName = "Schulze", UserName = "Peder_User@test.com", Email = "Peder_User@test.com", PhoneNumber = "073-962 83 59"},
                        new User { FirstName = "Per", LastName = "Markström", UserName = "Per_SuperAdmin", Email = "Per_SuperAdmin@test.com", PhoneNumber = "012-345 67 89"},
                        new User { FirstName = "Per", LastName = "Markström", UserName = "Per_Auditor", Email = "Per_Auditor@test.com", PhoneNumber = "012-345 67 89"},
                        new User { FirstName = "Per", LastName = "Markström", UserName = "Per_Admin", Email = "Per_Admin@test.com", PhoneNumber = "012-345 67 89"},
                        new User { FirstName = "Per", LastName = "Markström", UserName = "Per_User", Email = "Per_User@test.com", PhoneNumber = "012-345 67 89"},
                };

                var password = "*Qwerty123";

                foreach (var usr in pedersUsers)
                {
                    User user = new()
                    {
                        Email = usr.Email,
                        UserName = usr.UserName,
                        FirstName = usr.FirstName,
                        LastName = usr.LastName,
                        PhoneNumber = usr.PhoneNumber,
                        EmailConfirmed = true,
                    };


                    // Seed the user
                    IdentityResult userResult = await userManager.CreateAsync(user, password);

                    if (userResult.Succeeded)
                    {
                        // Add the role to that user
                        if (usr.UserName.Contains("SuperAdmin"))
                        {
                            userManager.AddToRoleAsync(user, role: "SuperAdmin").Wait();
                        }
                        else if (usr.UserName.Contains("Auditor"))
                        {
                            userManager.AddToRoleAsync(user, role: "Auditor").Wait();
                        }
                        else if (usr.UserName.Contains("Admin"))
                        {
                            userManager.AddToRoleAsync(user, role: "Admin").Wait();
                        }
                        else
                        {
                            userManager.AddToRoleAsync(user, role: "User").Wait();
                        }

                    }
                }
            }
        }
    }
}
