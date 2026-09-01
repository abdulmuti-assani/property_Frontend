using RealEstate.Application.DTOs.Contact;

namespace RealEstate.Application.Interfaces
{
    public interface IContactService
    {
        Task CreateAsync(CreateContactRequest request);
        Task<ContactListResponse> GetAllAsync();
    }
}
