using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddUserIdPluggToData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PluggToCode",
                table: "PluggToData");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "PluggToData",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PluggToData");

            migrationBuilder.AddColumn<string>(
                name: "PluggToCode",
                table: "PluggToData",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
