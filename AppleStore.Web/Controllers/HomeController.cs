using AppleStore.Web.Data;
using AppleStore.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppleStore.Web.Controllers;

public class HomeController(IProductRepository products, ApplicationDbContext db) : BaseController
{
    public async Task<IActionResult> Index()
    {
        var featured = await db.Products.AsNoTracking().Include(x => x.Category).Include(x => x.Variants).Where(x => x.IsActive && x.IsFeatured).OrderByDescending(x => x.CreatedDate).Take(8).ToListAsync();
        var latest = await db.Products.AsNoTracking().Include(x => x.Category).Include(x => x.Variants).Where(x => x.IsActive).OrderByDescending(x => x.CreatedDate).Take(8).ToListAsync();
        return View(new HomeViewModel { FeaturedProducts = featured, LatestProducts = latest, Categories = await products.GetCategoriesAsync() });
    }
    public IActionResult Error() => View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
}
public class HomeViewModel
{
    public IReadOnlyList<AppleStore.Web.Models.Entities.Product> FeaturedProducts { get; set; } = [];
    public IReadOnlyList<AppleStore.Web.Models.Entities.Product> LatestProducts { get; set; } = [];
    public IReadOnlyList<AppleStore.Web.Models.Entities.Category> Categories { get; set; } = [];
}
public class ErrorViewModel { public string? RequestId { get; set; } public bool ShowRequestId => !string.IsNullOrEmpty(RequestId); }
