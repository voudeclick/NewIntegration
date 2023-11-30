using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddImageIntegrationSaves : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MillenniumListProductProcessId",
                table: "MillenniumImageIntegrations");

            migrationBuilder.DropColumn(
                name: "Routine",
                table: "MillenniumImageIntegrations");

            migrationBuilder.AddColumn<string>(
                name: "Exception",
                table: "MillenniumImageIntegrations",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MillenniumIntegrationProductReferenceId",
                table: "MillenniumImageIntegrations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MillenniumResult",
                table: "MillenniumImageIntegrations",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MillenniumProductImageIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<long>(nullable: false),
                    IdProduto = table.Column<long>(nullable: false),
                    ExternalId = table.Column<string>(nullable: true),
                    Payload = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    IntegrationDate = table.Column<DateTime>(nullable: false),
                    MillenniumListProductProcessId = table.Column<Guid>(nullable: true),
                    MillenniumIntegrationProductReferenceId = table.Column<Guid>(nullable: true),
                    Routine = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumProductImageIntegrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopifyProductImageIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<long>(nullable: false),
                    ProductShopifyId = table.Column<long>(nullable: true),
                    ExternalProductId = table.Column<string>(nullable: true),
                    Images = table.Column<string>(nullable: true),
                    Payload = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    ReferenceIntegrationId = table.Column<Guid>(nullable: true),
                    Result = table.Column<string>(nullable: true),
                    IntegrationDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopifyProductImageIntegrations", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MillenniumProductImageIntegrations");

            migrationBuilder.DropTable(
                name: "ShopifyProductImageIntegrations");

            migrationBuilder.DropColumn(
                name: "Exception",
                table: "MillenniumImageIntegrations");

            migrationBuilder.DropColumn(
                name: "MillenniumIntegrationProductReferenceId",
                table: "MillenniumImageIntegrations");

            migrationBuilder.DropColumn(
                name: "MillenniumResult",
                table: "MillenniumImageIntegrations");

            migrationBuilder.AddColumn<Guid>(
                name: "MillenniumListProductProcessId",
                table: "MillenniumImageIntegrations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Routine",
                table: "MillenniumImageIntegrations",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
