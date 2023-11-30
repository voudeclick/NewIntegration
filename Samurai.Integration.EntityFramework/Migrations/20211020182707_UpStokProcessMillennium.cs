using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class UpStokProcessMillennium : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstId",
                table: "MillenniumNewStockProcesses",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastId",
                table: "MillenniumNewStockProcesses",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstId",
                table: "MillenniumNewStockProcesses");

            migrationBuilder.DropColumn(
                name: "LastId",
                table: "MillenniumNewStockProcesses");
        }
    }
}
