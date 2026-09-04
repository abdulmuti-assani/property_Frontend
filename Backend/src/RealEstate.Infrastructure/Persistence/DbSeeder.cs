using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;
using RealEstate.Infrastructure.Identity;

namespace RealEstate.Infrastructure.Persistence;

public static class DbSeeder
{
    // Fixed development accounts. "Owner" (property owner) maps to the Seller role,
    // "Guest" (browsing user) maps to the Buyer role.
    private sealed record SeedAccount(
        string Email, string Password, string Role,
        string FirstName, string LastName, string Phone, bool IsApproved);

    private static readonly SeedAccount[] Accounts =
    {
        new("admin@test.com",  "Admin1234!", "Admin",  "Site",  "Admin", "0500000000", true),
        new("owner1@test.com", "Test1234!",  "Seller", "Owner", "One",   "0500000001", true),
        new("owner2@test.com", "Test1234!",  "Seller", "Owner", "Two",   "0500000002", true),
        new("guest1@test.com", "Test1234!",  "Buyer",  "Guest", "One",   "0500000003", true),
        new("guest2@test.com", "Test1234!",  "Buyer",  "Guest", "Two",   "0500000004", true),
    };

    // Mirrors Frontend/src/assets/dummyProperties.js so the same Syrian-market
    // listings appear whether the API is reachable or the frontend falls back
    // to its bundled dummy data. Image1-3 are wwwroot/uploads/dummy/{n}.jpg,
    // copied from Frontend/src/assets/properties/{n}.jpg.
    private sealed record SeedProperty(
        string Title, string Description, decimal Price, string City, string Area,
        PropertyType Type, PropertyStatus Status, Furnishing Furnishing,
        int Bhk, int Bathrooms, int AreaSize, string[] Amenities, int Views,
        int Image1, int Image2, int Image3);

    private static readonly SeedProperty[] Properties =
    {
        new("Modern 3BHK Apartment in Mezzeh",
            "Bright, high-floor apartment in Mezzeh with modern finishes and easy access to central Damascus.",
            8500000m, "Damascus", "Mezzeh", PropertyType.Flat, PropertyStatus.Sale, Furnishing.Furnished,
            3, 2, 1450, new[] { "Parking", "Security", "Wifi" }, 128, 1, 2, 3),
        new("Luxury Villa with Private Pool",
            "Spacious family villa in Al Furqan featuring a private pool, landscaped garden, and premium finishes.",
            32000000m, "Aleppo", "Al Furqan", PropertyType.Villa, PropertyStatus.Sale, Furnishing.SemiFurnished,
            5, 4, 4200, new[] { "Pool", "Garden", "Parking", "Security" }, 342, 4, 5, 6),
        new("Sky Penthouse with Sea View",
            "Full-floor penthouse in Al Ziraa with panoramic Mediterranean sea views and a private terrace.",
            45000000m, "Lattakia", "Al Ziraa", PropertyType.Penthouse, PropertyStatus.Sale, Furnishing.Furnished,
            4, 3, 3100, new[] { "Security", "Power Backup", "Club House" }, 567, 7, 8, 9),
        new("Prime Commercial Office Space",
            "Ground-floor commercial space in Al Salihiyah with excellent footfall, ideal for retail or office use.",
            15000000m, "Damascus", "Al Salihiyah", PropertyType.Commercial, PropertyStatus.Rent, Furnishing.Unfurnished,
            0, 2, 2200, new[] { "Parking", "Power Backup" }, 89, 10, 11, 12),
        new("Cozy 2BHK Flat Near the Coast",
            "Well-maintained flat in the heart of Tartus, just minutes from the coastline and city amenities.",
            6200000m, "Tartus", "City Center", PropertyType.Flat, PropertyStatus.Sale, Furnishing.SemiFurnished,
            2, 1, 980, new[] { "Parking", "Wifi" }, 210, 13, 14, 15),
        new("Family Villa with Garden",
            "Quiet family villa in Al Waer with a private garden, close to parks and schools.",
            21000000m, "Homs", "Al Waer", PropertyType.Villa, PropertyStatus.Sale, Furnishing.SemiFurnished,
            4, 3, 3300, new[] { "Garden", "Parking", "Club House", "Security" }, 175, 16, 17, 18),
    };

    public static async Task SeedAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var baseUrl = (configuration["FileStorage:PublicBaseUrl"] ?? "http://localhost:5145").TrimEnd('/');

        await SeedAccountsAsync(userManager, context);
        await SeedSamplePropertiesAsync(context, baseUrl);
    }

    private static async Task SeedAccountsAsync(UserManager<ApplicationUser> userManager, AppDbContext context)
    {
        foreach (var account in Accounts)
        {
            if (await userManager.FindByEmailAsync(account.Email) is not null)
                continue;

            var identityUser = new ApplicationUser
            {
                UserName = account.Email,
                Email = account.Email,
                PhoneNumber = account.Phone,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(identityUser, account.Password);
            if (!result.Succeeded)
                continue;

            await userManager.AddToRoleAsync(identityUser, account.Role);

            context.UserProfiles.Add(new User
            {
                Id = identityUser.Id, // shared PK
                FirstName = account.FirstName,
                LastName = account.LastName,
                Email = account.Email,
                Phone = account.Phone,
                Role = account.Role,
                IsApproved = account.IsApproved
            });

            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedSamplePropertiesAsync(AppDbContext context, string baseUrl)
    {
        if (await context.Properties.AnyAsync())
            return;

        var owner = await context.UserProfiles.FirstOrDefaultAsync(u => u.Email == "owner1@test.com");
        if (owner is null)
            return;

        string ImageUrl(int n) => $"{baseUrl}/uploads/dummy/{n}.jpg";

        foreach (var p in Properties)
        {
            context.Properties.Add(new Property
            {
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                City = p.City,
                Area = p.Area,
                PropertyType = p.Type,
                Status = p.Status,
                Furnishing = p.Furnishing,
                Bhk = p.Bhk,
                Bathrooms = p.Bathrooms,
                AreaSize = p.AreaSize,
                Amenities = p.Amenities.ToList(),
                Views = p.Views,
                UserId = owner.Id,
                IsApproved = true, // the site's own demo listings are live immediately

                PropertyImgs = new List<PropertyImg>
                {
                    new() { ImgUrl = ImageUrl(p.Image1), IsPrimary = true },
                    new() { ImgUrl = ImageUrl(p.Image2), IsPrimary = false },
                    new() { ImgUrl = ImageUrl(p.Image3), IsPrimary = false }
                }
            });
        }

        await context.SaveChangesAsync();
    }
}
