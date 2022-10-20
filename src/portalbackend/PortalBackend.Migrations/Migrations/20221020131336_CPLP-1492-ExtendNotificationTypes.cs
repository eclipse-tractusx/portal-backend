using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1492ExtendNotificationTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "portal",
                table: "notification_type",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 13, "SERVICE_REQUEST" },
                    { 14, "SERVICE_ACTIVATION" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_offer_subscriptions_requester_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "requester_id");

            migrationBuilder.AddForeignKey(
                name: "fk_offer_subscriptions_company_users_requester_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "requester_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_offer_subscriptions_company_users_requester_id",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.DropIndex(
                name: "ix_offer_subscriptions_requester_id",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "notification_type",
                keyColumn: "id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "notification_type",
                keyColumn: "id",
                keyValue: 14);
        }
    }
}
