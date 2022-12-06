using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1519AddReleaseRejectionNotifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "portal",
                table: "notification_type",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 19, "APP_RELEASE_REJECTION" },
                    { 20, "SERVICE_RELEASE_REJECTION" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "notification_type_assigned_topic",
                columns: new[] { "notification_topic_id", "notification_type_id" },
                values: new object[,]
                {
                    { 3, 19 },
                    { 3, 20 }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "portal",
                table: "notification_type_assigned_topic",
                keyColumns: new[] { "notification_topic_id", "notification_type_id" },
                keyValues: new object[] { 3, 19 });

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "notification_type_assigned_topic",
                keyColumns: new[] { "notification_topic_id", "notification_type_id" },
                keyValues: new object[] { 3, 20 });

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "notification_type",
                keyColumn: "id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "notification_type",
                keyColumn: "id",
                keyValue: 20);
        }
    }
}
