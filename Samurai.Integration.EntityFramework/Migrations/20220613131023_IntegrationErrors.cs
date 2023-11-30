using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class IntegrationErrors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntegrationErrors",
                columns: table => new
                {
                    Tag = table.Column<string>(maxLength: 50, nullable: false),
                    Message = table.Column<string>(maxLength: 200, nullable: true),
                    Description = table.Column<string>(maxLength: 400, nullable: true),
                    MessagePattern = table.Column<string>(maxLength: 200, nullable: true),
                    SourceId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationErrors", x => x.Tag);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntegrationErrors");
        }
    }
}
