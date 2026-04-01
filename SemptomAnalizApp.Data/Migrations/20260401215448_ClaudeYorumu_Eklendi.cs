using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemptomAnalizApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ClaudeYorumu_Eklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClaudeYorumu",
                table: "AnalizSonuclari",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClaudeYorumu",
                table: "AnalizSonuclari");
        }
    }
}
