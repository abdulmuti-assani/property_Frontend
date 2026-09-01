using RealEstate.Application.DTOs.Properties;

namespace RealEstate.Application.Interfaces
{
    public interface IPropertyService
    {
        Task<List<PropertyDto>> GetAllAsync(PropertyFilterRequest filter);
        Task<PropertyCountsResponse> GetCountsAsync();
        Task<PropertyDetailsResponse> GetByIdAsync(int id);
        Task<List<PropertyDto>> GetMineAsync(int sellerId);
        Task<SellerDashboardResponse> GetSellerDashboardAsync(int sellerId);
        Task<PropertySavedResponse> CreateAsync(int sellerId, CreatePropertyRequest request);
        Task<PropertySavedResponse> UpdateAsync(int sellerId, int id, UpdatePropertyRequest request);
        Task UpdateStatusAsync(int sellerId, int id, UpdateStatusRequest request);
        Task DeleteAsync(int sellerId, int id);
    }
}
