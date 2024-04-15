/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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
    public partial class issue579InvitationProcessStepTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.Sql("WITH retrigger_invitation_create_database_idp As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 420)");

            migrationBuilder.Sql("WITH retrigger_invitation_create_user As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 419)");

            migrationBuilder.Sql("WITH retrigger_invitation_enable_central_idp As( SELECT DISTINCT id from portal.process_steps where process_step_type_id = 418)");

            migrationBuilder.Sql("WITH retrigger_invitation_enable_central_idp As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 418)");

            migrationBuilder.Sql("WITH retrigger_invitation_create_shared_client As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 417)");

            migrationBuilder.Sql("WITH retrigger_invitation_create_shared_realm As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 416)");

            migrationBuilder.Sql("WITH retrigger_invitation_create_central_idp_org_mapper As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 415)");

            migrationBuilder.Sql("WITH retrigger_invitation_add_realm_role As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 414)");

            migrationBuilder.Sql("WITH retrigger_invitation_update_central_idp_urls As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 413)");

            migrationBuilder.Sql("WITH retrigger_invitation_create_shared_idp_service_account As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 412)");

            migrationBuilder.Sql("WITH retrigger_invitation_create_central_idp As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 411)");

            migrationBuilder.Sql("WITH invitation_create_shared_realm As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 405)");

            migrationBuilder.Sql("WITH invitation_add_realm_role As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 403)");

            migrationBuilder.Sql("WITH invitation_update_central_idp_urls As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 402)");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 419 from retrigger_invitation_create_database_idp as up where up.id = process_steps.id;");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 418 from retrigger_invitation_create_user as up where up.id = process_steps.id;");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 417 from retrigger_invitation_enable_central_idp as up where up.id = process_steps.id; ");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 416 from retrigger_invitation_create_shared_client as up where up.id = process_steps.id; ");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 413 from retrigger_invitation_create_shared_realm as up where up.id = process_steps.id; ");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 414 from retrigger_invitation_create_central_idp_org_mapper as up where up.id = process_steps.id; ");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 412 from retrigger_invitation_add_realm_role as up where up.id = process_steps.id; ");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 415 from retrigger_invitation_update_central_idp_urls as up where up.id = process_steps.id;");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 411 from retrigger_invitation_create_shared_idp_service_account as up where up.id = process_steps.id;");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 410 from retrigger_invitation_create_central_idp as up where up.id = process_steps.id;");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 403 from invitation_create_shared_realm as up where up.id = process_steps.id;");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 402 from invitation_add_realm_role as up where up.id = process_steps.id;");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 405 from invitation_update_central_idp_urls as up where up.id = process_steps.id;");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.Sql("WITH invitation_add_realm_role As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 402)");

            migrationBuilder.Sql("WITH invitation_create_shared_realm As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 403)");

            migrationBuilder.Sql("WITH invitation_update_central_idp_urls As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 405)");

            migrationBuilder.Sql("WITH retrigger_invitation_create_central_idp As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 410)");

            migrationBuilder.Sql("WITH retrigger_invitation_create_shared_idp_service_account As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 411)");

            migrationBuilder.Sql("WITH retrigger_invitation_add_realm_role As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 412)");

            migrationBuilder.Sql("WITH retrigger_invitation_create_shared_realm As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 413)");

            migrationBuilder.Sql("WITH retrigger_invitation_create_central_idp_org_mapper As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 414)");

            migrationBuilder.Sql("WITH retrigger_invitation_update_central_idp_urls As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 415)");

            migrationBuilder.Sql("WITH retrigger_invitation_create_shared_client As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 416)");

            migrationBuilder.Sql("WITH retrigger_invitation_enable_central_idp As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 417)");

            migrationBuilder.Sql("WITH retrigger_invitation_create_user As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 418)");

            migrationBuilder.Sql("WITH retrigger_invitation_create_database_idp As(SELECT DISTINCT id from portal.process_steps where process_step_type_id = 419)");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 403 from invitation_add_realm_role as up where up.id = process_steps.id;");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 405 from invitation_create_shared_realm as up where up.id = process_steps.id;");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 402 from invitation_update_central_idp_urls as up where up.id = process_steps.id;");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 411 from retrigger_invitation_create_central_idp as up where up.id = process_steps.id;");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 412 from retrigger_invitation_create_shared_idp_service_account as up where up.id = process_steps.id;");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 414 from retrigger_invitation_add_realm_role as up where up.id = process_steps.id;");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 416 from retrigger_invitation_create_shared_realm as up where up.id = process_steps.id;");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 415 from retrigger_invitation_create_central_idp_org_mapper as up where up.id = process_steps.id;");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 413 from retrigger_invitation_update_central_idp_urls as up where up.id = process_steps.id;");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 417 from retrigger_invitation_create_shared_client as up where up.id = process_steps.id;");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 418 from retrigger_invitation_enable_central_idp as up where up.id = process_steps.id;");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 419 from retrigger_invitation_create_user as up where up.id = process_steps.id;");

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 420 from retrigger_invitation_create_database_idp as up where up.id = process_steps.id;");

        }
    }
}
