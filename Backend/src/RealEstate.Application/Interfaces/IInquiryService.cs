using RealEstate.Application.DTOs.Inquiries;

namespace RealEstate.Application.Interfaces
{
    public interface IInquiryService
    {
        Task CreateAsync(int buyerId, CreateInquiryRequest request);
        Task<InquiryListResponse> GetMineAsync(int buyerId);
        Task<InquiryListResponse> GetForSellerAsync(int sellerId);
        Task MarkReadAsync(int sellerId, int inquiryId);
        Task<AdminInquiryListResponse> GetAllAsync();
    }
}
