﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddColumnEnabledStockMto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnabledStockMto",
                table: "MillenniumData",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnabledStockMto",
                table: "MillenniumData");
        }
    }
}
