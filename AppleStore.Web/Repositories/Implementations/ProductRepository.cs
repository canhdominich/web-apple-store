using AppleStore.Web.Data;
using AppleStore.Web.Models.Entities;
using AppleStore.Web.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppleStore.Web.Repositories.Implementations;

public class ProductRepository(ApplicationDbContext db) : IProductRepository
{
    public async Task<(IReadOnlyList<Product> Products, int Total)> SearchAsync(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice, string? color, string? storage, string sort, int page, int pageSize)
    {
        var query = db.Products.AsNoTracking().Include(x => x.Category).Include(x => x.Variants).Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Name.Contains(search) || x.Description.Contains(search));
        if (categoryId.HasValue) query = query.Where(x => x.CategoryId == categoryId.Value);
        if (minPrice.HasValue) query = query.Where(x => x.Variants.Any(v => v.Price >= minPrice.Value));
        if (maxPrice.HasValue) query = query.Where(x => x.Variants.Any(v => v.Price <= maxPrice.Value));
        if (!string.IsNullOrWhiteSpace(color)) query = query.Where(x => x.Variants.Any(v => v.Color == color));
        if (!string.IsNullOrWhiteSpace(storage)) query = query.Where(x => x.Variants.Any(v => v.Storage == storage));
        query = sort switch
        {
            "price-asc" => query.OrderBy(x => x.Variants.Min(v => v.Price)),
            "price-desc" => query.OrderByDescending(x => x.Variants.Max(v => v.Price)),
            "name" => query.OrderBy(x => x.Name),
            _ => query.OrderByDescending(x => x.CreatedDate)
        };
        var total = await query.CountAsync();
        var products = await query.Skip((Math.Max(page, 1) - 1) * pageSize).Take(pageSize).ToListAsync();
        return (products, total);
    }

    public Task<Product?> GetBySlugAsync(string slug) => db.Products.AsNoTracking().Include(x => x.Category).Include(x => x.Variants.Where(v => v.IsActive)).Include(x => x.Images).FirstOrDefaultAsync(x => x.Slug == slug && x.IsActive);
    public async Task<IReadOnlyList<Category>> GetCategoriesAsync() => await db.Categories.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync();
}
