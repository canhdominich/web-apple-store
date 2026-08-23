using AppleStore.Web.Data;
using AppleStore.Web.Infrastructure;
using AppleStore.Web.Models.Entities;
using AppleStore.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace AppleStore.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class CustomerController(ApplicationDbContext db, UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index(string? search, string status = "", int page = 1, int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = pageSize is 25 or 50 ? pageSize : 10;
        var customers = CustomerQuery();
        var totalCustomers = await customers.CountAsync();
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var activeCustomers = await customers.CountAsync(x => x.IsActive);
        var lockedCustomers = await customers.CountAsync(x => !x.IsActive);
        var newCustomers = await customers.CountAsync(x => x.CreatedDate >= monthStart);

        if (!string.IsNullOrWhiteSpace(search))
            customers = customers.Where(x => x.Email!.Contains(search) || x.FullName.Contains(search) || (x.PhoneNumber != null && x.PhoneNumber.Contains(search)));
        customers = status switch
        {
            "active" => customers.Where(x => x.IsActive),
            "inactive" => customers.Where(x => !x.IsActive),
            _ => customers
        };

        var totalItems = await customers.CountAsync();
        var users = await customers.OrderByDescending(x => x.CreatedDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var model = new CustomerListViewModel
        {
            Users = users,
            Search = search,
            Status = status,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            Stats =
            [
                new() { Label = "Tổng khách hàng", Value = totalCustomers.ToString("N0"), Icon = "bi-people", Tone = "primary" },
                new() { Label = "Khách mới tháng này", Value = newCustomers.ToString("N0"), Icon = "bi-person-plus", Tone = "success" },
                new() { Label = "Đang hoạt động", Value = activeCustomers.ToString("N0"), Icon = "bi-check-circle", Tone = "success-strong" },
                new() { Label = "Đã khóa", Value = lockedCustomers.ToString("N0"), Icon = "bi-lock", Tone = "danger" }
            ]
        };
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") return PartialView("_CustomerTable", model);
        return View(model);
    }

    public async Task<IActionResult> ExportCsv(string? search, string status = "")
    {
        var customers = CustomerQuery();
        if (!string.IsNullOrWhiteSpace(search))
            customers = customers.Where(x => x.Email!.Contains(search) || x.FullName.Contains(search) || (x.PhoneNumber != null && x.PhoneNumber.Contains(search)));
        customers = status switch
        {
            "active" => customers.Where(x => x.IsActive),
            "inactive" => customers.Where(x => !x.IsActive),
            _ => customers
        };
        var users = await customers.OrderByDescending(x => x.CreatedDate).ToListAsync();
        var csv = new StringBuilder();
        csv.AppendLine("Ho va ten,Email,So dien thoai,Ngay tham gia,Trang thai");
        foreach (var user in users)
            csv.AppendLine($"\"{user.FullName}\",\"{user.Email}\",\"{user.PhoneNumber}\",\"{user.CreatedDate.ToVietnamTime():dd/MM/yyyy}\",\"{(user.IsActive ? "Hoat dong" : "Da khoa")}\"");
        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray(), "text/csv", "khach-hang.csv");
    }

    public async Task<IActionResult> Details(string id) { var user = await db.Users.Include(x => x.Orders).ThenInclude(x => x.OrderDetails).FirstOrDefaultAsync(x => x.Id == id); return user is null ? NotFound() : View(user); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Toggle(string id, string? returnUrl) { var user = await userManager.FindByIdAsync(id); if (user is null) return NotFound(); user.IsActive = !user.IsActive; await userManager.UpdateAsync(user); return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction(nameof(Index)); }

    private IQueryable<ApplicationUser> CustomerQuery()
    {
        var adminRoleIds = db.Roles.Where(x => x.Name == "Admin").Select(x => x.Id);
        var adminUserIds = db.UserRoles.Where(x => adminRoleIds.Contains(x.RoleId)).Select(x => x.UserId);
        return db.Users.AsNoTracking().Where(x => !adminUserIds.Contains(x.Id));
    }
}
