using AppleStore.Web.Models.ViewModels;
using AppleStore.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AppleStore.Web.Controllers;

public class ProductController(IProductRepository repository) : BaseController
{
    public async Task<IActionResult> Index(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice, string? color, string? storage, string sort = "newest", int page = 1)
    {
        var result = await repository.SearchAsync(search, categoryId, minPrice, maxPrice, color, storage, sort, page, 12);
        return View(new ProductListViewModel { Products = result.Products, TotalItems = result.Total, Categories = await repository.GetCategoriesAsync(), Search = search, CategoryId = categoryId, MinPrice = minPrice, MaxPrice = maxPrice, Color = color, Storage = storage, Sort = sort, Page = page });
    }
    public async Task<IActionResult> Details(string slug)
    {
        var product = await repository.GetBySlugAsync(slug);
        if (product is null) return NotFound();
        var related = (await repository.SearchAsync(null, product.CategoryId, null, null, null, null, "newest", 1, 5)).Products.Where(x => x.Id != product.Id).ToList();
        return View(new ProductDetailViewModel { Product = product, RelatedProducts = related });
    }
}
