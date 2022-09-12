using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatenaX.NetworkServices.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1409ExpandNotificationTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "portal",
                table: "notification_type",
                columns: new[] { "id", "label" },
                values: new object[] { 12, "TECHNICAL_USER_CREATION" });

            migrationBuilder.CreateIndex(
                name: "ix_agreement_assigned_offers_offer_id",
                schema: "portal",
                table: "agreement_assigned_offers",
                column: "offer_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_agreement_assigned_offers_offer_id",
                schema: "portal",
                table: "agreement_assigned_offers");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "notification_type",
                keyColumn: "id",
                keyValue: 12);
        }
    }
}
