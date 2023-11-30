using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddProductGroupingEnabled : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ProductGroupingEnabled",
                table: "Tenants",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductGroupingEnabled",
                table: "Tenants");
        }
    }
}
