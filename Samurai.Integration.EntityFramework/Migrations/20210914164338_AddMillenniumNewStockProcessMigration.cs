using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddMillenniumNewStockProcessMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MillenniumNewStockProcessId",
                table: "MillenniumProductStockIntegrations",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MillenniumNewStockProcesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<long>(nullable: false),
                    ProcessDate = table.Column<DateTime>(nullable: false),
                    InitialTransId = table.Column<long>(nullable: true),
                    FinalTransId = table.Column<long>(nullable: true),
                    InitialUpdateDate = table.Column<DateTime>(nullable: true),
                    FinalUpdateDate = table.Column<DateTime>(nullable: true),
                    Top = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumNewStockProcesses", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MillenniumNewStockProcesses");

            migrationBuilder.DropColumn(
                name: "MillenniumNewStockProcessId",
                table: "MillenniumProductStockIntegrations");
        }
    }
}
