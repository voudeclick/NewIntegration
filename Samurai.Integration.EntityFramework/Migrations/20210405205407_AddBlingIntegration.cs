using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddBlingIntegration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlingData",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    StoreHandle = table.Column<string>(nullable: true),
                    ProductIntegrationStatus = table.Column<bool>(nullable: false),
                    OrderIntegrationStatus = table.Column<bool>(nullable: false),
                    ApiBaseUrl = table.Column<string>(nullable: false),
                    APIKey = table.Column<string>(nullable: false),
                    LastProductUpdateDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlingData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlingData_Tenants_Id",
                        column: x => x.Id,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlingData");
        }
    }
}
