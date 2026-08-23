using System.ComponentModel.DataAnnotations;

namespace AppleStore.Web.Models.Entities;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    [Required, MaxLength(30)] public string Provider { get; set; } = "VNPay";
    [Required, MaxLength(80)] public string TxnRef { get; set; } = string.Empty;
    [MaxLength(100)] public string? TransactionNo { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    [MaxLength(10)] public string? ResponseCode { get; set; }
    [MaxLength(10)] public string? TransactionStatus { get; set; }
    [MaxLength(50)] public string? BankCode { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? PaymentDate { get; set; }
    [MaxLength(4000)] public string? RawResponse { get; set; }
    public Order? Order { get; set; }
}
