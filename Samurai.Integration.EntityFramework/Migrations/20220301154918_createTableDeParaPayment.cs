using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class createTableDeParaPayment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {    
            migrationBuilder.CreateTable(
                name: "MethodPayment",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentTypeShopify = table.Column<string>(nullable: false),
                    PaymentTypeMillenniun = table.Column<string>(nullable: false),
                    TenantId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MethodPayment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MethodPayment_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MethodPayment_TenantId",
                table: "MethodPayment",
                column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MethodPayment");            
        }
    }
}
