using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemptomAnalizApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ClaudeAnalizJson_Guncelleme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ClaudeYorumu",
                table: "AnalizSonuclari",
                newName: "ClaudeAnalizJson");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ClaudeAnalizJson",
                table: "AnalizSonuclari",
                newName: "ClaudeYorumu");
        }
    }
}
