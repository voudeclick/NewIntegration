using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddOmieData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OmieData",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    AppKey = table.Column<string>(nullable: false),
                    AppSecret = table.Column<string>(nullable: false),
                    OrderPrefix = table.Column<string>(nullable: true),
                    CodigoLocalEstoque = table.Column<int>(nullable: true),
                    CodigoCategoria = table.Column<string>(nullable: true),
                    CodigoContaCorrente = table.Column<int>(nullable: true),
                    SendNotaFiscalEmailToClient = table.Column<bool>(nullable: false),
                    ExtraNotaFiscalEmailsJson = table.Column<string>(nullable: true),
                    VariantCaracteristicasJson = table.Column<string>(nullable: true),
                    CategoryCaracteristicasJson = table.Column<string>(nullable: true),
                    FreteMappingJson = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OmieData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OmieData_Tenants_Id",
                        column: x => x.Id,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OmieData");
        }
    }
}
