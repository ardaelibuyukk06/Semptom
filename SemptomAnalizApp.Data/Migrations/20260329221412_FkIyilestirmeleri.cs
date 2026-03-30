using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemptomAnalizApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class FkIyilestirmeleri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SemptomId",
                table: "SemptomKatalog",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HastalikId",
                table: "OlasiDurumlar",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SemptomKatalog_SemptomId",
                table: "SemptomKatalog",
                column: "SemptomId");

            migrationBuilder.CreateIndex(
                name: "IX_OlasiDurumlar_HastalikId",
                table: "OlasiDurumlar",
                column: "HastalikId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalizOturumlari_SemptomImzasi",
                table: "AnalizOturumlari",
                column: "SemptomImzasi");

            migrationBuilder.AddForeignKey(
                name: "FK_OlasiDurumlar_Hastaliklar_HastalikId",
                table: "OlasiDurumlar",
                column: "HastalikId",
                principalTable: "Hastaliklar",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SemptomKatalog_Semptomlar_SemptomId",
                table: "SemptomKatalog",
                column: "SemptomId",
                principalTable: "Semptomlar",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OlasiDurumlar_Hastaliklar_HastalikId",
                table: "OlasiDurumlar");

            migrationBuilder.DropForeignKey(
                name: "FK_SemptomKatalog_Semptomlar_SemptomId",
                table: "SemptomKatalog");

            migrationBuilder.DropIndex(
                name: "IX_SemptomKatalog_SemptomId",
                table: "SemptomKatalog");

            migrationBuilder.DropIndex(
                name: "IX_OlasiDurumlar_HastalikId",
                table: "OlasiDurumlar");

            migrationBuilder.DropIndex(
                name: "IX_AnalizOturumlari_SemptomImzasi",
                table: "AnalizOturumlari");

            migrationBuilder.DropColumn(
                name: "SemptomId",
                table: "SemptomKatalog");

            migrationBuilder.DropColumn(
                name: "HastalikId",
                table: "OlasiDurumlar");
        }
    }
}
