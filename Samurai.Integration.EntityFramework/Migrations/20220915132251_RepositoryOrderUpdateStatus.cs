using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class RepositoryOrderUpdateStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MillenniumOrderStatusUpdate",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CodPedidov = table.Column<string>(nullable: true),
                    OrderStatus = table.Column<int>(nullable: false),
                    Order = table.Column<string>(nullable: true),
                    CreationDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumOrderStatusUpdate", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MillenniumOrderStatusUpdate");
        }
    }
}
