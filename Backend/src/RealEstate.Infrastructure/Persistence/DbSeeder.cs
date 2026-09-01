using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RealEstate.Application.Common.Settings;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;
using RealEstate.Infrastructure.Identity;

namespace RealEstate.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var seed = configuration.GetSection("SeedSettings").Get<SeedSettings>() ?? new SeedSettings();

        await SeedAdminAsync(userManager, context, seed);
        await SeedSamplePropertiesAsync(userManager, context);
    }

    private static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userManager, AppDbContext context, SeedSettings seed)
    {
        if (string.IsNullOrWhiteSpace(seed.AdminEmail) || string.IsNullOrWhiteSpace(seed.AdminPassword))
            return;

        if (await userManager.FindByEmailAsync(seed.AdminEmail) is not null)
            return;

        var identityUser = new ApplicationUser
        {
            UserName = seed.AdminEmail,
            Email = seed.AdminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(identityUser, seed.AdminPassword);
        if (!result.Succeeded)
            return;

        await userManager.AddToRoleAsync(identityUser, "Admin");

        var parts = seed.AdminName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        context.UserProfiles.Add(new User
        {
            Id = identityUser.Id,
            FirstName = parts.Length > 0 ? parts[0] : "System",
            LastName = parts.Length > 1 ? parts[1] : "Admin",
            Email = seed.AdminEmail,
            Role = "Admin",
            IsApproved = true
        });

        await context.SaveChangesAsync();
    }

    private static async Task SeedSamplePropertiesAsync(
        UserManager<ApplicationUser> userManager, AppDbContext context)
    {
        if (await context.Properties.AnyAsync())
            return;

        const string sellerEmail = "sample.seller@realestate.local";
        var sellerId = (await context.UserProfiles.FirstOrDefaultAsync(u => u.Email == sellerEmail))?.Id;

        if (sellerId is null)
        {
            var sellerUser = new ApplicationUser { UserName = sellerEmail, Email = sellerEmail, EmailConfirmed = true };
            var created = await userManager.CreateAsync(sellerUser, "Seller@12345");
            if (!created.Succeeded)
                return;

            await userManager.AddToRoleAsync(sellerUser, "Seller");
            context.UserProfiles.Add(new User
            {
                Id = sellerUser.Id,
                FirstName = "Sample",
                LastName = "Seller",
                Email = sellerEmail,
                Phone = "0500000000",
                Role = "Seller",
                IsApproved = true
            });
            await context.SaveChangesAsync();
            sellerId = sellerUser.Id;
        }

        var samples = new[]
        {
            ("Modern 3BHK Apartment in Downtown", "Bright, high-floor apartment with skyline views and premium finishes.", 8500000m, "Dubai", "Downtown", PropertyType.Flat, PropertyStatus.Sale, Furnishing.Furnished, 3, 2, 1450, new[] { "Parking", "Pool", "Gym", "Security" }),
            ("Luxury Villa with Private Garden", "Spacious family villa with a landscaped garden and maid's room.", 22000000m, "Dubai", "Emirates Hills", PropertyType.Villa, PropertyStatus.Sale, Furnishing.SemiFurnished, 5, 6, 6200, new[] { "Parking", "Garden", "Security", "Club House" }),
            ("Sea-View Penthouse", "Full-floor penthouse with a wraparound terrace and private lift.", 41000000m, "Dubai", "Palm Jumeirah", PropertyType.Penthouse, PropertyStatus.Sale, Furnishing.Furnished, 4, 5, 5400, new[] { "Pool", "Gym", "Security", "Power Backup" }),
            ("Cozy 1BHK for Rent", "Well-maintained unit close to the metro, ideal for young professionals.", 65000m, "Dubai", "JLT", PropertyType.Flat, PropertyStatus.Rent, Furnishing.Unfurnished, 1, 1, 720, new[] { "Parking", "Security", "Wifi" }),
            ("Retail Shop on Main Road", "Ground-floor commercial space with excellent footfall and frontage.", 3200000m, "Sharjah", "Al Nahda", PropertyType.Commercial, PropertyStatus.Sale, Furnishing.Unfurnished, 0, 2, 1100, new[] { "Parking", "Power Backup" }),
            ("Family Villa Near Park", "Quiet community villa steps from a green park and schools.", 9800000m, "Abu Dhabi", "Khalifa City", PropertyType.Villa, PropertyStatus.Sale, Furnishing.SemiFurnished, 4, 4, 3800, new[] { "Garden", "Parking", "Club House", "Security" })
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
                UserId = sellerId.Value,
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
