using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.Common.Models;
using RealEstate.Application.DTOs.Properties;
using RealEstate.Application.Interfaces;
using RealEstate.WebApi.Common;

namespace RealEstate.WebApi.Controllers;

[ApiController]
[Route("api/property")]
public class PropertyController : ControllerBase
{
    private readonly IPropertyService _propertyService;

    public PropertyController(IPropertyService propertyService) => _propertyService = propertyService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PropertyFilterRequest filter)
        => Ok(await _propertyService.GetAllAsync(filter));

    [HttpGet("counts")]
    public async Task<IActionResult> GetCounts()
        => Ok(await _propertyService.GetCountsAsync());

    [Authorize(Roles = "Seller")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMine()
        => Ok(await _propertyService.GetMineAsync(User.GetUserId()));

    [Authorize(Roles = "Seller")]
    [HttpGet("seller/dashboard")]
    public async Task<IActionResult> GetSellerDashboard()
        => Ok(await _propertyService.GetSellerDashboardAsync(User.GetUserId()));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _propertyService.GetByIdAsync(id, User.TryGetUserId(), User.IsInRole("Admin")));

    [Authorize(Roles = "Seller")]
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreatePropertyRequest request)
        => Ok(await _propertyService.CreateAsync(User.GetUserId(), request));

    [Authorize(Roles = "Seller")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdatePropertyRequest request)
        => Ok(await _propertyService.UpdateAsync(User.GetUserId(), id, request));

    [Authorize(Roles = "Seller")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateStatusRequest request)
    {
        await _propertyService.UpdateStatusAsync(User.GetUserId(), id, request);
        return Ok(new SuccessResponse());
    }

    [Authorize(Roles = "Seller")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _propertyService.DeleteAsync(User.GetUserId(), id);
        return Ok(new SuccessResponse());
    }
}
