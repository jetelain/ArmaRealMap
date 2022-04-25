using Microsoft.EntityFrameworkCore.Migrations;

namespace ArmaRealMapWebSite.Migrations
{
    public partial class BoundingCenter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "BoundingCenterX",
                table: "Asset",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "BoundingCenterY",
                table: "Asset",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "BoundingCenterZ",
                table: "Asset",
                type: "REAL",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BoundingCenterX",
                table: "Asset");

            migrationBuilder.DropColumn(
                name: "BoundingCenterY",
                table: "Asset");

            migrationBuilder.DropColumn(
                name: "BoundingCenterZ",
                table: "Asset");
        }
    }
}
