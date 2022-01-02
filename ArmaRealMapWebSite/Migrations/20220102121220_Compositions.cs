using Microsoft.EntityFrameworkCore.Migrations;

namespace ArmaRealMapWebSite.Migrations
{
    public partial class Compositions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ObjectLibraryComposition",
                columns: table => new
                {
                    ObjectLibraryCompositionID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ObjectLibraryID = table.Column<int>(type: "INTEGER", nullable: false),
                    Probability = table.Column<float>(type: "REAL", nullable: true),
                    Width = table.Column<float>(type: "REAL", nullable: false),
                    Depth = table.Column<float>(type: "REAL", nullable: false),
                    Height = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectLibraryComposition", x => x.ObjectLibraryCompositionID);
                    table.ForeignKey(
                        name: "FK_ObjectLibraryComposition_ObjectLibrary_ObjectLibraryID",
                        column: x => x.ObjectLibraryID,
                        principalTable: "ObjectLibrary",
                        principalColumn: "ObjectLibraryID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ObjectLibraryCompositionAsset",
                columns: table => new
                {
                    ObjectLibraryCompositionAssetID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ObjectLibraryCompositionID = table.Column<int>(type: "INTEGER", nullable: false),
                    AssetID = table.Column<int>(type: "INTEGER", nullable: false),
                    X = table.Column<float>(type: "REAL", nullable: false),
                    Y = table.Column<float>(type: "REAL", nullable: false),
                    Z = table.Column<float>(type: "REAL", nullable: false),
                    Angle = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectLibraryCompositionAsset", x => x.ObjectLibraryCompositionAssetID);
                    table.ForeignKey(
                        name: "FK_ObjectLibraryCompositionAsset_Asset_AssetID",
                        column: x => x.AssetID,
                        principalTable: "Asset",
                        principalColumn: "AssetID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ObjectLibraryCompositionAsset_ObjectLibraryComposition_ObjectLibraryCompositionID",
                        column: x => x.ObjectLibraryCompositionID,
                        principalTable: "ObjectLibraryComposition",
                        principalColumn: "ObjectLibraryCompositionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ObjectLibraryComposition_ObjectLibraryID",
                table: "ObjectLibraryComposition",
                column: "ObjectLibraryID");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectLibraryCompositionAsset_AssetID",
                table: "ObjectLibraryCompositionAsset",
                column: "AssetID");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectLibraryCompositionAsset_ObjectLibraryCompositionID",
                table: "ObjectLibraryCompositionAsset",
                column: "ObjectLibraryCompositionID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObjectLibraryCompositionAsset");

            migrationBuilder.DropTable(
                name: "ObjectLibraryComposition");
        }
    }
}
