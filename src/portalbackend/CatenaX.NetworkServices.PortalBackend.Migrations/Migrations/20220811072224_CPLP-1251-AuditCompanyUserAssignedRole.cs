using System;
using Microsoft.EntityFrameworkCore.Migrations;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.AuditEntities;

#nullable disable

namespace CatenaX.NetworkServices.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1251AuditCompanyUserAssignedRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "id",
                schema: "portal",
                table: "company_user_assigned_roles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "last_editor_id",
                schema: "portal",
                table: "company_user_assigned_roles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_company_user_assigned_roles_cplp_1251_audit_company_user_assigned_roles",
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
                    table.PrimaryKey("pk_audit_company_user_assigned_roles_cplp_1251_audit_company_u", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_operation",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_operation", x => x.id);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "audit_operation",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "INSERT" },
                    { 2, "UPDATE" },
                    { 3, "DELETE" }
                });
            migrationBuilder.AddAuditTrigger<AuditCompanyUserAssignedRole>("cplp_1251_audit_company_user_assigned_roles");   
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropAuditTrigger<AuditCompanyUserAssignedRole>();
            migrationBuilder.DropTable(
                name: "audit_company_user_assigned_roles_cplp_1251_audit_company_user_assigned_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_operation",
                schema: "portal");

            migrationBuilder.DropColumn(
                name: "id",
                schema: "portal",
                table: "company_user_assigned_roles");

            migrationBuilder.DropColumn(
                name: "last_editor_id",
                schema: "portal",
                table: "company_user_assigned_roles");
        }
    }
}
