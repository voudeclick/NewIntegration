using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.Identity.Migrations
{
    public partial class AddRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "5bee2ae0-abf6-4154-9fd4-419019b73786", "6c8d3c79-1fb3-4789-a93b-50d0d3902c21", "Administrador", "ADMINISTRADOR" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "3baa77ad-5458-4f80-a30d-2546f82cfb86", "31a4ca00-ea5f-4d5b-ace0-2ca01fa2603b", "Suporte", "SUPORTE" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "b88219f4-634e-473b-a572-dfe800260e9e", "b6f0cd00-b812-459a-9189-c45f98f93f38", "Viewer", "VIEWER" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3baa77ad-5458-4f80-a30d-2546f82cfb86");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5bee2ae0-abf6-4154-9fd4-419019b73786");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b88219f4-634e-473b-a572-dfe800260e9e");
        }
    }
}
