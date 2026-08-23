using AppleStore.Web.Models.Entities;
using System.Globalization;

namespace AppleStore.Web.Infrastructure;

public static class DisplayExtensions
{
    private static readonly CultureInfo VietnamCulture = CultureInfo.GetCultureInfo("vi-VN");
    private static readonly TimeZoneInfo VietnamTimeZone = CreateVietnamTimeZone();

    public static string ToMoney(this decimal value) => value.ToMoneyNumber() + "₫";

    public static string ToMoneyNumber(this decimal value) => value.ToString("N0", VietnamCulture);

    public static string ToMoneyValue(this decimal value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    public static DateTime ToVietnamTime(this DateTime value)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
        return TimeZoneInfo.ConvertTimeFromUtc(utc, VietnamTimeZone);
    }

    public static DateTime? ToVietnamTime(this DateTime? value) => value.HasValue ? value.Value.ToVietnamTime() : null;

    public static string ToOrderStatusText(this OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Đang chờ",
        OrderStatus.Confirmed => "Đã xác nhận",
        OrderStatus.Shipping => "Đang giao hàng",
        OrderStatus.Completed => "Đã hoàn thành",
        OrderStatus.Cancelled => "Đã hủy",
        _ => "Không xác định"
    };

    public static string ToPaymentStatusText(this PaymentStatus status) => status switch
    {
        PaymentStatus.Pending => "Chờ thanh toán",
        PaymentStatus.Paid => "Đã thanh toán",
        PaymentStatus.Failed => "Thanh toán thất bại",
        PaymentStatus.Refunded => "Đã hoàn tiền",
        PaymentStatus.Cancelled => "Đã hủy",
        _ => "Không xác định"
    };

    private static TimeZoneInfo CreateVietnamTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.CreateCustomTimeZone("UTC+7", TimeSpan.FromHours(7), "UTC+7", "UTC+7");
        }
    }
}
