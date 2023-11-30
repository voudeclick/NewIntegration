using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddMillenniumOptionFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorField",
                table: "MillenniumData",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstampaField",
                table: "MillenniumData",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TamanhoField",
                table: "MillenniumData",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorField",
                table: "MillenniumData");

            migrationBuilder.DropColumn(
                name: "EstampaField",
                table: "MillenniumData");

            migrationBuilder.DropColumn(
                name: "TamanhoField",
                table: "MillenniumData");
        }
    }
}
