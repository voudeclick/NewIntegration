using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddFulfillmentNotificationSettingShopifyDataMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BlockFulfillmentNotificationPerShipmentService",
                table: "ShopifyData",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ShipmentServicesForFulfillmentNotification",
                table: "ShopifyData",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockFulfillmentNotificationPerShipmentService",
                table: "ShopifyData");

            migrationBuilder.DropColumn(
                name: "ShipmentServicesForFulfillmentNotification",
                table: "ShopifyData");
        }
    }
}
