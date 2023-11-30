﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddOmieCenarioImposto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CodigoCenarioImposto",
                table: "OmieData",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoCenarioImposto",
                table: "OmieData");
        }
    }
}
