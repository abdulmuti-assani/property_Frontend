using RealEstate.Domain.Common;

namespace RealEstate.Domain.Entities;

public class ContactMessage : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    public ContactMessage() { }
}
