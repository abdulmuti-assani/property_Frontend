using RealEstate.Application.Common.Models;
using RealEstate.Application.DTOs.Auth;
using RealEstate.Application.DTOs.Users;

namespace RealEstate.Application.Interfaces
{
    public interface IAuthService
    {
        Task<MessageResponse> RegisterAsync(RegisterRequest request);
        Task<AuthLoginResponse> LoginAsync(LoginRequest request);
        Task<UserDto> GetCurrentUserAsync(int userId);
        Task LogoutAsync(int userId);
        Task<AuthLoginResponse> RefreshTokenAsync(string refreshToken);
    }
}
