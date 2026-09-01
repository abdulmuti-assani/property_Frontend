using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RealEstate.Application.DTOs.Properties;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Infrastructure.Services;

public class PropertyService : IPropertyService
{
    private const string ImageSubFolder = "properties";

    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public PropertyService(IApplicationDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<List<PropertyDto>> GetAllAsync(PropertyFilterRequest filter)
    {
        var query = BaseQuery();

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            var term = filter.City.Trim();
            query = query.Where(p => p.City.Contains(term) || p.Area.Contains(term));
        }

        var types = ParseCsv(filter.PropertyType, PropertyMappings.ParsePropertyType);
        if (types.Count > 0)
            query = query.Where(p => types.Contains(p.PropertyType));

        var furnishings = ParseCsv(filter.Furnishing, PropertyMappings.ParseFurnishing);
        if (furnishings.Count > 0)
            query = query.Where(p => furnishings.Contains(p.Furnishing));

        if (!string.IsNullOrWhiteSpace(filter.Bhk))
        {
            if (filter.Bhk.Trim() is "5+" or "5")
                query = query.Where(p => p.Bhk >= 5);
            else if (int.TryParse(filter.Bhk.Trim(), out var bhk))
                query = query.Where(p => p.Bhk == bhk);
        }

        if (filter.MaxPrice is > 0)
            query = query.Where(p => p.Price <= filter.MaxPrice);

        query = filter.Sort?.Trim().ToLowerInvariant() switch
        {
            "pricelow" => query.OrderBy(p => p.Price),
            "pricehigh" => query.OrderByDescending(p => p.Price),
            _ => query.OrderByDescending(p => p.CreatedAtUtc)
        };

        var properties = await query.ToListAsync();
        return properties.Select(p => p.ToDto()).ToList();
    }

    public async Task<PropertyCountsResponse> GetCountsAsync()
    {
        var grouped = await _context.Properties
            .GroupBy(p => p.PropertyType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();

        int CountFor(PropertyType type) => grouped.FirstOrDefault(x => x.Type == type)?.Count ?? 0;

        var counts = new PropertyCounts(
            CountFor(PropertyType.Flat),
            CountFor(PropertyType.Villa),
            CountFor(PropertyType.Penthouse),
            CountFor(PropertyType.Commercial));

        return new PropertyCountsResponse(true, counts);
    }

    public async Task<PropertyDetailsResponse> GetByIdAsync(int id)
    {
        var property = await BaseQuery().FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException(nameof(Property), id);

        property.Views += 1;
        await _context.SaveChangesAsync();

        var similar = await BaseQuery()
            .Where(p => p.Id != id && (p.PropertyType == property.PropertyType || p.City == property.City))
            .OrderByDescending(p => p.CreatedAtUtc)
            .Take(6)
            .ToListAsync();

        return new PropertyDetailsResponse(property.ToDto(), similar.Select(p => p.ToDto()).ToList());
    }

    public async Task<List<PropertyDto>> GetMineAsync(int sellerId)
    {
        var properties = await BaseQuery()
            .Where(p => p.UserId == sellerId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync();

        return properties.Select(p => p.ToDto()).ToList();
    }

    public async Task<SellerDashboardResponse> GetSellerDashboardAsync(int sellerId)
    {
        var totalProperties = await _context.Properties.CountAsync(p => p.UserId == sellerId);
        var soldProperties = await _context.Properties.CountAsync(p => p.UserId == sellerId && p.Status == PropertyStatus.Sold);
        var activeListings = await _context.Properties.CountAsync(p => p.UserId == sellerId && p.Status != PropertyStatus.Sold);
        var totalInquiries = await _context.Inquiries.CountAsync(i => i.Property.UserId == sellerId);
        var totalViews = await _context.Properties
            .Where(p => p.UserId == sellerId)
            .SumAsync(p => (int?)p.Views) ?? 0;

        return new SellerDashboardResponse(new SellerDashboardStats(
            totalProperties, activeListings, soldProperties, totalInquiries, totalViews));
    }

    public async Task<PropertySavedResponse> CreateAsync(int sellerId, CreatePropertyRequest request)
    {
        await EnsureApprovedSellerAsync(sellerId);

        var property = new Property
        {
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            City = request.City,
            Area = request.Area,
            Pincode = request.Pincode,
            PropertyType = PropertyMappings.ParsePropertyType(request.PropertyType),
            Status = PropertyMappings.ParseStatus(request.Status),
            Furnishing = PropertyMappings.ParseFurnishing(request.Furnishing),
            Bhk = request.Bhk,
            Bathrooms = request.Bathrooms,
            AreaSize = request.AreaSize,
            SecurityDeposit = request.SecurityDeposit,
            Maintenance = request.Maintenance,
            Amenities = request.Amenities ?? new List<string>(),
            UserId = sellerId
        };

        var images = request.Images ?? new List<Microsoft.AspNetCore.Http.IFormFile>();
        for (var i = 0; i < images.Count; i++)
        {
            var url = await _fileStorage.SaveAsync(images[i], ImageSubFolder);
            property.PropertyImgs.Add(new PropertyImg { ImgUrl = url, IsPrimary = i == 0 });
        }

        _context.Properties.Add(property);
        await _context.SaveChangesAsync();

        var saved = await BaseQuery().FirstAsync(p => p.Id == property.Id);
        return new PropertySavedResponse(true, saved.ToDto());
    }

    public async Task<PropertySavedResponse> UpdateAsync(int sellerId, int id, UpdatePropertyRequest request)
    {
        var property = await _context.Properties
            .Include(p => p.PropertyImgs)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException(nameof(Property), id);

        if (property.UserId != sellerId)
            throw new ForbiddenException("You can only edit your own listings.");

        property.Title = request.Title;
        property.Description = request.Description;
        property.Price = request.Price;
        property.City = request.City;
        property.Area = request.Area;
        property.Pincode = request.Pincode;
        property.PropertyType = PropertyMappings.ParsePropertyType(request.PropertyType);
        property.Status = PropertyMappings.ParseStatus(request.Status);
        property.Furnishing = PropertyMappings.ParseFurnishing(request.Furnishing);
        property.Bhk = request.Bhk;
        property.Bathrooms = request.Bathrooms;
        property.AreaSize = request.AreaSize;
        property.SecurityDeposit = request.SecurityDeposit;
        property.Maintenance = request.Maintenance;
        property.Amenities = ParseJsonArray(request.Amenities);

        var keepUrls = ParseJsonArray(request.ExistingImages);
        var removed = property.PropertyImgs.Where(img => !keepUrls.Contains(img.ImgUrl)).ToList();
        foreach (var img in removed)
        {
            _fileStorage.Delete(img.ImgUrl);
            property.PropertyImgs.Remove(img);
            _context.PropertyImages.Remove(img);
        }

        var newImages = request.Images ?? new List<Microsoft.AspNetCore.Http.IFormFile>();
        foreach (var file in newImages)
        {
            var url = await _fileStorage.SaveAsync(file, ImageSubFolder);
            property.PropertyImgs.Add(new PropertyImg { ImgUrl = url, IsPrimary = false });
        }

        if (property.PropertyImgs.Count > 0 && property.PropertyImgs.All(img => !img.IsPrimary))
            property.PropertyImgs.First().IsPrimary = true;

        await _context.SaveChangesAsync();

        var saved = await BaseQuery().FirstAsync(p => p.Id == property.Id);
        return new PropertySavedResponse(true, saved.ToDto());
    }

    public async Task UpdateStatusAsync(int sellerId, int id, UpdateStatusRequest request)
    {
        var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException(nameof(Property), id);

        if (property.UserId != sellerId)
            throw new ForbiddenException("You can only update your own listings.");

        property.Status = PropertyMappings.ParseStatus(request.Status);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int sellerId, int id)
    {
        var property = await _context.Properties
            .Include(p => p.PropertyImgs)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException(nameof(Property), id);

        if (property.UserId != sellerId)
            throw new ForbiddenException("You can only delete your own listings.");

        foreach (var img in property.PropertyImgs)
            _fileStorage.Delete(img.ImgUrl);

        _context.Properties.Remove(property);
        await _context.SaveChangesAsync();
    }

    private IQueryable<Property> BaseQuery() => _context.Properties
        .Include(p => p.User)
        .Include(p => p.PropertyImgs)
        .AsQueryable();

    private async Task EnsureApprovedSellerAsync(int sellerId)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Id == sellerId)
            ?? throw new NotFoundException("User", sellerId);

        if (!profile.IsApproved)
            throw new ForbiddenException("Your seller account is pending admin approval.");
    }

    private static List<TEnum> ParseCsv<TEnum>(string? csv, Func<string, TEnum> parser)
    {
        if (string.IsNullOrWhiteSpace(csv))
            return new List<TEnum>();

        var result = new List<TEnum>();
        foreach (var part in csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            try { result.Add(parser(part)); }
            catch (InvalidOperationException) { /* ignore unknown filter value */ }
        }
        return result;
    }

    private static List<string> ParseJsonArray(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<string>();

        try { return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>(); }
        catch (JsonException) { return new List<string>(); }
    }
}
