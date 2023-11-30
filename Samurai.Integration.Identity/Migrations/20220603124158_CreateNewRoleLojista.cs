using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.Identity.Migrations
{
    public partial class CreateNewRoleLojista : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "15c73368-5862-44b6-8e45-09d0e6bfb2f6", "9c9cb9d3-6ad3-449c-a5ff-f1d153f2a881", "Lojista", "LOJISTA" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "15c73368-5862-44b6-8e45-09d0e6bfb2f6");

        }
    }
}
