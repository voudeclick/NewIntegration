using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddMillenniumSendDefault : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SendDefaultCor",
                table: "MillenniumData",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SendDefaultEstampa",
                table: "MillenniumData",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SendDefaultTamanho",
                table: "MillenniumData",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SendDefaultCor",
                table: "MillenniumData");

            migrationBuilder.DropColumn(
                name: "SendDefaultEstampa",
                table: "MillenniumData");

            migrationBuilder.DropColumn(
                name: "SendDefaultTamanho",
                table: "MillenniumData");
        }
    }
}
