namespace RealEstate.Application.DTOs.Contact;

public record CreateContactRequest(string Name, string Email, string? Phone, string Message, string Role);

public record ContactDto(
    int Id,
    string Name,
    string Email,
    string? Phone,
    string Message,
    string Role,
    DateTimeOffset CreatedAt);

public record ContactListResponse(bool Success, List<ContactDto> Contacts);
