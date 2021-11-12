using Microsoft.EntityFrameworkCore.Migrations;

namespace ArmaRealMapWebSite.Migrations
{
    public partial class ClusterName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClusterName",
                table: "Asset",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClusterName",
                table: "Asset");
        }
    }
}
