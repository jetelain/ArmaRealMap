using Microsoft.EntityFrameworkCore.Migrations;

namespace ArmaRealMapWebSite.Migrations
{
    public partial class Maps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Map",
                columns: table => new
                {
                    MapID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Label = table.Column<string>(type: "TEXT", nullable: true),
                    Workshop = table.Column<string>(type: "TEXT", nullable: true),
                    GridSize = table.Column<int>(type: "INTEGER", nullable: false),
                    CellSize = table.Column<double>(type: "REAL", nullable: false),
                    Resolution = table.Column<double>(type: "REAL", nullable: false),
                    TerrainRegion = table.Column<int>(type: "INTEGER", nullable: false),
                    MgrsBottomLeft = table.Column<string>(type: "TEXT", nullable: true),
                    Preview = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Map", x => x.MapID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Map_Name",
                table: "Map",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Map");
        }
    }
}
