using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddSellerIdPluggToData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PluggToData");

            migrationBuilder.AddColumn<string>(
                name: "AccountSellerId",
                table: "PluggToData",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountUserId",
                table: "PluggToData",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountSellerId",
                table: "PluggToData");

            migrationBuilder.DropColumn(
                name: "AccountUserId",
                table: "PluggToData");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "PluggToData",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
