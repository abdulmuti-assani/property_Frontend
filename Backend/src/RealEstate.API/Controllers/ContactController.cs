using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.Common.Models;
using RealEstate.Application.DTOs.Contact;
using RealEstate.Application.Interfaces;

namespace RealEstate.WebApi.Controllers;

[ApiController]
[Route("api/contact")]
public class ContactController : ControllerBase
{
    private readonly IContactService _contactService;

    public ContactController(IContactService contactService) => _contactService = contactService;

    [HttpPost]
    public async Task<IActionResult> Create(CreateContactRequest request)
    {
        await _contactService.CreateAsync(request);
        return Ok(new SuccessResponse());
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _contactService.GetAllAsync());
}
