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

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class CPLP3383AddNewProcesses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "company_invitations",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_name = table.Column<string>(type: "text", nullable: true),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    organisation_name = table.Column<string>(type: "text", nullable: false),
                    process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    application_id = table.Column<Guid>(type: "uuid", nullable: true),
                    idp_name = table.Column<string>(type: "text", nullable: true),
                    password = table.Column<byte[]>(type: "bytea", nullable: true),
                    client_id = table.Column<string>(type: "text", nullable: true),
                    client_secret = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_invitations", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_invitations_company_applications_application_id",
                        column: x => x.application_id,
                        principalSchema: "portal",
                        principalTable: "company_applications",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_invitations_processes_process_id",
                        column: x => x.process_id,
                        principalSchema: "portal",
                        principalTable: "processes",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mailing_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mailing_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mailing_informations",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    template = table.Column<string>(type: "text", nullable: false),
                    mailing_status_id = table.Column<int>(type: "integer", nullable: false),
                    mail_parameter = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mailing_informations", x => x.id);
                    table.ForeignKey(
                        name: "fk_mailing_informations_mailing_statuses_mailing_status_id",
                        column: x => x.mailing_status_id,
                        principalSchema: "portal",
                        principalTable: "mailing_statuses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_mailing_informations_processes_process_id",
                        column: x => x.process_id,
                        principalSchema: "portal",
                        principalTable: "processes",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "mailing_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "PENDING" },
                    { 2, "SENT" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 301, "SEND_MAIL" },
                    { 302, "RETRIGGER_SEND_MAIL" },
                    { 400, "INVITATION_CREATE_CENTRAL_IDP" },
                    { 401, "INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT" },
                    { 402, "INVITATION_UPDATE_CENTRAL_IDP_URLS" },
                    { 403, "INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER" },
                    { 404, "INVITATION_CREATE_SHARED_REALM_IDP_CLIENT" },
                    { 405, "INVITATION_ENABLE_CENTRAL_IDP" },
                    { 406, "INVITATION_CREATE_DATABASE_IDP" },
                    { 407, "INVITATION_CREATE_USER" },
                    { 408, "INVITATION_SEND_MAIL" },
                    { 409, "RETRIGGER_INVITATION_CREATE_CENTRAL_IDP" },
                    { 410, "RETRIGGER_INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT" },
                    { 411, "RETRIGGER_INVITATION_UPDATE_CENTRAL_IDP_URLS" },
                    { 412, "RETRIGGER_INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER" },
                    { 413, "RETRIGGER_INVITATION_CREATE_SHARED_REALM_IDP_CLIENT" },
                    { 414, "RETRIGGER_INVITATION_ENABLE_CENTRAL_IDP" },
                    { 415, "RETRIGGER_INVITATION_CREATE_USER" },
                    { 416, "RETRIGGER_INVITATION_CREATE_DATABASE_IDP" },
                    { 417, "RETRIGGER_INVITATION_SEND_MAIL" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 5, "MAILING" },
                    { 6, "INVITATION" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_company_invitations_application_id",
                schema: "portal",
                table: "company_invitations",
                column: "application_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_company_invitations_process_id",
                schema: "portal",
                table: "company_invitations",
                column: "process_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mailing_informations_mailing_status_id",
                schema: "portal",
                table: "mailing_informations",
                column: "mailing_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_mailing_informations_process_id",
                schema: "portal",
                table: "mailing_informations",
                column: "process_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "company_invitations",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "mailing_informations",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "mailing_statuses",
                schema: "portal");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 301);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 302);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 400);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 401);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 402);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 403);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 404);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 405);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 406);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 407);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 408);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 409);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 410);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 411);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 412);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 413);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 414);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 415);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 416);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 417);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_types",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_types",
                keyColumn: "id",
                keyValue: 6);
        }
    }
}
