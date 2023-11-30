using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddIntegrationInfosStockMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {     
            migrationBuilder.CreateTable(
                name: "MillenniumProductStockIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<long>(nullable: false),
                    ProductId = table.Column<string>(nullable: true),
                    ProductSku = table.Column<string>(nullable: true),
                    Payload = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    IntegrationDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumProductStockIntegrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopifyProductStockIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<long>(nullable: false),
                    ProductShopifyId = table.Column<long>(nullable: true),
                    ProductShopifySku = table.Column<string>(nullable: true),
                    Payload = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    MillenniumIntegrationId = table.Column<Guid>(nullable: false),
                    Result = table.Column<string>(nullable: true),
                    IntegrationDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopifyProductStockIntegrations", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MillenniumProductStockIntegrations");

            migrationBuilder.DropTable(
                name: "ShopifyProductStockIntegrations");
        }
    }
}
