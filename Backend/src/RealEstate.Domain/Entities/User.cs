using RealEstate.Domain.Common;

namespace RealEstate.Domain.Entities;

public class User : AuditableEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // Denormalized from ApplicationUser for read-side queries (kept in sync on register / profile update).
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }

    // "Admin" | "Seller" | "Buyer" (set once at registration).
    public string Role { get; set; } = string.Empty;

    public string? Address { get; set; }
    public string? ProfilePicUrl { get; set; }

    public bool IsApproved { get; set; } = true;
    public bool IsBlocked { get; set; }

    public User() { }

    public ICollection<Property> Properties { get; set; } = new List<Property>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
}
