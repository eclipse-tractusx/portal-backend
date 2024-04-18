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
    public partial class _200rc2 : Migration
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

            // migrate data that is affected by reorder of process_step_types:

            migrationBuilder.Sql("DELETE from portal.process_steps WHERE process_step_type_id = 421");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 421 WHERE process_step_type_id = 405"); // use empty 421 as temp storage

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 405 WHERE process_step_type_id = 402");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 402 WHERE process_step_type_id = 403");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 403 WHERE process_step_type_id = 421");

            migrationBuilder.Sql("DELETE from portal.process_steps WHERE process_step_type_id = 410");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 410 WHERE process_step_type_id = 411");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 411 WHERE process_step_type_id = 412");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 412 WHERE process_step_type_id = 414");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 414 WHERE process_step_type_id = 415");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 415 WHERE process_step_type_id = 413");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 413 WHERE process_step_type_id = 416");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 416 WHERE process_step_type_id = 417");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 417 WHERE process_step_type_id = 418");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 418 WHERE process_step_type_id = 419");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 419 WHERE process_step_type_id = 420");

            // end of data migration

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 420);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 421);

            migrationBuilder.AddColumn<string>(
                name: "did_document_location",
                schema: "portal",
                table: "companies",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "application_checklist_types",
                keyColumn: "id",
                keyValue: 4,
                column: "label",
                value: "BPNL_CREDENTIAL");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "application_checklist_types",
                keyColumn: "id",
                keyValue: 5,
                column: "label",
                value: "MEMBERSHIP_CREDENTIAL");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "application_checklist_types",
                keyColumn: "id",
                keyValue: 6,
                column: "label",
                value: "CLEARING_HOUSE");

            migrationBuilder.InsertData(
                schema: "portal",
                table: "application_checklist_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 7, "SELF_DESCRIPTION_LP" },
                    { 8, "APPLICATION_ACTIVATION" }
                });

            migrationBuilder.Sql("UPDATE portal.application_checklist SET application_checklist_entry_type_id = 8 WHERE application_checklist_entry_type_id = 6");
            migrationBuilder.Sql("UPDATE portal.application_checklist SET application_checklist_entry_type_id = 7 WHERE application_checklist_entry_type_id = 5");
            migrationBuilder.Sql("UPDATE portal.application_checklist SET application_checklist_entry_type_id = 6 WHERE application_checklist_entry_type_id = 4");

            migrationBuilder.InsertData(
                schema: "portal",
                table: "notification_type",
                columns: new[] { "id", "label" },
                values: new object[] { 26, "CREDENTIAL_EXPIRY" });

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 402,
                column: "label",
                value: "INVITATION_ADD_REALM_ROLE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 403,
                column: "label",
                value: "INVITATION_CREATE_SHARED_REALM");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 405,
                column: "label",
                value: "INVITATION_UPDATE_CENTRAL_IDP_URLS");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 410,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_CENTRAL_IDP");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 411,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 412,
                column: "label",
                value: "RETRIGGER_INVITATION_ADD_REALM_ROLE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 413,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_SHARED_REALM");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 414,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 415,
                column: "label",
                value: "RETRIGGER_INVITATION_UPDATE_CENTRAL_IDP_URLS");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 416,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_SHARED_CLIENT");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 417,
                column: "label",
                value: "RETRIGGER_INVITATION_ENABLE_CENTRAL_IDP");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 418,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_USER");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 419,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_DATABASE_IDP");

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 25, "REQUEST_BPN_CREDENTIAL" },
                    { 26, "STORED_BPN_CREDENTIAL" },
                    { 27, "REQUEST_MEMBERSHIP_CREDENTIAL" },
                    { 28, "STORED_MEMBERSHIP_CREDENTIAL" },
                    { 29, "TRANSMIT_BPN_DID" },
                    { 30, "RETRIGGER_TRANSMIT_DID_BPN" }
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE from portal.application_checklist WHERE application_checklist_entry_type_id = 4");
            migrationBuilder.Sql("DELETE from portal.application_checklist WHERE application_checklist_entry_type_id = 5");
            migrationBuilder.Sql("UPDATE portal.application_checklist SET application_checklist_entry_type_id = 4 WHERE application_checklist_entry_type_id = 6");
            migrationBuilder.Sql("UPDATE portal.application_checklist SET application_checklist_entry_type_id = 5 WHERE application_checklist_entry_type_id = 7");
            migrationBuilder.Sql("UPDATE portal.application_checklist SET application_checklist_entry_type_id = 6 WHERE application_checklist_entry_type_id = 8");

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

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "application_checklist_types",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "application_checklist_types",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "notification_type",
                keyColumn: "id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 30);

            migrationBuilder.DropColumn(
                name: "did_document_location",
                schema: "portal",
                table: "companies");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "application_checklist_types",
                keyColumn: "id",
                keyValue: 4,
                column: "label",
                value: "CLEARING_HOUSE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "application_checklist_types",
                keyColumn: "id",
                keyValue: 5,
                column: "label",
                value: "SELF_DESCRIPTION_LP");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "application_checklist_types",
                keyColumn: "id",
                keyValue: 6,
                column: "label",
                value: "APPLICATION_ACTIVATION");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 402,
                column: "label",
                value: "INVITATION_UPDATE_CENTRAL_IDP_URLS");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 403,
                column: "label",
                value: "INVITATION_ADD_REALM_ROLE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 405,
                column: "label",
                value: "INVITATION_CREATE_SHARED_REALM");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 410,
                column: "label",
                value: "INVITATION_SEND_MAIL");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 411,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_CENTRAL_IDP");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 412,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 413,
                column: "label",
                value: "RETRIGGER_INVITATION_UPDATE_CENTRAL_IDP_URLS");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 414,
                column: "label",
                value: "RETRIGGER_INVITATION_ADD_REALM_ROLE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 415,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 416,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_SHARED_REALM");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 417,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_SHARED_CLIENT");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 418,
                column: "label",
                value: "RETRIGGER_INVITATION_ENABLE_CENTRAL_IDP");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 419,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_USER");

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 420, "RETRIGGER_INVITATION_CREATE_DATABASE_IDP" },
                    { 421, "RETRIGGER_INVITATION_SEND_MAIL" }
                });

            // migrate data that is affected by reorder of process_step_types:

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 421 WHERE process_step_type_id = 403"); // use empty 421 as temp storage

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 403 WHERE process_step_type_id = 402");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 402 WHERE process_step_type_id = 405");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 405 WHERE process_step_type_id = 421");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 420 WHERE process_step_type_id = 419");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 419 WHERE process_step_type_id = 418");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 418 WHERE process_step_type_id = 417");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 417 WHERE process_step_type_id = 416");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 416 WHERE process_step_type_id = 413");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 413 WHERE process_step_type_id = 415");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 415 WHERE process_step_type_id = 414");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 414 WHERE process_step_type_id = 412");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 412 WHERE process_step_type_id = 411");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 411 WHERE process_step_type_id = 410");

            // end of data migration

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
