using AppleStore.Web.Data;
using AppleStore.Web.Models.Entities;
using AppleStore.Web.Models.ViewModels;
using AppleStore.Web.Repositories.Implementations;
using AppleStore.Web.Repositories.Interfaces;
using AppleStore.Web.Services.Implementations;
using AppleStore.Web.Services.Interfaces;
using AppleStore.Web.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sql => sql.EnableRetryOnFailure(5)));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
}).AddErrorDescriber<VietnameseIdentityErrorDescriber>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.Name = "AppleStore.Auth";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});
builder.Services.Configure<VnPaySettings>(builder.Configuration.GetSection("VnPaySettings"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((value, fieldName) => $"{fieldName} không hợp lệ.");
    options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(fieldName => $"Vui lòng nhập {fieldName}.");
    options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(() => "Thiếu thông tin bắt buộc.");
    options.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor(fieldName => $"{fieldName} không hợp lệ.");
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(value => $"Giá trị '{value}' không hợp lệ.");
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(fieldName => $"{fieldName} phải là số.");
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(fieldName => $"Vui lòng nhập {fieldName}.");
});
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICartSessionService, CartSessionService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();

var app = builder.Build();
await ApplyDatabaseAndSeedAsync(app.Services, app.Logger);
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();

static async Task ApplyDatabaseAndSeedAsync(IServiceProvider root, ILogger logger)
{
    const int retries = 10;
    for (var attempt = 1; attempt <= retries; attempt++)
    {
        try
        {
            await using var scope = root.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();
            await SeedData.InitializeAsync(scope.ServiceProvider);
            logger.LogInformation("Database migrations applied and seed completed.");
            return;
        }
        catch (Exception ex) when (attempt < retries)
        {
            logger.LogWarning(ex, "Database is not ready (attempt {Attempt}/{Retries}). Retrying...", attempt, retries);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
    logger.LogCritical("Database initialization failed after {Retries} attempts.", retries);
    throw new InvalidOperationException("Database initialization failed.");
}

public partial class Program { }
