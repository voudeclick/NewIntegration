using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddTableShopifyData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ShopifyStoreDomain",
                table: "Tenants",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ShopifyAppJson",
                table: "Tenants",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "IntegrationType",
                table: "Tenants",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "ShopifyData",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    Status = table.Column<bool>(nullable: false),
                    ProductIntegrationStatus = table.Column<bool>(nullable: false),
                    SetProductsAsUnpublished = table.Column<bool>(nullable: false),
                    BodyIntegrationType = table.Column<int>(nullable: false),
                    ProductGroupingEnabled = table.Column<bool>(nullable: false),
                    ProductDescriptionIsHTML = table.Column<bool>(nullable: false),
                    WriteCategoryNameTags = table.Column<bool>(nullable: false),
                    ImageIntegrationEnabled = table.Column<bool>(nullable: false),
                    OrderIntegrationStatus = table.Column<bool>(nullable: false),
                    DisableCustomerDocument = table.Column<bool>(nullable: false),
                    DisableAddressParse = table.Column<bool>(nullable: false),
                    ParsePhoneDDD = table.Column<bool>(nullable: false),
                    ShopifyStoreDomain = table.Column<string>(nullable: true),
                    ShopifyAppJson = table.Column<string>(nullable: true),
                    DaysToDelivery = table.Column<int>(nullable: false),
                    MinOrderId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopifyData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopifyData_Tenants_Id",
                        column: x => x.Id,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopifyData");

            migrationBuilder.DropColumn(
                name: "IntegrationType",
                table: "Tenants");

            migrationBuilder.AlterColumn<string>(
                name: "ShopifyStoreDomain",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShopifyAppJson",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
