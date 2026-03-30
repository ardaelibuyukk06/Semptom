using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SemptomAnalizApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class TeshisMotoru_HastalikSemptom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hastaliklar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "text", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: false),
                    OnerilenBolum = table.Column<string>(type: "text", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hastaliklar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Semptomlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "text", nullable: false),
                    KritikMi = table.Column<bool>(type: "boolean", nullable: false),
                    Kategori = table.Column<string>(type: "text", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semptomlar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HastalikSemptomlar",
                columns: table => new
                {
                    HastalikId = table.Column<int>(type: "integer", nullable: false),
                    SemptomId = table.Column<int>(type: "integer", nullable: false),
                    Agirlik = table.Column<int>(type: "integer", nullable: false, defaultValue: 5)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HastalikSemptomlar", x => new { x.HastalikId, x.SemptomId });
                    table.ForeignKey(
                        name: "FK_HastalikSemptomlar_Hastaliklar_HastalikId",
                        column: x => x.HastalikId,
                        principalTable: "Hastaliklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HastalikSemptomlar_Semptomlar_SemptomId",
                        column: x => x.SemptomId,
                        principalTable: "Semptomlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HastalikSemptomlar_SemptomId",
                table: "HastalikSemptomlar",
                column: "SemptomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HastalikSemptomlar");

            migrationBuilder.DropTable(
                name: "Hastaliklar");

            migrationBuilder.DropTable(
                name: "Semptomlar");
        }
    }
}
