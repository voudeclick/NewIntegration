using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddDelayProcessOfShopifyUpdateOrderMessagesAtShopifyData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DelayProcessOfShopifyUpdateOrderMessages",
                table: "ShopifyData",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DelayProcessOfShopifyUpdateOrderMessages",
                table: "ShopifyData");
        }
    }
}
