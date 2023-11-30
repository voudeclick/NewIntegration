using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddillenniumLastUpdateDateAtMillenniumTransId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "MillenniumLastUpdateDate",
                table: "MillenniumTransId",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MillenniumLastUpdateDate",
                table: "MillenniumTransId");
        }
    }
}
