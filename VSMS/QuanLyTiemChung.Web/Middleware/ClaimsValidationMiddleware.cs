using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using QuanLyTiemChung.Web.Models;

namespace QuanLyTiemChung.Web
{
    public class ClaimsValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ClaimsValidationMiddleware> _logger;

        public ClaimsValidationMiddleware(RequestDelegate next, ILogger<ClaimsValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<User> userManager)
        {
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        var currentRoles = await userManager.GetRolesAsync(user);
                        var claimRoles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

                        // Check if roles in claims match database
                        if (!currentRoles.SequenceEqual(claimRoles.OrderBy(x => x)))
                        {
                            _logger.LogWarning("Role mismatch detected for user {UserId}. DB roles: {DbRoles}, Claim roles: {ClaimRoles}",
                                userId, string.Join(",", currentRoles), string.Join(",", claimRoles));

                            // Force logout by clearing authentication
                            await context.SignOutAsync(IdentityConstants.ApplicationScheme);
                            context.Response.Redirect("/Account/Login?returnUrl=" + context.Request.Path);
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}