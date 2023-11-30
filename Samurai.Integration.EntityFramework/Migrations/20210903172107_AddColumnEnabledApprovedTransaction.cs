using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddColumnEnabledApprovedTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnabledApprovedTransaction",
                table: "MillenniumData",
                nullable: true,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE MillenniumData SET EnabledApprovedTransaction = 0 WHERE EnabledApprovedTransaction IS NULL");

            migrationBuilder.AlterColumn<bool>(
                name: "EnabledApprovedTransaction",
                table: "MillenniumData",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnabledApprovedTransaction",
                table: "MillenniumData");
        }
    }
}
