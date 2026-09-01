using RealEstate.Application.DTOs.Users;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Services;

internal static class UserMappings
{
    public static UserDto ToDto(this User profile) => new(
        profile.Id,
        $"{profile.FirstName} {profile.LastName}".Trim(),
        profile.Email,
        profile.Phone,
        profile.Address,
        profile.Role.ToLowerInvariant(),
        profile.ProfilePicUrl,
        profile.IsApproved,
        profile.IsBlocked);

    public static (string FirstName, string LastName) SplitName(string? name)
    {
        var trimmed = (name ?? string.Empty).Trim();
        if (trimmed.Length == 0)
            return ("", "");

        var spaceIndex = trimmed.IndexOf(' ');
        return spaceIndex < 0
            ? (trimmed, "")
            : (trimmed[..spaceIndex], trimmed[(spaceIndex + 1)..].Trim());
    }
}
