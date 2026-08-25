/*
    APPLE STORE - SQL SERVER DATABASE SCHEMA
    Target: SQL Server 2022 / EF Core 8 / ASP.NET Core Identity

    Cach su dung:
      - Cach thong thuong: khong can chay file nay. Ung dung tu dong chay EF Core
        migrations va SeedData khi khoi dong.
      - Cach khoi tao thu cong: chay toan bo file bang SSMS hoac sqlcmd. Script
        tao database AppleStoreDb, cac bang, khoa, chi muc va danh dau ba migration
        hien co la da ap dung. SeedData cua ung dung se bo sung role, tai khoan demo,
        danh muc, san pham va don hang mau o lan khoi dong tiep theo.

    Script khong DROP bang hay xoa du lieu va co the chay lai an toan tren database
    da duoc khoi tao day du.

    Gia tri enum luu trong Orders/Payments:
      PaymentMethod: 0 = COD, 1 = VNPay
      PaymentStatus: 0 = Pending, 1 = Paid, 2 = Failed,
                     3 = Refunded, 4 = Cancelled
      OrderStatus:   0 = Pending, 1 = Confirmed, 2 = Shipping,
                     3 = Completed, 4 = Cancelled
*/

USE [master];
GO

IF DB_ID(N'AppleStoreDb') IS NULL
BEGIN
    CREATE DATABASE [AppleStoreDb];
END;
GO

USE [AppleStoreDb];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

/* ========================= EF CORE MIGRATION HISTORY ====================== */

IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[__EFMigrationsHistory]
    (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

/* ========================== ASP.NET CORE IDENTITY ========================= */

IF OBJECT_ID(N'[dbo].[AspNetRoles]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetRoles]
    (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );

    CREATE UNIQUE INDEX [RoleNameIndex]
        ON [dbo].[AspNetRoles] ([NormalizedName])
        WHERE [NormalizedName] IS NOT NULL;
END;

IF OBJECT_ID(N'[dbo].[AspNetUsers]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUsers]
    (
        [Id] nvarchar(450) NOT NULL,
        [FullName] nvarchar(150) NOT NULL,
        [Address] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );

    CREATE INDEX [EmailIndex]
        ON [dbo].[AspNetUsers] ([NormalizedEmail]);
    CREATE INDEX [IX_AspNetUsers_Email]
        ON [dbo].[AspNetUsers] ([Email]);
    CREATE UNIQUE INDEX [UserNameIndex]
        ON [dbo].[AspNetUsers] ([NormalizedUserName])
        WHERE [NormalizedUserName] IS NOT NULL;
END;

IF OBJECT_ID(N'[dbo].[AspNetRoleClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetRoleClaims]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
            FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id])
            ON DELETE CASCADE
    );

    CREATE INDEX [IX_AspNetRoleClaims_RoleId]
        ON [dbo].[AspNetRoleClaims] ([RoleId]);
END;

IF OBJECT_ID(N'[dbo].[AspNetUserClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserClaims]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
            ON DELETE CASCADE
    );

    CREATE INDEX [IX_AspNetUserClaims_UserId]
        ON [dbo].[AspNetUserClaims] ([UserId]);
END;

IF OBJECT_ID(N'[dbo].[AspNetUserLogins]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserLogins]
    (
        [LoginProvider] nvarchar(128) NOT NULL,
        [ProviderKey] nvarchar(128) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins]
            PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
            ON DELETE CASCADE
    );

    CREATE INDEX [IX_AspNetUserLogins_UserId]
        ON [dbo].[AspNetUserLogins] ([UserId]);
END;

IF OBJECT_ID(N'[dbo].[AspNetUserTokens]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserTokens]
    (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(128) NOT NULL,
        [Name] nvarchar(128) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens]
            PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
            ON DELETE CASCADE
    );
END;

IF OBJECT_ID(N'[dbo].[AspNetUserRoles]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserRoles]
    (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
            ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
            FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id])
            ON DELETE CASCADE
    );

    CREATE INDEX [IX_AspNetUserRoles_RoleId]
        ON [dbo].[AspNetUserRoles] ([RoleId]);
END;

/* ========================== PRODUCT CATALOG =============================== */

IF OBJECT_ID(N'[dbo].[Categories]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Categories]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(120) NOT NULL,
        [Slug] nvarchar(140) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
    );

    CREATE UNIQUE INDEX [IX_Categories_Slug]
        ON [dbo].[Categories] ([Slug]);
END;

IF OBJECT_ID(N'[dbo].[Products]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Products]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [CategoryId] int NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Slug] nvarchar(220) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [BasePrice] decimal(18,2) NOT NULL,
        [ImageThumbnail] nvarchar(500) NULL,
        [IsFeatured] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Products_Categories_CategoryId]
            FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories] ([Id])
            ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX [IX_Products_Slug]
        ON [dbo].[Products] ([Slug]);
    CREATE INDEX [IX_Products_CategoryId]
        ON [dbo].[Products] ([CategoryId]);
END;

IF OBJECT_ID(N'[dbo].[ProductVariants]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ProductVariants]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [ProductId] int NOT NULL,
        [Color] nvarchar(80) NOT NULL,
        [Storage] nvarchar(80) NOT NULL,
        [SKU] nvarchar(80) NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [StockQuantity] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_ProductVariants] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductVariants_Products_ProductId]
            FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id])
            ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX [IX_ProductVariants_SKU]
        ON [dbo].[ProductVariants] ([SKU]);
    CREATE INDEX [IX_ProductVariants_ProductId]
        ON [dbo].[ProductVariants] ([ProductId]);
END;

IF OBJECT_ID(N'[dbo].[ProductImages]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ProductImages]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [ProductId] int NOT NULL,
        [ImageUrl] nvarchar(500) NOT NULL,
        [IsPrimary] bit NOT NULL,
        CONSTRAINT [PK_ProductImages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductImages_Products_ProductId]
            FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id])
            ON DELETE CASCADE
    );

    CREATE INDEX [IX_ProductImages_ProductId]
        ON [dbo].[ProductImages] ([ProductId]);
END;

/* =============================== CART ===================================== */

IF OBJECT_ID(N'[dbo].[Carts]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Carts]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        -- UserId chua user Id hoac khoa guest:<guid>, nen khong tao foreign key.
        [UserId] nvarchar(80) NOT NULL,
        [UpdatedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Carts] PRIMARY KEY ([Id])
    );

    CREATE UNIQUE INDEX [IX_Carts_UserId]
        ON [dbo].[Carts] ([UserId]);
END;

IF OBJECT_ID(N'[dbo].[CartItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CartItems]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [CartId] int NOT NULL,
        [ProductVariantId] int NOT NULL,
        [Quantity] int NOT NULL,
        CONSTRAINT [PK_CartItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CartItems_Carts_CartId]
            FOREIGN KEY ([CartId]) REFERENCES [dbo].[Carts] ([Id])
            ON DELETE CASCADE,
        CONSTRAINT [FK_CartItems_ProductVariants_ProductVariantId]
            FOREIGN KEY ([ProductVariantId])
            REFERENCES [dbo].[ProductVariants] ([Id])
            ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX [IX_CartItems_CartId_ProductVariantId]
        ON [dbo].[CartItems] ([CartId], [ProductVariantId]);
    CREATE INDEX [IX_CartItems_ProductVariantId]
        ON [dbo].[CartItems] ([ProductVariantId]);
END;

/* ========================== ORDER AND PAYMENT ============================= */

IF OBJECT_ID(N'[dbo].[Orders]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Orders]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [OrderCode] nvarchar(40) NOT NULL,
        -- NULL voi don hang cua khach vang lai.
        [UserId] nvarchar(450) NULL,
        [OrderDate] datetime2 NOT NULL,
        [CustomerName] nvarchar(150) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [PhoneNumber] nvarchar(30) NOT NULL,
        [ShippingAddress] nvarchar(500) NOT NULL,
        [Note] nvarchar(1000) NULL,
        [PaymentMethod] int NOT NULL,
        [PaymentStatus] int NOT NULL,
        [TransactionNo] nvarchar(100) NULL,
        [PaymentDate] datetime2 NULL,
        [OrderStatus] int NOT NULL,
        [Subtotal] decimal(18,2) NOT NULL,
        [ShippingFee] decimal(18,2) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [StockReserved] bit NOT NULL,
        [StockReleased] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Orders_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
            ON DELETE SET NULL
    );

    CREATE UNIQUE INDEX [IX_Orders_OrderCode]
        ON [dbo].[Orders] ([OrderCode]);
    CREATE INDEX [IX_Orders_UserId]
        ON [dbo].[Orders] ([UserId]);
END;

IF OBJECT_ID(N'[dbo].[OrderDetails]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[OrderDetails]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [OrderId] int NOT NULL,
        [ProductVariantId] int NOT NULL,
        -- Cac cot snapshot giu nguyen thong tin luc dat hang.
        [ProductName] nvarchar(200) NOT NULL,
        [VariantName] nvarchar(180) NOT NULL,
        [SKU] nvarchar(80) NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [TotalPrice] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_OrderDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderDetails_Orders_OrderId]
            FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id])
            ON DELETE CASCADE,
        CONSTRAINT [FK_OrderDetails_ProductVariants_ProductVariantId]
            FOREIGN KEY ([ProductVariantId])
            REFERENCES [dbo].[ProductVariants] ([Id])
            ON DELETE NO ACTION
    );

    CREATE INDEX [IX_OrderDetails_OrderId]
        ON [dbo].[OrderDetails] ([OrderId]);
    CREATE INDEX [IX_OrderDetails_ProductVariantId]
        ON [dbo].[OrderDetails] ([ProductVariantId]);
END;

IF OBJECT_ID(N'[dbo].[Payments]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Payments]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [OrderId] int NOT NULL,
        [Provider] nvarchar(30) NOT NULL,
        [TxnRef] nvarchar(80) NOT NULL,
        [TransactionNo] nvarchar(100) NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [ResponseCode] nvarchar(10) NULL,
        [TransactionStatus] nvarchar(10) NULL,
        [BankCode] nvarchar(50) NULL,
        [CreatedDate] datetime2 NOT NULL,
        [PaymentDate] datetime2 NULL,
        [RawResponse] nvarchar(4000) NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Payments_Orders_OrderId]
            FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id])
            ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX [IX_Payments_TxnRef]
        ON [dbo].[Payments] ([TxnRef]);
    CREATE INDEX [IX_Payments_OrderId]
        ON [dbo].[Payments] ([OrderId]);
END;

/* Danh dau schema tuong ung voi cac migration dang co trong source code. */
IF NOT EXISTS
(
    SELECT 1 FROM [dbo].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'202608220001_InitialCreate'
)
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'202608220001_InitialCreate', N'8.0.19');
END;

IF NOT EXISTS
(
    SELECT 1 FROM [dbo].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'202608220002_AllowGuestCarts'
)
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'202608220002_AllowGuestCarts', N'8.0.19');
END;

IF NOT EXISTS
(
    SELECT 1 FROM [dbo].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'202608220003_ClearSeedPlaceholderImages'
)
BEGIN
    UPDATE [dbo].[Products]
    SET [ImageThumbnail] = NULL
    WHERE [ImageThumbnail] = N'/images/products/placeholder.svg';

    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'202608220003_ClearSeedPlaceholderImages', N'8.0.19');
END;

COMMIT TRANSACTION;
GO

PRINT N'AppleStoreDb da san sang. Khoi dong ung dung de chay SeedData idempotent.';
GO
