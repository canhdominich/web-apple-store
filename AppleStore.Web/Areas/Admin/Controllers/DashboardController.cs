using AppleStore.Web.Data;
using AppleStore.Web.Infrastructure;
using AppleStore.Web.Models.Entities;
using AppleStore.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppleStore.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class DashboardController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var today = DateTime.UtcNow.ToVietnamTime().Date;
        var chartStartVietnam = today.AddDays(-6);
        var chartStartUtc = DateTime.SpecifyKind(chartStartVietnam.AddHours(-7), DateTimeKind.Utc);
        var tomorrowUtc = DateTime.SpecifyKind(today.AddDays(1).AddHours(-7), DateTimeKind.Utc);
        var paidOrders = await db.Orders
            .AsNoTracking()
            .Where(x => x.PaymentStatus == PaymentStatus.Paid && x.CreatedDate >= chartStartUtc && x.CreatedDate < tomorrowUtc)
            .Select(x => new { x.CreatedDate, x.TotalAmount })
            .ToListAsync();
        var points = paidOrders
            .GroupBy(x => x.CreatedDate.ToVietnamTime().Date)
            .Select(x => new { Date = x.Key, Value = x.Sum(o => o.TotalAmount) })
            .ToList();
        var todayStartUtc = DateTime.SpecifyKind(today.AddHours(-7), DateTimeKind.Utc);
        var model = new DashboardViewModel { TotalProducts = await db.Products.CountAsync(x => x.IsActive), TotalCustomers = await db.Users.CountAsync(x => x.IsActive && !db.UserRoles.Any(ur => ur.UserId == x.Id && db.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Admin"))), TotalOrders = await db.Orders.CountAsync(), OrdersToday = await db.Orders.CountAsync(x => x.CreatedDate >= todayStartUtc && x.CreatedDate < tomorrowUtc), PendingOrders = await db.Orders.CountAsync(x => x.OrderStatus == OrderStatus.Pending), LowStock = await db.ProductVariants.CountAsync(x => x.IsActive && x.StockQuantity <= 5), Revenue = await db.Orders.Where(x => x.PaymentStatus == PaymentStatus.Paid).SumAsync(x => (decimal?)x.TotalAmount) ?? 0, RevenuePoints = Enumerable.Range(0, 7).Select(i => { var date = today.AddDays(-6 + i); return new RevenuePointViewModel { Label = date.ToString("dd/MM"), Value = points.FirstOrDefault(p => p.Date == date)?.Value ?? 0 }; }).ToList() };
        model.TopProducts = await db.OrderDetails.Where(x => x.Order!.PaymentStatus == PaymentStatus.Paid).GroupBy(x => x.ProductName).OrderByDescending(x => x.Sum(d => d.Quantity)).Take(5).Select(x => new TopProductViewModel { Name = x.Key, Quantity = x.Sum(d => d.Quantity) }).ToListAsync();
        return View(model);
    }
}
