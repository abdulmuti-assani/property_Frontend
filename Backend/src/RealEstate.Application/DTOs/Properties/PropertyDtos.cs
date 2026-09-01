using Microsoft.AspNetCore.Http;

namespace RealEstate.Application.DTOs.Properties;

public record PropertySellerDto(int Id, string Name, string? ProfilePic, string Email);

public record PropertyDto(
    int Id,
    string Title,
    string Description,
    decimal Price,
    List<string> Images,
    string PropertyType,
    string City,
    string Area,
    string? Pincode,
    int AreaSize,
    int Bhk,
    int Bathrooms,
    string Furnishing,
    string Status,
    List<string> Amenities,
    int Views,
    decimal SecurityDeposit,
    decimal Maintenance,
    PropertySellerDto Seller,
    DateTimeOffset CreatedAt);

public record PropertyDetailsResponse(PropertyDto Property, List<PropertyDto> SimilarProperties);

public record PropertyCountsResponse(bool Success, PropertyCounts Counts);
public record PropertyCounts(int Flat, int Villa, int Penthouse, int Commercial);

public record SellerDashboardResponse(SellerDashboardStats Stats);
public record SellerDashboardStats(
    int TotalProperties,
    int ActiveListings,
    int SoldProperties,
    int TotalInquiries,
    int TotalViews);

public record PropertySavedResponse(bool Success, PropertyDto Property);

public class PropertyFilterRequest
{
    public string? City { get; set; }
    public string? PropertyType { get; set; }
    public string? Bhk { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Furnishing { get; set; }
    public string? Sort { get; set; }
}

public class CreatePropertyRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string City { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string? Pincode { get; set; }
    public string PropertyType { get; set; } = string.Empty;
    public int Bhk { get; set; }
    public int Bathrooms { get; set; }
    public int AreaSize { get; set; }
    public string Furnishing { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<string> Amenities { get; set; } = new();
    public decimal SecurityDeposit { get; set; }
    public decimal Maintenance { get; set; }
    public List<IFormFile> Images { get; set; } = new();
}

public class UpdatePropertyRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string City { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string? Pincode { get; set; }
    public string PropertyType { get; set; } = string.Empty;
    public int Bhk { get; set; }
    public int Bathrooms { get; set; }
    public int AreaSize { get; set; }
    public string Furnishing { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    // EditProperty.jsx sends amenities as a JSON-stringified array.
    public string? Amenities { get; set; }
    public decimal SecurityDeposit { get; set; }
    public decimal Maintenance { get; set; }
    // JSON-stringified array of image URLs to keep.
    public string? ExistingImages { get; set; }
    public List<IFormFile> Images { get; set; } = new();
}

public record UpdateStatusRequest(string Status);
