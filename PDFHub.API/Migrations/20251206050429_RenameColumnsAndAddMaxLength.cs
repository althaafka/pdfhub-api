using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PDFHub.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumnsAndAddMaxLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename OriginalFileName to FileName
            migrationBuilder.RenameColumn(
                name: "OriginalFileName",
                table: "PdfFiles",
                newName: "FileName");

            // Rename StoredFilePath to FilePath
            migrationBuilder.RenameColumn(
                name: "StoredFilePath",
                table: "PdfFiles",
                newName: "FilePath");

            // Add MaxLength constraint to FileName
            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "PdfFiles",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            // Add MaxLength constraint to Description
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "PdfFiles",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert FileName MaxLength
            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "PdfFiles",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            // Revert Description MaxLength
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "PdfFiles",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            // Rename FileName back to OriginalFileName
            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "PdfFiles",
                newName: "OriginalFileName");

            // Rename FilePath back to StoredFilePath
            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "PdfFiles",
                newName: "StoredFilePath");
        }
    }
}
