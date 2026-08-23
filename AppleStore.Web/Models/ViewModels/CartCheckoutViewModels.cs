using System.ComponentModel.DataAnnotations;
using AppleStore.Web.Models.Entities;

namespace AppleStore.Web.Models.ViewModels;

public class CartViewModel
{
    public IReadOnlyList<CartItem> Items { get; set; } = [];
    public IReadOnlyList<Product> Recommendations { get; set; } = [];
    public decimal Subtotal => Items.Sum(i => (i.ProductVariant?.Price ?? 0) * i.Quantity);
}
public class CheckoutViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên."), Display(Name = "Họ và tên")] public string CustomerName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập email."), EmailAddress(ErrorMessage = "Email không đúng định dạng."), Display(Name = "Email")] public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại."), Phone(ErrorMessage = "Số điện thoại không đúng định dạng."), Display(Name = "Số điện thoại")] public string PhoneNumber { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng."), StringLength(500, ErrorMessage = "Địa chỉ nhận hàng không được vượt quá 500 ký tự."), Display(Name = "Địa chỉ nhận hàng")] public string ShippingAddress { get; set; } = string.Empty;
    [StringLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự.")] public string? Note { get; set; }
    [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")] public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;
    public bool HasSavedAddress { get; set; }
    public string? SavedAddress { get; set; }
    public CartViewModel Cart { get; set; } = new();
}
public class PaymentResultViewModel
{
    public Order Order { get; set; } = new();
    public Payment? Payment { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}
