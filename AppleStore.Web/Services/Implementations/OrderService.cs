using System.Data;
using System.Globalization;
using System.Text.Json;
using AppleStore.Web.Data;
using AppleStore.Web.Models.Entities;
using AppleStore.Web.Models.ViewModels;
using AppleStore.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppleStore.Web.Services.Implementations;

public class OrderService(ApplicationDbContext db, ILogger<OrderService> logger) : IOrderService
{
    public async Task<(Order? Order, Payment? Payment, string? Error)> CreateAsync(string cartKey, string? userId, CheckoutViewModel model)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<(Order? Order, Payment? Payment, string? Error)>(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var cart = await db.Carts.Include(x => x.Items).ThenInclude(x => x.ProductVariant!).ThenInclude(x => x.Product).FirstOrDefaultAsync(x => x.UserId == cartKey);
            if (cart is null || cart.Items.Count == 0) return (null, null, "Giỏ hàng đang trống.");
            foreach (var item in cart.Items)
                if (item.ProductVariant is null || !item.ProductVariant.IsActive || item.ProductVariant.Product is null || !item.ProductVariant.Product.IsActive || item.Quantity > item.ProductVariant.StockQuantity)
                    return (null, null, "Một sản phẩm trong giỏ đã hết hàng hoặc thay đổi số lượng.");

            var subtotal = cart.Items.Sum(x => x.ProductVariant!.Price * x.Quantity);
            var order = new Order { OrderCode = $"AS{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}", UserId = string.IsNullOrWhiteSpace(userId) ? null : userId, CustomerName = model.CustomerName.Trim(), Email = model.Email.Trim(), PhoneNumber = model.PhoneNumber.Trim(), ShippingAddress = model.ShippingAddress.Trim(), Note = model.Note?.Trim(), PaymentMethod = model.PaymentMethod, Subtotal = subtotal, ShippingFee = subtotal >= 1000000 ? 0m : 30000m, TotalAmount = subtotal + (subtotal >= 1000000 ? 0m : 30000m), StockReserved = true, PaymentStatus = PaymentStatus.Pending, OrderStatus = OrderStatus.Pending };
            foreach (var item in cart.Items)
            {
                var variant = item.ProductVariant!;
                variant.StockQuantity -= item.Quantity;
                order.OrderDetails.Add(new OrderDetail { ProductVariantId = variant.Id, ProductName = variant.Product!.Name, VariantName = $"{variant.Color} / {variant.Storage}", SKU = variant.SKU, Quantity = item.Quantity, UnitPrice = variant.Price, TotalPrice = variant.Price * item.Quantity });
            }
            db.Orders.Add(order);
            db.CartItems.RemoveRange(cart.Items);
            if (model.PaymentMethod == PaymentMethod.VNPay) order.Payments.Add(new Payment { Provider = "VNPay", TxnRef = CreateTxnRef(order.OrderCode), Amount = order.TotalAmount });
            else order.Payments.Add(new Payment { Provider = "COD", TxnRef = $"COD-{order.OrderCode}", Amount = order.TotalAmount });
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return (order, order.Payments.Single(), null);
        });
    }

    public Task<Order?> GetForUserAsync(int id, string userId) => db.Orders.AsNoTracking().Include(x => x.OrderDetails).Include(x => x.Payments.OrderByDescending(p => p.CreatedDate)).FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
    public Task<Order?> GetReceiptAsync(int id, string? userId, string? orderCode) => db.Orders.AsNoTracking().Include(x => x.OrderDetails).Include(x => x.Payments.OrderByDescending(p => p.CreatedDate)).FirstOrDefaultAsync(x => x.Id == id && ((userId != null && x.UserId == userId) || (x.UserId == null && x.OrderCode == orderCode)));
    public async Task<IReadOnlyList<Order>> GetHistoryAsync(string userId) => await db.Orders.AsNoTracking().Where(x => x.UserId == userId).Include(x => x.OrderDetails).OrderByDescending(x => x.CreatedDate).ToListAsync();

    public async Task<(Order? Order, Payment? Payment, string? Error)> RetryPaymentAsync(int id, string userId)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<(Order? Order, Payment? Payment, string? Error)>(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var order = await db.Orders.Include(x => x.OrderDetails).Include(x => x.Payments).FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (order is null) return (null, null, "Không tìm thấy đơn hàng.");
            if (order.PaymentMethod != PaymentMethod.VNPay || order.OrderStatus is OrderStatus.Cancelled or OrderStatus.Completed || order.PaymentStatus == PaymentStatus.Paid) return (null, null, "Đơn hàng không thể thanh toán lại.");
            if (order.StockReleased)
            {
                foreach (var detail in order.OrderDetails)
                {
                    var variant = await db.ProductVariants.FirstAsync(x => x.Id == detail.ProductVariantId);
                    if (!variant.IsActive || variant.StockQuantity < detail.Quantity) return (null, null, "Sản phẩm trong đơn không còn đủ tồn kho.");
                    variant.StockQuantity -= detail.Quantity;
                }
                order.StockReleased = false; order.StockReserved = true;
            }
            order.PaymentStatus = PaymentStatus.Pending; order.UpdatedDate = DateTime.UtcNow;
            var payment = new Payment { OrderId = order.Id, Provider = "VNPay", TxnRef = CreateTxnRef(order.OrderCode), Amount = order.TotalAmount };
            db.Payments.Add(payment);
            await db.SaveChangesAsync(); await transaction.CommitAsync();
            return (order, payment, null);
        });
    }

    public async Task<PaymentResultViewModel> ProcessVnPayCallbackAsync(VnPayResponseModel response)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<PaymentResultViewModel>(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var payment = await db.Payments.Include(x => x.Order!).ThenInclude(x => x.OrderDetails).FirstOrDefaultAsync(x => x.TxnRef == response.TxnRef);
            if (payment is null) return new PaymentResultViewModel { IsSuccess = false, Message = "Không tìm thấy giao dịch." };
            var order = payment.Order!;
            if (payment.Status == PaymentStatus.Paid || order.PaymentStatus == PaymentStatus.Paid)
            {
                logger.LogInformation("Duplicate VNPay callback for {TxnRef}", response.TxnRef);
                return new PaymentResultViewModel { IsSuccess = true, Order = order, Payment = payment, Message = "Giao dịch đã được ghi nhận trước đó." };
            }
            var validAmount = response.Amount == order.TotalAmount && payment.Amount == order.TotalAmount;
            if (!validAmount)
            {
                logger.LogWarning("VNPay amount mismatch for {TxnRef}", response.TxnRef);
                return await FailPaymentAsync(payment, order, response, "Số tiền giao dịch không khớp.", transaction);
            }
            var success = response.ResponseCode == "00" && (string.IsNullOrEmpty(response.TransactionStatus) || response.TransactionStatus == "00");
            payment.ResponseCode = response.ResponseCode; payment.TransactionStatus = response.TransactionStatus; payment.TransactionNo = response.TransactionNo; payment.BankCode = response.BankCode; payment.RawResponse = JsonSerializer.Serialize(response.Parameters);
            if (success)
            {
                payment.Status = PaymentStatus.Paid; payment.PaymentDate = ParsePayDate(response.PayDate) ?? DateTime.UtcNow;
                order.PaymentStatus = PaymentStatus.Paid; order.OrderStatus = OrderStatus.Confirmed; order.TransactionNo = response.TransactionNo; order.PaymentDate = payment.PaymentDate; order.UpdatedDate = DateTime.UtcNow;
                await db.SaveChangesAsync(); await transaction.CommitAsync(); logger.LogInformation("VNPay payment succeeded for {OrderCode}", order.OrderCode);
                return new PaymentResultViewModel { IsSuccess = true, Order = order, Payment = payment, Message = "Thanh toán thành công." };
            }
            return await FailPaymentAsync(payment, order, response, VnPayErrorMessage(response.ResponseCode), transaction);
        });
    }

    private async Task<PaymentResultViewModel> FailPaymentAsync(Payment payment, Order order, VnPayResponseModel response, string message, Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction)
    {
        payment.Status = PaymentStatus.Failed; payment.ResponseCode = response.ResponseCode; payment.TransactionStatus = response.TransactionStatus; payment.TransactionNo = response.TransactionNo; payment.BankCode = response.BankCode; payment.RawResponse = JsonSerializer.Serialize(response.Parameters);
        if (order.PaymentStatus != PaymentStatus.Paid && order.StockReserved && !order.StockReleased)
        {
            foreach (var detail in order.OrderDetails)
            {
                var variant = await db.ProductVariants.FirstOrDefaultAsync(x => x.Id == detail.ProductVariantId);
                if (variant is not null) variant.StockQuantity += detail.Quantity;
            }
            order.StockReleased = true; order.StockReserved = false;
        }
        order.PaymentStatus = PaymentStatus.Failed; order.UpdatedDate = DateTime.UtcNow;
        await db.SaveChangesAsync(); await transaction.CommitAsync(); logger.LogInformation("VNPay payment failed for {OrderCode}: {Code}", order.OrderCode, response.ResponseCode);
        return new PaymentResultViewModel { IsSuccess = false, Order = order, Payment = payment, Message = message };
    }

    private static string CreateTxnRef(string orderCode) => $"{orderCode}-{DateTime.UtcNow:HHmmssfff}-{Random.Shared.Next(100, 999)}";
    private static DateTime? ParsePayDate(string? value) => DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date) ? date : null;
    public static string VnPayErrorMessage(string? code) => code switch { "00" => "Giao dịch thành công.", "07" => "Giao dịch bị nghi ngờ.", "09" => "Thẻ hoặc tài khoản chưa đăng ký Internet Banking.", "10" => "Xác thực thông tin thẻ quá số lần cho phép.", "11" => "Giao dịch đã hết thời gian chờ thanh toán.", "12" => "Thẻ hoặc tài khoản bị khóa.", "13" => "Nhập sai OTP.", "24" => "Khách hàng đã hủy giao dịch.", "51" => "Tài khoản không đủ số dư.", "65" => "Vượt hạn mức giao dịch.", "75" => "Ngân hàng đang bảo trì.", "79" => "Nhập sai mật khẩu thanh toán quá số lần.", _ => "Giao dịch không thành công, vui lòng thử lại." };
}
