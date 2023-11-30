using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class FixNameOfReferenceIntegrationIdShopifyStockIntegration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MillenniumIntegrationId",
                table: "ShopifyProductStockIntegrations");

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceIntegrationId",
                table: "ShopifyProductStockIntegrations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferenceIntegrationId",
                table: "ShopifyProductStockIntegrations");

            migrationBuilder.AddColumn<Guid>(
                name: "MillenniumIntegrationId",
                table: "ShopifyProductStockIntegrations",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
