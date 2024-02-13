using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class CPLP468certificateupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "portal",
                table: "company_certificate_statuses",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "company_certificate_statuses",
                keyColumn: "id",
                keyValue: 1,
                column: "label",
                value: "ACTIVE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "company_certificate_statuses",
                keyColumn: "id",
                keyValue: 2,
                column: "label",
                value: "INACTVIE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "portal",
                table: "company_certificate_statuses",
                keyColumn: "id",
                keyValue: 1,
                column: "label",
                value: "IN_REVIEW");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "company_certificate_statuses",
                keyColumn: "id",
                keyValue: 2,
                column: "label",
                value: "ACTIVE");

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_certificate_statuses",
                columns: new[] { "id", "label" },
                values: new object[] { 3, "INACTVIE" });
        }
    }
}
