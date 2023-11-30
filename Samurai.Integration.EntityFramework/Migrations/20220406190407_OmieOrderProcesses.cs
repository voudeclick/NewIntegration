using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class OmieOrderProcesses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OmieUpdateOrderProcesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<long>(nullable: false),
                    OrderId = table.Column<long>(nullable: false),
                    Payload = table.Column<string>(nullable: true),
                    ShopifyListOrderProcessReferenceId = table.Column<Guid>(nullable: true),
                    ProcessDate = table.Column<DateTime>(nullable: false),
                    OmieResponse = table.Column<string>(nullable: true),
                    Exception = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OmieUpdateOrderProcesses", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OmieUpdateOrderProcesses");
        }
    }
}
