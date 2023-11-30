using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddExceptionAtPriceAndStockIntegration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Exception",
                table: "MillenniumNewStockProcesses",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Exception",
                table: "MillenniumNewPricesProcesses",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Exception",
                table: "MillenniumNewStockProcesses");

            migrationBuilder.DropColumn(
                name: "Exception",
                table: "MillenniumNewPricesProcesses");
        }
    }
}
