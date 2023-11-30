using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class logsAbandonMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LogsAbandonMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    LogId = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<long>(nullable: false),
                    Method = table.Column<string>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    WebJob = table.Column<string>(nullable: false),
                    Error = table.Column<string>(nullable: false),
                    Payload = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsAbandonMessages", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogsAbandonMessages");
        }
    }
}
