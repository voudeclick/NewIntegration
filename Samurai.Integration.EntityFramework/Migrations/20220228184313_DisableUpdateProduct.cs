using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class DisableUpdateProduct  : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {           
            migrationBuilder.RenameColumn(
                name: "EnableUpdateProduct",
                newName: "DisableUpdateProduct",
                table: "SellerCenterData"
                );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
               name: "DisableUpdateProduct",
               newName: "EnableUpdateProduct",
               table: "SellerCenterData"
               );
        }
    }
}
