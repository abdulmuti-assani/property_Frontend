using RealEstate.Application.DTOs.Users;

namespace RealEstate.Application.DTOs.Admin;

public record AdminStats(int TotalUsers, int TotalProperties, int ActiveListings, int SoldProperties);
public record AdminStatsResponse(bool Success, AdminStats Stats);

public record AdminUsersResponse(bool Success, List<UserDto> Users);
public record BlockUserResponse(bool Success, bool IsBlocked);

public record PendingSellersResponse(bool Success, List<UserDto> PendingSellers);
