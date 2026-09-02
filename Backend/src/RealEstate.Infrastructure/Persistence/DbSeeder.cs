using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

    public static async Task SeedAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SeedAccountsAsync(userManager, context);
        await SeedSamplePropertiesAsync(context);
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

    private static async Task SeedSamplePropertiesAsync(AppDbContext context)
    {
        if (await context.Properties.AnyAsync())
            return;

        var owner = await context.UserProfiles.FirstOrDefaultAsync(u => u.Email == "owner1@test.com");
        if (owner is null)
            return;

        var samples = new[]
        {
            ("Modern 3BHK Apartment in Downtown", "Bright, high-floor apartment with skyline views and premium finishes.", 850000m, "Dubai", "Downtown", PropertyType.Flat, PropertyStatus.Sale, Furnishing.Furnished, 3, 2, 1450, new[] { "Parking", "Pool", "Gym", "Security" }),
            ("Luxury Villa with Private Garden", "Spacious family villa with a landscaped garden and maid's room.", 2200000m, "Dubai", "Emirates Hills", PropertyType.Villa, PropertyStatus.Sale, Furnishing.SemiFurnished, 5, 6, 6200, new[] { "Parking", "Garden", "Security", "Club House" }),
            ("Sea-View Penthouse", "Full-floor penthouse with a wraparound terrace and private lift.", 4100000m, "Dubai", "Palm Jumeirah", PropertyType.Penthouse, PropertyStatus.Sale, Furnishing.Furnished, 4, 5, 5400, new[] { "Pool", "Gym", "Security", "Power Backup" }),
            ("Cozy 1BHK for Rent", "Well-maintained unit close to the metro, ideal for young professionals.", 12000m, "Dubai", "JLT", PropertyType.Flat, PropertyStatus.Rent, Furnishing.Unfurnished, 1, 1, 720, new[] { "Parking", "Security", "Wifi" }),
            ("Retail Shop on Main Road", "Ground-floor commercial space with excellent footfall and frontage.", 320000m, "Sharjah", "Al Nahda", PropertyType.Commercial, PropertyStatus.Sale, Furnishing.Unfurnished, 0, 2, 1100, new[] { "Parking", "Power Backup" }),
            ("Family Villa Near Park", "Quiet community villa steps from a green park and schools.", 980000m, "Abu Dhabi", "Khalifa City", PropertyType.Villa, PropertyStatus.Sale, Furnishing.SemiFurnished, 4, 4, 3800, new[] { "Garden", "Parking", "Club House", "Security" })
        };

        var index = 1;
        foreach (var s in samples)
        {
            context.Properties.Add(new Property
            {
                Title = s.Item1,
                Description = s.Item2,
                Price = s.Item3,
                City = s.Item4,
                Area = s.Item5,
                PropertyType = s.Item6,
                Status = s.Item7,
                Furnishing = s.Item8,
                Bhk = s.Item9,
                Bathrooms = s.Item10,
                AreaSize = s.Item11,
                Amenities = s.Item12.ToList(),
                Views = 0,
                UserId = owner.Id,
                PropertyImgs = new List<PropertyImg>
                {
                    new() { ImgUrl = $"https://picsum.photos/seed/realestate{index}/800/600", IsPrimary = true },
                    new() { ImgUrl = $"https://picsum.photos/seed/realestate{index}b/800/600", IsPrimary = false }
                }
            });
            index++;
        }

        await context.SaveChangesAsync();
    }
}
