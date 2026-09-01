using RealEstate.Application.DTOs.Wishlist;

namespace RealEstate.Application.Interfaces
{
    public interface IWishlistService
    {
        Task<List<WishlistItemDto>> GetAsync(int userId);
        Task AddAsync(int userId, int propertyId);
        Task RemoveAsync(int userId, int propertyId);
    }
}
