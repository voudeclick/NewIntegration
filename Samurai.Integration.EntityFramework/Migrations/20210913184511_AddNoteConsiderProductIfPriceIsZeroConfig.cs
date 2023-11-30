using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddNoteConsiderProductIfPriceIsZeroConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ZeroedPriceCase",
                table: "Tenants");

            migrationBuilder.AddColumn<bool>(
                name: "NotConsiderProductIfPriceIsZero",
                table: "ShopifyData",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotConsiderProductIfPriceIsZero",
                table: "ShopifyData");

            migrationBuilder.AddColumn<bool>(
                name: "ZeroedPriceCase",
                table: "Tenants",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
