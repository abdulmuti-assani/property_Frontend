using RealEstate.Application.DTOs.Admin;
using RealEstate.Application.DTOs.Properties;

namespace RealEstate.Application.Interfaces
{
    public interface IAdminService
    {
        Task<AdminStatsResponse> GetStatsAsync();
        Task<AdminUsersResponse> GetUsersAsync();
        Task<BlockUserResponse> ToggleBlockAsync(int userId);
        Task DeleteUserAsync(int userId);
        Task<List<PropertyDto>> GetPropertiesAsync();
        Task DeletePropertyAsync(int propertyId);
        Task<PendingSellersResponse> GetPendingSellersAsync();
        Task ApproveSellerAsync(int userId);
    }
}
