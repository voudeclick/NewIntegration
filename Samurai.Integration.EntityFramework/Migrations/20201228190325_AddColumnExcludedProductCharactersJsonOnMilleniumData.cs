using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddColumnExcludedProductCharactersJsonOnMilleniumData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExcludedProductCharactersJson",
                table: "MillenniumData",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExcludedProductCharactersJson",
                table: "MillenniumData");
        }
    }
}
