using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class AddNexasPickupPoint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryTimeTemplate",
                table: "NexaasData",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPickupPointEnabled",
                table: "NexaasData",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ServiceNameTemplate",
                table: "NexaasData",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryTimeTemplate",
                table: "NexaasData");

            migrationBuilder.DropColumn(
                name: "IsPickupPointEnabled",
                table: "NexaasData");

            migrationBuilder.DropColumn(
                name: "ServiceNameTemplate",
                table: "NexaasData");
        }
    }
}
