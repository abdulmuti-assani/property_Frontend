using Microsoft.EntityFrameworkCore;
using RealEstate.Application.DTOs.Users;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Infrastructure.Services;

public class UserService : IUserService
{
    private const string ProfilePicSubFolder = "profiles";

    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public UserService(IApplicationDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<UserEnvelope> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException("User", userId);

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var (firstName, lastName) = UserMappings.SplitName(request.Name);
            profile.FirstName = firstName;
            profile.LastName = lastName;
        }

        profile.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        profile.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();

        if (request.RemoveProfilePic)
        {
            _fileStorage.Delete(profile.ProfilePicUrl);
            profile.ProfilePicUrl = null;
        }

        if (request.ProfilePic is not null)
        {
            _fileStorage.Delete(profile.ProfilePicUrl);
            profile.ProfilePicUrl = await _fileStorage.SaveAsync(request.ProfilePic, ProfilePicSubFolder);
        }

        await _context.SaveChangesAsync();

        return new UserEnvelope(true, profile.ToDto());
    }
}
