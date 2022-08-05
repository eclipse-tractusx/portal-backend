using System;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.AuditEntities;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatenaX.NetworkServices.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1253appsubscriptionaudit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "id",
                schema: "portal",
                table: "company_assigned_apps",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");
            
            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "portal",
                table: "company_assigned_apps",
                type: "uuid",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "last_editor_id",
                schema: "portal",
                table: "company_assigned_apps",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_company_assigned_apps_cplp_1253_company_assigned_app",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_operation_id = table.Column<int>(type: "integer", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_subscription_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_assigned_apps_cplp_1253_company_assigned_app", x => x.id);
                });
            
            migrationBuilder.AddAuditTrigger<AuditCompanyAssignedApp>("cplp_1253_company_assigned_app");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropAuditTrigger<AuditCompanyAssignedApp>();

            migrationBuilder.DropTable(
                name: "audit_company_assigned_apps_cplp_1253_company_assigned_app",
                schema: "portal");

            migrationBuilder.DropColumn(
                name: "id",
                schema: "portal",
                table: "company_assigned_apps");

            migrationBuilder.DropColumn(
                name: "last_editor_id",
                schema: "portal",
                table: "company_assigned_apps");
        }
    }
}
