/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using Microsoft.EntityFrameworkCore.Migrations;
using System.Text.Json;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class _466auditingcertificatemanagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_addresses_countries_country_temp_id",
                schema: "portal",
                table: "addresses");

            migrationBuilder.DropForeignKey(
                name: "fk_app_languages_languages_language_temp_id",
                schema: "portal",
                table: "app_languages");

            migrationBuilder.DropForeignKey(
                name: "fk_company_certificate_type_descriptions_languages_language_te",
                schema: "portal",
                table: "company_certificate_type_descriptions");

            migrationBuilder.DropForeignKey(
                name: "fk_company_role_descriptions_languages_language_temp_id2",
                schema: "portal",
                table: "company_role_descriptions");

            migrationBuilder.DropForeignKey(
                name: "fk_company_service_accounts_identities_identity_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_company_users_identities_identity_id",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropForeignKey(
                name: "fk_connectors_countries_location_temp_id1",
                schema: "portal",
                table: "connectors");

            migrationBuilder.DropForeignKey(
                name: "fk_country_long_names_countries_country_alpha2code",
                schema: "portal",
                table: "country_long_names");

            migrationBuilder.DropForeignKey(
                name: "fk_country_long_names_languages_language_temp_id3",
                schema: "portal",
                table: "country_long_names");

            migrationBuilder.DropForeignKey(
                name: "fk_identities_identity_user_statuses_identity_status_id",
                schema: "portal",
                table: "identities");

            migrationBuilder.DropForeignKey(
                name: "fk_language_long_names_languages_language_short_name",
                schema: "portal",
                table: "language_long_names");

            migrationBuilder.DropForeignKey(
                name: "fk_language_long_names_languages_long_name_language_short_name",
                schema: "portal",
                table: "language_long_names");

            migrationBuilder.DropForeignKey(
                name: "fk_notifications_company_users_receiver_id",
                schema: "portal",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "fk_notifications_identities_creator_id",
                schema: "portal",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "fk_technical_user_profile_assigned_user_roles_user_roles_user_r",
                schema: "portal",
                table: "technical_user_profile_assigned_user_roles");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "date_last_changed",
                schema: "portal",
                table: "company_certificates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "last_editor_id",
                schema: "portal",
                table: "company_certificates",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_certificate_management20240416",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    valid_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    valid_till = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    company_certificate_type_id = table.Column<int>(type: "integer", nullable: true),
                    company_certificate_status_id = table.Column<int>(type: "integer", nullable: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_certificate_management20240416", x => x.audit_v1id);
                });

            migrationBuilder.AddForeignKey(
                name: "fk_addresses_countries_country_alpha2code",
                schema: "portal",
                table: "addresses",
                column: "country_alpha2code",
                principalSchema: "portal",
                principalTable: "countries",
                principalColumn: "alpha2code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_app_languages_languages_language_short_name",
                schema: "portal",
                table: "app_languages",
                column: "language_short_name",
                principalSchema: "portal",
                principalTable: "languages",
                principalColumn: "short_name");

            migrationBuilder.AddForeignKey(
                name: "fk_company_certificate_type_descriptions_languages_language_sh",
                schema: "portal",
                table: "company_certificate_type_descriptions",
                column: "language_short_name",
                principalSchema: "portal",
                principalTable: "languages",
                principalColumn: "short_name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_company_role_descriptions_languages_language_short_name",
                schema: "portal",
                table: "company_role_descriptions",
                column: "language_short_name",
                principalSchema: "portal",
                principalTable: "languages",
                principalColumn: "short_name",
                onDelete: ReferentialAction.Cascade);

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
                name: "fk_company_users_identities_id",
                schema: "portal",
                table: "company_users",
                column: "id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_connectors_countries_location_id",
                schema: "portal",
                table: "connectors",
                column: "location_id",
                principalSchema: "portal",
                principalTable: "countries",
                principalColumn: "alpha2code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_country_long_names_countries_alpha2code",
                schema: "portal",
                table: "country_long_names",
                column: "alpha2code",
                principalSchema: "portal",
                principalTable: "countries",
                principalColumn: "alpha2code");

            migrationBuilder.AddForeignKey(
                name: "fk_country_long_names_languages_short_name",
                schema: "portal",
                table: "country_long_names",
                column: "short_name",
                principalSchema: "portal",
                principalTable: "languages",
                principalColumn: "short_name");

            migrationBuilder.AddForeignKey(
                name: "fk_identities_identity_user_statuses_user_status_id",
                schema: "portal",
                table: "identities",
                column: "user_status_id",
                principalSchema: "portal",
                principalTable: "identity_user_statuses",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_language_long_names_languages_language_short_name",
                schema: "portal",
                table: "language_long_names",
                column: "language_short_name",
                principalSchema: "portal",
                principalTable: "languages",
                principalColumn: "short_name");

            migrationBuilder.AddForeignKey(
                name: "fk_language_long_names_languages_short_name",
                schema: "portal",
                table: "language_long_names",
                column: "short_name",
                principalSchema: "portal",
                principalTable: "languages",
                principalColumn: "short_name");

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_company_users_receiver_user_id",
                schema: "portal",
                table: "notifications",
                column: "receiver_user_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_identities_creator_user_id",
                schema: "portal",
                table: "notifications",
                column: "creator_user_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_technical_user_profile_assigned_user_roles_user_roles_user_",
                schema: "portal",
                table: "technical_user_profile_assigned_user_roles",
                column: "user_role_id",
                principalSchema: "portal",
                principalTable: "user_roles",
                principalColumn: "id");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_certificate_management20240416\" (\"id\", \"valid_from\", \"valid_till\", \"company_certificate_type_id\", \"company_certificate_status_id\", \"company_id\", \"document_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"valid_from\", \r\n  NEW.\"valid_till\", \r\n  NEW.\"company_certificate_type_id\", \r\n  NEW.\"company_certificate_status_id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"document_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE AFTER INSERT\r\nON \"portal\".\"company_certificates\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_certificate_management20240416\" (\"id\", \"valid_from\", \"valid_till\", \"company_certificate_type_id\", \"company_certificate_status_id\", \"company_id\", \"document_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"valid_from\", \r\n  NEW.\"valid_till\", \r\n  NEW.\"company_certificate_type_id\", \r\n  NEW.\"company_certificate_status_id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"document_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE AFTER UPDATE\r\nON \"portal\".\"company_certificates\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE\"() CASCADE;");

            migrationBuilder.DropForeignKey(
                name: "fk_addresses_countries_country_alpha2code",
                schema: "portal",
                table: "addresses");

            migrationBuilder.DropForeignKey(
                name: "fk_app_languages_languages_language_short_name",
                schema: "portal",
                table: "app_languages");

            migrationBuilder.DropForeignKey(
                name: "fk_company_certificate_type_descriptions_languages_language_sh",
                schema: "portal",
                table: "company_certificate_type_descriptions");

            migrationBuilder.DropForeignKey(
                name: "fk_company_role_descriptions_languages_language_short_name",
                schema: "portal",
                table: "company_role_descriptions");

            migrationBuilder.DropForeignKey(
                name: "fk_company_service_accounts_identities_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_company_users_identities_id",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropForeignKey(
                name: "fk_connectors_countries_location_id",
                schema: "portal",
                table: "connectors");

            migrationBuilder.DropForeignKey(
                name: "fk_country_long_names_countries_alpha2code",
                schema: "portal",
                table: "country_long_names");

            migrationBuilder.DropForeignKey(
                name: "fk_country_long_names_languages_short_name",
                schema: "portal",
                table: "country_long_names");

            migrationBuilder.DropForeignKey(
                name: "fk_identities_identity_user_statuses_user_status_id",
                schema: "portal",
                table: "identities");

            migrationBuilder.DropForeignKey(
                name: "fk_language_long_names_languages_language_short_name",
                schema: "portal",
                table: "language_long_names");

            migrationBuilder.DropForeignKey(
                name: "fk_language_long_names_languages_short_name",
                schema: "portal",
                table: "language_long_names");

            migrationBuilder.DropForeignKey(
                name: "fk_notifications_company_users_receiver_user_id",
                schema: "portal",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "fk_notifications_identities_creator_user_id",
                schema: "portal",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "fk_technical_user_profile_assigned_user_roles_user_roles_user_",
                schema: "portal",
                table: "technical_user_profile_assigned_user_roles");

            migrationBuilder.DropTable(
                name: "audit_certificate_management20240416",
                schema: "portal");

            migrationBuilder.DropColumn(
                name: "date_last_changed",
                schema: "portal",
                table: "company_certificates");

            migrationBuilder.DropColumn(
                name: "last_editor_id",
                schema: "portal",
                table: "company_certificates");

            migrationBuilder.AddForeignKey(
                name: "fk_addresses_countries_country_temp_id",
                schema: "portal",
                table: "addresses",
                column: "country_alpha2code",
                principalSchema: "portal",
                principalTable: "countries",
                principalColumn: "alpha2code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_app_languages_languages_language_temp_id",
                schema: "portal",
                table: "app_languages",
                column: "language_short_name",
                principalSchema: "portal",
                principalTable: "languages",
                principalColumn: "short_name");

            migrationBuilder.AddForeignKey(
                name: "fk_company_certificate_type_descriptions_languages_language_te",
                schema: "portal",
                table: "company_certificate_type_descriptions",
                column: "language_short_name",
                principalSchema: "portal",
                principalTable: "languages",
                principalColumn: "short_name",
                onDelete: ReferentialAction.Cascade);

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
                name: "fk_company_service_accounts_identities_identity_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_company_users_identities_identity_id",
                schema: "portal",
                table: "company_users",
                column: "id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_connectors_countries_location_temp_id1",
                schema: "portal",
                table: "connectors",
                column: "location_id",
                principalSchema: "portal",
                principalTable: "countries",
                principalColumn: "alpha2code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_country_long_names_countries_country_alpha2code",
                schema: "portal",
                table: "country_long_names",
                column: "alpha2code",
                principalSchema: "portal",
                principalTable: "countries",
                principalColumn: "alpha2code");

            migrationBuilder.AddForeignKey(
                name: "fk_country_long_names_languages_language_temp_id3",
                schema: "portal",
                table: "country_long_names",
                column: "short_name",
                principalSchema: "portal",
                principalTable: "languages",
                principalColumn: "short_name");

            migrationBuilder.AddForeignKey(
                name: "fk_identities_identity_user_statuses_identity_status_id",
                schema: "portal",
                table: "identities",
                column: "user_status_id",
                principalSchema: "portal",
                principalTable: "identity_user_statuses",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_language_long_names_languages_language_short_name",
                schema: "portal",
                table: "language_long_names",
                column: "short_name",
                principalSchema: "portal",
                principalTable: "languages",
                principalColumn: "short_name");

            migrationBuilder.AddForeignKey(
                name: "fk_language_long_names_languages_long_name_language_short_name",
                schema: "portal",
                table: "language_long_names",
                column: "language_short_name",
                principalSchema: "portal",
                principalTable: "languages",
                principalColumn: "short_name");

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_company_users_receiver_id",
                schema: "portal",
                table: "notifications",
                column: "receiver_user_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_identities_creator_id",
                schema: "portal",
                table: "notifications",
                column: "creator_user_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_technical_user_profile_assigned_user_roles_user_roles_user_r",
                schema: "portal",
                table: "technical_user_profile_assigned_user_roles",
                column: "user_role_id",
                principalSchema: "portal",
                principalTable: "user_roles",
                principalColumn: "id");
        }
    }
}
