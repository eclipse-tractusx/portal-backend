using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1732ChangeDocumentTypeEnum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "portal",
                table: "document_types",
                keyColumn: "id",
                keyValue: 4,
                column: "label",
                value: "APP_DATA_DETAILS");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "document_types",
                keyColumn: "id",
                keyValue: 8,
                column: "label",
                value: "SELF_DESCRIPTION");

            migrationBuilder.InsertData(
                schema: "portal",
                table: "document_types",
                columns: new[] { "id", "label" },
                values: new object[] { 9, "APP_TECHNICAL_INFORMATION" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "portal",
                table: "document_types",
                keyColumn: "id",
                keyValue: 9);

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "document_types",
                keyColumn: "id",
                keyValue: 4,
                column: "label",
                value: "DATA_CONTRACT");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "document_types",
                keyColumn: "id",
                keyValue: 8,
                column: "label",
                value: "SELF_DESCRIPTION_EDC");
        }
    }
}
