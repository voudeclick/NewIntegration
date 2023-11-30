using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    Status = table.Column<bool>(nullable: false),
                    ProductIntegrationStatus = table.Column<bool>(nullable: false),
                    OrderIntegrationStatus = table.Column<bool>(nullable: false),
                    StoreName = table.Column<string>(nullable: false),
                    StoreHandle = table.Column<string>(nullable: false),
                    ShopifyStoreDomain = table.Column<string>(nullable: false),
                    ShopifyAppJson = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MillenniumData",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    LoginJson = table.Column<string>(nullable: false),
                    Url = table.Column<string>(nullable: false),
                    VitrineId = table.Column<long>(nullable: false),
                    DescriptionField = table.Column<string>(nullable: false),
                    OrderPrefix = table.Column<string>(nullable: true),
                    CorDescription = table.Column<string>(nullable: true),
                    TamanhoDescription = table.Column<string>(nullable: true),
                    EstampaDescription = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MillenniumData_Tenants_Id",
                        column: x => x.Id,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MillenniumTransId",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MillenniumDataId = table.Column<long>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Value = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumTransId", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MillenniumTransId_MillenniumData_MillenniumDataId",
                        column: x => x.MillenniumDataId,
                        principalTable: "MillenniumData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MillenniumTransId_MillenniumDataId",
                table: "MillenniumTransId",
                column: "MillenniumDataId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MillenniumTransId");

            migrationBuilder.DropTable(
                name: "MillenniumData");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
