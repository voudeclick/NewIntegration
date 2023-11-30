using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddTableLocationMap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MultiLocation",
                table: "Tenants",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "LocationMap",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    JsonMap = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationMap", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationMap_Tenants_Id",
                        column: x => x.Id,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationMap");

            migrationBuilder.DropColumn(
                name: "MultiLocation",
                table: "Tenants");
        }
    }
}
