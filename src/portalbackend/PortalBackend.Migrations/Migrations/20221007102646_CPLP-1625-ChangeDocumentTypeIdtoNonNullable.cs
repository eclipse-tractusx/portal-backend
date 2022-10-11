using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1625ChangeDocumentTypeIdtoNonNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_documents_document_types_document_type_id",
                schema: "portal",
                table: "documents");

            migrationBuilder.AlterColumn<int>(
                name: "document_type_id",
                schema: "portal",
                table: "documents",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_documents_document_types_document_type_id",
                schema: "portal",
                table: "documents",
                column: "document_type_id",
                principalSchema: "portal",
                principalTable: "document_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_documents_document_types_document_type_id",
                schema: "portal",
                table: "documents");

            migrationBuilder.AlterColumn<int>(
                name: "document_type_id",
                schema: "portal",
                table: "documents",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "fk_documents_document_types_document_type_id",
                schema: "portal",
                table: "documents",
                column: "document_type_id",
                principalSchema: "portal",
                principalTable: "document_types",
                principalColumn: "id");
        }
    }
}
