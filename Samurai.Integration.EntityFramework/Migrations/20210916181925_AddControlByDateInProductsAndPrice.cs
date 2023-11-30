using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddControlByDateInProductsAndPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FinalUpdateDate",
                table: "MillenniumNewProductProcesses",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InitialUpdateDate",
                table: "MillenniumNewProductProcesses",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinalUpdateDate",
                table: "MillenniumNewPricesProcesses",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InitialUpdateDate",
                table: "MillenniumNewPricesProcesses",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ControlPriceByUpdateDate",
                table: "MillenniumData",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ControlProductByUpdateDate",
                table: "MillenniumData",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableSaveProcessIntegrations",
                table: "MillenniumData",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalUpdateDate",
                table: "MillenniumNewProductProcesses");

            migrationBuilder.DropColumn(
                name: "InitialUpdateDate",
                table: "MillenniumNewProductProcesses");

            migrationBuilder.DropColumn(
                name: "FinalUpdateDate",
                table: "MillenniumNewPricesProcesses");

            migrationBuilder.DropColumn(
                name: "InitialUpdateDate",
                table: "MillenniumNewPricesProcesses");

            migrationBuilder.DropColumn(
                name: "ControlPriceByUpdateDate",
                table: "MillenniumData");

            migrationBuilder.DropColumn(
                name: "ControlProductByUpdateDate",
                table: "MillenniumData");

            migrationBuilder.DropColumn(
                name: "EnableSaveProcessIntegrations",
                table: "MillenniumData");
        }
    }
}
