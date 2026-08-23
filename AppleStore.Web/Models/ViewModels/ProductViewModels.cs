using System.ComponentModel.DataAnnotations;
using AppleStore.Web.Models.Entities;

namespace AppleStore.Web.Models.ViewModels;

public class ProductListViewModel
{
    public IReadOnlyList<Product> Products { get; set; } = [];
    public IReadOnlyList<Category> Categories { get; set; } = [];
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Color { get; set; }
    public string? Storage { get; set; }
    public string Sort { get; set; } = "newest";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
}
public class ProductDetailViewModel
{
    public Product Product { get; set; } = new();
    public IReadOnlyList<Product> RelatedProducts { get; set; } = [];
}
public class AddToCartRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn phiên bản sản phẩm hợp lệ.")] public int VariantId { get; set; }
    [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100.")] public int Quantity { get; set; } = 1;
}
