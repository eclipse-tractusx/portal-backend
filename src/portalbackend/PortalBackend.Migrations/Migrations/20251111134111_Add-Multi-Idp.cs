using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiIdp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shared_idp_instance_details",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shared_idp_url = table.Column<string>(type: "text", nullable: false),
                    client_id = table.Column<string>(type: "text", nullable: false),
                    client_secret = table.Column<byte[]>(type: "bytea", nullable: false),
                    initialization_vector = table.Column<byte[]>(type: "bytea", nullable: true),
                    encryption_mode = table.Column<int>(type: "integer", nullable: false),
                    auth_realm = table.Column<string>(type: "text", nullable: true),
                    use_auth_trail = table.Column<bool>(type: "boolean", nullable: false),
                    realm_used = table.Column<int>(type: "integer", nullable: false),
                    max_realm_count = table.Column<int>(type: "integer", nullable: false),
                    is_running = table.Column<bool>(type: "boolean", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shared_idp_instance_details", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shared_idp_realm_mappings",
                schema: "portal",
                columns: table => new
                {
                    shared_idp_instance_detail_id = table.Column<Guid>(type: "uuid", nullable: false),
                    realm_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shared_idp_realm_mappings", x => new { x.shared_idp_instance_detail_id, x.realm_name });
                    table.ForeignKey(
                        name: "fk_shared_idp_realm_mappings_shared_idp_instance_details_share",
                        column: x => x.shared_idp_instance_detail_id,
                        principalSchema: "portal",
                        principalTable: "shared_idp_instance_details",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[] { 707, "SYNC_MULTI_SHARED_IDP" });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_types",
                columns: new[] { "id", "label" },
                values: new object[] { 11, "MULTI_SHARED_IDENTITY_PROVIDER" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shared_idp_realm_mappings",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "shared_idp_instance_details",
                schema: "portal");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 707);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_types",
                keyColumn: "id",
                keyValue: 11);
        }
    }
}
