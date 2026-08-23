using AppleStore.Web.Models.Entities;

namespace AppleStore.Web.Repositories.Interfaces;

public interface IProductRepository
{
    Task<(IReadOnlyList<Product> Products, int Total)> SearchAsync(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice, string? color, string? storage, string sort, int page, int pageSize);
    Task<Product?> GetBySlugAsync(string slug);
    Task<IReadOnlyList<Category>> GetCategoriesAsync();
}
