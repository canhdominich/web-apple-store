using AppleStore.Web.Models.Entities;
using AppleStore.Web.Models.ViewModels;
using AppleStore.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AppleStore.Web.Controllers;

public class AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ICartService cartService, ICartSessionService cartSession, IWebHostEnvironment environment) : BaseController
{
    [HttpGet] public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is null || !user.IsActive) { ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng."); return View(model); }
        var guestCartKey = cartSession.GetGuestCartKey();
        var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(guestCartKey)) await cartService.MergeAsync(guestCartKey, user.Id);
            cartSession.ClearGuestCart();
            return LocalRedirect(model.ReturnUrl ?? Url.Action("Index", "Home")!);
        }
        ModelState.AddModelError(string.Empty, result.IsLockedOut ? "Tài khoản tạm thời bị khóa." : "Email hoặc mật khẩu không đúng."); return View(model);
    }
    [HttpGet] public IActionResult Register() => View(new RegisterViewModel());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var guestCartKey = cartSession.GetGuestCartKey();
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName, PhoneNumber = model.Phone, Address = model.Address, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Customer");
            if (!string.IsNullOrWhiteSpace(guestCartKey)) await cartService.MergeAsync(guestCartKey, user.Id);
            cartSession.ClearGuestCart();
            await signInManager.SignInAsync(user, false);
            return RedirectToAction("Index", "Home");
        }
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description); return View(model);
    }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Logout() { await signInManager.SignOutAsync(); return RedirectToAction("Index", "Home"); }
    [HttpGet] public IActionResult ForgotPassword() => View();
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is not null && user.IsActive)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var link = Url.Action(nameof(ResetPassword), "Account", new { email = user.Email, token }, Request.Scheme);
            if (environment.IsDevelopment()) TempData["ResetLink"] = link;
        }
        TempData["Success"] = "Nếu email tồn tại, hướng dẫn đặt lại mật khẩu đã được tạo. Hãy cấu hình email provider để gửi link trong production.";
        return RedirectToAction(nameof(ForgotPassword));
    }
    [HttpGet] public IActionResult ResetPassword(string email, string token) => View(new ResetPasswordViewModel { Email = email, Token = token });
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is null) { ModelState.AddModelError(string.Empty, "Link đặt lại mật khẩu không hợp lệ."); return View(model); }
        var result = await userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (result.Succeeded) { TempData["Success"] = "Đặt lại mật khẩu thành công."; return RedirectToAction(nameof(Login)); }
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description); return View(model);
    }
    [Authorize] public async Task<IActionResult> Profile() { var user = await userManager.GetUserAsync(User); return user is null ? Challenge() : View(user); }
    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(string fullName, string phoneNumber, string address)
    {
        var user = await userManager.GetUserAsync(User); if (user is null) return Challenge();
        user.FullName = fullName.Trim(); user.PhoneNumber = phoneNumber.Trim(); user.Address = address.Trim(); await userManager.UpdateAsync(user); TempData["Success"] = "Đã cập nhật thông tin cá nhân."; return RedirectToAction(nameof(Profile));
    }
    [Authorize] public IActionResult ChangePassword() => View(new ChangePasswordViewModel());
    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model); var user = await userManager.GetUserAsync(User); if (user is null) return Challenge();
        var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (result.Succeeded) { TempData["Success"] = "Đổi mật khẩu thành công."; return RedirectToAction(nameof(Profile)); }
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description); return View(model);
    }
    public IActionResult AccessDenied() => View();
}
