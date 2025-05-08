using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class _250rc1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL\"() CASCADE;");

            migrationBuilder.AddColumn<string>(
                name: "auth_url",
                schema: "portal",
                table: "provider_company_details",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "client_id",
                schema: "portal",
                table: "provider_company_details",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "client_secret",
                schema: "portal",
                table: "provider_company_details",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<int>(
                name: "encryption_mode",
                schema: "portal",
                table: "provider_company_details",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "initialization_vector",
                schema: "portal",
                table: "provider_company_details",
                type: "bytea",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_provider_company_detail20241210",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    auto_setup_url = table.Column<string>(type: "text", nullable: true),
                    auto_setup_callback_url = table.Column<string>(type: "text", nullable: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    auth_url = table.Column<string>(type: "text", nullable: true),
                    client_id = table.Column<string>(type: "text", nullable: true),
                    client_secret = table.Column<byte[]>(type: "bytea", nullable: true),
                    initialization_vector = table.Column<byte[]>(type: "bytea", nullable: true),
                    encryption_mode = table.Column<int>(type: "integer", nullable: true),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_provider_company_detail20241210", x => x.audit_v1id);
                });

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_provider_company_detail20241210\" (\"id\", \"date_created\", \"auto_setup_url\", \"auto_setup_callback_url\", \"company_id\", \"auth_url\", \"client_id\", \"client_secret\", \"initialization_vector\", \"encryption_mode\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"auto_setup_url\", \r\n  NEW.\"auto_setup_callback_url\", \r\n  NEW.\"company_id\", \r\n  NEW.\"auth_url\", \r\n  NEW.\"client_id\", \r\n  NEW.\"client_secret\", \r\n  NEW.\"initialization_vector\", \r\n  NEW.\"encryption_mode\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL AFTER INSERT\r\nON \"portal\".\"provider_company_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_provider_company_detail20241210\" (\"id\", \"date_created\", \"auto_setup_url\", \"auto_setup_callback_url\", \"company_id\", \"auth_url\", \"client_id\", \"client_secret\", \"initialization_vector\", \"encryption_mode\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"auto_setup_url\", \r\n  NEW.\"auto_setup_callback_url\", \r\n  NEW.\"company_id\", \r\n  NEW.\"auth_url\", \r\n  NEW.\"client_id\", \r\n  NEW.\"client_secret\", \r\n  NEW.\"initialization_vector\", \r\n  NEW.\"encryption_mode\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL AFTER UPDATE\r\nON \"portal\".\"provider_company_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL\"() CASCADE;");

            migrationBuilder.DropTable(
                name: "audit_provider_company_detail20241210",
                schema: "portal");

            migrationBuilder.DropColumn(
                name: "auth_url",
                schema: "portal",
                table: "provider_company_details");

            migrationBuilder.DropColumn(
                name: "client_id",
                schema: "portal",
                table: "provider_company_details");

            migrationBuilder.DropColumn(
                name: "client_secret",
                schema: "portal",
                table: "provider_company_details");

            migrationBuilder.DropColumn(
                name: "encryption_mode",
                schema: "portal",
                table: "provider_company_details");

            migrationBuilder.DropColumn(
                name: "initialization_vector",
                schema: "portal",
                table: "provider_company_details");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_provider_company_detail20231115\" (\"id\", \"date_created\", \"auto_setup_url\", \"auto_setup_callback_url\", \"company_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"auto_setup_url\", \r\n  NEW.\"auto_setup_callback_url\", \r\n  NEW.\"company_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL AFTER INSERT\r\nON \"portal\".\"provider_company_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_provider_company_detail20231115\" (\"id\", \"date_created\", \"auto_setup_url\", \"auto_setup_callback_url\", \"company_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"auto_setup_url\", \r\n  NEW.\"auto_setup_callback_url\", \r\n  NEW.\"company_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL AFTER UPDATE\r\nON \"portal\".\"provider_company_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL\"();");
        }
    }
}
