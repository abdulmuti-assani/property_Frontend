using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.Common.Models;
using RealEstate.Application.Interfaces;
using RealEstate.WebApi.Common;

namespace RealEstate.WebApi.Controllers;

[ApiController]
[Route("api/wishlist")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService) => _wishlistService = wishlistService;

    [HttpGet]
    public async Task<IActionResult> Get()
        => Ok(await _wishlistService.GetAsync(User.GetUserId()));

    [HttpPost("{propertyId:int}")]
    public async Task<IActionResult> Add(int propertyId)
    {
        await _wishlistService.AddAsync(User.GetUserId(), propertyId);
        return Ok(new SuccessResponse());
    }

    [HttpDelete("{propertyId:int}")]
    public async Task<IActionResult> Remove(int propertyId)
    {
        await _wishlistService.RemoveAsync(User.GetUserId(), propertyId);
        return Ok(new SuccessResponse());
    }
}
