using AppleStore.Web.Data;
using AppleStore.Web.Models.Entities;
using AppleStore.Web.Models.ViewModels;
using AppleStore.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppleStore.Web.Services.Implementations;

public class CartService(ApplicationDbContext db) : ICartService
{
    public async Task<CartViewModel> GetAsync(string cartKey)
    {
        if (string.IsNullOrWhiteSpace(cartKey)) return new CartViewModel();
        var cart = await db.Carts.AsNoTracking().Include(x => x.Items).ThenInclude(x => x.ProductVariant!).ThenInclude(x => x.Product).FirstOrDefaultAsync(x => x.UserId == cartKey);
        return new CartViewModel { Items = cart?.Items.OrderBy(x => x.Id).ToList() ?? [] };
    }

    public async Task<(bool Success, string Message)> AddAsync(string cartKey, AddToCartRequest request)
    {
        if (string.IsNullOrWhiteSpace(cartKey)) return (false, "Không thể xác định giỏ hàng.");
        var variant = await db.ProductVariants.Include(x => x.Product).FirstOrDefaultAsync(x => x.Id == request.VariantId && x.IsActive && x.Product!.IsActive);
        if (variant is null) return (false, "Sản phẩm không tồn tại.");
        var cart = await db.Carts.Include(x => x.Items).FirstOrDefaultAsync(x => x.UserId == cartKey);
        if (cart is null) { cart = new Cart { UserId = cartKey }; db.Carts.Add(cart); }
        var item = cart.Items.FirstOrDefault(x => x.ProductVariantId == request.VariantId);
        var quantity = (item?.Quantity ?? 0) + request.Quantity;
        if (quantity > variant.StockQuantity) return (false, $"Chỉ còn {variant.StockQuantity} sản phẩm trong kho.");
        if (item is null) cart.Items.Add(new CartItem { ProductVariantId = variant.Id, Quantity = request.Quantity }); else item.Quantity = quantity;
        cart.UpdatedDate = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return (true, "Đã thêm sản phẩm vào giỏ hàng.");
    }

    public async Task<(bool Success, string Message)> UpdateAsync(string cartKey, int itemId, int quantity)
    {
        var item = await db.CartItems.Include(x => x.Cart).Include(x => x.ProductVariant).FirstOrDefaultAsync(x => x.Id == itemId && x.Cart!.UserId == cartKey);
        if (item is null) return (false, "Mặt hàng không tồn tại.");
        if (quantity <= 0) { db.CartItems.Remove(item); await db.SaveChangesAsync(); return (true, "Đã xóa sản phẩm."); }
        if (quantity > item.ProductVariant!.StockQuantity) return (false, $"Chỉ còn {item.ProductVariant.StockQuantity} sản phẩm trong kho.");
        item.Quantity = quantity; item.Cart!.UpdatedDate = DateTime.UtcNow; await db.SaveChangesAsync(); return (true, "Đã cập nhật giỏ hàng.");
    }
    public async Task<bool> RemoveAsync(string cartKey, int itemId)
    {
        var item = await db.CartItems.Include(x => x.Cart).FirstOrDefaultAsync(x => x.Id == itemId && x.Cart!.UserId == cartKey);
        if (item is null) return false; db.CartItems.Remove(item); await db.SaveChangesAsync(); return true;
    }
    public async Task ClearAsync(string cartKey)
    {
        var cart = await db.Carts.Include(x => x.Items).FirstOrDefaultAsync(x => x.UserId == cartKey);
        if (cart is not null) { db.CartItems.RemoveRange(cart.Items); cart.UpdatedDate = DateTime.UtcNow; await db.SaveChangesAsync(); }
    }
    public Task<int> CountAsync(string cartKey) => string.IsNullOrWhiteSpace(cartKey)
        ? Task.FromResult(0)
        : db.CartItems.Where(x => x.Cart!.UserId == cartKey).SumAsync(x => (int?)x.Quantity).ContinueWith(t => t.Result ?? 0);

    public async Task MergeAsync(string sourceCartKey, string targetCartKey)
    {
        if (string.IsNullOrWhiteSpace(sourceCartKey) || string.IsNullOrWhiteSpace(targetCartKey) || sourceCartKey == targetCartKey) return;
        var source = await db.Carts.Include(x => x.Items).ThenInclude(x => x.ProductVariant).FirstOrDefaultAsync(x => x.UserId == sourceCartKey);
        if (source is null || source.Items.Count == 0) return;
        var target = await db.Carts.Include(x => x.Items).ThenInclude(x => x.ProductVariant).FirstOrDefaultAsync(x => x.UserId == targetCartKey);
        if (target is null)
        {
            source.UserId = targetCartKey;
            source.UpdatedDate = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return;
        }
        foreach (var sourceItem in source.Items.ToList())
        {
            var targetItem = target.Items.FirstOrDefault(x => x.ProductVariantId == sourceItem.ProductVariantId);
            if (targetItem is null)
            {
                db.CartItems.Add(new CartItem { CartId = target.Id, ProductVariantId = sourceItem.ProductVariantId, Quantity = sourceItem.Quantity });
                db.CartItems.Remove(sourceItem);
                continue;
            }
            var stock = sourceItem.ProductVariant?.StockQuantity ?? targetItem.ProductVariant?.StockQuantity ?? int.MaxValue;
            targetItem.Quantity = Math.Min(stock, targetItem.Quantity + sourceItem.Quantity);
            db.CartItems.Remove(sourceItem);
        }
        db.Carts.Remove(source);
        target.UpdatedDate = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }
}
