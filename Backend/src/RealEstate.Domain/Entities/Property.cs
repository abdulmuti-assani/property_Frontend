using RealEstate.Domain.Common;
using RealEstate.Domain.Enums;

namespace RealEstate.Domain.Entities;

public class Property : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PropertyType PropertyType { get; set; }
    public PropertyStatus Status { get; set; }
    public Furnishing Furnishing { get; set; }

    public string City { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string? Pincode { get; set; }

    public decimal Price { get; set; }
    public int Bhk { get; set; }
    public int Bathrooms { get; set; }
    public int AreaSize { get; set; }
    public decimal SecurityDeposit { get; set; }
    public decimal Maintenance { get; set; }
    public int Views { get; set; }

    // New listings stay hidden from public browsing until an admin approves them.
    public bool IsApproved { get; set; }

    public List<string> Amenities { get; set; } = new();

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public Property() { }

    public ICollection<PropertyImg> PropertyImgs { get; set; } = new List<PropertyImg>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
}
