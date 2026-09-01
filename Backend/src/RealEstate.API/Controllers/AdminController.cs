using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.Common.Models;
using RealEstate.Application.Interfaces;

namespace RealEstate.WebApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IInquiryService _inquiryService;

    public AdminController(IAdminService adminService, IInquiryService inquiryService)
    {
        _adminService = adminService;
        _inquiryService = inquiryService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
        => Ok(await _adminService.GetStatsAsync());

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
        => Ok(await _adminService.GetUsersAsync());

    [HttpPatch("users/{id:int}/block")]
    public async Task<IActionResult> ToggleBlock(int id)
        => Ok(await _adminService.ToggleBlockAsync(id));

    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _adminService.DeleteUserAsync(id);
        return Ok(new SuccessResponse());
    }

    [HttpGet("properties")]
    public async Task<IActionResult> GetProperties()
        => Ok(await _adminService.GetPropertiesAsync());

    [HttpDelete("properties/{id:int}")]
    public async Task<IActionResult> DeleteProperty(int id)
    {
        await _adminService.DeletePropertyAsync(id);
        return Ok(new SuccessResponse());
    }

    [HttpGet("pending-sellers")]
    public async Task<IActionResult> GetPendingSellers()
        => Ok(await _adminService.GetPendingSellersAsync());

    [HttpPatch("approve-seller/{id:int}")]
    public async Task<IActionResult> ApproveSeller(int id)
    {
        await _adminService.ApproveSellerAsync(id);
        return Ok(new SuccessResponse());
    }

    [HttpGet("inquiries")]
    public async Task<IActionResult> GetInquiries()
        => Ok(await _inquiryService.GetAllAsync());
}
