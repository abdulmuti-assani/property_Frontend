namespace RealEstate.Application.DTOs.Inquiries;

public record InquiryPropertyDto(int Id, string Title);
public record InquiryUserDto(int Id, string Name, string Email, string? Phone);

public record InquiryDto(
    int Id,
    InquiryPropertyDto Property,
    InquiryUserDto Buyer,
    InquiryUserDto Seller,
    string Message,
    bool IsRead,
    string Status,
    DateTimeOffset CreatedAt);

public record CreateInquiryRequest(int PropertyId, string Message);
public record InquiryListResponse(List<InquiryDto> Inquiries);
public record AdminInquiryListResponse(bool Success, List<InquiryDto> Inquiries);
