using AppleStore.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppleStore.Web.Controllers;

public class OrderController(IOrderService orders) : BaseController
{
    [Authorize]
    public async Task<IActionResult> Index() => View(await orders.GetHistoryAsync(CurrentUserId));

    [Authorize]
    public async Task<IActionResult> Details(int id) { var order = await orders.GetForUserAsync(id, CurrentUserId); return order is null ? NotFound() : View(order); }

    [AllowAnonymous]
    public async Task<IActionResult> Success(int id, string? code)
    {
        var userId = string.IsNullOrWhiteSpace(CurrentUserId) ? null : CurrentUserId;
        var order = await orders.GetReceiptAsync(id, userId, code);
        return order is null ? NotFound() : View(order);
    }
}
