using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1606AddServiceReleaseApprovalToNotificationTypeId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "portal",
                table: "notification_type_assigned_topic",
                columns: new[] { "notification_topic_id", "notification_type_id" },
                values: new object[] { 3, 17 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "portal",
                table: "notification_type_assigned_topic",
                keyColumns: new[] { "notification_topic_id", "notification_type_id" },
                keyValues: new object[] { 3, 17 });
        }
    }
}
