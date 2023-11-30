using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddMillenniumResultAtIntegrations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MillenniumResult",
                table: "MillenniumNewStockProcesses",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MillenniumResult",
                table: "MillenniumNewProductProcesses",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MillenniumResult",
                table: "MillenniumNewPricesProcesses",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MillenniumResult",
                table: "MillenniumNewStockProcesses");

            migrationBuilder.DropColumn(
                name: "MillenniumResult",
                table: "MillenniumNewProductProcesses");

            migrationBuilder.DropColumn(
                name: "MillenniumResult",
                table: "MillenniumNewPricesProcesses");
        }
    }
}
