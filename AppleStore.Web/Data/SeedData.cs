using AppleStore.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AppleStore.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        foreach (var role in new[] { "Admin", "Customer" })
            if (!await roleManager.RoleExistsAsync(role)) await roleManager.CreateAsync(new IdentityRole(role));

        const string adminEmail = "admin@applestore.local";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true, FullName = "Quản trị Apple Store", PhoneNumber = "0900000000", Address = "TP. Hồ Chí Minh" };
            var result = await userManager.CreateAsync(admin, "Admin@123456");
            if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
            await userManager.AddToRoleAsync(admin, "Admin");
        }
        else if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        var categories = await EnsureCategoriesAsync(db);
        await EnsureProductsAsync(db, categories);
        await EnsureDemoCustomersAndOrdersAsync(db, userManager);
    }

    private static async Task<List<Category>> EnsureCategoriesAsync(ApplicationDbContext db)
    {
        var categories = await db.Categories.OrderBy(x => x.Id).ToListAsync();
        if (categories.Count > 0) return categories;

        var categoryData = new[] { ("iPhone", "iphone"), ("Mac", "mac"), ("iPad", "ipad"), ("Apple Watch", "apple-watch"), ("AirPods", "airpods"), ("Phụ kiện", "accessories") };
        categories = categoryData.Select(x => new Category { Name = x.Item1, Slug = x.Item2, Description = $"Các sản phẩm {x.Item1} chính hãng" }).ToList();
        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();
        return categories;
    }

    private static async Task EnsureProductsAsync(ApplicationDbContext db, IReadOnlyCollection<Category> categories)
    {
        if (await db.Products.AnyAsync()) return;

        Category Category(string slug) => categories.First(x => x.Slug == slug);

        var products = new List<Product>();
        AddProduct(products, Category("iphone"), "iPhone 16 Pro", "iphone-16-pro", 28990000, true, ("Titan đen", "256GB", 28990000, 18), ("Titan tự nhiên", "512GB", 34990000, 10));
        AddProduct(products, Category("iphone"), "iPhone 16", "iphone-16", 22990000, true, ("Đen", "128GB", 22990000, 20), ("Xanh mòng két", "256GB", 25990000, 14));
        AddProduct(products, Category("iphone"), "iPhone 15", "iphone-15", 18990000, false, ("Hồng", "128GB", 18990000, 16), ("Xanh dương", "256GB", 21990000, 8));
        AddProduct(products, Category("mac"), "MacBook Air M3", "macbook-air-m3", 27490000, true, ("Midnight", "8GB/256GB", 27490000, 12), ("Starlight", "16GB/512GB", 34990000, 7));
        AddProduct(products, Category("mac"), "MacBook Pro M4", "macbook-pro-m4", 39990000, true, ("Space Black", "16GB/512GB", 39990000, 8), ("Silver", "24GB/1TB", 49990000, 4));
        AddProduct(products, Category("mac"), "iMac M4", "imac-m4", 34990000, false, ("Xanh", "16GB/256GB", 34990000, 6), ("Cam", "16GB/512GB", 41990000, 3));
        AddProduct(products, Category("ipad"), "iPad Pro M4", "ipad-pro-m4", 28990000, true, ("Space Black", "256GB", 28990000, 9), ("Silver", "512GB", 34990000, 5));
        AddProduct(products, Category("ipad"), "iPad Air M2", "ipad-air-m2", 16990000, false, ("Tím", "128GB", 16990000, 15), ("Xanh", "256GB", 19990000, 11));
        AddProduct(products, Category("ipad"), "iPad mini", "ipad-mini", 13990000, false, ("Starlight", "128GB", 13990000, 12), ("Tím", "256GB", 16990000, 6));
        AddProduct(products, Category("apple-watch"), "Apple Watch Series 10", "apple-watch-series-10", 10990000, true, ("Jet Black", "42mm", 10990000, 10), ("Rose Gold", "46mm", 12490000, 7));
        AddProduct(products, Category("apple-watch"), "Apple Watch Ultra 2", "apple-watch-ultra-2", 21990000, false, ("Titan tự nhiên", "49mm", 21990000, 5), ("Titan đen", "49mm", 22990000, 3));
        AddProduct(products, Category("airpods"), "AirPods Pro 2", "airpods-pro-2", 5990000, true, ("Trắng", "USB-C", 5990000, 25));
        AddProduct(products, Category("airpods"), "AirPods 4", "airpods-4", 3490000, false, ("Trắng", "Tiêu chuẩn", 3490000, 20), ("Trắng", "ANC", 4790000, 13));
        AddProduct(products, Category("accessories"), "Magic Keyboard", "magic-keyboard", 2490000, false, ("Trắng", "USB-C", 2490000, 18));
        AddProduct(products, Category("accessories"), "AirTag", "airtag", 790000, false, ("Trắng", "1 Pack", 790000, 30), ("Trắng", "4 Pack", 2690000, 14));
        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }

    private static async Task EnsureDemoCustomersAndOrdersAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        var now = DateTime.UtcNow;
        var demoCustomerData = new[]
        {
            ("minh.anh@example.com", "Nguyễn Minh Anh", "0901234567", "42 Nguyễn Huệ, Quận 1, TP. Hồ Chí Minh", 4, true),
            ("quoc.bao@example.com", "Trần Quốc Bảo", "0912345678", "18 Lý Thường Kiệt, Hoàn Kiếm, Hà Nội", 12, true),
            ("hoang.linh@example.com", "Lê Hoàng Linh", "0923456789", "75 Bạch Đằng, Hải Châu, Đà Nẵng", 21, true),
            ("thu.trang@example.com", "Phạm Thu Trang", "0934567890", "12 Trần Phú, Nha Trang, Khánh Hòa", 35, true),
            ("duc.khang@example.com", "Võ Đức Khang", "0945678901", "09 Võ Văn Ngân, TP. Thủ Đức, TP. Hồ Chí Minh", 48, false),
            ("ngoc.han@example.com", "Đặng Ngọc Hân", "0956789012", "27 Hai Bà Trưng, Đà Lạt, Lâm Đồng", 60, true)
        };

        var customers = new Dictionary<string, ApplicationUser>();
        foreach (var data in demoCustomerData)
        {
            var user = await userManager.FindByEmailAsync(data.Item1);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = data.Item1,
                    Email = data.Item1,
                    EmailConfirmed = true,
                    FullName = data.Item2,
                    PhoneNumber = data.Item3,
                    Address = data.Item4,
                    IsActive = data.Item6,
                    CreatedDate = now.AddDays(-data.Item5)
                };
                var result = await userManager.CreateAsync(user, "Customer@123456");
                if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
            }

            if (!await userManager.IsInRoleAsync(user, "Customer")) await userManager.AddToRoleAsync(user, "Customer");
            customers[data.Item1] = user;
        }

        if (await db.Orders.AnyAsync(x => x.OrderCode.StartsWith("ASD"))) return;

        var variants = await db.ProductVariants
            .Include(x => x.Product)
            .Where(x => x.IsActive && x.Product != null && x.Product.IsActive)
            .OrderBy(x => x.Product!.Slug)
            .ThenBy(x => x.Price)
            .ToListAsync();
        if (variants.Count == 0) return;

        ProductVariant Pick(string slug, string? storage = null)
        {
            return variants.FirstOrDefault(x => x.Product?.Slug == slug && (string.IsNullOrWhiteSpace(storage) || x.Storage.Contains(storage, StringComparison.OrdinalIgnoreCase)))
                ?? variants.First(x => x.Product?.Slug == slug)
                ?? variants.First();
        }

        void AddOrder(ApplicationUser? user, string code, OrderStatus orderStatus, PaymentMethod paymentMethod, PaymentStatus paymentStatus, int daysAgo, string? note, params (ProductVariant variant, int quantity)[] items)
        {
            var created = now.AddDays(-daysAgo).AddHours(-Random.Shared.Next(1, 8)).AddMinutes(Random.Shared.Next(5, 55));
            if (orderStatus == OrderStatus.Completed) paymentStatus = PaymentStatus.Paid;

            var order = new Order
            {
                OrderCode = code,
                UserId = user?.Id,
                CustomerName = user?.FullName ?? "Khách vãng lai",
                Email = user?.Email ?? "guest.checkout@example.com",
                PhoneNumber = user?.PhoneNumber ?? "0987654321",
                ShippingAddress = user?.Address ?? "88 Điện Biên Phủ, Bình Thạnh, TP. Hồ Chí Minh",
                Note = note,
                PaymentMethod = paymentMethod,
                PaymentStatus = paymentStatus,
                PaymentDate = paymentStatus == PaymentStatus.Paid ? created.AddMinutes(12) : null,
                OrderStatus = orderStatus,
                OrderDate = created,
                CreatedDate = created,
                UpdatedDate = orderStatus == OrderStatus.Pending ? null : created.AddHours(3),
                StockReserved = orderStatus is OrderStatus.Pending or OrderStatus.Confirmed or OrderStatus.Shipping,
                StockReleased = orderStatus == OrderStatus.Cancelled
            };

            foreach (var item in items)
            {
                var variant = item.variant;
                var quantity = Math.Max(1, item.quantity);
                order.OrderDetails.Add(new OrderDetail
                {
                    ProductVariantId = variant.Id,
                    ProductName = variant.Product?.Name ?? "Sản phẩm Apple",
                    VariantName = $"{variant.Color} / {variant.Storage}",
                    SKU = variant.SKU,
                    Quantity = quantity,
                    UnitPrice = variant.Price,
                    TotalPrice = variant.Price * quantity
                });

                if (orderStatus != OrderStatus.Cancelled)
                {
                    variant.StockQuantity = Math.Max(0, variant.StockQuantity - quantity);
                }
            }

            order.Subtotal = order.OrderDetails.Sum(x => x.TotalPrice);
            order.ShippingFee = order.Subtotal >= 1000000 ? 0m : 30000m;
            order.TotalAmount = order.Subtotal + order.ShippingFee;
            order.TransactionNo = paymentStatus == PaymentStatus.Paid && paymentMethod == PaymentMethod.VNPay ? $"VNP{created:yyMMddHHmm}{Random.Shared.Next(100, 999)}" : null;
            order.Payments.Add(new Payment
            {
                Provider = paymentMethod == PaymentMethod.VNPay ? "VNPay" : "COD",
                TxnRef = paymentMethod == PaymentMethod.VNPay ? $"DEMO-{code}" : $"COD-{code}",
                TransactionNo = order.TransactionNo,
                Amount = order.TotalAmount,
                Status = paymentStatus,
                ResponseCode = paymentStatus == PaymentStatus.Paid ? "00" : paymentStatus == PaymentStatus.Failed ? "24" : null,
                TransactionStatus = paymentStatus == PaymentStatus.Paid ? "00" : paymentStatus == PaymentStatus.Failed ? "02" : null,
                BankCode = paymentMethod == PaymentMethod.VNPay ? "NCB" : null,
                CreatedDate = created,
                PaymentDate = paymentStatus == PaymentStatus.Paid ? created.AddMinutes(12) : null
            });

            db.Orders.Add(order);
        }

        AddOrder(customers["minh.anh@example.com"], $"ASD{now:yyMMdd}001", OrderStatus.Pending, PaymentMethod.COD, PaymentStatus.Pending, 0, "Gọi trước khi giao giúp mình.", (Pick("iphone-16-pro", "256GB"), 1), (Pick("airpods-pro-2"), 1));
        AddOrder(customers["quoc.bao@example.com"], $"ASD{now:yyMMdd}002", OrderStatus.Confirmed, PaymentMethod.VNPay, PaymentStatus.Paid, 1, null, (Pick("macbook-air-m3", "512GB"), 1));
        AddOrder(customers["hoang.linh@example.com"], $"ASD{now:yyMMdd}003", OrderStatus.Shipping, PaymentMethod.COD, PaymentStatus.Pending, 2, "Giao giờ hành chính.", (Pick("ipad-air-m2", "256GB"), 1), (Pick("airtag", "4 Pack"), 1));
        AddOrder(customers["thu.trang@example.com"], $"ASD{now:yyMMdd}004", OrderStatus.Completed, PaymentMethod.VNPay, PaymentStatus.Paid, 5, null, (Pick("apple-watch-series-10", "46mm"), 1));
        AddOrder(customers["ngoc.han@example.com"], $"ASD{now:yyMMdd}005", OrderStatus.Completed, PaymentMethod.COD, PaymentStatus.Paid, 8, "Xuất hóa đơn công ty.", (Pick("iphone-16", "256GB"), 1), (Pick("magic-keyboard"), 1));
        AddOrder(customers["minh.anh@example.com"], $"ASD{now:yyMMdd}006", OrderStatus.Cancelled, PaymentMethod.VNPay, PaymentStatus.Cancelled, 12, "Khách đổi sang nhận tại cửa hàng.", (Pick("ipad-pro-m4", "256GB"), 1));
        AddOrder(customers["quoc.bao@example.com"], $"ASD{now:yyMMdd}007", OrderStatus.Completed, PaymentMethod.VNPay, PaymentStatus.Paid, 18, null, (Pick("macbook-pro-m4", "512GB"), 1), (Pick("airpods-4", "ANC"), 1));
        AddOrder(null, $"ASD{now:yyMMdd}008", OrderStatus.Pending, PaymentMethod.COD, PaymentStatus.Pending, 0, "Khách vãng lai, giao buổi tối.", (Pick("iphone-15", "128GB"), 1));
        AddOrder(customers["duc.khang@example.com"], $"ASD{now:yyMMdd}009", OrderStatus.Cancelled, PaymentMethod.COD, PaymentStatus.Cancelled, 24, "Không liên hệ được khách hàng.", (Pick("apple-watch-ultra-2", "49mm"), 1));
        AddOrder(customers["hoang.linh@example.com"], $"ASD{now:yyMMdd}010", OrderStatus.Completed, PaymentMethod.VNPay, PaymentStatus.Paid, 31, null, (Pick("imac-m4", "512GB"), 1));

        await db.SaveChangesAsync();
    }

    private static void AddProduct(List<Product> list, Category category, string name, string slug, decimal basePrice, bool featured, params (string color, string storage, decimal price, int stock)[] variants)
    {
        var product = new Product { Category = category, Name = name, Slug = slug, BasePrice = basePrice, IsFeatured = featured, Description = $"{name} mang đến trải nghiệm mạnh mẽ, tinh tế và liền mạch cho mọi nhu cầu hàng ngày.", ImageThumbnail = null };
        foreach (var v in variants) product.Variants.Add(new ProductVariant { Color = v.color, Storage = v.storage, Price = v.price, StockQuantity = v.stock, SKU = $"{slug.ToUpperInvariant().Replace('-', '-')}-{v.storage.Replace("/", "-").Replace(" ", "")}-{Guid.NewGuid():N}"[..30] });
        list.Add(product);
    }
}
