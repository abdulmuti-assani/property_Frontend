using RealEstate.Application.DTOs.Properties;

namespace RealEstate.Application.DTOs.Wishlist;

public record WishlistItemDto(int Id, PropertyDto? Property);
