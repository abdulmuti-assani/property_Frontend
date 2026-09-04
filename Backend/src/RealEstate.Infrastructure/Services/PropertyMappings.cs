using RealEstate.Application.DTOs.Properties;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;

namespace RealEstate.Infrastructure.Services;

/// <summary>
/// Manual string &lt;-&gt; enum conversion for the values the frontend sends/reads
/// (lowercase, and "semi-furnished" with a hyphen), plus Property -&gt; PropertyDto mapping.
/// </summary>
internal static class PropertyMappings
{
    public static PropertyDto ToDto(this Property p) => new(
        p.Id,
        p.Title,
        p.Description,
        p.Price,
        p.PropertyImgs.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.Id).Select(i => i.ImgUrl).ToList(),
        p.PropertyType.ToApiString(),
        p.City,
        p.Area,
        p.Pincode,
        p.AreaSize,
        p.Bhk,
        p.Bathrooms,
        p.Furnishing.ToApiString(),
        p.Status.ToApiString(),
        p.Amenities,
        p.Views,
        p.SecurityDeposit,
        p.Maintenance,
        p.IsApproved,
        new PropertySellerDto(
            p.User.Id,
            $"{p.User.FirstName} {p.User.LastName}".Trim(),
            p.User.ProfilePicUrl,
            p.User.Email),
        p.CreatedAtUtc);

    public static PropertyType ParsePropertyType(string value) => value?.Trim().ToLowerInvariant() switch
    {
        "flat" => PropertyType.Flat,
        "villa" => PropertyType.Villa,
        "penthouse" => PropertyType.Penthouse,
        "commercial" => PropertyType.Commercial,
        _ => throw new InvalidOperationException($"Invalid property type: '{value}'.")
    };

    public static string ToApiString(this PropertyType value) => value switch
    {
        PropertyType.Flat => "flat",
        PropertyType.Villa => "villa",
        PropertyType.Penthouse => "penthouse",
        PropertyType.Commercial => "commercial",
        _ => "flat"
    };

    public static PropertyStatus ParseStatus(string value) => value?.Trim().ToLowerInvariant() switch
    {
        "sale" => PropertyStatus.Sale,
        "rent" => PropertyStatus.Rent,
        "sold" => PropertyStatus.Sold,
        _ => throw new InvalidOperationException($"Invalid property status: '{value}'.")
    };

    public static string ToApiString(this PropertyStatus value) => value switch
    {
        PropertyStatus.Sale => "sale",
        PropertyStatus.Rent => "rent",
        PropertyStatus.Sold => "sold",
        _ => "sale"
    };

    public static Furnishing ParseFurnishing(string value) => value?.Trim().ToLowerInvariant() switch
    {
        "unfurnished" => Furnishing.Unfurnished,
        "semi-furnished" => Furnishing.SemiFurnished,
        "furnished" => Furnishing.Furnished,
        _ => throw new InvalidOperationException($"Invalid furnishing: '{value}'.")
    };

    public static string ToApiString(this Furnishing value) => value switch
    {
        Furnishing.Unfurnished => "unfurnished",
        Furnishing.SemiFurnished => "semi-furnished",
        Furnishing.Furnished => "furnished",
        _ => "unfurnished"
    };
}
