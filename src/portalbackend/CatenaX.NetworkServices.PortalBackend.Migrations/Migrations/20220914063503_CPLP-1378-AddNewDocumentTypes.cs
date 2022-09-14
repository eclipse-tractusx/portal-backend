using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatenaX.NetworkServices.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1378AddNewDocumentTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "portal",
                table: "document_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 5, "ADDITIONAL_DETAILS" },
                    { 6, "APP_LEADIMAGE" },
                    { 7, "APP_IMAGE" },
                    { 8, "SELF_DESCRIPTION_EDC" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "portal",
                table: "document_types",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "document_types",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "document_types",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "document_types",
                keyColumn: "id",
                keyValue: 8);
        }
    }
}
