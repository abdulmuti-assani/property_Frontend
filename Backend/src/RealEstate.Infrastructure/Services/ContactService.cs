using Microsoft.EntityFrameworkCore;
using RealEstate.Application.DTOs.Contact;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Services;

public class ContactService : IContactService
{
    private readonly IApplicationDbContext _context;

    public ContactService(IApplicationDbContext context) => _context = context;

    public async Task CreateAsync(CreateContactRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Message))
            throw new InvalidOperationException("Name, email and message are required.");

        _context.ContactMessages.Add(new ContactMessage
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            Message = request.Message.Trim(),
            Role = string.IsNullOrWhiteSpace(request.Role) ? "buyer" : request.Role.Trim().ToLowerInvariant()
        });

        await _context.SaveChangesAsync();
    }

    public async Task<ContactListResponse> GetAllAsync()
    {
        var messages = await _context.ContactMessages
            .OrderByDescending(c => c.CreatedAtUtc)
            .Select(c => new ContactDto(c.Id, c.Name, c.Email, c.Phone, c.Message, c.Role, c.CreatedAtUtc))
            .ToListAsync();

        return new ContactListResponse(true, messages);
    }
}
