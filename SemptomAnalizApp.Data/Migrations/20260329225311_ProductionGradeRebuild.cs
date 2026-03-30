using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemptomAnalizApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProductionGradeRebuild : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AkutMu",
                table: "Hastaliklar",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "ErkekKatsayi",
                table: "Hastaliklar",
                type: "numeric(4,2)",
                precision: 4,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "KadinKatsayi",
                table: "Hastaliklar",
                type: "numeric(4,2)",
                precision: 4,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MaxYas",
                table: "Hastaliklar",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinYas",
                table: "Hastaliklar",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Prevalans",
                table: "Hastaliklar",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AkutMu",
                table: "Hastaliklar");

            migrationBuilder.DropColumn(
                name: "ErkekKatsayi",
                table: "Hastaliklar");

            migrationBuilder.DropColumn(
                name: "KadinKatsayi",
                table: "Hastaliklar");

            migrationBuilder.DropColumn(
                name: "MaxYas",
                table: "Hastaliklar");

            migrationBuilder.DropColumn(
                name: "MinYas",
                table: "Hastaliklar");

            migrationBuilder.DropColumn(
                name: "Prevalans",
                table: "Hastaliklar");
        }
    }
}
