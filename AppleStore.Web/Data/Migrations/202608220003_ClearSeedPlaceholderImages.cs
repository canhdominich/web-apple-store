using AppleStore.Web.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppleStore.Web.Data.Migrations;

[Migration("202608220003_ClearSeedPlaceholderImages")]
[DbContext(typeof(ApplicationDbContext))]
public partial class ClearSeedPlaceholderImages : Migration
{
    protected override void Up(MigrationBuilder m)
    {
        m.Sql("UPDATE [Products] SET [ImageThumbnail] = NULL WHERE [ImageThumbnail] = N'/images/products/placeholder.svg';");
    }

    protected override void Down(MigrationBuilder m)
    {
        m.Sql("UPDATE [Products] SET [ImageThumbnail] = N'/images/products/placeholder.svg' WHERE [ImageThumbnail] IS NULL;");
    }

    protected override void BuildTargetModel(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder) { }
}
