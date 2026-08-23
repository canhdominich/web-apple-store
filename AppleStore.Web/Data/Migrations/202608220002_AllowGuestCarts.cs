using AppleStore.Web.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppleStore.Web.Data.Migrations;

[Migration("202608220002_AllowGuestCarts")]
[DbContext(typeof(ApplicationDbContext))]
public partial class AllowGuestCarts : Migration
{
    protected override void Up(MigrationBuilder m)
    {
        m.Sql(@"
DECLARE @fk nvarchar(128);
SELECT @fk = [name] FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'[dbo].[Carts]') AND referenced_object_id = OBJECT_ID(N'[dbo].[AspNetUsers]');
IF @fk IS NOT NULL EXEC(N'ALTER TABLE [dbo].[Carts] DROP CONSTRAINT [' + @fk + N']');
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Carts_UserId' AND object_id = OBJECT_ID(N'[dbo].[Carts]'))
    DROP INDEX [IX_Carts_UserId] ON [dbo].[Carts];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Carts]') AND name = N'UserId' AND max_length <> 160)
    ALTER TABLE [dbo].[Carts] ALTER COLUMN [UserId] nvarchar(80) NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Carts_UserId' AND object_id = OBJECT_ID(N'[dbo].[Carts]'))
    CREATE UNIQUE INDEX [IX_Carts_UserId] ON [dbo].[Carts]([UserId]);
");
    }

    protected override void Down(MigrationBuilder m)
    {
        m.Sql(@"
IF EXISTS (SELECT 1 FROM [dbo].[Carts] WHERE [UserId] LIKE N'guest:%')
    THROW 50001, N'Không thể khôi phục ràng buộc user vì vẫn còn giỏ hàng khách vãng lai.', 1;
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Carts_UserId' AND object_id = OBJECT_ID(N'[dbo].[Carts]'))
    DROP INDEX [IX_Carts_UserId] ON [dbo].[Carts];
ALTER TABLE [dbo].[Carts] ALTER COLUMN [UserId] nvarchar(450) NOT NULL;
CREATE UNIQUE INDEX [IX_Carts_UserId] ON [dbo].[Carts]([UserId]);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'[dbo].[Carts]') AND referenced_object_id = OBJECT_ID(N'[dbo].[AspNetUsers]'))
    ALTER TABLE [dbo].[Carts] ADD CONSTRAINT [FK_Carts_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE;
");
    }

    protected override void BuildTargetModel(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder) { }
}
