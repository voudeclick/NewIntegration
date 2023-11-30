using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddSaveIntegrationInformationsConfigMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableSaveIntegrationInformations",
                table: "ShopifyData",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableSaveIntegrationInformations",
                table: "MillenniumData",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableSaveIntegrationInformations",
                table: "ShopifyData");

            migrationBuilder.DropColumn(
                name: "EnableSaveIntegrationInformations",
                table: "MillenniumData");
        }
    }
}
