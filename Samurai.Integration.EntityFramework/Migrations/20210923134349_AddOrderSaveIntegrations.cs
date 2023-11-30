using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddOrderSaveIntegrations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MillenniumUpdateOrderProcesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<long>(nullable: false),
                    OrderId = table.Column<long>(nullable: false),
                    ShopifyListOrderProcessReferenceId = table.Column<Guid>(nullable: true),
                    ProcessDate = table.Column<DateTime>(nullable: false),
                    MillenniumResponse = table.Column<string>(nullable: true),
                    Exception = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumUpdateOrderProcesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopifyListOrderIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<long>(nullable: false),
                    OrderId = table.Column<long>(nullable: false),
                    Payload = table.Column<string>(nullable: true),
                    Action = table.Column<string>(nullable: true),
                    IntegrationDate = table.Column<DateTime>(nullable: false),
                    ShopifyListOrderProcessId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopifyListOrderIntegrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopifyListOrderProcesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<long>(nullable: false),
                    OrderId = table.Column<long>(nullable: false),
                    ProcessDate = table.Column<DateTime>(nullable: false),
                    ShopifyResult = table.Column<string>(nullable: true),
                    Exception = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopifyListOrderProcesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopifyUpdateOrderTagNumberProcesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<long>(nullable: false),
                    OrderId = table.Column<long>(nullable: false),
                    OrderExternalId = table.Column<string>(nullable: true),
                    OrderNumber = table.Column<string>(nullable: true),
                    ProcessDate = table.Column<DateTime>(nullable: false),
                    OrderUpdateMutationInput = table.Column<string>(nullable: true),
                    ShopifyResult = table.Column<string>(nullable: true),
                    Exception = table.Column<string>(nullable: true),
                    ShopifyListOrderProcessReferenceId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopifyUpdateOrderTagNumberProcesses", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MillenniumUpdateOrderProcesses");

            migrationBuilder.DropTable(
                name: "ShopifyListOrderIntegrations");

            migrationBuilder.DropTable(
                name: "ShopifyListOrderProcesses");

            migrationBuilder.DropTable(
                name: "ShopifyUpdateOrderTagNumberProcesses");
        }
    }
}
