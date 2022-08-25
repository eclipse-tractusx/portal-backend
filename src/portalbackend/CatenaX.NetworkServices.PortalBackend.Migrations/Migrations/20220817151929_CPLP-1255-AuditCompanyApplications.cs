using System;
using Microsoft.EntityFrameworkCore.Migrations;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.AuditEntities;

#nullable disable

namespace CatenaX.NetworkServices.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1255AuditCompanyApplications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "id",
                schema: "portal",
                table: "company_user_assigned_roles",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "last_editor_id",
                schema: "portal",
                table: "company_user_assigned_roles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "last_editor_id",
                schema: "portal",
                table: "company_applications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_company_applications_cplp_1255_audit_company_applications",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_operation_id = table.Column<int>(type: "integer", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    application_status_id = table.Column<int>(type: "integer", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_applications_cplp_1255_audit_company_applicat", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_company_user_assigned_roles_cplp_1255_audit_company_applications",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_operation_id = table.Column<int>(type: "integer", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_user_assigned_roles_cplp_1255_audit_company_a", x => x.id);
                });
            
            migrationBuilder.AddAuditTrigger<AuditCompanyUserAssignedRole>("cplp_1255_audit_company_applications");
            migrationBuilder.AddAuditTrigger<AuditCompanyApplication>("cplp_1255_audit_company_applications"); 
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropAuditTrigger<AuditCompanyUserAssignedRole>(); 
            migrationBuilder.DropAuditTrigger<AuditCompanyApplication>();

            migrationBuilder.DropTable(
                name: "audit_company_applications_cplp_1255_audit_company_applications",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_company_user_assigned_roles_cplp_1255_audit_company_applications",
                schema: "portal");

            migrationBuilder.DropColumn(
                name: "id",
                schema: "portal",
                table: "company_user_assigned_roles");

            migrationBuilder.DropColumn(
                name: "last_editor_id",
                schema: "portal",
                table: "company_user_assigned_roles");

            migrationBuilder.DropColumn(
                name: "last_editor_id",
                schema: "portal",
                table: "company_applications");
        }
    }
}
