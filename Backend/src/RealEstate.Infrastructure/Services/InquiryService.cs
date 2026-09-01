using Microsoft.EntityFrameworkCore;
using RealEstate.Application.DTOs.Inquiries;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Infrastructure.Services;

public class InquiryService : IInquiryService
{
    private readonly IApplicationDbContext _context;

    public InquiryService(IApplicationDbContext context) => _context = context;

    public async Task CreateAsync(int buyerId, CreateInquiryRequest request)
    {
        var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == request.PropertyId)
            ?? throw new NotFoundException(nameof(Property), request.PropertyId);

        if (property.UserId == buyerId)
            throw new InvalidOperationException("You cannot send an inquiry on your own listing.");

        if (string.IsNullOrWhiteSpace(request.Message))
            throw new InvalidOperationException("Message is required.");

        _context.Inquiries.Add(new Inquiry
        {
            PropertyId = request.PropertyId,
            BuyerId = buyerId,
            Message = request.Message.Trim()
        });

        await _context.SaveChangesAsync();
    }

    public async Task<InquiryListResponse> GetMineAsync(int buyerId)
    {
        var inquiries = await BaseQuery()
            .Where(i => i.BuyerId == buyerId)
            .ToListAsync();

        return new InquiryListResponse(inquiries.Select(MapInquiry).ToList());
    }

    public async Task<InquiryListResponse> GetForSellerAsync(int sellerId)
    {
        var inquiries = await BaseQuery()
            .Where(i => i.Property.UserId == sellerId)
            .ToListAsync();

        return new InquiryListResponse(inquiries.Select(MapInquiry).ToList());
    }

    public async Task MarkReadAsync(int sellerId, int inquiryId)
    {
        var inquiry = await _context.Inquiries
            .Include(i => i.Property)
            .FirstOrDefaultAsync(i => i.Id == inquiryId)
            ?? throw new NotFoundException(nameof(Inquiry), inquiryId);

        if (inquiry.Property.UserId != sellerId)
            throw new ForbiddenException("You can only update inquiries on your own listings.");

        inquiry.IsRead = true;
        await _context.SaveChangesAsync();
    }

    public async Task<AdminInquiryListResponse> GetAllAsync()
    {
        var inquiries = await BaseQuery().ToListAsync();
        return new AdminInquiryListResponse(true, inquiries.Select(MapInquiry).ToList());
    }

    private IQueryable<Inquiry> BaseQuery() => _context.Inquiries
        .Include(i => i.Buyer)
        .Include(i => i.Property).ThenInclude(p => p.User)
        .OrderByDescending(i => i.CreatedAtUtc);

    private static InquiryDto MapInquiry(Inquiry i) => new(
        i.Id,
        new InquiryPropertyDto(i.Property.Id, i.Property.Title),
        new InquiryUserDto(i.Buyer.Id, $"{i.Buyer.FirstName} {i.Buyer.LastName}".Trim(), i.Buyer.Email, i.Buyer.Phone),
        new InquiryUserDto(
            i.Property.User.Id,
            $"{i.Property.User.FirstName} {i.Property.User.LastName}".Trim(),
            i.Property.User.Email,
            i.Property.User.Phone),
        i.Message,
        i.IsRead,
        i.IsRead ? "read" : "new",
        i.CreatedAtUtc);
}
