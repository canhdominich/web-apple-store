using System.Security.Claims;
using AppleStore.Web.Services.Interfaces;

namespace AppleStore.Web.Services.Implementations;

public class CartSessionService(IHttpContextAccessor httpContextAccessor) : ICartSessionService
{
    private const string GuestCartCookie = "AppleStore.GuestCart";

    public string GetCartKey()
    {
        var context = httpContextAccessor.HttpContext;
        if (context is null) return string.Empty;
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId)) return userId;
        return context.Request.Cookies.TryGetValue(GuestCartCookie, out var guestKey) ? guestKey : string.Empty;
    }

    public string EnsureCartKey()
    {
        var context = httpContextAccessor.HttpContext;
        if (context is null) return string.Empty;
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId)) return userId;
        if (context.Request.Cookies.TryGetValue(GuestCartCookie, out var guestKey) && !string.IsNullOrWhiteSpace(guestKey)) return guestKey;
        guestKey = $"guest:{Guid.NewGuid():N}";
        context.Response.Cookies.Append(GuestCartCookie, guestKey, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = context.Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });
        return guestKey;
    }

    public string? GetGuestCartKey()
    {
        var context = httpContextAccessor.HttpContext;
        if (context?.Request.Cookies.TryGetValue(GuestCartCookie, out var guestKey) == true && guestKey.StartsWith("guest:", StringComparison.Ordinal))
            return guestKey;
        return null;
    }

    public void ClearGuestCart()
    {
        httpContextAccessor.HttpContext?.Response.Cookies.Delete(GuestCartCookie);
    }
}
