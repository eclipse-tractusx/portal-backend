using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class _170alpha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_APPSUBSCRIPTIONDETAIL() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYASSIGNEDROLE() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYASSIGNEDROLE() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYASSIGNEDROLE() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYSSIDETAIL() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSER() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSER() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSER() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_CONSENT() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_CONSENT() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_CONSENT() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_IDENTITY() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_IDENTITY() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_IDENTITY() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_IDENTITYASSIGNEDROLE() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_OFFER() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_OFFER() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_OFFER() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_PROVIDERCOMPANYDETAIL() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_USERROLE() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_USERROLE() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_USERROLE() CASCADE;");

            migrationBuilder.AddColumn<int>(
                name: "identity_provider_type_id",
                schema: "portal",
                table: "identity_providers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "owner_id",
                schema: "portal",
                table: "identity_providers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // set the owner of shared idps to the using company (if not ambigious)
            migrationBuilder.Sql(@"UPDATE portal.identity_providers AS ip
                SET owner_id = 
                    (
                        SELECT cip.company_id
                        FROM portal.company_identity_providers AS cip
                        WHERE cip.identity_provider_id = ip.id
                    )
                WHERE ip.identity_provider_type_id = 3
                AND (
                        SELECT COUNT(*)
                        FROM portal.company_identity_providers
                        WHERE identity_provider_id = ip.id
                    ) = 1;");

            // ensure any leftover unassigned idps (there shouln't be any but data might be inconsistent) will finally have a valid owner
            migrationBuilder.Sql(@"UPDATE portal.identity_providers
                SET owner_id =
                    (
                        SELECT c.id
                        FROM portal.companies AS c
                        JOIN portal.company_assigned_roles AS cr ON c.id = cr.company_id
                        WHERE cr.company_role_id = 4
                        LIMIT 1
                    )
                WHERE owner_id IS null;");

            // define trigger function for validation only a single company may be assigned to an identity-provider that is not managed.
            migrationBuilder.Sql(@"CREATE FUNCTION portal.tr_company_identity_providers_is_valid_identity_provider_type()
                RETURNS trigger
                VOLATILE
                COST 100
                AS $$
                DECLARE idp_type integer;
                BEGIN
                SELECT identity_provider_type_id
                    INTO idp_type
                    FROM portal.identity_providers AS ip
                    WHERE ip.id = NEW.identity_provider_id;
                IF idp_type = 2
                    OR
                    (
                        SELECT COUNT(*)
                        FROM portal.company_identity_providers AS cip
                        WHERE cip.identity_provider_id = NEW.identity_provider_id
                    ) = 1
                THEN RETURN NEW;
                END IF;
                RAISE EXCEPTION 'identity_provider % of type % is already assigned to a different company_id', NEW.identity_provider_id, idp_type;
                END
                $$ LANGUAGE plpgsql;");

            // create the constraint trigger based on the previously defined trigger function. To ensure it does not depend on the order of inserts it runs deferred at the end of the transaction.
            migrationBuilder.Sql(@"CREATE CONSTRAINT TRIGGER ct_company_identity_providers_is_valid_identity_provider_type
                AFTER INSERT
                ON portal.company_identity_providers
                INITIALLY DEFERRED
                FOR EACH ROW
                EXECUTE PROCEDURE portal.tr_company_identity_providers_is_valid_identity_provider_type();");

            migrationBuilder.AddColumn<int>(
                name: "company_application_type_id",
                schema: "portal",
                table: "company_applications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "onboarding_service_provider_id",
                schema: "portal",
                table: "company_applications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "agreement_link",
                schema: "portal",
                table: "agreements",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_company_application20230824",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    application_status_id = table.Column<int>(type: "integer", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    checklist_process_id = table.Column<Guid>(type: "uuid", nullable: true),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    company_application_type_id = table.Column<int>(type: "integer", nullable: false),
                    onboarding_service_provider_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_application20230824", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "company_application_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_application_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "company_user_assigned_identity_providers",
                schema: "portal",
                columns: table => new
                {
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    identity_provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<string>(type: "text", nullable: false),
                    user_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_user_assigned_identity_providers", x => new { x.company_user_id, x.identity_provider_id });
                    table.ForeignKey(
                        name: "fk_company_user_assigned_identity_providers_company_users_comp",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_user_assigned_identity_providers_identity_providers",
                        column: x => x.identity_provider_id,
                        principalSchema: "portal",
                        principalTable: "identity_providers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "identity_provider_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_provider_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "network_registrations",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    external_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    onboarding_service_provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    application_id = table.Column<Guid>(type: "uuid", nullable: false),
                    process_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_network_registrations", x => x.id);
                    table.ForeignKey(
                        name: "fk_network_registrations_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_network_registrations_companies_onboarding_service_provider",
                        column: x => x.onboarding_service_provider_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_network_registrations_company_applications_application_id",
                        column: x => x.application_id,
                        principalSchema: "portal",
                        principalTable: "company_applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_network_registrations_processes_process_id",
                        column: x => x.process_id,
                        principalSchema: "portal",
                        principalTable: "processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "onboarding_service_provider_details",
                schema: "portal",
                columns: table => new
                {
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    callback_url = table.Column<string>(type: "text", nullable: false),
                    auth_url = table.Column<string>(type: "text", nullable: false),
                    client_id = table.Column<string>(type: "text", nullable: false),
                    client_secret = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_onboarding_service_provider_details", x => x.company_id);
                    table.ForeignKey(
                        name: "fk_onboarding_service_provider_details_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_application_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "INTERNAL" },
                    { 2, "EXTERNAL" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_roles",
                columns: new[] { "id", "label" },
                values: new object[] { 5, "ONBOARDING_SERVICE_PROVIDER" });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "identity_provider_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "OWN" },
                    { 2, "MANAGED" },
                    { 3, "SHARED" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "identity_user_statuses",
                columns: new[] { "id", "label" },
                values: new object[] { 4, "PENDING" });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 111, "TRIGGER_ACTIVATE_SUBSCRIPTION" },
                    { 200, "SYNCHRONIZE_USER" },
                    { 201, "RETRIGGER_SYNCHRONIZE_USER" },
                    { 202, "TRIGGER_CALLBACK_OSP_SUBMITTED" },
                    { 203, "TRIGGER_CALLBACK_OSP_APPROVED" },
                    { 204, "TRIGGER_CALLBACK_OSP_DECLINED" },
                    { 205, "RETRIGGER_CALLBACK_OSP_SUBMITTED" },
                    { 206, "RETRIGGER_CALLBACK_OSP_APPROVED" },
                    { 207, "RETRIGGER_CALLBACK_OSP_DECLINED" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_types",
                columns: new[] { "id", "label" },
                values: new object[] { 4, "PARTNER_REGISTRATION" });

            migrationBuilder.Sql(@"UPDATE portal.identity_providers AS ip SET
                    identity_provider_type_id = CASE
                        WHEN ip.identity_provider_category_id = 1 THEN 3
                        WHEN ip.identity_provider_category_id IN (2, 3) THEN
                            CASE
                                WHEN (
                                         SELECT COUNT(*)
                                         FROM portal.company_identity_providers
                                         WHERE identity_provider_id = ip.id
                                     ) = 1 THEN 1
                                ELSE 2
                                END
                    END,
                    owner_id = CASE
                        WHEN ip.identity_provider_category_id = 1 THEN
                           (SELECT c.id
                            FROM portal.companies AS c
                                     JOIN portal.company_assigned_roles AS cr ON c.id = cr.company_id
                            WHERE cr.company_role_id = 4
                            LIMIT 1)
                       WHEN ip.identity_provider_category_id IN (2, 3) THEN
                           CASE
                               WHEN (
                                        SELECT COUNT(*)
                                        FROM portal.company_identity_providers
                                        WHERE identity_provider_id = ip.id
                                    ) = 1 THEN
                                   (
                                       SELECT company_id
                                       FROM portal.company_identity_providers
                                       WHERE identity_provider_id = ip.id
                                   )
                               ELSE
                                   (
                                       SELECT c.id
                                       FROM portal.companies AS c
                                                JOIN portal.company_assigned_roles AS cr ON c.id = cr.company_id
                                       WHERE cr.company_role_id = 4
                                       LIMIT 1
                                   )
                               END
                        END;");
            migrationBuilder.Sql("UPDATE portal.identity_providers set identity_provider_category_id = 2 where identity_provider_category_id = 1;");
            migrationBuilder.Sql("UPDATE portal.company_applications set company_application_type_id = 1;");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "identity_provider_categories",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "service_types",
                keyColumn: "id",
                keyValue: 2,
                column: "label",
                value: "CONSULTANCY_SERVICE");

            migrationBuilder.CreateIndex(
                name: "ix_identity_providers_identity_provider_type_id",
                schema: "portal",
                table: "identity_providers",
                column: "identity_provider_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_providers_owner_id",
                schema: "portal",
                table: "identity_providers",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_applications_company_application_type_id",
                schema: "portal",
                table: "company_applications",
                column: "company_application_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_applications_onboarding_service_provider_id",
                schema: "portal",
                table: "company_applications",
                column: "onboarding_service_provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_user_assigned_identity_providers_identity_provider_",
                schema: "portal",
                table: "company_user_assigned_identity_providers",
                column: "identity_provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_network_registrations_application_id",
                schema: "portal",
                table: "network_registrations",
                column: "application_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_network_registrations_company_id",
                schema: "portal",
                table: "network_registrations",
                column: "company_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_network_registrations_external_id_onboarding_service_provid",
                schema: "portal",
                table: "network_registrations",
                columns: new[] { "external_id", "onboarding_service_provider_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_network_registrations_onboarding_service_provider_id",
                schema: "portal",
                table: "network_registrations",
                column: "onboarding_service_provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_network_registrations_process_id",
                schema: "portal",
                table: "network_registrations",
                column: "process_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_company_applications_companies_onboarding_service_provider_",
                schema: "portal",
                table: "company_applications",
                column: "onboarding_service_provider_id",
                principalSchema: "portal",
                principalTable: "companies",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_applications_company_application_types_company_appl",
                schema: "portal",
                table: "company_applications",
                column: "company_application_type_id",
                principalSchema: "portal",
                principalTable: "company_application_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_identity_providers_companies_owner_id",
                schema: "portal",
                table: "identity_providers",
                column: "owner_id",
                principalSchema: "portal",
                principalTable: "companies",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_identity_providers_identity_provider_types_identity_provide",
                schema: "portal",
                table: "identity_providers",
                column: "identity_provider_type_id",
                principalSchema: "portal",
                principalTable: "identity_provider_types",
                principalColumn: "id");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_app_subscription_detail20221118\" (\"id\", \"offer_subscription_id\", \"app_instance_id\", \"app_subscription_url\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"offer_subscription_id\", \r\n  NEW.\"app_instance_id\", \r\n  NEW.\"app_subscription_url\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL AFTER INSERT\r\nON \"portal\".\"app_subscription_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_app_subscription_detail20221118\" (\"id\", \"offer_subscription_id\", \"app_instance_id\", \"app_subscription_url\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"offer_subscription_id\", \r\n  NEW.\"app_instance_id\", \r\n  NEW.\"app_subscription_url\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL AFTER UPDATE\r\nON \"portal\".\"app_subscription_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_company_application20230824\" (\"id\", \"date_created\", \"date_last_changed\", \"application_status_id\", \"company_id\", \"checklist_process_id\", \"company_application_type_id\", \"onboarding_service_provider_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"application_status_id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"checklist_process_id\", \r\n  NEW.\"company_application_type_id\", \r\n  NEW.\"onboarding_service_provider_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION AFTER INSERT\r\nON \"portal\".\"company_applications\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_company_application20230824\" (\"id\", \"date_created\", \"date_last_changed\", \"application_status_id\", \"company_id\", \"checklist_process_id\", \"company_application_type_id\", \"onboarding_service_provider_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"application_status_id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"checklist_process_id\", \r\n  NEW.\"company_application_type_id\", \r\n  NEW.\"onboarding_service_provider_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION AFTER UPDATE\r\nON \"portal\".\"company_applications\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYASSIGNEDROLE\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_company_assigned_role2023316\" (\"company_id\", \"company_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"company_id\", \r\n  NEW.\"company_role_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYASSIGNEDROLE AFTER INSERT\r\nON \"portal\".\"company_assigned_roles\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYASSIGNEDROLE\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYASSIGNEDROLE\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_company_assigned_role2023316\" (\"company_id\", \"company_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"company_id\", \r\n  NEW.\"company_role_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYASSIGNEDROLE AFTER UPDATE\r\nON \"portal\".\"company_assigned_roles\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYASSIGNEDROLE\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_company_ssi_detail20230621\" (\"id\", \"company_id\", \"verified_credential_type_id\", \"company_ssi_detail_status_id\", \"document_id\", \"date_created\", \"creator_user_id\", \"expiry_date\", \"verified_credential_external_type_use_case_detail_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"verified_credential_type_id\", \r\n  NEW.\"company_ssi_detail_status_id\", \r\n  NEW.\"document_id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"creator_user_id\", \r\n  NEW.\"expiry_date\", \r\n  NEW.\"verified_credential_external_type_use_case_detail_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL AFTER INSERT\r\nON \"portal\".\"company_ssi_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_company_ssi_detail20230621\" (\"id\", \"company_id\", \"verified_credential_type_id\", \"company_ssi_detail_status_id\", \"document_id\", \"date_created\", \"creator_user_id\", \"expiry_date\", \"verified_credential_external_type_use_case_detail_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"verified_credential_type_id\", \r\n  NEW.\"company_ssi_detail_status_id\", \r\n  NEW.\"document_id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"creator_user_id\", \r\n  NEW.\"expiry_date\", \r\n  NEW.\"verified_credential_external_type_use_case_detail_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL AFTER UPDATE\r\nON \"portal\".\"company_ssi_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYUSER\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_company_user20230523\" (\"id\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"email\", \r\n  NEW.\"firstname\", \r\n  NEW.\"lastlogin\", \r\n  NEW.\"lastname\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYUSER AFTER INSERT\r\nON \"portal\".\"company_users\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYUSER\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYUSER\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_company_user20230523\" (\"id\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"email\", \r\n  NEW.\"firstname\", \r\n  NEW.\"lastlogin\", \r\n  NEW.\"lastname\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYUSER AFTER UPDATE\r\nON \"portal\".\"company_users\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYUSER\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_connector20230803\" (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"connector_url\", \r\n  NEW.\"type_id\", \r\n  NEW.\"status_id\", \r\n  NEW.\"provider_id\", \r\n  NEW.\"host_id\", \r\n  NEW.\"self_description_document_id\", \r\n  NEW.\"location_id\", \r\n  NEW.\"self_description_message\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"company_service_account_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_CONNECTOR AFTER INSERT\r\nON \"portal\".\"connectors\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_connector20230803\" (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"connector_url\", \r\n  NEW.\"type_id\", \r\n  NEW.\"status_id\", \r\n  NEW.\"provider_id\", \r\n  NEW.\"host_id\", \r\n  NEW.\"self_description_document_id\", \r\n  NEW.\"location_id\", \r\n  NEW.\"self_description_message\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"company_service_account_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_CONNECTOR AFTER UPDATE\r\nON \"portal\".\"connectors\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONSENT\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_CONSENT$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_consent20230412\" (\"id\", \"date_created\", \"comment\", \"consent_status_id\", \"target\", \"agreement_id\", \"company_id\", \"document_id\", \"company_user_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"comment\", \r\n  NEW.\"consent_status_id\", \r\n  NEW.\"target\", \r\n  NEW.\"agreement_id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"document_id\", \r\n  NEW.\"company_user_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_CONSENT$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_CONSENT AFTER INSERT\r\nON \"portal\".\"consents\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONSENT\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONSENT\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_CONSENT$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_consent20230412\" (\"id\", \"date_created\", \"comment\", \"consent_status_id\", \"target\", \"agreement_id\", \"company_id\", \"document_id\", \"company_user_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"comment\", \r\n  NEW.\"consent_status_id\", \r\n  NEW.\"target\", \r\n  NEW.\"agreement_id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"document_id\", \r\n  NEW.\"company_user_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_CONSENT$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_CONSENT AFTER UPDATE\r\nON \"portal\".\"consents\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONSENT\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_IDENTITY\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_IDENTITY$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_identity20230526\" (\"id\", \"date_created\", \"company_id\", \"user_status_id\", \"user_entity_id\", \"identity_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"company_id\", \r\n  NEW.\"user_status_id\", \r\n  NEW.\"user_entity_id\", \r\n  NEW.\"identity_type_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_IDENTITY$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_IDENTITY AFTER INSERT\r\nON \"portal\".\"identities\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_IDENTITY\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_IDENTITY\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_IDENTITY$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_identity20230526\" (\"id\", \"date_created\", \"company_id\", \"user_status_id\", \"user_entity_id\", \"identity_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"company_id\", \r\n  NEW.\"user_status_id\", \r\n  NEW.\"user_entity_id\", \r\n  NEW.\"identity_type_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_IDENTITY$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_IDENTITY AFTER UPDATE\r\nON \"portal\".\"identities\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_IDENTITY\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_identity_assigned_role20230522\" (\"identity_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"identity_id\", \r\n  NEW.\"user_role_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE AFTER INSERT\r\nON \"portal\".\"identity_assigned_roles\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_identity_assigned_role20230522\" (\"identity_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"identity_id\", \r\n  NEW.\"user_role_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE AFTER UPDATE\r\nON \"portal\".\"identity_assigned_roles\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFER\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_OFFER$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer20230406\" (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"provider\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"license_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"date_created\", \r\n  NEW.\"date_released\", \r\n  NEW.\"marketing_url\", \r\n  NEW.\"contact_email\", \r\n  NEW.\"contact_number\", \r\n  NEW.\"provider\", \r\n  NEW.\"offer_type_id\", \r\n  NEW.\"sales_manager_id\", \r\n  NEW.\"provider_company_id\", \r\n  NEW.\"offer_status_id\", \r\n  NEW.\"license_type_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_OFFER AFTER INSERT\r\nON \"portal\".\"offers\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFER\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFER\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_OFFER$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer20230406\" (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"provider\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"license_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"date_created\", \r\n  NEW.\"date_released\", \r\n  NEW.\"marketing_url\", \r\n  NEW.\"contact_email\", \r\n  NEW.\"contact_number\", \r\n  NEW.\"provider\", \r\n  NEW.\"offer_type_id\", \r\n  NEW.\"sales_manager_id\", \r\n  NEW.\"provider_company_id\", \r\n  NEW.\"offer_status_id\", \r\n  NEW.\"license_type_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_OFFER AFTER UPDATE\r\nON \"portal\".\"offers\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFER\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer_subscription20230317\" (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"process_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"offer_id\", \r\n  NEW.\"offer_subscription_status_id\", \r\n  NEW.\"display_name\", \r\n  NEW.\"description\", \r\n  NEW.\"requester_id\", \r\n  NEW.\"last_editor_id\", \r\n  NEW.\"process_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION AFTER INSERT\r\nON \"portal\".\"offer_subscriptions\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer_subscription20230317\" (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"process_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"offer_id\", \r\n  NEW.\"offer_subscription_status_id\", \r\n  NEW.\"display_name\", \r\n  NEW.\"description\", \r\n  NEW.\"requester_id\", \r\n  NEW.\"last_editor_id\", \r\n  NEW.\"process_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION AFTER UPDATE\r\nON \"portal\".\"offer_subscriptions\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_provider_company_detail20230614\" (\"id\", \"date_created\", \"auto_setup_url\", \"auto_setup_callback_url\", \"company_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"auto_setup_url\", \r\n  NEW.\"auto_setup_callback_url\", \r\n  NEW.\"company_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL AFTER INSERT\r\nON \"portal\".\"provider_company_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_provider_company_detail20230614\" (\"id\", \"date_created\", \"auto_setup_url\", \"auto_setup_callback_url\", \"company_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"auto_setup_url\", \r\n  NEW.\"auto_setup_callback_url\", \r\n  NEW.\"company_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL AFTER UPDATE\r\nON \"portal\".\"provider_company_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_USERROLE\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_USERROLE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_user_role20221017\" (\"id\", \"user_role\", \"offer_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"user_role\", \r\n  NEW.\"offer_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_USERROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_USERROLE AFTER INSERT\r\nON \"portal\".\"user_roles\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_USERROLE\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_USERROLE\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_USERROLE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_user_role20221017\" (\"id\", \"user_role\", \"offer_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"user_role\", \r\n  NEW.\"offer_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_USERROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_USERROLE AFTER UPDATE\r\nON \"portal\".\"user_roles\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_USERROLE\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYASSIGNEDROLE\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYASSIGNEDROLE\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYUSER\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYUSER\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONSENT\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONSENT\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_IDENTITY\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_IDENTITY\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFER\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFER\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_USERROLE\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_USERROLE\"() CASCADE;");

            migrationBuilder.Sql(@"DROP TRIGGER ct_company_identity_providers_is_valid_identity_provider_type ON portal.company_identity_providers;");
            migrationBuilder.Sql(@"DROP FUNCTION portal.tr_company_identity_providers_is_valid_identity_provider_type;");

            migrationBuilder.InsertData(
                schema: "portal",
                table: "identity_provider_categories",
                columns: new[] { "id", "label" },
                values: new object[] { 1, "KEYCLOAK_SHARED" });

            migrationBuilder.Sql("UPDATE portal.identity_providers set identity_provider_category_id = 1 where identity_provider_type_id = 3;");
            migrationBuilder.Sql("DELETE from portal.agreement_assigned_company_roles where company_role_id = 5");
            migrationBuilder.Sql("DELETE from portal.company_assigned_roles where company_role_id = 5");

            migrationBuilder.DropForeignKey(
                name: "fk_company_applications_companies_onboarding_service_provider_",
                schema: "portal",
                table: "company_applications");

            migrationBuilder.DropForeignKey(
                name: "fk_company_applications_company_application_types_company_appl",
                schema: "portal",
                table: "company_applications");

            migrationBuilder.DropForeignKey(
                name: "fk_identity_providers_companies_owner_id",
                schema: "portal",
                table: "identity_providers");

            migrationBuilder.DropForeignKey(
                name: "fk_identity_providers_identity_provider_types_identity_provide",
                schema: "portal",
                table: "identity_providers");

            migrationBuilder.DropTable(
                name: "audit_company_application20230824",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_application_types",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_user_assigned_identity_providers",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "identity_provider_types",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "network_registrations",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "onboarding_service_provider_details",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_identity_providers_identity_provider_type_id",
                schema: "portal",
                table: "identity_providers");

            migrationBuilder.DropIndex(
                name: "ix_identity_providers_owner_id",
                schema: "portal",
                table: "identity_providers");

            migrationBuilder.DropIndex(
                name: "ix_company_applications_company_application_type_id",
                schema: "portal",
                table: "company_applications");

            migrationBuilder.DropIndex(
                name: "ix_company_applications_onboarding_service_provider_id",
                schema: "portal",
                table: "company_applications");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "company_roles",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "identity_user_statuses",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 111);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 200);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 201);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 202);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 203);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 204);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 205);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 206);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 207);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_types",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "identity_provider_type_id",
                schema: "portal",
                table: "identity_providers");

            migrationBuilder.DropColumn(
                name: "owner_id",
                schema: "portal",
                table: "identity_providers");

            migrationBuilder.DropColumn(
                name: "company_application_type_id",
                schema: "portal",
                table: "company_applications");

            migrationBuilder.DropColumn(
                name: "onboarding_service_provider_id",
                schema: "portal",
                table: "company_applications");

            migrationBuilder.DropColumn(
                name: "agreement_link",
                schema: "portal",
                table: "agreements");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "service_types",
                keyColumn: "id",
                keyValue: 2,
                column: "label",
                value: "CONSULTANCE_SERVICE");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_app_subscription_detail20221118 (\"id\", \"offer_subscription_id\", \"app_instance_id\", \"app_subscription_url\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.offer_subscription_id, \r\n  NEW.app_instance_id, \r\n  NEW.app_subscription_url, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL AFTER INSERT\r\nON portal.app_subscription_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_app_subscription_detail20221118 (\"id\", \"offer_subscription_id\", \"app_instance_id\", \"app_subscription_url\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.offer_subscription_id, \r\n  NEW.app_instance_id, \r\n  NEW.app_subscription_url, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL AFTER UPDATE\r\nON portal.app_subscription_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_APPSUBSCRIPTIONDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_APPSUBSCRIPTIONDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_app_subscription_detail20221118 (\"id\", \"offer_subscription_id\", \"app_instance_id\", \"app_subscription_url\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.offer_subscription_id, \r\n  OLD.app_instance_id, \r\n  OLD.app_subscription_url, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_APPSUBSCRIPTIONDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_APPSUBSCRIPTIONDETAIL AFTER DELETE\r\nON portal.app_subscription_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_APPSUBSCRIPTIONDETAIL();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_application20230214 (\"id\", \"date_created\", \"date_last_changed\", \"application_status_id\", \"company_id\", \"checklist_process_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.date_last_changed, \r\n  NEW.application_status_id, \r\n  NEW.company_id, \r\n  NEW.checklist_process_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION AFTER INSERT\r\nON portal.company_applications\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_application20230214 (\"id\", \"date_created\", \"date_last_changed\", \"application_status_id\", \"company_id\", \"checklist_process_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.date_last_changed, \r\n  NEW.application_status_id, \r\n  NEW.company_id, \r\n  NEW.checklist_process_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION AFTER UPDATE\r\nON portal.company_applications\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_application20230214 (\"id\", \"date_created\", \"date_last_changed\", \"application_status_id\", \"company_id\", \"checklist_process_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.date_created, \r\n  OLD.date_last_changed, \r\n  OLD.application_status_id, \r\n  OLD.company_id, \r\n  OLD.checklist_process_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION AFTER DELETE\r\nON portal.company_applications\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_assigned_role2023316 (\"company_id\", \"company_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.company_id, \r\n  NEW.company_role_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYASSIGNEDROLE AFTER INSERT\r\nON portal.company_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_COMPANYASSIGNEDROLE();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_assigned_role2023316 (\"company_id\", \"company_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.company_id, \r\n  NEW.company_role_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYASSIGNEDROLE AFTER UPDATE\r\nON portal.company_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_COMPANYASSIGNEDROLE();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_assigned_role2023316 (\"company_id\", \"company_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.company_id, \r\n  OLD.company_role_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYASSIGNEDROLE AFTER DELETE\r\nON portal.company_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_COMPANYASSIGNEDROLE();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_ssi_detail20230621 (\"id\", \"company_id\", \"verified_credential_type_id\", \"company_ssi_detail_status_id\", \"document_id\", \"date_created\", \"creator_user_id\", \"expiry_date\", \"verified_credential_external_type_use_case_detail_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.company_id, \r\n  NEW.verified_credential_type_id, \r\n  NEW.company_ssi_detail_status_id, \r\n  NEW.document_id, \r\n  NEW.date_created, \r\n  NEW.creator_user_id, \r\n  NEW.expiry_date, \r\n  NEW.verified_credential_external_type_use_case_detail_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL AFTER INSERT\r\nON portal.company_ssi_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_ssi_detail20230621 (\"id\", \"company_id\", \"verified_credential_type_id\", \"company_ssi_detail_status_id\", \"document_id\", \"date_created\", \"creator_user_id\", \"expiry_date\", \"verified_credential_external_type_use_case_detail_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.company_id, \r\n  NEW.verified_credential_type_id, \r\n  NEW.company_ssi_detail_status_id, \r\n  NEW.document_id, \r\n  NEW.date_created, \r\n  NEW.creator_user_id, \r\n  NEW.expiry_date, \r\n  NEW.verified_credential_external_type_use_case_detail_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL AFTER UPDATE\r\nON portal.company_ssi_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYSSIDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYSSIDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_ssi_detail20230621 (\"id\", \"company_id\", \"verified_credential_type_id\", \"company_ssi_detail_status_id\", \"document_id\", \"date_created\", \"creator_user_id\", \"expiry_date\", \"verified_credential_external_type_use_case_detail_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.company_id, \r\n  OLD.verified_credential_type_id, \r\n  OLD.company_ssi_detail_status_id, \r\n  OLD.document_id, \r\n  OLD.date_created, \r\n  OLD.creator_user_id, \r\n  OLD.expiry_date, \r\n  OLD.verified_credential_external_type_use_case_detail_id, \r\n  OLD.date_last_changed, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYSSIDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYSSIDETAIL AFTER DELETE\r\nON portal.company_ssi_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_COMPANYSSIDETAIL();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user20230523 (\"id\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.email, \r\n  NEW.firstname, \r\n  NEW.lastlogin, \r\n  NEW.lastname, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYUSER AFTER INSERT\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSER();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user20230523 (\"id\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.email, \r\n  NEW.firstname, \r\n  NEW.lastlogin, \r\n  NEW.lastname, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYUSER AFTER UPDATE\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSER();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user20230523 (\"id\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.email, \r\n  OLD.firstname, \r\n  OLD.lastlogin, \r\n  OLD.lastname, \r\n  OLD.date_last_changed, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYUSER AFTER DELETE\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSER();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230803 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.connector_url, \r\n  NEW.type_id, \r\n  NEW.status_id, \r\n  NEW.provider_id, \r\n  NEW.host_id, \r\n  NEW.self_description_document_id, \r\n  NEW.location_id, \r\n  NEW.self_description_message, \r\n  NEW.date_last_changed, \r\n  NEW.company_service_account_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_CONNECTOR AFTER INSERT\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230803 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.connector_url, \r\n  NEW.type_id, \r\n  NEW.status_id, \r\n  NEW.provider_id, \r\n  NEW.host_id, \r\n  NEW.self_description_document_id, \r\n  NEW.location_id, \r\n  NEW.self_description_message, \r\n  NEW.date_last_changed, \r\n  NEW.company_service_account_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_CONNECTOR AFTER UPDATE\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230803 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.name, \r\n  OLD.connector_url, \r\n  OLD.type_id, \r\n  OLD.status_id, \r\n  OLD.provider_id, \r\n  OLD.host_id, \r\n  OLD.self_description_document_id, \r\n  OLD.location_id, \r\n  OLD.self_description_message, \r\n  OLD.date_last_changed, \r\n  OLD.company_service_account_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_CONNECTOR AFTER DELETE\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_CONSENT() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_CONSENT$\r\nBEGIN\r\n  INSERT INTO portal.audit_consent20230412 (\"id\", \"date_created\", \"comment\", \"consent_status_id\", \"target\", \"agreement_id\", \"company_id\", \"document_id\", \"company_user_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.comment, \r\n  NEW.consent_status_id, \r\n  NEW.target, \r\n  NEW.agreement_id, \r\n  NEW.company_id, \r\n  NEW.document_id, \r\n  NEW.company_user_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_CONSENT$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_CONSENT AFTER INSERT\r\nON portal.consents\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_CONSENT();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_CONSENT() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_CONSENT$\r\nBEGIN\r\n  INSERT INTO portal.audit_consent20230412 (\"id\", \"date_created\", \"comment\", \"consent_status_id\", \"target\", \"agreement_id\", \"company_id\", \"document_id\", \"company_user_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.comment, \r\n  NEW.consent_status_id, \r\n  NEW.target, \r\n  NEW.agreement_id, \r\n  NEW.company_id, \r\n  NEW.document_id, \r\n  NEW.company_user_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_CONSENT$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_CONSENT AFTER UPDATE\r\nON portal.consents\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_CONSENT();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_CONSENT() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_CONSENT$\r\nBEGIN\r\n  INSERT INTO portal.audit_consent20230412 (\"id\", \"date_created\", \"comment\", \"consent_status_id\", \"target\", \"agreement_id\", \"company_id\", \"document_id\", \"company_user_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.date_created, \r\n  OLD.comment, \r\n  OLD.consent_status_id, \r\n  OLD.target, \r\n  OLD.agreement_id, \r\n  OLD.company_id, \r\n  OLD.document_id, \r\n  OLD.company_user_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_CONSENT$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_CONSENT AFTER DELETE\r\nON portal.consents\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_CONSENT();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_IDENTITY() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_IDENTITY$\r\nBEGIN\r\n  INSERT INTO portal.audit_identity20230526 (\"id\", \"date_created\", \"company_id\", \"user_status_id\", \"user_entity_id\", \"identity_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.company_id, \r\n  NEW.user_status_id, \r\n  NEW.user_entity_id, \r\n  NEW.identity_type_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_IDENTITY$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_IDENTITY AFTER INSERT\r\nON portal.identities\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_IDENTITY();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_IDENTITY() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_IDENTITY$\r\nBEGIN\r\n  INSERT INTO portal.audit_identity20230526 (\"id\", \"date_created\", \"company_id\", \"user_status_id\", \"user_entity_id\", \"identity_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.company_id, \r\n  NEW.user_status_id, \r\n  NEW.user_entity_id, \r\n  NEW.identity_type_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_IDENTITY$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_IDENTITY AFTER UPDATE\r\nON portal.identities\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_IDENTITY();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_IDENTITY() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_IDENTITY$\r\nBEGIN\r\n  INSERT INTO portal.audit_identity20230526 (\"id\", \"date_created\", \"company_id\", \"user_status_id\", \"user_entity_id\", \"identity_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.date_created, \r\n  OLD.company_id, \r\n  OLD.user_status_id, \r\n  OLD.user_entity_id, \r\n  OLD.identity_type_id, \r\n  OLD.date_last_changed, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_IDENTITY$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_IDENTITY AFTER DELETE\r\nON portal.identities\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_IDENTITY();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_identity_assigned_role20230522 (\"identity_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.identity_id, \r\n  NEW.user_role_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE AFTER INSERT\r\nON portal.identity_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_identity_assigned_role20230522 (\"identity_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.identity_id, \r\n  NEW.user_role_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE AFTER UPDATE\r\nON portal.identity_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_IDENTITYASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_IDENTITYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_identity_assigned_role20230522 (\"identity_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.identity_id, \r\n  OLD.user_role_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_IDENTITYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_IDENTITYASSIGNEDROLE AFTER DELETE\r\nON portal.identity_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_IDENTITYASSIGNEDROLE();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_OFFER() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_OFFER$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer20230406 (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"provider\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"license_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.date_created, \r\n  NEW.date_released, \r\n  NEW.marketing_url, \r\n  NEW.contact_email, \r\n  NEW.contact_number, \r\n  NEW.provider, \r\n  NEW.offer_type_id, \r\n  NEW.sales_manager_id, \r\n  NEW.provider_company_id, \r\n  NEW.offer_status_id, \r\n  NEW.license_type_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_OFFER AFTER INSERT\r\nON portal.offers\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_OFFER();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_OFFER() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_OFFER$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer20230406 (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"provider\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"license_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.date_created, \r\n  NEW.date_released, \r\n  NEW.marketing_url, \r\n  NEW.contact_email, \r\n  NEW.contact_number, \r\n  NEW.provider, \r\n  NEW.offer_type_id, \r\n  NEW.sales_manager_id, \r\n  NEW.provider_company_id, \r\n  NEW.offer_status_id, \r\n  NEW.license_type_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_OFFER AFTER UPDATE\r\nON portal.offers\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_OFFER();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_OFFER() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_OFFER$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer20230406 (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"provider\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"license_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.name, \r\n  OLD.date_created, \r\n  OLD.date_released, \r\n  OLD.marketing_url, \r\n  OLD.contact_email, \r\n  OLD.contact_number, \r\n  OLD.provider, \r\n  OLD.offer_type_id, \r\n  OLD.sales_manager_id, \r\n  OLD.provider_company_id, \r\n  OLD.offer_status_id, \r\n  OLD.license_type_id, \r\n  OLD.date_last_changed, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_OFFER AFTER DELETE\r\nON portal.offers\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_OFFER();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer_subscription20230317 (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"process_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.company_id, \r\n  NEW.offer_id, \r\n  NEW.offer_subscription_status_id, \r\n  NEW.display_name, \r\n  NEW.description, \r\n  NEW.requester_id, \r\n  NEW.last_editor_id, \r\n  NEW.process_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION AFTER INSERT\r\nON portal.offer_subscriptions\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer_subscription20230317 (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"process_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.company_id, \r\n  NEW.offer_id, \r\n  NEW.offer_subscription_status_id, \r\n  NEW.display_name, \r\n  NEW.description, \r\n  NEW.requester_id, \r\n  NEW.last_editor_id, \r\n  NEW.process_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION AFTER UPDATE\r\nON portal.offer_subscriptions\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer_subscription20230317 (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"process_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.company_id, \r\n  OLD.offer_id, \r\n  OLD.offer_subscription_status_id, \r\n  OLD.display_name, \r\n  OLD.description, \r\n  OLD.requester_id, \r\n  OLD.last_editor_id, \r\n  OLD.process_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION AFTER DELETE\r\nON portal.offer_subscriptions\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_provider_company_detail20230614 (\"id\", \"date_created\", \"auto_setup_url\", \"auto_setup_callback_url\", \"company_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.auto_setup_url, \r\n  NEW.auto_setup_callback_url, \r\n  NEW.company_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL AFTER INSERT\r\nON portal.provider_company_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_provider_company_detail20230614 (\"id\", \"date_created\", \"auto_setup_url\", \"auto_setup_callback_url\", \"company_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.auto_setup_url, \r\n  NEW.auto_setup_callback_url, \r\n  NEW.company_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL AFTER UPDATE\r\nON portal.provider_company_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_PROVIDERCOMPANYDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_PROVIDERCOMPANYDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_provider_company_detail20230614 (\"id\", \"date_created\", \"auto_setup_url\", \"auto_setup_callback_url\", \"company_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.date_created, \r\n  OLD.auto_setup_url, \r\n  OLD.auto_setup_callback_url, \r\n  OLD.company_id, \r\n  OLD.date_last_changed, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_PROVIDERCOMPANYDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_PROVIDERCOMPANYDETAIL AFTER DELETE\r\nON portal.provider_company_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_PROVIDERCOMPANYDETAIL();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_USERROLE() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_USERROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_user_role20221017 (\"id\", \"user_role\", \"offer_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.user_role, \r\n  NEW.offer_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_USERROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_USERROLE AFTER INSERT\r\nON portal.user_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_USERROLE();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_USERROLE() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_USERROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_user_role20221017 (\"id\", \"user_role\", \"offer_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.user_role, \r\n  NEW.offer_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_USERROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_USERROLE AFTER UPDATE\r\nON portal.user_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_USERROLE();");
            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_USERROLE() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_USERROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_user_role20221017 (\"id\", \"user_role\", \"offer_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.user_role, \r\n  OLD.offer_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_USERROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_USERROLE AFTER DELETE\r\nON portal.user_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_USERROLE();");
        }
    }
}
