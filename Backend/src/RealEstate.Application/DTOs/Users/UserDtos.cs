using Microsoft.AspNetCore.Http;

namespace RealEstate.Application.DTOs.Users;

public record UserDto(
    int Id,
    string Name,
    string Email,
    string? Phone,
    string? Address,
    string Role,
    string? ProfilePic,
    bool IsApproved,
    bool IsBlocked);

public record UserEnvelope(bool Success, UserDto User);

public class UpdateProfileRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public IFormFile? ProfilePic { get; set; }
    public bool RemoveProfilePic { get; set; }
}
