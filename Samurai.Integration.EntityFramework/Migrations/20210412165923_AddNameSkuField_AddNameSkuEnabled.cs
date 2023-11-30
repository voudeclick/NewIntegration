using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddNameSkuField_AddNameSkuEnabled : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NameSkuEnabled",
                table: "MillenniumData",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NameSkuField",
                table: "MillenniumData",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameSkuEnabled",
                table: "MillenniumData");

            migrationBuilder.DropColumn(
                name: "NameSkuField",
                table: "MillenniumData");
        }
    }
}
