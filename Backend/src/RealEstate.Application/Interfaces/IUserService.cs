using RealEstate.Application.DTOs.Users;

namespace RealEstate.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserEnvelope> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    }
}
