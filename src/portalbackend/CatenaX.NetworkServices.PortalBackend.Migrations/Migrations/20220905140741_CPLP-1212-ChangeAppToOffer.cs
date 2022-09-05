using System;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.AuditEntities;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatenaX.NetworkServices.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1212ChangeAppToOffer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_agreements_apps_app_id",
                schema: "portal",
                table: "agreements");

            migrationBuilder.DropForeignKey(
                name: "fk_app_assigned_documents_apps_app_id",
                schema: "portal",
                table: "app_assigned_documents");

            migrationBuilder.DropForeignKey(
                name: "fk_app_assigned_use_cases_apps_app_id",
                schema: "portal",
                table: "app_assigned_use_cases");

            migrationBuilder.DropForeignKey(
                name: "fk_app_instances_apps_app_id",
                schema: "portal",
                table: "app_instances");

            migrationBuilder.DropForeignKey(
                name: "fk_app_languages_apps_app_id",
                schema: "portal",
                table: "app_languages");

            migrationBuilder.DropForeignKey(
                name: "fk_app_languages_languages_language_temp_id1",
                schema: "portal",
                table: "app_languages");

            migrationBuilder.DropForeignKey(
                name: "fk_company_role_descriptions_languages_language_temp_id2",
                schema: "portal",
                table: "company_role_descriptions");

            migrationBuilder.DropForeignKey(
                name: "fk_company_user_assigned_app_favourites_apps_app_id",
                schema: "portal",
                table: "company_user_assigned_app_favourites");

            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_apps_app_id",
                schema: "portal",
                table: "user_roles");

            migrationBuilder.DropTable(
                name: "app_assigned_licenses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_descriptions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_detail_images",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_tags",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_services_cplp_1213_add_services",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_assigned_apps",
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
                name: "app_licenses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_subscription_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "apps",
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
                name: "app_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "service_statuses",
                schema: "portal");

            migrationBuilder.Sql("DELETE FROM portal.app_assigned_use_cases");
            migrationBuilder.Sql("DELETE FROM portal.app_instances");
            migrationBuilder.Sql("DELETE FROM portal.app_languages");
            migrationBuilder.Sql("DELETE FROM portal.company_user_assigned_app_favourites");
            migrationBuilder.Sql("DELETE FROM portal.company_service_account_assigned_roles");
            migrationBuilder.Sql("DELETE FROM portal.company_user_assigned_roles");
            migrationBuilder.Sql("DELETE FROM portal.user_roles");

            migrationBuilder.RenameColumn(
                name: "app_id",
                schema: "portal",
                table: "user_roles",
                newName: "offer_id");

            migrationBuilder.RenameIndex(
                name: "ix_user_roles_app_id",
                schema: "portal",
                table: "user_roles",
                newName: "ix_user_roles_offer_id");

            migrationBuilder.RenameColumn(
                name: "app_id",
                schema: "portal",
                table: "agreements",
                newName: "offer_id");

            migrationBuilder.RenameIndex(
                name: "ix_agreements_app_id",
                schema: "portal",
                table: "agreements",
                newName: "ix_agreements_offer_id");

            migrationBuilder.CreateTable(
                name: "app_subscription_details",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_instance_id = table.Column<Guid>(type: "uuid", nullable: true),
                    app_subscription_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_subscription_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_app_subscription_details_app_instances_app_instance_id",
                        column: x => x.app_instance_id,
                        principalSchema: "portal",
                        principalTable: "app_instances",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "audit_offer_subscription_cplp_1212_change_app_to_offer",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_operation_id = table.Column<int>(type: "integer", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_subscription_status_id = table.Column<int>(type: "integer", nullable: false),
                    display_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    app_subscription_detail_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_offer_subscription_cplp_1212_change_app_to_offer", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "offer_licenses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    licensetext = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_licenses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "offer_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "offer_subscription_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_subscription_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "offer_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "offers",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_released = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    thumbnail_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    marketing_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    contact_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    contact_number = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    provider = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    offer_type_id = table.Column<int>(type: "integer", nullable: false),
                    sales_manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    provider_company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    offer_status_id = table.Column<int>(type: "integer", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offers", x => x.id);
                    table.ForeignKey(
                        name: "fk_offers_companies_provider_company_id",
                        column: x => x.provider_company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_offers_company_users_sales_manager_id",
                        column: x => x.sales_manager_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_offers_offer_statuses_offer_status_id",
                        column: x => x.offer_status_id,
                        principalSchema: "portal",
                        principalTable: "offer_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_offers_offer_types_offer_type_id",
                        column: x => x.offer_type_id,
                        principalSchema: "portal",
                        principalTable: "offer_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "offer_assigned_licenses",
                schema: "portal",
                columns: table => new
                {
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_license_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_assigned_licenses", x => new { x.offer_id, x.offer_license_id });
                    table.ForeignKey(
                        name: "fk_offer_assigned_licenses_offer_licenses_offer_license_id",
                        column: x => x.offer_license_id,
                        principalSchema: "portal",
                        principalTable: "offer_licenses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_offer_assigned_licenses_offers_offer_id",
                        column: x => x.offer_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "offer_descriptions",
                schema: "portal",
                columns: table => new
                {
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_short_name = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    description_long = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    description_short = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_descriptions", x => new { x.offer_id, x.language_short_name });
                    table.ForeignKey(
                        name: "fk_offer_descriptions_languages_language_short_name",
                        column: x => x.language_short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name");
                    table.ForeignKey(
                        name: "fk_offer_descriptions_offers_offer_id",
                        column: x => x.offer_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "offer_detail_images",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_detail_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_offer_detail_images_offers_offer_id",
                        column: x => x.offer_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "offer_subscriptions",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_subscription_status_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    display_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    app_subscription_detail_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "fk_offer_subscriptions_app_subscription_details_app_subscripti",
                        column: x => x.app_subscription_detail_id,
                        principalSchema: "portal",
                        principalTable: "app_subscription_details",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_offer_subscriptions_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_offer_subscriptions_offer_subscription_statuses_offer_subsc",
                        column: x => x.offer_subscription_status_id,
                        principalSchema: "portal",
                        principalTable: "offer_subscription_statuses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_offer_subscriptions_offers_offer_id",
                        column: x => x.offer_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "offer_tags",
                schema: "portal",
                columns: table => new
                {
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_tags", x => new { x.offer_id, x.tag_name });
                    table.ForeignKey(
                        name: "fk_offer_tags_offers_offer_id",
                        column: x => x.offer_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "offer_statuses",
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
                table: "offer_subscription_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "PENDING" },
                    { 2, "ACTIVE" },
                    { 3, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "offer_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "APP" },
                    { 2, "CORE_COMPONENT" },
                    { 3, "SERVICE" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_app_subscription_details_app_instance_id",
                schema: "portal",
                table: "app_subscription_details",
                column: "app_instance_id");

            migrationBuilder.CreateIndex(
                name: "ix_offer_assigned_licenses_offer_license_id",
                schema: "portal",
                table: "offer_assigned_licenses",
                column: "offer_license_id");

            migrationBuilder.CreateIndex(
                name: "ix_offer_descriptions_language_short_name",
                schema: "portal",
                table: "offer_descriptions",
                column: "language_short_name");

            migrationBuilder.CreateIndex(
                name: "ix_offer_detail_images_offer_id",
                schema: "portal",
                table: "offer_detail_images",
                column: "offer_id");

            migrationBuilder.CreateIndex(
                name: "ix_offer_subscriptions_app_subscription_detail_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "app_subscription_detail_id");

            migrationBuilder.CreateIndex(
                name: "ix_offer_subscriptions_company_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_offer_subscriptions_offer_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "offer_id");

            migrationBuilder.CreateIndex(
                name: "ix_offer_subscriptions_offer_subscription_status_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "offer_subscription_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_offers_offer_status_id",
                schema: "portal",
                table: "offers",
                column: "offer_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_offers_offer_type_id",
                schema: "portal",
                table: "offers",
                column: "offer_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_offers_provider_company_id",
                schema: "portal",
                table: "offers",
                column: "provider_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_offers_sales_manager_id",
                schema: "portal",
                table: "offers",
                column: "sales_manager_id");

            migrationBuilder.AddForeignKey(
                name: "fk_agreements_offers_offer_id",
                schema: "portal",
                table: "agreements",
                column: "offer_id",
                principalSchema: "portal",
                principalTable: "offers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_app_assigned_documents_offers_app_id",
                schema: "portal",
                table: "app_assigned_documents",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "offers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_app_assigned_use_cases_offers_app_id",
                schema: "portal",
                table: "app_assigned_use_cases",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "offers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_app_instances_offers_app_id",
                schema: "portal",
                table: "app_instances",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "offers",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_app_languages_languages_language_temp_id",
                schema: "portal",
                table: "app_languages",
                column: "language_short_name",
                principalSchema: "portal",
                principalTable: "languages",
                principalColumn: "short_name");

            migrationBuilder.AddForeignKey(
                name: "fk_app_languages_offers_app_id",
                schema: "portal",
                table: "app_languages",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "offers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_role_descriptions_languages_language_temp_id1",
                schema: "portal",
                table: "company_role_descriptions",
                column: "language_short_name",
                principalSchema: "portal",
                principalTable: "languages",
                principalColumn: "short_name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_company_user_assigned_app_favourites_offers_app_id",
                schema: "portal",
                table: "company_user_assigned_app_favourites",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "offers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_offers_offer_id",
                schema: "portal",
                table: "user_roles",
                column: "offer_id",
                principalSchema: "portal",
                principalTable: "offers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
            
            migrationBuilder.AddAuditTrigger<AuditOfferSubscription>("cplp_1212_change_app_to_offer");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {            
            migrationBuilder.DropAuditTrigger<AuditOfferSubscription>();

            migrationBuilder.DropForeignKey(
                name: "fk_agreements_offers_offer_id",
                schema: "portal",
                table: "agreements");

            migrationBuilder.DropForeignKey(
                name: "fk_app_assigned_documents_offers_app_id",
                schema: "portal",
                table: "app_assigned_documents");

            migrationBuilder.DropForeignKey(
                name: "fk_app_assigned_use_cases_offers_app_id",
                schema: "portal",
                table: "app_assigned_use_cases");

            migrationBuilder.DropForeignKey(
                name: "fk_app_instances_offers_app_id",
                schema: "portal",
                table: "app_instances");

            migrationBuilder.DropForeignKey(
                name: "fk_app_languages_languages_language_temp_id",
                schema: "portal",
                table: "app_languages");

            migrationBuilder.DropForeignKey(
                name: "fk_app_languages_offers_app_id",
                schema: "portal",
                table: "app_languages");

            migrationBuilder.DropForeignKey(
                name: "fk_company_role_descriptions_languages_language_temp_id1",
                schema: "portal",
                table: "company_role_descriptions");

            migrationBuilder.DropForeignKey(
                name: "fk_company_user_assigned_app_favourites_offers_app_id",
                schema: "portal",
                table: "company_user_assigned_app_favourites");

            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_offers_offer_id",
                schema: "portal",
                table: "user_roles");

            migrationBuilder.DropTable(
                name: "audit_offer_subscription_cplp_1212_change_app_to_offer",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "offer_assigned_licenses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "offer_descriptions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "offer_detail_images",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "offer_subscriptions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "offer_tags",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "offer_licenses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_subscription_details",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "offer_subscription_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "offers",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "offer_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "offer_types",
                schema: "portal");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "company_roles",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.RenameColumn(
                name: "offer_id",
                schema: "portal",
                table: "user_roles",
                newName: "app_id");

            migrationBuilder.RenameIndex(
                name: "ix_user_roles_offer_id",
                schema: "portal",
                table: "user_roles",
                newName: "ix_user_roles_app_id");

            migrationBuilder.RenameColumn(
                name: "offer_id",
                schema: "portal",
                table: "agreements",
                newName: "app_id");

            migrationBuilder.RenameIndex(
                name: "ix_agreements_offer_id",
                schema: "portal",
                table: "agreements",
                newName: "ix_agreements_app_id");

            migrationBuilder.CreateTable(
                name: "app_licenses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    licensetext = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_licenses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "app_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "app_subscription_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_subscription_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_company_assigned_apps_cplp_1254_db_audit",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_instance_id = table.Column<Guid>(type: "uuid", nullable: true),
                    app_subscription_status_id = table.Column<int>(type: "integer", nullable: false),
                    app_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    audit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_operation_id = table.Column<int>(type: "integer", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_assigned_apps_cplp_1254_db_audit", x => x.id);
                });

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
                name: "apps",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_status_id = table.Column<int>(type: "integer", nullable: false),
                    provider_company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sales_manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    contact_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    contact_number = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    date_released = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_core_component = table.Column<bool>(type: "boolean", nullable: false),
                    marketing_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    provider = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    thumbnail_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_apps", x => x.id);
                    table.ForeignKey(
                        name: "fk_apps_app_statuses_app_status_id",
                        column: x => x.app_status_id,
                        principalSchema: "portal",
                        principalTable: "app_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_apps_companies_provider_company_id",
                        column: x => x.provider_company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_apps_company_users_sales_manager_id",
                        column: x => x.sales_manager_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
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
                name: "app_assigned_licenses",
                schema: "portal",
                columns: table => new
                {
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_license_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_assigned_licenses", x => new { x.app_id, x.app_license_id });
                    table.ForeignKey(
                        name: "fk_app_assigned_licenses_app_licenses_app_license_id",
                        column: x => x.app_license_id,
                        principalSchema: "portal",
                        principalTable: "app_licenses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_app_assigned_licenses_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "app_descriptions",
                schema: "portal",
                columns: table => new
                {
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_short_name = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    description_long = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    description_short = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_descriptions", x => new { x.app_id, x.language_short_name });
                    table.ForeignKey(
                        name: "fk_app_descriptions_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_app_descriptions_languages_language_temp_id",
                        column: x => x.language_short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name");
                });

            migrationBuilder.CreateTable(
                name: "app_detail_images",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_detail_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_app_detail_images_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "app_tags",
                schema: "portal",
                columns: table => new
                {
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_tags", x => new { x.app_id, x.tag_name });
                    table.ForeignKey(
                        name: "fk_app_tags_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_assigned_apps",
                schema: "portal",
                columns: table => new
                {
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_instance_id = table.Column<Guid>(type: "uuid", nullable: true),
                    app_subscription_status_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    app_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_assigned_apps", x => new { x.company_id, x.app_id });
                    table.ForeignKey(
                        name: "fk_company_assigned_apps_app_instances_app_instance_id",
                        column: x => x.app_instance_id,
                        principalSchema: "portal",
                        principalTable: "app_instances",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_assigned_apps_app_subscription_statuses_app_subscri",
                        column: x => x.app_subscription_status_id,
                        principalSchema: "portal",
                        principalTable: "app_subscription_statuses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_assigned_apps_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_assigned_apps_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
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
                table: "app_statuses",
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
                table: "app_subscription_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "PENDING" },
                    { 2, "ACTIVE" },
                    { 3, "INACTIVE" }
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
                name: "ix_app_assigned_licenses_app_license_id",
                schema: "portal",
                table: "app_assigned_licenses",
                column: "app_license_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_descriptions_language_short_name",
                schema: "portal",
                table: "app_descriptions",
                column: "language_short_name");

            migrationBuilder.CreateIndex(
                name: "ix_app_detail_images_app_id",
                schema: "portal",
                table: "app_detail_images",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "ix_apps_app_status_id",
                schema: "portal",
                table: "apps",
                column: "app_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_apps_provider_company_id",
                schema: "portal",
                table: "apps",
                column: "provider_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_apps_sales_manager_id",
                schema: "portal",
                table: "apps",
                column: "sales_manager_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_assigned_apps_app_id",
                schema: "portal",
                table: "company_assigned_apps",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_assigned_apps_app_instance_id",
                schema: "portal",
                table: "company_assigned_apps",
                column: "app_instance_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_assigned_apps_app_subscription_status_id",
                schema: "portal",
                table: "company_assigned_apps",
                column: "app_subscription_status_id");

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

            migrationBuilder.AddForeignKey(
                name: "fk_agreements_apps_app_id",
                schema: "portal",
                table: "agreements",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "apps",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_app_assigned_documents_apps_app_id",
                schema: "portal",
                table: "app_assigned_documents",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "apps",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_app_assigned_use_cases_apps_app_id",
                schema: "portal",
                table: "app_assigned_use_cases",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "apps",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_app_instances_apps_app_id",
                schema: "portal",
                table: "app_instances",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "apps",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_app_languages_apps_app_id",
                schema: "portal",
                table: "app_languages",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "apps",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_app_languages_languages_language_temp_id1",
                schema: "portal",
                table: "app_languages",
                column: "language_short_name",
                principalSchema: "portal",
                principalTable: "languages",
                principalColumn: "short_name");

            migrationBuilder.AddForeignKey(
                name: "fk_company_role_descriptions_languages_language_temp_id2",
                schema: "portal",
                table: "company_role_descriptions",
                column: "language_short_name",
                principalSchema: "portal",
                principalTable: "languages",
                principalColumn: "short_name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_company_user_assigned_app_favourites_apps_app_id",
                schema: "portal",
                table: "company_user_assigned_app_favourites",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "apps",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_apps_app_id",
                schema: "portal",
                table: "user_roles",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "apps",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
