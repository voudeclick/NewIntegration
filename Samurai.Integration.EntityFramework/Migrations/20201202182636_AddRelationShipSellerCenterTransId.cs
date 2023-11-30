using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddRelationShipSellerCenterTransId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SellerCenterTransId_SellerCenterDataId",
                table: "SellerCenterTransId",
                column: "SellerCenterDataId");

            migrationBuilder.AddForeignKey(
                name: "FK_SellerCenterTransId_SellerCenterData_SellerCenterDataId",
                table: "SellerCenterTransId",
                column: "SellerCenterDataId",
                principalTable: "SellerCenterData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SellerCenterTransId_SellerCenterData_SellerCenterDataId",
                table: "SellerCenterTransId");

            migrationBuilder.DropIndex(
                name: "IX_SellerCenterTransId_SellerCenterDataId",
                table: "SellerCenterTransId");
        }
    }
}
