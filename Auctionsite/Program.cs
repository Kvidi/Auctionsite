using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using Auctionsite.Data;
using Auctionsite.Data.Seed;
using Auctionsite.Helpers;
using Auctionsite.Hubs;
using Auctionsite.Models.Database;
using Auctionsite.Services;
using Auctionsite.Services.Background;
using Auctionsite.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true; // Requires email confirmation before login
    options.User.RequireUniqueEmail = true; // Ensures each user has a unique email
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultUI()
.AddRoles<IdentityRole>()
.AddDefaultTokenProviders();

builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 1;
    options.Password.RequiredUniqueChars = 0;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzåäöABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

builder.Services.AddRazorPages();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
    .AddCookie("Cookies")
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId is not configured.");
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret is not configured.");
        options.SaveTokens = true;
        options.Scope.AddRange(new[]
        {
            "https://www.googleapis.com/auth/userinfo.profile",
            "https://www.googleapis.com/auth/userinfo.email",
            "https://www.googleapis.com/auth/user.addresses.read",
            "https://www.googleapis.com/auth/user.phonenumbers.read",
            "https://www.googleapis.com/auth/user.birthday.read",
        });
        options.Events.OnCreatingTicket = context =>
        {
            var accessToken = context.AccessToken;
            var refreshToken = context.RefreshToken;

            Console.WriteLine($"AccessToken: {accessToken}");
            Console.WriteLine($"RefreshToken: {refreshToken}");
            return Task.CompletedTask;
        };
    });

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAdService, AdService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHostedService<NotificationCleanupService>();
builder.Services.AddSignalR();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{  
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    // Seed Categories
    await DbInitializer.SeedCategoriesAsync(db);

    // Seed Roles
    var roles = new[] { "SuperAdmin", "Admin", "User", "Auditor" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Seed Admin user
    string email = "admin@admin.com";
    string password = "admin";
    if (await userManager.FindByEmailAsync(email) == null)
    {
        var user = new User
        {
            FirstName = "Admin",
            LastName = "Admin",
            UserName = email,
            Email = email,
            EmailConfirmed = true, // Set to true to avoid email confirmation requirement
        };
        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
        else
        {
            // Logga eller skriv ut felen
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Error: {error.Description}");
            }
        }
        //await userManager.AddToRoleAsync(user, "Admin");
    }

    // Seed Customer user
    string customerEmail = "customer@example.com";
    string customerPassword = "customer";
    var customer = await userManager.FindByEmailAsync(customerEmail);
    if (customer == null)
    {
        customer = new User
        {
            FirstName = "Customer",
            LastName = "Test",
            UserName = customerEmail,
            Email = customerEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(customer, customerPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Error creating customer user: {error.Description}");
            }
        }
    }

    // Seed advertisements for the customer user
    await DbInitializer.SeedAdvertisementsAsync(db, customer);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.MapHub<ChatHub>("/chatHub"); // Map the SignalR hub
app.MapHub<AdvertisementHub>("/advertisementhub");
app.MapHub<NotificationHub>("/notificationhub");

app.Run();