using System.ComponentModel.DataAnnotations;

namespace AppleStore.Web.Models.Entities;

public class Order
{
    public int Id { get; set; }
    [Required, MaxLength(40)] public string OrderCode { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    [Required, MaxLength(150)] public string CustomerName { get; set; } = string.Empty;
    [Required, MaxLength(256)] public string Email { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string PhoneNumber { get; set; } = string.Empty;
    [Required, MaxLength(500)] public string ShippingAddress { get; set; } = string.Empty;
    [MaxLength(1000)] public string? Note { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    [MaxLength(100)] public string? TransactionNo { get; set; }
    public DateTime? PaymentDate { get; set; }
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal TotalAmount { get; set; }
    public bool StockReserved { get; set; }
    public bool StockReleased { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
    public ApplicationUser? User { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
