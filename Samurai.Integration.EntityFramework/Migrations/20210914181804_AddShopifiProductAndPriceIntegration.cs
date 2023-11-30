using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddShopifiProductAndPriceIntegration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShopifyProductIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<long>(nullable: false),
                    ProductShopifyId = table.Column<long>(nullable: true),
                    ProductShopifySku = table.Column<string>(nullable: true),
                    Payload = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    ReferenceIntegrationId = table.Column<Guid>(nullable: true),
                    Result = table.Column<string>(nullable: true),
                    IntegrationDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopifyProductIntegrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopifyProductPriceIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<long>(nullable: false),
                    ProductShopifyId = table.Column<long>(nullable: true),
                    ProductShopifySku = table.Column<string>(nullable: true),
                    Payload = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    ReferenceIntegrationId = table.Column<Guid>(nullable: true),
                    Result = table.Column<string>(nullable: true),
                    IntegrationDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopifyProductPriceIntegrations", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopifyProductIntegrations");

            migrationBuilder.DropTable(
                name: "ShopifyProductPriceIntegrations");
        }
    }
}
