using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddExtraPaymentInformationMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableExtraPaymentInformation",
                table: "MillenniumData",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UrlExtraPaymentInformation",
                table: "MillenniumData",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableExtraPaymentInformation",
                table: "MillenniumData");

            migrationBuilder.DropColumn(
                name: "UrlExtraPaymentInformation",
                table: "MillenniumData");
        }
    }
}
