using System.ComponentModel.DataAnnotations;

namespace AppleStore.Web.Models.Entities;

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    [Required, MaxLength(500)] public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public Product? Product { get; set; }
}
