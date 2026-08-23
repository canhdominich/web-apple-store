using AppleStore.Web.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AppleStore.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ApplicationUser>(b =>
        {
            b.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            b.Property(x => x.Address).HasMaxLength(500);
            b.HasIndex(x => x.Email);
        });
        builder.Entity<Category>(b => b.HasIndex(x => x.Slug).IsUnique());
        builder.Entity<Product>(b =>
        {
            b.HasIndex(x => x.Slug).IsUnique();
            b.Property(x => x.BasePrice).HasPrecision(18, 2);
            b.HasOne(x => x.Category).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<ProductVariant>(b =>
        {
            b.HasIndex(x => x.SKU).IsUnique();
            b.Property(x => x.Price).HasPrecision(18, 2);
            b.HasOne(x => x.Product).WithMany(x => x.Variants).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<ProductImage>().HasOne(x => x.Product).WithMany(x => x.Images).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Cart>(b =>
        {
            b.Property(x => x.UserId).HasMaxLength(80).IsRequired();
            b.HasIndex(x => x.UserId).IsUnique();
        });
        builder.Entity<CartItem>(b =>
        {
            b.HasIndex(x => new { x.CartId, x.ProductVariantId }).IsUnique();
            b.HasOne(x => x.Cart).WithMany(x => x.Items).HasForeignKey(x => x.CartId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.ProductVariant).WithMany(x => x.CartItems).HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<Order>(b =>
        {
            b.HasIndex(x => x.OrderCode).IsUnique();
            b.Property(x => x.Subtotal).HasPrecision(18, 2);
            b.Property(x => x.ShippingFee).HasPrecision(18, 2);
            b.Property(x => x.TotalAmount).HasPrecision(18, 2);
            b.HasOne(x => x.User).WithMany(x => x.Orders).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });
        builder.Entity<OrderDetail>(b =>
        {
            b.Property(x => x.UnitPrice).HasPrecision(18, 2);
            b.Property(x => x.TotalPrice).HasPrecision(18, 2);
            b.HasOne(x => x.Order).WithMany(x => x.OrderDetails).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.ProductVariant).WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<Payment>(b =>
        {
            b.HasIndex(x => x.TxnRef).IsUnique();
            b.Property(x => x.Amount).HasPrecision(18, 2);
            b.HasOne(x => x.Order).WithMany(x => x.Payments).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
