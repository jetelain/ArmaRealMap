using Microsoft.EntityFrameworkCore.Migrations;

namespace ArmaRealMapWebSite.Migrations
{
    public partial class Libraries : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ObjectLibrary",
                columns: table => new
                {
                    ObjectLibraryID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    TerrainRegion = table.Column<int>(type: "INTEGER", nullable: false),
                    ObjectCategory = table.Column<int>(type: "INTEGER", nullable: false),
                    Density = table.Column<double>(type: "REAL", nullable: true),
                    Probability = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectLibrary", x => x.ObjectLibraryID);
                });

            migrationBuilder.CreateTable(
                name: "ObjectLibraryAsset",
                columns: table => new
                {
                    ObjectLibraryAssetID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ObjectLibraryID = table.Column<int>(type: "INTEGER", nullable: false),
                    AssetID = table.Column<int>(type: "INTEGER", nullable: false),
                    Probability = table.Column<float>(type: "REAL", nullable: true),
                    PlacementRadius = table.Column<float>(type: "REAL", nullable: true),
                    ReservedRadius = table.Column<float>(type: "REAL", nullable: true),
                    MaxZ = table.Column<float>(type: "REAL", nullable: true),
                    MinZ = table.Column<float>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectLibraryAsset", x => x.ObjectLibraryAssetID);
                    table.ForeignKey(
                        name: "FK_ObjectLibraryAsset_Asset_AssetID",
                        column: x => x.AssetID,
                        principalTable: "Asset",
                        principalColumn: "AssetID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ObjectLibraryAsset_ObjectLibrary_ObjectLibraryID",
                        column: x => x.ObjectLibraryID,
                        principalTable: "ObjectLibrary",
                        principalColumn: "ObjectLibraryID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ObjectLibraryAsset_AssetID",
                table: "ObjectLibraryAsset",
                column: "AssetID");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectLibraryAsset_ObjectLibraryID",
                table: "ObjectLibraryAsset",
                column: "ObjectLibraryID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObjectLibraryAsset");

            migrationBuilder.DropTable(
                name: "ObjectLibrary");
        }
    }
}
