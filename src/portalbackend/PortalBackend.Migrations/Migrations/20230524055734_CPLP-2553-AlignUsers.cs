using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP2553AlignUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSER() CASCADE;");

            migrationBuilder.AddColumn<string>(
                name: "client_client_id",
                schema: "portal",
                table: "company_service_accounts",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "client_id",
                schema: "portal",
                table: "company_service_accounts",
                type: "character varying(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_company_user20230523",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    firstname = table.Column<string>(type: "text", nullable: true),
                    lastlogin = table.Column<byte[]>(type: "bytea", nullable: true),
                    lastname = table.Column<string>(type: "text", nullable: true),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_user20230523", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "audit_identity_assigned_role20230522",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    identity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_identity_assigned_role20230522", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "identity_type",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "identities",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_status_id = table.Column<int>(type: "integer", nullable: false),
                    user_entity_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    identity_type_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identities", x => x.id);
                    table.ForeignKey(
                        name: "fk_identities_identity_type_identity_type_id",
                        column: x => x.identity_type_id,
                        principalSchema: "portal",
                        principalTable: "identity_type",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_identities_identity_user_statuses_user_status_id",
                        column: x => x.user_status_id,
                        principalSchema: "portal",
                        principalTable: "identity_user_statuses",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "identity_assigned_roles",
                schema: "portal",
                columns: table => new
                {
                    identity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_assigned_roles", x => new { x.identity_id, x.user_role_id });
                    table.ForeignKey(
                        name: "fk_identity_assigned_roles_identities_identity_id",
                        column: x => x.identity_id,
                        principalSchema: "portal",
                        principalTable: "identities",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_identity_assigned_roles_user_roles_user_role_id",
                        column: x => x.user_role_id,
                        principalSchema: "portal",
                        principalTable: "user_roles",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "identity_type",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "COMPANY_USER" },
                    { 2, "COMPANY_SERVICE_ACCOUNT" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "identity_user_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "ACTIVE" },
                    { 2, "INACTIVE" },
                    { 3, "DELETED" }
                });

            migrationBuilder.Sql("INSERT INTO portal.identities (id, date_created, company_id, user_status_id, user_entity_id, identity_type_id) SELECT cu.id, cu.date_created, cu.company_id, cu.company_user_status_id, i.user_entity_id, 1 FROM portal.company_users as cu LEFT JOIN portal.iam_users as i ON cu.id = i.company_user_id WHERE NOT EXISTS (SELECT 1 FROM portal.identities AS id WHERE id.id = cu.id);");
            migrationBuilder.Sql("INSERT INTO portal.identities (id, date_created, company_id, user_status_id, user_entity_id, identity_type_id) SELECT cu.id, cu.date_created, cu.service_account_owner_id, cu.company_service_account_status_id, i.user_entity_id, 1 FROM portal.company_service_accounts as cu LEFT JOIN portal.iam_service_accounts as i ON cu.id = i.company_service_account_id WHERE NOT EXISTS (SELECT 1 FROM portal.identities AS id WHERE id.id = cu.id);");
            migrationBuilder.Sql("UPDATE portal.company_service_accounts as sa SET client_id = i.client_id, client_client_id = i.client_client_id FROM ( SELECT company_service_account_id, client_id, client_client_id FROM portal.iam_service_accounts) AS i WHERE i.company_service_account_id = sa.id");
            migrationBuilder.Sql("INSERT INTO portal.identity_assigned_roles (identity_id, user_role_id, last_editor_id) SELECT company_user_id, user_role_id, last_editor_id FROM portal.company_user_assigned_roles;");
            migrationBuilder.Sql("INSERT INTO portal.identity_assigned_roles (identity_id, user_role_id, last_editor_id) SELECT company_service_account_id, user_role_id, null FROM portal.company_service_accounts as sa JOIN portal.company_service_account_assigned_roles as sar ON sa.id = sar.company_service_account_id;");

            migrationBuilder.DropForeignKey(
                name: "fk_app_instance_assigned_service_accounts_company_service_acco",
                schema: "portal",
                table: "app_instance_assigned_service_accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_company_service_accounts_companies_service_account_owner_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_company_service_accounts_company_service_account_statuses_c",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_company_user_assigned_app_favourites_company_users_company_",
                schema: "portal",
                table: "company_user_assigned_app_favourites");

            migrationBuilder.DropForeignKey(
                name: "fk_company_user_assigned_business_partners_company_users_compa",
                schema: "portal",
                table: "company_user_assigned_business_partners");

            migrationBuilder.DropForeignKey(
                name: "fk_company_users_companies_company_id",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropForeignKey(
                name: "fk_company_users_company_user_statuses_company_user_status_id",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropForeignKey(
                name: "fk_connectors_company_service_accounts_company_service_account",
                schema: "portal",
                table: "connectors");

            migrationBuilder.DropForeignKey(
                name: "fk_connectors_company_users_last_editor_id",
                schema: "portal",
                table: "connectors");

            migrationBuilder.DropForeignKey(
                name: "fk_consents_company_users_company_user_id",
                schema: "portal",
                table: "consents");

            migrationBuilder.DropForeignKey(
                name: "fk_documents_company_users_company_user_id",
                schema: "portal",
                table: "documents");

            migrationBuilder.DropForeignKey(
                name: "fk_invitations_company_users_company_user_id",
                schema: "portal",
                table: "invitations");

            migrationBuilder.DropForeignKey(
                name: "fk_offer_subscriptions_company_users_requester_id",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.DropForeignKey(
                name: "fk_offers_company_users_sales_manager_id",
                schema: "portal",
                table: "offers");

            migrationBuilder.DropForeignKey(
                name: "fk_notifications_company_users_creator_id",
                schema: "portal",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "fk_notifications_company_users_receiver_id",
                schema: "portal",
                table: "notifications");

            migrationBuilder.DropTable(
                name: "company_service_account_assigned_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_service_account_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_user_assigned_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_user_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "iam_service_accounts",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "iam_users",
                schema: "portal");

            migrationBuilder.DropPrimaryKey(
                name: "pk_company_users",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropIndex(
                name: "ix_company_users_company_id",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropIndex(
                name: "ix_company_users_company_user_status_id",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_company_service_accounts",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropIndex(
                name: "ix_company_service_accounts_company_service_account_status_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropIndex(
                name: "ix_company_service_accounts_service_account_owner_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropColumn(
                name: "company_id",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropColumn(
                name: "company_user_status_id",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropColumn(
                name: "date_created",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropColumn(
                name: "company_service_account_status_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropColumn(
                name: "date_created",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropColumn(
                name: "service_account_owner_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_company_users",
                schema: "portal",
                table: "company_users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_company_service_accounts",
                schema: "portal",
                table: "company_service_accounts",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_client_client_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "client_client_id",
                unique: true,
                filter: "client_client_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_identities_identity_type_id",
                schema: "portal",
                table: "identities",
                column: "identity_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_identities_user_entity_id",
                schema: "portal",
                table: "identities",
                column: "user_entity_id",
                unique: true,
                filter: "user_entity_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_identities_user_status_id",
                schema: "portal",
                table: "identities",
                column: "user_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_assigned_roles_user_role_id",
                schema: "portal",
                table: "identity_assigned_roles",
                column: "user_role_id");

            migrationBuilder.AddForeignKey(
                name: "fk_app_instance_assigned_service_accounts_identities_company_s",
                schema: "portal",
                table: "app_instance_assigned_service_accounts",
                column: "company_service_account_id",
                principalSchema: "portal",
                principalTable: "company_service_accounts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_service_accounts_identities_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_company_user_assigned_app_favourites_identities_company_use",
                schema: "portal",
                table: "company_user_assigned_app_favourites",
                column: "company_user_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_user_assigned_business_partners_identities_company_",
                schema: "portal",
                table: "company_user_assigned_business_partners",
                column: "company_user_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_company_users_identities_id",
                schema: "portal",
                table: "company_users",
                column: "id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_connectors_identities_company_service_account_id",
                schema: "portal",
                table: "connectors",
                column: "company_service_account_id",
                principalSchema: "portal",
                principalTable: "company_service_accounts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_connectors_identities_last_editor_id",
                schema: "portal",
                table: "connectors",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_consents_identities_company_user_id",
                schema: "portal",
                table: "consents",
                column: "company_user_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_documents_identities_company_user_id",
                schema: "portal",
                table: "documents",
                column: "company_user_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_invitations_identities_company_user_id",
                schema: "portal",
                table: "invitations",
                column: "company_user_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_offer_subscriptions_identities_requester_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "requester_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_offers_identities_sales_manager_id",
                schema: "portal",
                table: "offers",
                column: "sales_manager_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_company_users_creator_id",
                schema: "portal",
                table: "notifications",
                column: "creator_user_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_company_users_receiver_id",
                schema: "portal",
                table: "notifications",
                column: "receiver_user_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_IDENTITYASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_IDENTITYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_identity_assigned_role20230522 (\"identity_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.identity_id, \r\n  OLD.user_role_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_IDENTITYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_IDENTITYASSIGNEDROLE AFTER DELETE\r\nON portal.identity_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_IDENTITYASSIGNEDROLE();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_identity_assigned_role20230522 (\"identity_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.identity_id, \r\n  NEW.user_role_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE AFTER INSERT\r\nON portal.identity_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_identity_assigned_role20230522 (\"identity_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.identity_id, \r\n  NEW.user_role_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE AFTER UPDATE\r\nON portal.identity_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user20230523 (\"id\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.email, \r\n  OLD.firstname, \r\n  OLD.lastlogin, \r\n  OLD.lastname, \r\n  OLD.date_last_changed, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYUSER AFTER DELETE\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSER();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user20230523 (\"id\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.email, \r\n  NEW.firstname, \r\n  NEW.lastlogin, \r\n  NEW.lastname, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYUSER AFTER INSERT\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSER();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user20230523 (\"id\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.email, \r\n  NEW.firstname, \r\n  NEW.lastlogin, \r\n  NEW.lastname, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYUSER AFTER UPDATE\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSER();");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_IDENTITYASSIGNEDROLE() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSER() CASCADE;");

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                schema: "portal",
                table: "company_users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "company_user_status_id",
                schema: "portal",
                table: "company_users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "date_created",
                schema: "portal",
                table: "company_users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "company_service_account_status_id",
                schema: "portal",
                table: "company_service_accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "date_created",
                schema: "portal",
                table: "company_service_accounts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "service_account_owner_id",
                schema: "portal",
                table: "company_service_accounts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "company_service_account_assigned_roles",
                schema: "portal",
                columns: table => new
                {
                    company_service_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_service_account_assigned_roles", x => new { x.company_service_account_id, x.user_role_id });
                    table.ForeignKey(
                        name: "fk_company_service_account_assigned_roles_company_service_acco",
                        column: x => x.company_service_account_id,
                        principalSchema: "portal",
                        principalTable: "company_service_accounts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_service_account_assigned_roles_user_roles_user_role",
                        column: x => x.user_role_id,
                        principalSchema: "portal",
                        principalTable: "user_roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_service_account_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_service_account_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "company_user_assigned_roles",
                schema: "portal",
                columns: table => new
                {
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_user_assigned_roles", x => new { x.company_user_id, x.user_role_id });
                    table.ForeignKey(
                        name: "fk_company_user_assigned_roles_company_users_company_user_id",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_user_assigned_roles_user_roles_user_role_id",
                        column: x => x.user_role_id,
                        principalSchema: "portal",
                        principalTable: "user_roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_user_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_user_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "iam_service_accounts",
                schema: "portal",
                columns: table => new
                {
                    client_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    company_service_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_client_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    user_entity_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iam_service_accounts", x => x.client_id);
                    table.ForeignKey(
                        name: "fk_iam_service_accounts_company_service_accounts_company_servi",
                        column: x => x.company_service_account_id,
                        principalSchema: "portal",
                        principalTable: "company_service_accounts",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "iam_users",
                schema: "portal",
                columns: table => new
                {
                    user_entity_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iam_users", x => x.user_entity_id);
                    table.ForeignKey(
                        name: "fk_iam_users_company_users_company_user_id",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_service_account_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "ACTIVE" },
                    { 2, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_user_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "ACTIVE" },
                    { 2, "INACTIVE" },
                    { 3, "DELETED" }
                });

            migrationBuilder.Sql("INSERT INTO portal.iam_users (user_entity_id, company_user_id) SELECT i.user_entity_id, cu.id FROM portal.identities AS i JOIN portal.company_users AS cu ON i.id = cu.id WHERE i.identity_type_id = 1 AND i.user_entity_id IS NOT NULL;");
            migrationBuilder.Sql("INSERT INTO portal.iam_service_accounts (client_id, company_service_account_id, client_client_id, user_entity_id) SELECT sa.client_id, i.id, sa.client_client_id, i.user_entity_id FROM portal.identities as i RIGHT JOIN portal.company_service_accounts as sa ON sa.id = i.Id WHERE sa.client_id != '' AND sa.client_client_id != '' AND i.user_entity_id is not null;");

            migrationBuilder.Sql("UPDATE portal.company_users as cu SET date_created = i.date_created, company_id = i.company_id, company_user_status_id = i.user_status_id FROM (SELECT id, date_created, company_id, user_status_id, identity_type_id FROM portal.identities) AS i WHERE i.id = cu.id");
            migrationBuilder.Sql("UPDATE portal.company_service_accounts SET date_created = i.date_created, service_account_owner_id = i.company_id, company_service_account_status_id = i.user_status_id FROM portal.identities AS i JOIN portal.company_service_accounts AS cu ON cu.id = i.id;");

            migrationBuilder.Sql("INSERT INTO portal.company_user_assigned_roles (company_user_id, user_role_id) SELECT cu.id, ir.user_role_id FROM portal.identity_assigned_roles AS ir INNER JOIN portal.identities AS i ON i.id = ir.identity_id INNER JOIN portal.company_users AS cu ON cu.id = i.id WHERE i.identity_type_id = 1;");
            migrationBuilder.Sql("INSERT INTO portal.company_service_account_assigned_roles (company_service_account_id, user_role_id) SELECT ir.identity_id, ir.user_role_id FROM portal.identity_assigned_roles as ir INNER JOIN portal.identities as i ON i.id = ir.identity_id WHERE i.identity_type_id = 2;");

            migrationBuilder.DropForeignKey(
                name: "fk_app_instance_assigned_service_accounts_identities_company_s",
                schema: "portal",
                table: "app_instance_assigned_service_accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_company_service_accounts_identities_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_company_user_assigned_app_favourites_identities_company_use",
                schema: "portal",
                table: "company_user_assigned_app_favourites");

            migrationBuilder.DropForeignKey(
                name: "fk_company_user_assigned_business_partners_identities_company_",
                schema: "portal",
                table: "company_user_assigned_business_partners");

            migrationBuilder.DropForeignKey(
                name: "fk_company_users_identities_id",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropForeignKey(
                name: "fk_connectors_identities_company_service_account_id",
                schema: "portal",
                table: "connectors");

            migrationBuilder.DropForeignKey(
                name: "fk_connectors_identities_last_editor_id",
                schema: "portal",
                table: "connectors");

            migrationBuilder.DropForeignKey(
                name: "fk_consents_identities_company_user_id",
                schema: "portal",
                table: "consents");

            migrationBuilder.DropForeignKey(
                name: "fk_documents_identities_company_user_id",
                schema: "portal",
                table: "documents");

            migrationBuilder.DropForeignKey(
                name: "fk_invitations_identities_company_user_id",
                schema: "portal",
                table: "invitations");

            migrationBuilder.DropForeignKey(
                name: "fk_offer_subscriptions_identities_requester_id",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.DropForeignKey(
                name: "fk_offers_identities_sales_manager_id",
                schema: "portal",
                table: "offers");

            migrationBuilder.DropForeignKey(
                name: "fk_notifications_company_users_creator_id",
                schema: "portal",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "fk_notifications_company_users_receiver_id",
                schema: "portal",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "fk_company_user_assigned_roles_company_users_company_user_id",
                schema: "portal",
                table: "company_user_assigned_roles");

            migrationBuilder.DropForeignKey(
                name: "fk_iam_users_company_users_company_user_id",
                schema: "portal",
                table: "iam_users");

            migrationBuilder.DropForeignKey(
                name: "fk_company_service_account_assigned_roles_company_service_acco",
                schema: "portal",
                table: "company_service_account_assigned_roles");

            migrationBuilder.DropForeignKey(
                name: "fk_iam_service_accounts_company_service_accounts_company_servi",
                schema: "portal",
                table: "iam_service_accounts");

            migrationBuilder.DropTable(
                name: "audit_company_user20230523",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_identity_assigned_role20230522",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "identity_assigned_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "identities",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "identity_type",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "identity_user_statuses",
                schema: "portal");

            migrationBuilder.DropPrimaryKey(
                name: "PK_company_users",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_company_service_accounts",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropIndex(
                name: "ix_company_service_accounts_client_client_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropColumn(
                name: "client_client_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropColumn(
                name: "client_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.AddPrimaryKey(
                name: "pk_company_users",
                schema: "portal",
                table: "company_users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_company_service_accounts",
                schema: "portal",
                table: "company_service_accounts",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_company_users_company_id",
                schema: "portal",
                table: "company_users",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_users_company_user_status_id",
                schema: "portal",
                table: "company_users",
                column: "company_user_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_company_service_account_status_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "company_service_account_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_service_account_owner_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "service_account_owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_account_assigned_roles_user_role_id",
                schema: "portal",
                table: "company_service_account_assigned_roles",
                column: "user_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_user_assigned_roles_user_role_id",
                schema: "portal",
                table: "company_user_assigned_roles",
                column: "user_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_iam_service_accounts_client_client_id",
                schema: "portal",
                table: "iam_service_accounts",
                column: "client_client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_iam_service_accounts_company_service_account_id",
                schema: "portal",
                table: "iam_service_accounts",
                column: "company_service_account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_iam_service_accounts_user_entity_id",
                schema: "portal",
                table: "iam_service_accounts",
                column: "user_entity_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_iam_users_company_user_id",
                schema: "portal",
                table: "iam_users",
                column: "company_user_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_app_instance_assigned_service_accounts_company_service_acco",
                schema: "portal",
                table: "app_instance_assigned_service_accounts",
                column: "company_service_account_id",
                principalSchema: "portal",
                principalTable: "company_service_accounts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_service_accounts_companies_service_account_owner_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "service_account_owner_id",
                principalSchema: "portal",
                principalTable: "companies",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_service_accounts_company_service_account_statuses_c",
                schema: "portal",
                table: "company_service_accounts",
                column: "company_service_account_status_id",
                principalSchema: "portal",
                principalTable: "company_service_account_statuses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_company_user_assigned_app_favourites_company_users_company_",
                schema: "portal",
                table: "company_user_assigned_app_favourites",
                column: "company_user_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_user_assigned_business_partners_company_users_compa",
                schema: "portal",
                table: "company_user_assigned_business_partners",
                column: "company_user_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_company_users_companies_company_id",
                schema: "portal",
                table: "company_users",
                column: "company_id",
                principalSchema: "portal",
                principalTable: "companies",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_users_company_user_statuses_company_user_status_id",
                schema: "portal",
                table: "company_users",
                column: "company_user_status_id",
                principalSchema: "portal",
                principalTable: "company_user_statuses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_connectors_company_service_accounts_company_service_account",
                schema: "portal",
                table: "connectors",
                column: "company_service_account_id",
                principalSchema: "portal",
                principalTable: "company_service_accounts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_connectors_company_users_last_editor_id",
                schema: "portal",
                table: "connectors",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_consents_company_users_company_user_id",
                schema: "portal",
                table: "consents",
                column: "company_user_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_documents_company_users_company_user_id",
                schema: "portal",
                table: "documents",
                column: "company_user_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_invitations_company_users_company_user_id",
                schema: "portal",
                table: "invitations",
                column: "company_user_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_offer_subscriptions_company_users_requester_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "requester_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_offers_company_users_sales_manager_id",
                schema: "portal",
                table: "offers",
                column: "sales_manager_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_company_users_creator_id",
                schema: "portal",
                table: "notifications",
                column: "creator_user_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_company_users_receiver_id",
                schema: "portal",
                table: "notifications",
                column: "receiver_user_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_user_assigned_roles_company_users_company_user_id",
                schema: "portal",
                table: "company_user_assigned_roles",
                column: "company_user_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_iam_users_company_users_company_user_id",
                schema: "portal",
                table: "iam_users",
                column: "company_user_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_service_account_assigned_roles_company_service_acco",
                schema: "portal",
                table: "company_service_account_assigned_roles",
                column: "company_service_account_id",
                principalSchema: "portal",
                principalTable: "company_service_accounts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_iam_service_accounts_company_service_accounts_company_servi",
                schema: "portal",
                table: "iam_service_accounts",
                column: "company_service_account_id",
                principalSchema: "portal",
                principalTable: "company_service_accounts",
                principalColumn: "id");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user_assigned_role20221018 (\"company_user_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.company_user_id, \r\n  OLD.user_role_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE AFTER DELETE\r\nON portal.company_user_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user_assigned_role20221018 (\"company_user_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.company_user_id, \r\n  NEW.user_role_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE AFTER INSERT\r\nON portal.company_user_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user_assigned_role20221018 (\"company_user_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.company_user_id, \r\n  NEW.user_role_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE AFTER UPDATE\r\nON portal.company_user_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user20221005 (\"id\", \"date_created\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"company_id\", \"company_user_status_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.date_created, \r\n  OLD.email, \r\n  OLD.firstname, \r\n  OLD.lastlogin, \r\n  OLD.lastname, \r\n  OLD.company_id, \r\n  OLD.company_user_status_id, \r\n  OLD.date_last_changed, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYUSER AFTER DELETE\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSER();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user20221005 (\"id\", \"date_created\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"company_id\", \"company_user_status_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.email, \r\n  NEW.firstname, \r\n  NEW.lastlogin, \r\n  NEW.lastname, \r\n  NEW.company_id, \r\n  NEW.company_user_status_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYUSER AFTER INSERT\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSER();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user20221005 (\"id\", \"date_created\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"company_id\", \"company_user_status_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.email, \r\n  NEW.firstname, \r\n  NEW.lastlogin, \r\n  NEW.lastname, \r\n  NEW.company_id, \r\n  NEW.company_user_status_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYUSER AFTER UPDATE\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSER();");
        }
    }
}
