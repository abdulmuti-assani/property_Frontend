using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstate.Application.DTOs.Admin;
using RealEstate.Application.DTOs.Properties;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;
using RealEstate.Domain.Exceptions;
using RealEstate.Infrastructure.Identity;

namespace RealEstate.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileStorageService _fileStorage;

    public AdminService(
        IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IFileStorageService fileStorage)
    {
        _context = context;
        _userManager = userManager;
        _fileStorage = fileStorage;
    }

    public async Task<AdminStatsResponse> GetStatsAsync()
    {
        var stats = new AdminStats(
            await _context.UserProfiles.CountAsync(),
            await _context.Properties.CountAsync(),
            await _context.Properties.CountAsync(p => p.Status != PropertyStatus.Sold),
            await _context.Properties.CountAsync(p => p.Status == PropertyStatus.Sold));

        return new AdminStatsResponse(true, stats);
    }

    public async Task<AdminUsersResponse> GetUsersAsync()
    {
        var users = await _context.UserProfiles
            .OrderByDescending(u => u.CreatedAtUtc)
            .ToListAsync();

        return new AdminUsersResponse(true, users.Select(u => u.ToDto()).ToList());
    }

    public async Task<BlockUserResponse> ToggleBlockAsync(int userId)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException("User", userId);

        if (profile.Role == "Admin")
            throw new InvalidOperationException("Admin accounts cannot be blocked.");

        profile.IsBlocked = !profile.IsBlocked;
        await _context.SaveChangesAsync();

        return new BlockUserResponse(true, profile.IsBlocked);
    }

    public async Task DeleteUserAsync(int userId)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException("User", userId);

        if (profile.Role == "Admin")
            throw new InvalidOperationException("Admin accounts cannot be deleted.");

        var ownedImages = await _context.PropertyImages
            .Where(img => img.Property.UserId == userId)
            .Select(img => img.ImgUrl)
            .ToListAsync();

        foreach (var url in ownedImages)
            _fileStorage.Delete(url);

        _fileStorage.Delete(profile.ProfilePicUrl);

        var favorites = await _context.Favorites.Where(f => f.UserId == userId).ToListAsync();
        _context.Favorites.RemoveRange(favorites);

        var inquiries = await _context.Inquiries.Where(i => i.BuyerId == userId).ToListAsync();
        _context.Inquiries.RemoveRange(inquiries);

        await _context.SaveChangesAsync();

        _context.UserProfiles.Remove(profile);
        await _context.SaveChangesAsync();

        var identityUser = await _userManager.FindByIdAsync(userId.ToString());
        if (identityUser is not null)
            await _userManager.DeleteAsync(identityUser);
    }

    public async Task<List<PropertyDto>> GetPropertiesAsync()
    {
        var properties = await _context.Properties
            .Include(p => p.User)
            .Include(p => p.PropertyImgs)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync();

        return properties.Select(p => p.ToDto()).ToList();
    }

    public async Task DeletePropertyAsync(int propertyId)
    {
        var property = await _context.Properties
            .Include(p => p.PropertyImgs)
            .FirstOrDefaultAsync(p => p.Id == propertyId)
            ?? throw new NotFoundException(nameof(Property), propertyId);

        foreach (var img in property.PropertyImgs)
            _fileStorage.Delete(img.ImgUrl);

        _context.Properties.Remove(property);
        await _context.SaveChangesAsync();
    }

    public async Task<PendingSellersResponse> GetPendingSellersAsync()
    {
        var pending = await _context.UserProfiles
            .Where(u => u.Role == "Seller" && !u.IsApproved)
            .OrderByDescending(u => u.CreatedAtUtc)
            .ToListAsync();

        return new PendingSellersResponse(true, pending.Select(u => u.ToDto()).ToList());
    }

    public async Task ApproveSellerAsync(int userId)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException("User", userId);

        if (profile.Role != "Seller")
            throw new InvalidOperationException("This user is not a seller.");

        profile.IsApproved = true;
        await _context.SaveChangesAsync();
    }
}
