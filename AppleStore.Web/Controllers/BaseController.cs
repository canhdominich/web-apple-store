using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace AppleStore.Web.Controllers;

public abstract class BaseController : Controller
{
    protected string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    protected string? CurrentUserEmail => User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
}
