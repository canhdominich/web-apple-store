using AppleStore.Web.Data;
using AppleStore.Web.Infrastructure;
using AppleStore.Web.Models.ViewModels;
using AppleStore.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppleStore.Web.Controllers;

public class CartController(ICartService cart, ICartSessionService cartSession, ApplicationDbContext db) : BaseController
{
    public async Task<IActionResult> Index()
    {
        var model = await cart.GetAsync(cartSession.GetCartKey());
        if (model.Items.Count == 0)
        {
            model.Recommendations = await db.Products
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Variants.Where(v => v.IsActive))
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.IsFeatured)
                .ThenByDescending(x => x.CreatedDate)
                .Take(4)
                .ToListAsync();
        }
        return View(model);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(AddToCartRequest request)
    {
        var cartKey = cartSession.EnsureCartKey();
        var result = await cart.AddAsync(cartKey, request);
        if (Request.Headers.XRequestedWith == "XMLHttpRequest") return Json(new { success = result.Success, message = result.Message, count = await cart.CountAsync(cartKey) });
        if (!result.Success) TempData["Error"] = result.Message; else TempData["Success"] = result.Message;
        return RedirectToAction("Index", "Cart");
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int itemId, int quantity)
    {
        var cartKey = cartSession.GetCartKey();
        var result = await cart.UpdateAsync(cartKey, itemId, quantity);
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            var model = await cart.GetAsync(cartKey);
            var updatedItem = model.Items.FirstOrDefault(x => x.Id == itemId);
            var shippingFee = model.Subtotal >= 1000000 ? 0m : 30000m;
            return Json(new
            {
                success = result.Success,
                message = result.Message,
                count = await cart.CountAsync(cartKey),
                itemId,
                removed = updatedItem is null,
                quantity = updatedItem?.Quantity ?? 0,
                itemTotal = updatedItem is null ? string.Empty : ((updatedItem.ProductVariant?.Price ?? 0) * updatedItem.Quantity).ToMoney(),
                subtotal = model.Subtotal.ToMoney(),
                shipping = shippingFee == 0 ? "Miễn phí" : shippingFee.ToMoney(),
                total = (model.Subtotal + shippingFee).ToMoney()
            });
        }
        if (!result.Success) TempData["Error"] = result.Message; return RedirectToAction(nameof(Index));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int itemId) { await cart.RemoveAsync(cartSession.GetCartKey(), itemId); return RedirectToAction(nameof(Index)); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear() { await cart.ClearAsync(cartSession.GetCartKey()); return RedirectToAction(nameof(Index)); }
}
