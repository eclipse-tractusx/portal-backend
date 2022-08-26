using System;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.AuditEntities;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatenaX.NetworkServices.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1427CombineServiceAndApps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_services_cplp_1213_add_services",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_assigned_services",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "service_assigned_licenses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "service_descriptions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "service_subscription_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "service_licenses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "services",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "service_statuses",
                schema: "portal");

            migrationBuilder.DropPrimaryKey(
                name: "pk_company_assigned_apps",
                schema: "portal",
                table: "company_assigned_apps");
         
            migrationBuilder.CreateTable(
                name: "audit_company_assigned_apps_cplp_1427_combine_service_and_apps",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_operation_id = table.Column<int>(type: "integer", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_subscription_status_id = table.Column<int>(type: "integer", nullable: false),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    display_name = table.Column<string>(type: "character varying(256)", nullable: true),
                    description= table.Column<string>(type: "character varying(4096)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("audit_company_assigned_apps_cplp_1427_combine_service_and_a", x => x.id);
                });

            migrationBuilder.DropColumn(
                name: "is_core_component",
                schema: "portal",
                table: "apps");

            migrationBuilder.AddColumn<string>(
                name: "description",
                schema: "portal",
                table: "company_assigned_apps",
                type: "character varying(4096)",
                maxLength: 4096,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "display_name",
                schema: "portal",
                table: "company_assigned_apps",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "app_type",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_type", x => x.id);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "app_type",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "APP" },
                    { 2, "CORE_COMPONENT" },
                    { 3, "SERVICE" }
                });

            migrationBuilder.AddColumn<int>(
                name: "app_type_id",
                schema: "portal",
                table: "apps",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddPrimaryKey(
                name: "pk_company_assigned_apps",
                schema: "portal",
                table: "company_assigned_apps",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_company_assigned_apps_company_id",
                schema: "portal",
                table: "company_assigned_apps",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_apps_app_type_id",
                schema: "portal",
                table: "apps",
                column: "app_type_id");

            migrationBuilder.AddForeignKey(
                name: "fk_apps_app_type_app_type_id",
                schema: "portal",
                table: "apps",
                column: "app_type_id",
                principalSchema: "portal",
                principalTable: "app_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
            
            migrationBuilder.AddAuditTrigger<AuditCompanyAssignedApp>("cplp_1427_combine_service_and_apps");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropAuditTrigger<AuditCompanyAssignedApp>();

            migrationBuilder.DropForeignKey(
                name: "fk_apps_app_type_app_type_id",
                schema: "portal",
                table: "apps");

            migrationBuilder.DropTable(
                name: "app_type",
                schema: "portal");

            migrationBuilder.DropPrimaryKey(
                name: "pk_company_assigned_apps",
                schema: "portal",
                table: "company_assigned_apps");

            migrationBuilder.DropIndex(
                name: "ix_company_assigned_apps_company_id",
                schema: "portal",
                table: "company_assigned_apps");

            migrationBuilder.DropIndex(
                name: "ix_apps_app_type_id",
                schema: "portal",
                table: "apps");

            migrationBuilder.DropPrimaryKey(
                name: "pk_audit_company_assigned_apps_cplp_1427_combine_service_and_a",
                schema: "portal",
                table: "audit_company_assigned_apps_cplp_1427_combine_service_and_apps");

            migrationBuilder.DropColumn(
                name: "description",
                schema: "portal",
                table: "company_assigned_apps");

            migrationBuilder.DropColumn(
                name: "display_name",
                schema: "portal",
                table: "company_assigned_apps");

            migrationBuilder.DropColumn(
                name: "app_type_id",
                schema: "portal",
                table: "apps");

            migrationBuilder.DropColumn(
                name: "description",
                schema: "portal",
                table: "audit_company_assigned_apps_cplp_1427_combine_service_and_apps");

            migrationBuilder.DropColumn(
                name: "display_name",
                schema: "portal",
                table: "audit_company_assigned_apps_cplp_1427_combine_service_and_apps");

            migrationBuilder.RenameTable(
                name: "audit_company_assigned_apps_cplp_1427_combine_service_and_apps",
                schema: "portal",
                newName: "audit_company_assigned_apps_cplp_1254_db_audit",
                newSchema: "portal");

            migrationBuilder.AddColumn<bool>(
                name: "is_core_component",
                schema: "portal",
                table: "apps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "pk_company_assigned_apps",
                schema: "portal",
                table: "company_assigned_apps",
                columns: new[] { "company_id", "app_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_audit_company_assigned_apps_cplp_1254_db_audit",
                schema: "portal",
                table: "audit_company_assigned_apps_cplp_1254_db_audit",
                column: "id");

            migrationBuilder.CreateTable(
                name: "audit_services_cplp_1213_add_services",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_operation_id = table.Column<int>(type: "integer", nullable: false),
                    contact_email = table.Column<string>(type: "text", nullable: true),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    provider_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sales_manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    service_status_id = table.Column<int>(type: "integer", nullable: false),
                    thumbnail_url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_services_cplp_1213_add_services", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "service_licenses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    license_text = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_licenses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "service_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "service_subscription_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_subscription_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "services",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sales_manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    service_status_id = table.Column<int>(type: "integer", nullable: false),
                    contact_email = table.Column<string>(type: "text", nullable: true),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    thumbnail_url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_services", x => x.id);
                    table.ForeignKey(
                        name: "fk_services_companies_provider_company_id",
                        column: x => x.provider_company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_services_company_users_sales_manager_id",
                        column: x => x.sales_manager_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_services_service_statuses_service_status_id",
                        column: x => x.service_status_id,
                        principalSchema: "portal",
                        principalTable: "service_statuses",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_assigned_services",
                schema: "portal",
                columns: table => new
                {
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_subscription_status_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_assigned_services", x => new { x.service_id, x.company_id });
                    table.ForeignKey(
                        name: "fk_company_assigned_services_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_assigned_services_company_users_requester_id",
                        column: x => x.requester_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_assigned_services_service_subscription_statuses_ser",
                        column: x => x.service_subscription_status_id,
                        principalSchema: "portal",
                        principalTable: "service_subscription_statuses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_assigned_services_services_service_id",
                        column: x => x.service_id,
                        principalSchema: "portal",
                        principalTable: "services",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "service_assigned_licenses",
                schema: "portal",
                columns: table => new
                {
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_license_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_assigned_licenses", x => new { x.service_id, x.service_license_id });
                    table.ForeignKey(
                        name: "fk_service_assigned_licenses_service_licenses_service_license_",
                        column: x => x.service_license_id,
                        principalSchema: "portal",
                        principalTable: "service_licenses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_service_assigned_licenses_services_service_id",
                        column: x => x.service_id,
                        principalSchema: "portal",
                        principalTable: "services",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "service_descriptions",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    language_short_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_descriptions", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_descriptions_services_service_id",
                        column: x => x.service_id,
                        principalSchema: "portal",
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "service_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "CREATED" },
                    { 2, "IN_REVIEW" },
                    { 3, "ACTIVE" },
                    { 4, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "service_subscription_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "PENDING" },
                    { 2, "ACTIVE" },
                    { 3, "INACTIVE" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_company_assigned_services_company_id",
                schema: "portal",
                table: "company_assigned_services",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_assigned_services_requester_id",
                schema: "portal",
                table: "company_assigned_services",
                column: "requester_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_assigned_services_service_subscription_status_id",
                schema: "portal",
                table: "company_assigned_services",
                column: "service_subscription_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_assigned_licenses_service_license_id",
                schema: "portal",
                table: "service_assigned_licenses",
                column: "service_license_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_descriptions_service_id",
                schema: "portal",
                table: "service_descriptions",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_services_provider_company_id",
                schema: "portal",
                table: "services",
                column: "provider_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_services_sales_manager_id",
                schema: "portal",
                table: "services",
                column: "sales_manager_id");

            migrationBuilder.CreateIndex(
                name: "ix_services_service_status_id",
                schema: "portal",
                table: "services",
                column: "service_status_id");
        }
    }
}
