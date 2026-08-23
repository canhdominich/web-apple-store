using System.ComponentModel.DataAnnotations;

namespace AppleStore.Web.Models.Entities;

public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    [MaxLength(80)] public string Color { get; set; } = string.Empty;
    [MaxLength(80)] public string Storage { get; set; } = string.Empty;
    [Required, MaxLength(80)] public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public Product? Product { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
