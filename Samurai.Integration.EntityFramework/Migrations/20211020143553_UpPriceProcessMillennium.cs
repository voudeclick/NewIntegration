using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class UpPriceProcessMillennium : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstPrice",
                table: "MillenniumNewPricesProcesses",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastPrice",
                table: "MillenniumNewPricesProcesses",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstPrice",
                table: "MillenniumNewPricesProcesses");

            migrationBuilder.DropColumn(
                name: "LastPrice",
                table: "MillenniumNewPricesProcesses");
        }
    }
}
