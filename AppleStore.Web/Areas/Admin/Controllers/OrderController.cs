using AppleStore.Web.Data;
using AppleStore.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppleStore.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class OrderController(ApplicationDbContext db, ILogger<OrderController> logger) : Controller
{
    public async Task<IActionResult> Index(string? search, OrderStatus? status, PaymentStatus? paymentStatus) => View(await db.Orders.Include(x => x.User).Where(x => (string.IsNullOrWhiteSpace(search) || x.OrderCode.Contains(search) || x.CustomerName.Contains(search) || x.Email.Contains(search)) && (!status.HasValue || x.OrderStatus == status) && (!paymentStatus.HasValue || x.PaymentStatus == paymentStatus)).OrderByDescending(x => x.CreatedDate).ToListAsync());
    public async Task<IActionResult> Details(int id) { var order = await db.Orders.Include(x => x.User).Include(x => x.OrderDetails).Include(x => x.Payments.OrderByDescending(p => p.CreatedDate)).FirstOrDefaultAsync(x => x.Id == id); return order is null ? NotFound() : View(order); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
    {
        var order = await db.Orders.Include(x => x.OrderDetails).Include(x => x.Payments).FirstOrDefaultAsync(x => x.Id == id); if (order is null) return NotFound();
        var allowed = order.OrderStatus switch { OrderStatus.Pending => status is OrderStatus.Confirmed or OrderStatus.Cancelled, OrderStatus.Confirmed => status is OrderStatus.Shipping or OrderStatus.Cancelled, OrderStatus.Shipping => status == OrderStatus.Completed, _ => false };
        if (!allowed && status != order.OrderStatus) { TempData["Error"] = "Trạng thái chuyển đổi không hợp lệ."; return RedirectToAction(nameof(Details), new { id }); }
        if (status == OrderStatus.Cancelled && order.StockReserved && !order.StockReleased) { foreach (var detail in order.OrderDetails) { var variant = await db.ProductVariants.FindAsync(detail.ProductVariantId); if (variant is not null) variant.StockQuantity += detail.Quantity; } order.StockReleased = true; order.StockReserved = false; if (order.PaymentStatus == PaymentStatus.Pending) order.PaymentStatus = PaymentStatus.Cancelled; }
        if (status == OrderStatus.Completed)
        {
            var now = DateTime.UtcNow;
            order.PaymentStatus = PaymentStatus.Paid;
            order.PaymentDate ??= now;
            var payment = order.Payments.OrderByDescending(x => x.CreatedDate).FirstOrDefault();
            if (payment is not null)
            {
                payment.Status = PaymentStatus.Paid;
                payment.PaymentDate ??= now;
                payment.ResponseCode ??= "00";
                payment.TransactionStatus ??= "00";
            }
        }
        order.OrderStatus = status; order.UpdatedDate = DateTime.UtcNow; await db.SaveChangesAsync(); logger.LogInformation("Order {OrderCode} status changed to {Status}", order.OrderCode, status); return RedirectToAction(nameof(Details), new { id });
    }
}
