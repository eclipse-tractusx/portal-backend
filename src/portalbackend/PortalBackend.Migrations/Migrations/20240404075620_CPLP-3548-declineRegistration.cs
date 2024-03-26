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

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class CPLP3548declineRegistration : Migration
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

            migrationBuilder.AddColumn<Guid>(
                name: "process_id",
                schema: "portal",
                table: "identity_providers",
                type: "uuid",
                nullable: true);

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 500, "TRIGGER_DELETE_IDP_SHARED_REALM" },
                    { 501, "TRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT" },
                    { 502, "TRIGGER_DELETE_IDENTITY_LINKED_USERS" },
                    { 503, "TRIGGER_DELETE_CENTRAL_IDENTITY_PROVIDER" },
                    { 504, "TRIGGER_DELETE_IDENTITY_PROVIDER" },
                    { 505, "RETRIGGER_DELETE_IDP_SHARED_REALM" },
                    { 506, "RETRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT" },
                    { 507, "RETRIGGER_DELETE_IDENTITY_LINKED_USERS" },
                    { 508, "RETRIGGER_DELETE_CENTRAL_IDENTITY_PROVIDER" },
                    { 509, "RETRIGGER_DELETE_IDENTITY_PROVIDER" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_types",
                columns: new[] { "id", "label" },
                values: new object[] { 7, "IDP_DELETION" });

            migrationBuilder.CreateIndex(
                name: "ix_identity_providers_process_id",
                schema: "portal",
                table: "identity_providers",
                column: "process_id",
                unique: true);

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
                name: "fk_identity_providers_processes_process_id",
                schema: "portal",
                table: "identity_providers",
                column: "process_id",
                principalSchema: "portal",
                principalTable: "processes",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "fk_identity_providers_processes_process_id",
                schema: "portal",
                table: "identity_providers");

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

            migrationBuilder.DropIndex(
                name: "ix_identity_providers_process_id",
                schema: "portal",
                table: "identity_providers");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 500);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 501);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 502);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 503);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 504);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 505);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 506);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 507);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 508);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 509);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_types",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DropColumn(
                name: "process_id",
                schema: "portal",
                table: "identity_providers");

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
