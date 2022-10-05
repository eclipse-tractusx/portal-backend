using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1440DbAuditing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_company_applications_cplp_1255_audit_company_applications",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_company_user_assigned_roles_cplp_1255_audit_company_applications",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_company_users_cplp_1254_db_audit",
                schema: "portal");

            migrationBuilder.CreateTable(
                name: "audit_company_application20221005",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    application_status_id = table.Column<int>(type: "integer", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_application20221005", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "audit_company_user_assigned_role20221005",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_user_assigned_role20221005", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "audit_company_user20221005",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    firstname = table.Column<string>(type: "text", nullable: true),
                    lastlogin = table.Column<byte[]>(type: "bytea", nullable: true),
                    lastname = table.Column<string>(type: "text", nullable: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_user_status_id = table.Column<int>(type: "integer", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_user20221005", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "audit_offer_subscription20221005",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_subscription_status_id = table.Column<int>(type: "integer", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_offer_subscription20221005", x => x.audit_v1id);
                });

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_application20221005 (\"id\", \"date_created\", \"date_last_changed\", \"application_status_id\", \"company_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.date_created, \r\n  OLD.date_last_changed, \r\n  OLD.application_status_id, \r\n  OLD.company_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION AFTER DELETE\r\nON portal.company_applications\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_application20221005 (\"id\", \"date_created\", \"date_last_changed\", \"application_status_id\", \"company_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.date_last_changed, \r\n  NEW.application_status_id, \r\n  NEW.company_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION AFTER INSERT\r\nON portal.company_applications\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_application20221005 (\"id\", \"date_created\", \"date_last_changed\", \"application_status_id\", \"company_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.date_last_changed, \r\n  NEW.application_status_id, \r\n  NEW.company_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION AFTER UPDATE\r\nON portal.company_applications\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user20221005 (\"id\", \"date_created\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"company_id\", \"company_user_status_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.date_created, \r\n  OLD.email, \r\n  OLD.firstname, \r\n  OLD.lastlogin, \r\n  OLD.lastname, \r\n  OLD.company_id, \r\n  OLD.company_user_status_id, \r\n  OLD.date_last_changed, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYUSER AFTER DELETE\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSER();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user20221005 (\"id\", \"date_created\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"company_id\", \"company_user_status_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.email, \r\n  NEW.firstname, \r\n  NEW.lastlogin, \r\n  NEW.lastname, \r\n  NEW.company_id, \r\n  NEW.company_user_status_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYUSER AFTER INSERT\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSER();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user20221005 (\"id\", \"date_created\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"company_id\", \"company_user_status_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.email, \r\n  NEW.firstname, \r\n  NEW.lastlogin, \r\n  NEW.lastname, \r\n  NEW.company_id, \r\n  NEW.company_user_status_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYUSER AFTER UPDATE\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSER();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user_assigned_role20221005 (\"id\", \"company_user_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.company_user_id, \r\n  OLD.user_role_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE AFTER DELETE\r\nON portal.company_user_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user_assigned_role20221005 (\"id\", \"company_user_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.company_user_id, \r\n  NEW.user_role_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE AFTER INSERT\r\nON portal.company_user_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user_assigned_role20221005 (\"id\", \"company_user_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.company_user_id, \r\n  NEW.user_role_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE AFTER UPDATE\r\nON portal.company_user_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer_subscription20221005 (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.company_id, \r\n  OLD.offer_id, \r\n  OLD.offer_subscription_status_id, \r\n  OLD.display_name, \r\n  OLD.description, \r\n  OLD.requester_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION AFTER DELETE\r\nON portal.offer_subscriptions\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer_subscription20221005 (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.company_id, \r\n  NEW.offer_id, \r\n  NEW.offer_subscription_status_id, \r\n  NEW.display_name, \r\n  NEW.description, \r\n  NEW.requester_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION AFTER INSERT\r\nON portal.offer_subscriptions\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer_subscription20221005 (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.company_id, \r\n  NEW.offer_id, \r\n  NEW.offer_subscription_status_id, \r\n  NEW.display_name, \r\n  NEW.description, \r\n  NEW.requester_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION AFTER UPDATE\r\nON portal.offer_subscriptions\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION();");
            
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS audit_company_users ON portal.company_users;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS audit_company_user_assigned_roles ON portal.company_user_assigned_roles;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS audit_company_applications ON portal.company_applications; ");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS portal.process_audit_company_users_audit(); DROP FUNCTION IF EXISTS portal.process_company_user_assigned_roles_audit();");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS portal.process_audit_company_user_assigned_roles_audit(); DROP FUNCTION IF EXISTS portal.process_company_users_audit();");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS portal.process_audit_company_applications_audit(); DROP FUNCTION IF EXISTS portal.process_company_applications_audit();");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION LC_TRIGGER_AFTER_DELETE_COMPANYUSER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION LC_TRIGGER_AFTER_INSERT_COMPANYUSER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION LC_TRIGGER_AFTER_UPDATE_COMPANYUSER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION() CASCADE;");

            migrationBuilder.DropTable(
                name: "audit_company_application20221005",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_company_user_assigned_role20221005",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_company_user20221005",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_offer_subscription20221005",
                schema: "portal");

            migrationBuilder.CreateTable(
                name: "audit_company_applications_cplp_1255_audit_company_applications",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    application_status_id = table.Column<int>(type: "integer", nullable: false),
                    audit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_operation_id = table.Column<int>(type: "integer", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_user_assigned_roles_cplp_1255_audit_company_a", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_company_users_cplp_1254_db_audit",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_user_status_id = table.Column<int>(type: "integer", nullable: false),
                    audit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_operation_id = table.Column<int>(type: "integer", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    firstname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    lastlogin = table.Column<byte[]>(type: "bytea", nullable: true),
                    lastname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_users_cplp_1254_db_audit", x => x.id);
                    table.ForeignKey(
                        name: "fk_audit_company_users_cplp_1254_db_audit_company_user_statuse",
                        column: x => x.company_user_status_id,
                        principalSchema: "portal",
                        principalTable: "company_user_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_company_users_cplp_1254_db_audit_company_user_status_",
                schema: "portal",
                table: "audit_company_users_cplp_1254_db_audit",
                column: "company_user_status_id");
        }
    }
}
