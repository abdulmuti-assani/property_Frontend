using System.Security.Claims;
using RealEstate.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace RealEstate.WebApi.Middleware;

/// <summary>
/// Rejects any authenticated request from a blocked user with 403 + a "blocked" message,
/// which the frontend axios interceptor uses to force a client-side logout.
/// </summary>
public class BlockedUserMiddleware
{
    private readonly RequestDelegate _next;

    public BlockedUserMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IApplicationDbContext dbContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var idValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(idValue, out var userId))
            {
                var isBlocked = await dbContext.UserProfiles
                    .Where(u => u.Id == userId)
                    .Select(u => (bool?)u.IsBlocked)
                    .FirstOrDefaultAsync();

                if (isBlocked == true)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new { message = "Your account has been blocked." });
                    return;
                }
            }
        }

        await _next(context);
    }
}
