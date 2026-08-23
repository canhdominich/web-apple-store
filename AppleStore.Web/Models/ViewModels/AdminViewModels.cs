using System.ComponentModel.DataAnnotations;
using AppleStore.Web.Models.Entities;

namespace AppleStore.Web.Models.ViewModels;

public class DashboardViewModel
{
    public int TotalProducts { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalOrders { get; set; }
    public int OrdersToday { get; set; }
    public int PendingOrders { get; set; }
    public int LowStock { get; set; }
    public decimal Revenue { get; set; }
    public IReadOnlyList<TopProductViewModel> TopProducts { get; set; } = [];
    public IReadOnlyList<RevenuePointViewModel> RevenuePoints { get; set; } = [];
}
public class TopProductViewModel { public string Name { get; set; } = string.Empty; public int Quantity { get; set; } }
public class RevenuePointViewModel { public string Label { get; set; } = string.Empty; public decimal Value { get; set; } }
public class AdminStatCardViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-circle";
    public string Tone { get; set; } = string.Empty;
}

public class CustomerListViewModel
{
    public IReadOnlyList<ApplicationUser> Users { get; set; } = [];
    public IReadOnlyList<AdminStatCardViewModel> Stats { get; set; } = [];
    public string? Search { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalItems { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalItems / (double)Math.Max(1, PageSize)));
    public int FromItem => TotalItems == 0 ? 0 : ((Page - 1) * PageSize) + 1;
    public int ToItem => Math.Min(Page * PageSize, TotalItems);
}
public class CategoryFormViewModel
{
    [Required(ErrorMessage = "Mã danh mục không hợp lệ."), Display(Name = "Mã danh mục")] public int Id { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tên danh mục."), StringLength(120, ErrorMessage = "Tên danh mục không được vượt quá 120 ký tự."), Display(Name = "Tên danh mục")] public string Name { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập slug."), StringLength(140, ErrorMessage = "Slug không được vượt quá 140 ký tự."), Display(Name = "Slug")] public string Slug { get; set; } = string.Empty;
    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự."), Display(Name = "Mô tả")] public string? Description { get; set; }
    [Required(ErrorMessage = "Trạng thái danh mục không hợp lệ."), Display(Name = "Trạng thái danh mục")] public bool IsActive { get; set; } = true;
}
public class ProductFormViewModel
{
    [Required(ErrorMessage = "Mã sản phẩm không hợp lệ."), Display(Name = "Mã sản phẩm")] public int Id { get; set; }
    [Required(ErrorMessage = "Vui lòng chọn danh mục."), Display(Name = "Danh mục")] public int CategoryId { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm."), StringLength(200, ErrorMessage = "Tên sản phẩm không được vượt quá 200 ký tự."), Display(Name = "Tên sản phẩm")] public string Name { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập slug."), StringLength(220, ErrorMessage = "Slug không được vượt quá 220 ký tự."), Display(Name = "Slug")] public string Slug { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập mô tả sản phẩm."), StringLength(4000, ErrorMessage = "Mô tả sản phẩm không được vượt quá 4000 ký tự."), Display(Name = "Mô tả")] public string Description { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập giá cơ bản."), Range(0.01, 999999999, ErrorMessage = "Giá bán phải lớn hơn 0."), Display(Name = "Giá cơ bản")] public decimal BasePrice { get; set; }
    [Required(ErrorMessage = "Trạng thái nổi bật không hợp lệ."), Display(Name = "Nổi bật")] public bool IsFeatured { get; set; }
    [Required(ErrorMessage = "Trạng thái bán không hợp lệ."), Display(Name = "Trạng thái bán")] public bool IsActive { get; set; } = true;
    public IFormFile? Image { get; set; }
    public List<VariantFormViewModel> Variants { get; set; } = [];
    public List<Category> Categories { get; set; } = [];
}
public class VariantFormViewModel
{
    [Required(ErrorMessage = "Mã phiên bản không hợp lệ."), Display(Name = "Mã phiên bản")] public int Id { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập màu sắc."), Display(Name = "Màu sắc")] public string Color { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập dung lượng."), Display(Name = "Dung lượng")] public string Storage { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập SKU."), Display(Name = "SKU")] public string SKU { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập giá phiên bản."), Range(0.01, 999999999, ErrorMessage = "Giá phiên bản phải lớn hơn 0."), Display(Name = "Giá phiên bản")] public decimal Price { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tồn kho."), Range(0, int.MaxValue, ErrorMessage = "Tồn kho không được âm."), Display(Name = "Tồn kho")] public int StockQuantity { get; set; }
    [Required(ErrorMessage = "Trạng thái phiên bản không hợp lệ."), Display(Name = "Trạng thái phiên bản")] public bool IsActive { get; set; } = true;
}
