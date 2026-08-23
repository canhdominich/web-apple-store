using AppleStore.Web.Data;
using AppleStore.Web.Models.Entities;
using AppleStore.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppleStore.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class CategoryController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? search) => View(await db.Categories.Include(x => x.Products).Where(x => string.IsNullOrWhiteSpace(search) || x.Name.Contains(search)).OrderBy(x => x.Name).ToListAsync());
    [HttpGet] public IActionResult Create() => View(new CategoryFormViewModel());
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Create(CategoryFormViewModel model) { if (await db.Categories.AnyAsync(x => x.Slug == model.Slug)) ModelState.AddModelError(nameof(model.Slug), "Slug đã tồn tại."); if (!ModelState.IsValid) return View(model); db.Categories.Add(new Category { Name = model.Name.Trim(), Slug = model.Slug.Trim().ToLowerInvariant(), Description = model.Description?.Trim(), IsActive = model.IsActive }); await db.SaveChangesAsync(); TempData["Success"] = "Đã tạo danh mục."; return RedirectToAction(nameof(Index)); }
    [HttpGet] public async Task<IActionResult> Edit(int id) { var c = await db.Categories.FindAsync(id); return c is null ? NotFound() : View(new CategoryFormViewModel { Id = c.Id, Name = c.Name, Slug = c.Slug, Description = c.Description, IsActive = c.IsActive }); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Edit(CategoryFormViewModel model) { if (await db.Categories.AnyAsync(x => x.Slug == model.Slug && x.Id != model.Id)) ModelState.AddModelError(nameof(model.Slug), "Slug đã tồn tại."); if (!ModelState.IsValid) return View(model); var c = await db.Categories.FindAsync(model.Id); if (c is null) return NotFound(); c.Name = model.Name.Trim(); c.Slug = model.Slug.Trim().ToLowerInvariant(); c.Description = model.Description?.Trim(); c.IsActive = model.IsActive; await db.SaveChangesAsync(); TempData["Success"] = "Đã cập nhật danh mục."; return RedirectToAction(nameof(Index)); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Toggle(int id) { var c = await db.Categories.FindAsync(id); if (c is null) return NotFound(); c.IsActive = !c.IsActive; await db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
}
