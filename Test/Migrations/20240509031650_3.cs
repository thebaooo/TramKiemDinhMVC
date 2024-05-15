using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Test.Migrations
{
    /// <inheritdoc />
    public partial class _3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PdfFile_Certification_CertificationId",
                table: "PdfFile");

            migrationBuilder.DropIndex(
                name: "IX_PdfFile_CertificationId",
                table: "PdfFile");

            migrationBuilder.DropColumn(
                name: "CertificationId",
                table: "PdfFile");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CertificationId",
                table: "PdfFile",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PdfFile_CertificationId",
                table: "PdfFile",
                column: "CertificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_PdfFile_Certification_CertificationId",
                table: "PdfFile",
                column: "CertificationId",
                principalTable: "Certification",
                principalColumn: "CertificationId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
