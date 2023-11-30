using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class FixCollumNamesIntegrationsMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MillenniumNewStockProcessId",
                table: "MillenniumProductPriceIntegrations");

            migrationBuilder.DropColumn(
                name: "MillenniumNewStockProcessId",
                table: "MillenniumProductIntegrations");

            migrationBuilder.AddColumn<Guid>(
                name: "MillenniumNewPriceProcessId",
                table: "MillenniumProductPriceIntegrations",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MillenniumNewProductProcessId",
                table: "MillenniumProductIntegrations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MillenniumNewPriceProcessId",
                table: "MillenniumProductPriceIntegrations");

            migrationBuilder.DropColumn(
                name: "MillenniumNewProductProcessId",
                table: "MillenniumProductIntegrations");

            migrationBuilder.AddColumn<Guid>(
                name: "MillenniumNewStockProcessId",
                table: "MillenniumProductPriceIntegrations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MillenniumNewStockProcessId",
                table: "MillenniumProductIntegrations",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
