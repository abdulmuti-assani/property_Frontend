using RealEstate.Application.DTOs.Users;

namespace RealEstate.Application.DTOs.Auth;

public record RegisterRequest(string Name, string Email, string Password, string Role);
public record LoginRequest(string Email, string Password);
public record RefreshTokenRequest(string RefreshToken);
public record AuthLoginResponse(string Token, UserDto User);
public record MeResponse(bool Success, UserDto User);
