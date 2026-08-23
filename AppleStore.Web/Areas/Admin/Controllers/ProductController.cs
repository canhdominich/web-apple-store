using AppleStore.Web.Data;
using AppleStore.Web.Models.Entities;
using AppleStore.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppleStore.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class ProductController(ApplicationDbContext db, IWebHostEnvironment environment, ILogger<ProductController> logger) : Controller
{
    public async Task<IActionResult> Index(string? search) => View(await db.Products.Include(x => x.Category).Include(x => x.Variants).Where(x => string.IsNullOrWhiteSpace(search) || x.Name.Contains(search) || x.Slug.Contains(search)).OrderByDescending(x => x.CreatedDate).ToListAsync());
    [HttpGet] public async Task<IActionResult> Create() => View(new ProductFormViewModel { Variants = [new VariantFormViewModel { Price = 1 }] , Categories = await CategoriesAsync() });
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Create(ProductFormViewModel model) { return await SaveProductAsync(model, null); }
    [HttpGet] public async Task<IActionResult> Edit(int id) { var product = await db.Products.Include(x => x.Variants).FirstOrDefaultAsync(x => x.Id == id); if (product is null) return NotFound(); return View(new ProductFormViewModel { Id = product.Id, CategoryId = product.CategoryId, Name = product.Name, Slug = product.Slug, Description = product.Description, BasePrice = product.BasePrice, IsActive = product.IsActive, IsFeatured = product.IsFeatured, Variants = product.Variants.Select(v => new VariantFormViewModel { Id = v.Id, Color = v.Color, Storage = v.Storage, SKU = v.SKU, Price = v.Price, StockQuantity = v.StockQuantity, IsActive = v.IsActive }).ToList(), Categories = await CategoriesAsync() }); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Edit(ProductFormViewModel model) { return await SaveProductAsync(model, model.Id); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Toggle(int id) { var p = await db.Products.FindAsync(id); if (p is null) return NotFound(); p.IsActive = !p.IsActive; await db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }

    private async Task<IActionResult> SaveProductAsync(ProductFormViewModel model, int? existingId)
    {
        model.Categories = await CategoriesAsync();
        if (!string.IsNullOrWhiteSpace(model.Slug) && await db.Products.AnyAsync(x => x.Slug == model.Slug && x.Id != existingId)) ModelState.AddModelError(nameof(model.Slug), "Slug đã tồn tại.");
        if (model.Variants.Count == 0) ModelState.AddModelError(nameof(model.Variants), "Cần có ít nhất một phiên bản.");
        var variantSkus = model.Variants.Where(x => !string.IsNullOrWhiteSpace(x.SKU)).Select(x => x.SKU.Trim()).ToList();
        if (variantSkus.Count != variantSkus.Distinct(StringComparer.OrdinalIgnoreCase).Count() || (variantSkus.Count > 0 && await db.ProductVariants.AnyAsync(x => variantSkus.Contains(x.SKU) && (!existingId.HasValue || x.ProductId != existingId.Value)))) ModelState.AddModelError(nameof(model.Variants), "SKU phải duy nhất.");
        if (!ModelState.IsValid) return View(existingId.HasValue ? "Edit" : "Create", model);
        var product = existingId.HasValue ? await db.Products.Include(x => x.Variants).FirstOrDefaultAsync(x => x.Id == existingId.Value) : new Product();
        if (product is null) return NotFound();
        product.CategoryId = model.CategoryId; product.Name = model.Name.Trim(); product.Slug = model.Slug.Trim().ToLowerInvariant(); product.Description = model.Description.Trim(); product.BasePrice = model.BasePrice; product.IsActive = model.IsActive; product.IsFeatured = model.IsFeatured; product.UpdatedDate = DateTime.UtcNow;
        if (model.Image is not null) { var relative = await SaveImageAsync(model.Image); if (relative is null) ModelState.AddModelError(nameof(model.Image), "Ảnh phải là JPG, PNG hoặc WEBP và tối đa 5MB."); else product.ImageThumbnail = relative; }
        if (!ModelState.IsValid) return View(existingId.HasValue ? "Edit" : "Create", model);
        if (!existingId.HasValue) db.Products.Add(product);
        foreach (var input in model.Variants)
        {
            var variant = input.Id > 0 ? product.Variants.FirstOrDefault(x => x.Id == input.Id) : null;
            if (variant is null) product.Variants.Add(new ProductVariant { Color = input.Color.Trim(), Storage = input.Storage.Trim(), SKU = input.SKU.Trim(), Price = input.Price, StockQuantity = input.StockQuantity, IsActive = input.IsActive });
            else { variant.Color = input.Color.Trim(); variant.Storage = input.Storage.Trim(); variant.SKU = input.SKU.Trim(); variant.Price = input.Price; variant.StockQuantity = input.StockQuantity; variant.IsActive = input.IsActive; }
        }
        await db.SaveChangesAsync(); TempData["Success"] = existingId.HasValue ? "Đã cập nhật sản phẩm." : "Đã tạo sản phẩm."; return RedirectToAction(nameof(Index));
    }
    private async Task<List<Category>> CategoriesAsync() => await db.Categories.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync();
    private async Task<string?> SaveImageAsync(IFormFile file)
    {
        var allowed = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase) { [".jpg"] = ["image/jpeg"], [".jpeg"] = ["image/jpeg"], [".png"] = ["image/png"], [".webp"] = ["image/webp"] }; var ext = Path.GetExtension(file.FileName); if (file.Length is <= 0 or > 5 * 1024 * 1024 || !allowed.TryGetValue(ext, out var types) || !types.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase)) return null;
        var folder = Path.Combine(environment.WebRootPath, "images", "products"); Directory.CreateDirectory(folder); var fileName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}"; var path = Path.Combine(folder, fileName); await using var stream = System.IO.File.Create(path); await file.CopyToAsync(stream); logger.LogInformation("Product image uploaded as {FileName}", fileName); return $"/images/products/{fileName}";
    }
}
