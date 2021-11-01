using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ArmaRealMapWebSite.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameMod",
                columns: table => new
                {
                    GameModID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameMod", x => x.GameModID);
                });

            migrationBuilder.CreateTable(
                name: "Asset",
                columns: table => new
                {
                    AssetID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    ClassName = table.Column<string>(type: "TEXT", nullable: true),
                    ModelPath = table.Column<string>(type: "TEXT", nullable: true),
                    Width = table.Column<float>(type: "REAL", nullable: false),
                    Depth = table.Column<float>(type: "REAL", nullable: false),
                    Height = table.Column<float>(type: "REAL", nullable: false),
                    CX = table.Column<float>(type: "REAL", nullable: false),
                    CY = table.Column<float>(type: "REAL", nullable: false),
                    CZ = table.Column<float>(type: "REAL", nullable: false),
                    TerrainRegions = table.Column<int>(type: "INTEGER", nullable: false),
                    AssetCategory = table.Column<int>(type: "INTEGER", nullable: false),
                    TerrainBuilderTemplateXML = table.Column<string>(type: "TEXT", nullable: true),
                    GameModID = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxZ = table.Column<float>(type: "REAL", nullable: false),
                    MaxY = table.Column<float>(type: "REAL", nullable: false),
                    MaxX = table.Column<float>(type: "REAL", nullable: false),
                    MinZ = table.Column<float>(type: "REAL", nullable: false),
                    MinY = table.Column<float>(type: "REAL", nullable: false),
                    MinX = table.Column<float>(type: "REAL", nullable: false),
                    BoundingSphereDiameter = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset", x => x.AssetID);
                    table.ForeignKey(
                        name: "FK_Asset_GameMod_GameModID",
                        column: x => x.GameModID,
                        principalTable: "GameMod",
                        principalColumn: "GameModID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssetPreview",
                columns: table => new
                {
                    AssetPreviewID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssetID = table.Column<int>(type: "INTEGER", nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: false),
                    Data = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetPreview", x => x.AssetPreviewID);
                    table.ForeignKey(
                        name: "FK_AssetPreview_Asset_AssetID",
                        column: x => x.AssetID,
                        principalTable: "Asset",
                        principalColumn: "AssetID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asset_GameModID",
                table: "Asset",
                column: "GameModID");

            migrationBuilder.CreateIndex(
                name: "IX_AssetPreview_AssetID",
                table: "AssetPreview",
                column: "AssetID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetPreview");

            migrationBuilder.DropTable(
                name: "Asset");

            migrationBuilder.DropTable(
                name: "GameMod");
        }
    }
}
