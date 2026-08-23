using System.ComponentModel.DataAnnotations;

namespace AppleStore.Web.Models.Entities;

public class OrderDetail
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductVariantId { get; set; }
    [Required, MaxLength(200)] public string ProductName { get; set; } = string.Empty;
    [Required, MaxLength(180)] public string VariantName { get; set; } = string.Empty;
    [Required, MaxLength(80)] public string SKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public Order? Order { get; set; }
    public ProductVariant? ProductVariant { get; set; }
}
