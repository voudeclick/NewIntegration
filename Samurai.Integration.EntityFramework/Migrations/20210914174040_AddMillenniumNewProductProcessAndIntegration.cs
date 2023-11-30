using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddMillenniumNewProductProcessAndIntegration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MillenniumNewProductProcesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<long>(nullable: false),
                    ProcessDate = table.Column<DateTime>(nullable: false),
                    InitialTransId = table.Column<long>(nullable: true),
                    FinalTransId = table.Column<long>(nullable: true),
                    Top = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumNewProductProcesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MillenniumProductIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<long>(nullable: false),
                    ProductId = table.Column<string>(nullable: true),
                    Payload = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    IntegrationDate = table.Column<DateTime>(nullable: false),
                    MillenniumNewStockProcessId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumProductIntegrations", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MillenniumNewProductProcesses");

            migrationBuilder.DropTable(
                name: "MillenniumProductIntegrations");
        }
    }
}
