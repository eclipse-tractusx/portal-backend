using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatenaX.NetworkServices.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1440AddAuditTrigger : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "audit_company_applications_cplp_1255_audit_company_applications",
                newName: "audit_company_application_cplp1440db_auditing",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "audit_company_user_assigned_roles_cplp_1255_audit_company_applications",
                newName: "audit_company_user_assigned_role_cplp1440db_auditing",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "audit_company_users_cplp_1254_db_audit",
                newName: "audit_company_user_cplp1440db_auditing",
                schema: "portal");

            migrationBuilder.CreateTable(
                name: "audit_offer_subscription_cplp1440db_auditing",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    audit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_subscription_status_id = table.Column<int>(type: "integer", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_DATE"),
                    audit_operation_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_offer_subscription_cplp1440db_auditing", x => x.id);
                });

            migrationBuilder.Sql("CREATE FUNCTION LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_application_cplp1440db_auditing (\"audit_id\", \"audit_operation_id\", \"application_status_id\", \"date_created\", \"company_id\", \"last_editor_id\") SELECT OLD.id, \r\n  3, \r\n  OLD.application_status_id, \r\n  OLD.date_created, \r\n  OLD.company_id, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION AFTER DELETE\r\nON portal.company_applications\r\nFOR EACH ROW EXECUTE PROCEDURE LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION();");

            migrationBuilder.Sql("CREATE FUNCTION LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_application_cplp1440db_auditing (\"audit_id\", \"audit_operation_id\", \"application_status_id\", \"date_created\", \"company_id\", \"last_editor_id\") SELECT NEW.id, \r\n  1, \r\n  NEW.application_status_id, \r\n  NEW.date_created, \r\n  NEW.company_id, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION AFTER INSERT\r\nON portal.company_applications\r\nFOR EACH ROW EXECUTE PROCEDURE LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION();");

            migrationBuilder.Sql("CREATE FUNCTION LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_application_cplp1440db_auditing (\"audit_id\", \"audit_operation_id\", \"application_status_id\", \"date_created\", \"company_id\", \"last_editor_id\") SELECT OLD.id, \r\n  2, \r\n  OLD.application_status_id, \r\n  OLD.date_created, \r\n  OLD.company_id, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION AFTER UPDATE\r\nON portal.company_applications\r\nFOR EACH ROW EXECUTE PROCEDURE LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION();");

            migrationBuilder.Sql("CREATE FUNCTION LC_TRIGGER_AFTER_DELETE_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user_cplp1440db_auditing (\"audit_id\", \"audit_operation_id\", \"company_id\", \"company_user_status_id\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"last_editor_id\") SELECT OLD.id, \r\n  3, \r\n  OLD.company_id, \r\n  OLD.company_user_status_id, \r\n  OLD.email, \r\n  OLD.firstname, \r\n  OLD.lastlogin, \r\n  OLD.lastname, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYUSER AFTER DELETE\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE LC_TRIGGER_AFTER_DELETE_COMPANYUSER();");

            migrationBuilder.Sql("CREATE FUNCTION LC_TRIGGER_AFTER_INSERT_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user_cplp1440db_auditing (\"audit_id\", \"audit_operation_id\", \"company_id\", \"company_user_status_id\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"last_editor_id\") SELECT NEW.id, \r\n  1, \r\n  NEW.company_id, \r\n  NEW.company_user_status_id, \r\n  NEW.email, \r\n  NEW.firstname, \r\n  NEW.lastlogin, \r\n  NEW.lastname, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYUSER AFTER INSERT\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE LC_TRIGGER_AFTER_INSERT_COMPANYUSER();");

            migrationBuilder.Sql("CREATE FUNCTION LC_TRIGGER_AFTER_UPDATE_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user_cplp1440db_auditing (\"audit_id\", \"audit_operation_id\", \"company_id\", \"company_user_status_id\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"last_editor_id\") SELECT OLD.id, \r\n  2, \r\n  OLD.company_id, \r\n  OLD.company_user_status_id, \r\n  OLD.email, \r\n  OLD.firstname, \r\n  OLD.lastlogin, \r\n  OLD.lastname, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYUSER AFTER UPDATE\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE LC_TRIGGER_AFTER_UPDATE_COMPANYUSER();");

            migrationBuilder.Sql("CREATE FUNCTION LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user_assigned_role_cplp1440db_auditing (\"audit_id\", \"audit_operation_id\", \"user_role_id\", \"company_user_id\", \"last_editor_id\") SELECT OLD.id, \r\n  3, \r\n  OLD.user_role_id, \r\n  OLD.company_user_id, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE AFTER DELETE\r\nON portal.company_user_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE();");

            migrationBuilder.Sql("CREATE FUNCTION LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user_assigned_role_cplp1440db_auditing (\"audit_id\", \"audit_operation_id\", \"user_role_id\", \"company_user_id\", \"last_editor_id\") SELECT NEW.id, \r\n  1, \r\n  NEW.user_role_id, \r\n  NEW.company_user_id, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE AFTER INSERT\r\nON portal.company_user_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE();");

            migrationBuilder.Sql("CREATE FUNCTION LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user_assigned_role_cplp1440db_auditing (\"audit_id\", \"audit_operation_id\", \"user_role_id\", \"company_user_id\", \"last_editor_id\") SELECT OLD.id, \r\n  2, \r\n  OLD.user_role_id, \r\n  OLD.company_user_id, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE AFTER UPDATE\r\nON portal.company_user_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE();");

            migrationBuilder.Sql("CREATE FUNCTION LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer_subscription_cplp1440db_auditing (\"audit_id\", \"audit_operation_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\") SELECT OLD.id, \r\n  3, \r\n  OLD.offer_subscription_status_id, \r\n  OLD.display_name, \r\n  OLD.description, \r\n  OLD.requester_id, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION AFTER DELETE\r\nON portal.offer_subscriptions\r\nFOR EACH ROW EXECUTE PROCEDURE LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION();");

            migrationBuilder.Sql("CREATE FUNCTION LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer_subscription_cplp1440db_auditing (\"audit_id\", \"audit_operation_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\") SELECT NEW.id, \r\n  1, \r\n  NEW.offer_subscription_status_id, \r\n  NEW.display_name, \r\n  NEW.description, \r\n  NEW.requester_id, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION AFTER INSERT\r\nON portal.offer_subscriptions\r\nFOR EACH ROW EXECUTE PROCEDURE LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION();");

            migrationBuilder.Sql("CREATE FUNCTION LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer_subscription_cplp1440db_auditing (\"audit_id\", \"audit_operation_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\") SELECT OLD.id, \r\n  2, \r\n  OLD.offer_subscription_status_id, \r\n  OLD.display_name, \r\n  OLD.description, \r\n  OLD.requester_id, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION AFTER UPDATE\r\nON portal.offer_subscriptions\r\nFOR EACH ROW EXECUTE PROCEDURE LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION();");
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

            migrationBuilder.RenameTable(
                name: "audit_company_application_cplp1440db_auditing",
                newName: "audit_company_applications_cplp_1255_audit_company_applications",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "audit_company_user_assigned_role_cplp1440db_auditing",
                newName: "audit_company_user_assigned_roles_cplp_1255_audit_company_applications",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "audit_company_user_cplp1440db_auditing",
                newName: "audit_company_users_cplp_1254_db_audit",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_offer_subscription_cplp1440db_auditing",
                schema: "portal");
        }
    }
}
