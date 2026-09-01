using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RealEstate.Application.Common.Models;
using RealEstate.Application.Common.Settings;
using RealEstate.Application.DTOs.Auth;
using RealEstate.Application.DTOs.Users;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Exceptions;
using RealEstate.Infrastructure.Identity;

namespace RealEstate.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IApplicationDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IApplicationDbContext context,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _context = context;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<MessageResponse> RegisterAsync(RegisterRequest request)
    {
        var roleName = (request.Role?.Trim().ToLowerInvariant()) switch
        {
            "buyer" => "Buyer",
            "seller" => "Seller",
            _ => throw new InvalidOperationException("Role must be either 'buyer' or 'seller'.")
        };

        if (await _userManager.FindByEmailAsync(request.Email) is not null)
            throw new InvalidOperationException("Email is already registered.");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var identityUser = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(identityUser, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        var roleResult = await _userManager.AddToRoleAsync(identityUser, roleName);
        if (!roleResult.Succeeded)
            throw new InvalidOperationException(string.Join(", ", roleResult.Errors.Select(e => e.Description)));

        var (firstName, lastName) = UserMappings.SplitName(request.Name);

        _context.UserProfiles.Add(new User
        {
            Id = identityUser.Id, // shared PK
            FirstName = firstName,
            LastName = lastName,
            Email = request.Email,
            Role = roleName,
            IsApproved = roleName != "Seller"
        });

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return new MessageResponse("Registration successful. You can now sign in.");
    }

    public async Task<AuthLoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Id == user.Id)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (profile.IsBlocked)
            throw new ForbiddenException("Your account has been blocked.");

        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
            .ToListAsync();

        foreach (var rt in activeTokens)
            rt.RevokedAt = DateTime.UtcNow;

        var roles = await _userManager.GetRolesAsync(user);
        var token = await IssueAccessTokenAsync(user.Id, user.Email!, roles);

        return new AuthLoginResponse(token, profile.ToDto());
    }

    public async Task<UserDto> GetCurrentUserAsync(int userId)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException("User", userId);

        return profile.ToDto();
    }

    public async Task LogoutAsync(int userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync();

        foreach (var rt in tokens)
            rt.RevokedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<AuthLoginResponse> RefreshTokenAsync(string refreshToken)
    {
        var incomingHash = _tokenService.HashToken(refreshToken);

        var stored = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == incomingHash);

        if (stored is null || !stored.IsActive)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var user = await _userManager.FindByIdAsync(stored.UserId.ToString())
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Id == user.Id)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (profile.IsBlocked)
            throw new ForbiddenException("Your account has been blocked.");

        stored.RevokedAt = DateTime.UtcNow; // rotation

        var roles = await _userManager.GetRolesAsync(user);
        var token = await IssueAccessTokenAsync(user.Id, user.Email!, roles);

        return new AuthLoginResponse(token, profile.ToDto());
    }

    private async Task<string> IssueAccessTokenAsync(int userId, string email, IEnumerable<string> roles)
    {
        var accessTokenResult = _tokenService.GenerateAccessToken(userId, email, roles.ToList());
        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = _tokenService.HashToken(refreshTokenValue);

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        });

        await _context.SaveChangesAsync();

        return accessTokenResult.Token;
    }
}
