using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddMillenniumNewPricesProcessAndIntegration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MillenniumNewPricesProcesses",
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
                    table.PrimaryKey("PK_MillenniumNewPricesProcesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MillenniumProductPriceIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<long>(nullable: false),
                    ProductId = table.Column<string>(nullable: true),
                    ProductSku = table.Column<string>(nullable: true),
                    Payload = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    IntegrationDate = table.Column<DateTime>(nullable: false),
                    MillenniumNewStockProcessId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumProductPriceIntegrations", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MillenniumNewPricesProcesses");

            migrationBuilder.DropTable(
                name: "MillenniumProductPriceIntegrations");
        }
    }
}
