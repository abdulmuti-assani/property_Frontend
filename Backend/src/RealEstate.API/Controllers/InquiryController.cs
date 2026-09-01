using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.Common.Models;
using RealEstate.Application.DTOs.Inquiries;
using RealEstate.Application.Interfaces;
using RealEstate.WebApi.Common;

namespace RealEstate.WebApi.Controllers;

[ApiController]
[Route("api/inquiry")]
[Authorize]
public class InquiryController : ControllerBase
{
    private readonly IInquiryService _inquiryService;

    public InquiryController(IInquiryService inquiryService) => _inquiryService = inquiryService;

    [Authorize(Roles = "Buyer")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateInquiryRequest request)
    {
        await _inquiryService.CreateAsync(User.GetUserId(), request);
        return Ok(new SuccessResponse());
    }

    [Authorize(Roles = "Buyer")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMine()
        => Ok(await _inquiryService.GetMineAsync(User.GetUserId()));

    [Authorize(Roles = "Seller")]
    [HttpGet("seller")]
    public async Task<IActionResult> GetForSeller()
        => Ok(await _inquiryService.GetForSellerAsync(User.GetUserId()));

    [Authorize(Roles = "Seller")]
    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        await _inquiryService.MarkReadAsync(User.GetUserId(), id);
        return Ok(new SuccessResponse());
    }
}
