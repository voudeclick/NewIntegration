using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddMilleniumImageAndListProductIntegration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MillenniumImageIntegrations",
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
                    Routine = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumImageIntegrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MillenniumListProductManualProcesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<long>(nullable: false),
                    ProductId = table.Column<string>(nullable: true),
                    ProcessDate = table.Column<DateTime>(nullable: false),
                    MillenniumResult = table.Column<string>(nullable: true),
                    Exception = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumListProductManualProcesses", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MillenniumImageIntegrations");

            migrationBuilder.DropTable(
                name: "MillenniumListProductManualProcesses");
        }
    }
}
