using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using AppleStore.Web.Data;

#nullable disable

namespace AppleStore.Web.Data.Migrations;

[Migration("202608220001_InitialCreate")]
[DbContext(typeof(ApplicationDbContext))]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder m)
    {
        m.Sql(@"
CREATE TABLE [AspNetRoles] ([Id] nvarchar(450) NOT NULL, [Name] nvarchar(256) NULL, [NormalizedName] nvarchar(256) NULL, [ConcurrencyStamp] nvarchar(max) NULL, CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id]));
CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
CREATE TABLE [AspNetUsers] ([Id] nvarchar(450) NOT NULL, [FullName] nvarchar(150) NOT NULL, [Address] nvarchar(500) NULL, [IsActive] bit NOT NULL, [CreatedDate] datetime2 NOT NULL, [UserName] nvarchar(256) NULL, [NormalizedUserName] nvarchar(256) NULL, [Email] nvarchar(256) NULL, [NormalizedEmail] nvarchar(256) NULL, [EmailConfirmed] bit NOT NULL, [PasswordHash] nvarchar(max) NULL, [SecurityStamp] nvarchar(max) NULL, [ConcurrencyStamp] nvarchar(max) NULL, [PhoneNumber] nvarchar(max) NULL, [PhoneNumberConfirmed] bit NOT NULL, [TwoFactorEnabled] bit NOT NULL, [LockoutEnd] datetimeoffset NULL, [LockoutEnabled] bit NOT NULL, [AccessFailedCount] int NOT NULL, CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id]));
CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
CREATE TABLE [AspNetRoleClaims] ([Id] int NOT NULL IDENTITY, [RoleId] nvarchar(450) NOT NULL, [ClaimType] nvarchar(max) NULL, [ClaimValue] nvarchar(max) NULL, CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]), CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE);
CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
CREATE TABLE [AspNetUserClaims] ([Id] int NOT NULL IDENTITY, [UserId] nvarchar(450) NOT NULL, [ClaimType] nvarchar(max) NULL, [ClaimValue] nvarchar(max) NULL, CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]), CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE);
CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
CREATE TABLE [AspNetUserLogins] ([LoginProvider] nvarchar(128) NOT NULL, [ProviderKey] nvarchar(128) NOT NULL, [ProviderDisplayName] nvarchar(max) NULL, [UserId] nvarchar(450) NOT NULL, CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]), CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE);
CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
CREATE TABLE [AspNetUserTokens] ([UserId] nvarchar(450) NOT NULL, [LoginProvider] nvarchar(128) NOT NULL, [Name] nvarchar(128) NOT NULL, [Value] nvarchar(max) NULL, CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]), CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE);
CREATE TABLE [AspNetUserRoles] ([UserId] nvarchar(450) NOT NULL, [RoleId] nvarchar(450) NOT NULL, CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]), CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE, CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE);
CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
CREATE TABLE [Categories] ([Id] int NOT NULL IDENTITY, [Name] nvarchar(120) NOT NULL, [Slug] nvarchar(140) NOT NULL, [Description] nvarchar(500) NULL, [IsActive] bit NOT NULL, [CreatedDate] datetime2 NOT NULL, CONSTRAINT [PK_Categories] PRIMARY KEY ([Id]));
CREATE UNIQUE INDEX [IX_Categories_Slug] ON [Categories] ([Slug]);
CREATE TABLE [Products] ([Id] int NOT NULL IDENTITY, [CategoryId] int NOT NULL, [Name] nvarchar(200) NOT NULL, [Slug] nvarchar(220) NOT NULL, [Description] nvarchar(max) NOT NULL, [BasePrice] decimal(18,2) NOT NULL, [ImageThumbnail] nvarchar(500) NULL, [IsFeatured] bit NOT NULL, [IsActive] bit NOT NULL, [CreatedDate] datetime2 NOT NULL, [UpdatedDate] datetime2 NULL, CONSTRAINT [PK_Products] PRIMARY KEY ([Id]), CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]));
CREATE UNIQUE INDEX [IX_Products_Slug] ON [Products] ([Slug]); CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);
CREATE TABLE [ProductVariants] ([Id] int NOT NULL IDENTITY, [ProductId] int NOT NULL, [Color] nvarchar(80) NOT NULL, [Storage] nvarchar(80) NOT NULL, [SKU] nvarchar(80) NOT NULL, [Price] decimal(18,2) NOT NULL, [StockQuantity] int NOT NULL, [IsActive] bit NOT NULL, CONSTRAINT [PK_ProductVariants] PRIMARY KEY ([Id]), CONSTRAINT [FK_ProductVariants_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE);
CREATE UNIQUE INDEX [IX_ProductVariants_SKU] ON [ProductVariants] ([SKU]); CREATE INDEX [IX_ProductVariants_ProductId] ON [ProductVariants] ([ProductId]);
CREATE TABLE [ProductImages] ([Id] int NOT NULL IDENTITY, [ProductId] int NOT NULL, [ImageUrl] nvarchar(500) NOT NULL, [IsPrimary] bit NOT NULL, CONSTRAINT [PK_ProductImages] PRIMARY KEY ([Id]), CONSTRAINT [FK_ProductImages_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE);
CREATE INDEX [IX_ProductImages_ProductId] ON [ProductImages] ([ProductId]);
CREATE TABLE [Carts] ([Id] int NOT NULL IDENTITY, [UserId] nvarchar(80) NOT NULL, [UpdatedDate] datetime2 NOT NULL, CONSTRAINT [PK_Carts] PRIMARY KEY ([Id]));
CREATE UNIQUE INDEX [IX_Carts_UserId] ON [Carts] ([UserId]);
CREATE TABLE [Orders] ([Id] int NOT NULL IDENTITY, [OrderCode] nvarchar(40) NOT NULL, [UserId] nvarchar(450) NULL, [OrderDate] datetime2 NOT NULL, [CustomerName] nvarchar(150) NOT NULL, [Email] nvarchar(256) NOT NULL, [PhoneNumber] nvarchar(30) NOT NULL, [ShippingAddress] nvarchar(500) NOT NULL, [Note] nvarchar(1000) NULL, [PaymentMethod] int NOT NULL, [PaymentStatus] int NOT NULL, [TransactionNo] nvarchar(100) NULL, [PaymentDate] datetime2 NULL, [OrderStatus] int NOT NULL, [Subtotal] decimal(18,2) NOT NULL, [ShippingFee] decimal(18,2) NOT NULL, [TotalAmount] decimal(18,2) NOT NULL, [StockReserved] bit NOT NULL, [StockReleased] bit NOT NULL, [CreatedDate] datetime2 NOT NULL, [UpdatedDate] datetime2 NULL, CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]), CONSTRAINT [FK_Orders_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL);
CREATE UNIQUE INDEX [IX_Orders_OrderCode] ON [Orders] ([OrderCode]); CREATE INDEX [IX_Orders_UserId] ON [Orders] ([UserId]);
CREATE TABLE [CartItems] ([Id] int NOT NULL IDENTITY, [CartId] int NOT NULL, [ProductVariantId] int NOT NULL, [Quantity] int NOT NULL, CONSTRAINT [PK_CartItems] PRIMARY KEY ([Id]), CONSTRAINT [FK_CartItems_Carts_CartId] FOREIGN KEY ([CartId]) REFERENCES [Carts] ([Id]) ON DELETE CASCADE, CONSTRAINT [FK_CartItems_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([Id]));
CREATE UNIQUE INDEX [IX_CartItems_CartId_ProductVariantId] ON [CartItems] ([CartId], [ProductVariantId]);
CREATE TABLE [OrderDetails] ([Id] int NOT NULL IDENTITY, [OrderId] int NOT NULL, [ProductVariantId] int NOT NULL, [ProductName] nvarchar(200) NOT NULL, [VariantName] nvarchar(180) NOT NULL, [SKU] nvarchar(80) NOT NULL, [Quantity] int NOT NULL, [UnitPrice] decimal(18,2) NOT NULL, [TotalPrice] decimal(18,2) NOT NULL, CONSTRAINT [PK_OrderDetails] PRIMARY KEY ([Id]), CONSTRAINT [FK_OrderDetails_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE, CONSTRAINT [FK_OrderDetails_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([Id]));
CREATE INDEX [IX_OrderDetails_OrderId] ON [OrderDetails] ([OrderId]); CREATE INDEX [IX_OrderDetails_ProductVariantId] ON [OrderDetails] ([ProductVariantId]);
CREATE TABLE [Payments] ([Id] int NOT NULL IDENTITY, [OrderId] int NOT NULL, [Provider] nvarchar(30) NOT NULL, [TxnRef] nvarchar(80) NOT NULL, [TransactionNo] nvarchar(100) NULL, [Amount] decimal(18,2) NOT NULL, [Status] int NOT NULL, [ResponseCode] nvarchar(10) NULL, [TransactionStatus] nvarchar(10) NULL, [BankCode] nvarchar(50) NULL, [CreatedDate] datetime2 NOT NULL, [PaymentDate] datetime2 NULL, [RawResponse] nvarchar(4000) NULL, CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]), CONSTRAINT [FK_Payments_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE);
CREATE UNIQUE INDEX [IX_Payments_TxnRef] ON [Payments] ([TxnRef]); CREATE INDEX [IX_Payments_OrderId] ON [Payments] ([OrderId]);");
    }
    protected override void Down(MigrationBuilder m)
    {
        m.Sql("DROP TABLE IF EXISTS [Payments]; DROP TABLE IF EXISTS [OrderDetails]; DROP TABLE IF EXISTS [CartItems]; DROP TABLE IF EXISTS [Orders]; DROP TABLE IF EXISTS [Carts]; DROP TABLE IF EXISTS [ProductImages]; DROP TABLE IF EXISTS [ProductVariants]; DROP TABLE IF EXISTS [Products]; DROP TABLE IF EXISTS [Categories]; DROP TABLE IF EXISTS [AspNetUserRoles]; DROP TABLE IF EXISTS [AspNetUserTokens]; DROP TABLE IF EXISTS [AspNetUserLogins]; DROP TABLE IF EXISTS [AspNetUserClaims]; DROP TABLE IF EXISTS [AspNetRoleClaims]; DROP TABLE IF EXISTS [AspNetUsers]; DROP TABLE IF EXISTS [AspNetRoles];");
    }
    protected override void BuildTargetModel(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder) { }
}
