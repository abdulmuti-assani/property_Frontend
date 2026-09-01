using Microsoft.EntityFrameworkCore;
using RealEstate.Application.DTOs.Properties;
using RealEstate.Application.DTOs.Wishlist;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Infrastructure.Services;

public class WishlistService : IWishlistService
{
    private readonly IApplicationDbContext _context;

    public WishlistService(IApplicationDbContext context) => _context = context;

    public async Task<List<WishlistItemDto>> GetAsync(int userId)
    {
        var favorites = await _context.Favorites
            .Where(f => f.UserId == userId)
            .Include(f => f.Property).ThenInclude(p => p.User)
            .Include(f => f.Property).ThenInclude(p => p.PropertyImgs)
            .OrderByDescending(f => f.CreatedAtUtc)
            .ToListAsync();

        return favorites
            .Select(f => new WishlistItemDto(f.Id, f.Property?.ToDto()))
            .ToList();
    }

    public async Task AddAsync(int userId, int propertyId)
    {
        var propertyExists = await _context.Properties.AnyAsync(p => p.Id == propertyId);
        if (!propertyExists)
            throw new NotFoundException(nameof(Property), propertyId);

        var already = await _context.Favorites.AnyAsync(f => f.UserId == userId && f.PropertyId == propertyId);
        if (already)
            return;

        _context.Favorites.Add(new Favorite { UserId = userId, PropertyId = propertyId });
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(int userId, int propertyId)
    {
        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.PropertyId == propertyId);

        if (favorite is null)
            return;

        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync();
    }
}
