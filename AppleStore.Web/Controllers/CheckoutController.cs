using AppleStore.Web.Models.Entities;
using AppleStore.Web.Models.ViewModels;
using AppleStore.Web.Services.Implementations;
using AppleStore.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AppleStore.Web.Controllers;

public class CheckoutController(ICartService cartService, ICartSessionService cartSession, IOrderService orderService, IVnPayService vnPayService, UserManager<ApplicationUser> userManager, ILogger<CheckoutController> logger) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cart = await cartService.GetAsync(cartSession.GetCartKey()); if (cart.Items.Count == 0) return RedirectToAction("Index", "Cart");
        var model = new CheckoutViewModel { Cart = cart, Email = CurrentUserEmail ?? string.Empty };
        await FillUserCheckoutDefaultsAsync(model, overwriteEditableFields: true);
        return View(model);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutViewModel model)
    {
        var cartKey = cartSession.GetCartKey();
        model.Cart = await cartService.GetAsync(cartKey);
        if (model.Cart.Items.Count == 0) { ModelState.AddModelError(string.Empty, "Giỏ hàng đang trống."); return View(model); }
        await FillUserCheckoutDefaultsAsync(model, overwriteEditableFields: false);
        if (!ModelState.IsValid) return View(model);
        var created = await orderService.CreateAsync(cartKey, string.IsNullOrWhiteSpace(CurrentUserId) ? null : CurrentUserId, model);
        if (created.Order is null || created.Payment is null) { ModelState.AddModelError(string.Empty, created.Error ?? "Không thể tạo đơn hàng."); return View(model); }
        if (model.PaymentMethod == PaymentMethod.COD) return RedirectToAction("Success", "Order", new { id = created.Order.Id, code = created.Order.OrderCode });
        try
        {
            var url = vnPayService.CreatePaymentUrl(HttpContext, new VnPayRequestModel { TxnRef = created.Payment.TxnRef, Amount = created.Order.TotalAmount, OrderInfo = $"Thanh toan don hang {created.Order.OrderCode}", IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1" });
            return Redirect(url);
        }
        catch (InvalidOperationException ex) { logger.LogError(ex, "VNPay configuration error for {OrderCode}", created.Order.OrderCode); TempData["Error"] = "VNPay chưa được cấu hình. Đơn hàng đang chờ thanh toán, bạn có thể thử lại sau khi cấu hình xong."; return RedirectToAction("Details", "Order", new { id = created.Order.Id }); }
    }
    [AllowAnonymous]
    public async Task<IActionResult> PaymentCallback()
    {
        var response = vnPayService.ValidateCallback(Request.Query);
        if (!response.IsValidSignature) return View("PaymentFailed", new PaymentResultViewModel { IsSuccess = false, Message = "Không thể xác thực phản hồi thanh toán." });
        var result = await orderService.ProcessVnPayCallbackAsync(response);
        return View(result.IsSuccess ? "PaymentSuccess" : "PaymentFailed", result);
    }

    private async Task FillUserCheckoutDefaultsAsync(CheckoutViewModel model, bool overwriteEditableFields)
    {
        if (User.Identity?.IsAuthenticated != true) return;
        var user = await userManager.GetUserAsync(User);
        if (user is null) return;
        model.HasSavedAddress = !string.IsNullOrWhiteSpace(user.Address);
        model.SavedAddress = user.Address;
        model.Email = user.Email ?? model.Email;
        if (!overwriteEditableFields) return;
        model.CustomerName = user.FullName;
        model.PhoneNumber = user.PhoneNumber ?? string.Empty;
        model.ShippingAddress = user.Address ?? string.Empty;
    }
    [HttpPost]
    public async Task<IActionResult> PayAgain(int id)
    {
        var result = await orderService.RetryPaymentAsync(id, CurrentUserId);
        if (result.Order is null || result.Payment is null) { TempData["Error"] = result.Error ?? "Không thể thanh toán lại."; return RedirectToAction("Details", "Order", new { id }); }
        try { return Redirect(vnPayService.CreatePaymentUrl(HttpContext, new VnPayRequestModel { TxnRef = result.Payment.TxnRef, Amount = result.Order.TotalAmount, OrderInfo = $"Thanh toan lai don hang {result.Order.OrderCode}", IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1" })); }
        catch (InvalidOperationException ex) { logger.LogError(ex, "VNPay retry configuration error"); TempData["Error"] = "VNPay chưa được cấu hình."; return RedirectToAction("Details", "Order", new { id }); }
    }
}
