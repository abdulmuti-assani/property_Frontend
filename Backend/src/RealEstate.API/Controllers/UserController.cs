using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.DTOs.Users;
using RealEstate.Application.Interfaces;
using RealEstate.WebApi.Common;

namespace RealEstate.WebApi.Controllers;

[ApiController]
[Route("api/user")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService) => _userService = userService;

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileRequest request)
        => Ok(await _userService.UpdateProfileAsync(User.GetUserId(), request));
}
