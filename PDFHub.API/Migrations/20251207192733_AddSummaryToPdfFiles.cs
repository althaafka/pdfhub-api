using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PDFHub.Migrations
{
    /// <inheritdoc />
    public partial class AddSummaryToPdfFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "PdfFiles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Summary",
                table: "PdfFiles");
        }
    }
}
