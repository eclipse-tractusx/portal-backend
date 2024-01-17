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
    public partial class _180rc1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM portal.process_steps WHERE process_step_type_id = '300'");
            migrationBuilder.Sql("DELETE FROM portal.processes WHERE process_type_id = '5'");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 300);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_types",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "country_name_de",
                schema: "portal",
                table: "countries");

            migrationBuilder.DropColumn(
                name: "country_name_en",
                schema: "portal",
                table: "countries");

            migrationBuilder.DropColumn(
                name: "client_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropColumn(
                name: "agreement_type",
                schema: "portal",
                table: "agreements");

            migrationBuilder.AlterColumn<string>(
                name: "template",
                schema: "portal",
                table: "verified_credential_external_type_use_case_detail_versions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "agreement_status_id",
                schema: "portal",
                table: "agreements",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "mandatory",
                schema: "portal",
                table: "agreements",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "agreement_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agreement_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "country_long_names",
                schema: "portal",
                columns: table => new
                {
                    alpha2code = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    short_name = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    long_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_country_long_names", x => new { x.alpha2code, x.short_name });
                    table.ForeignKey(
                        name: "fk_country_long_names_countries_country_alpha2code",
                        column: x => x.alpha2code,
                        principalSchema: "portal",
                        principalTable: "countries",
                        principalColumn: "alpha2code");
                    table.ForeignKey(
                        name: "fk_country_long_names_languages_language_temp_id2",
                        column: x => x.short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name");
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "agreement_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "ACTIVE" },
                    { 2, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "verified_credential_external_types",
                columns: new[] { "id", "label" },
                values: new object[] { 6, "Quality_Credential" });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "verified_credential_types",
                columns: new[] { "id", "label" },
                values: new object[] { 6, "FRAMEWORK_AGREEMENT_QUALITY" });

            migrationBuilder.CreateIndex(
                name: "ix_agreements_agreement_status_id",
                schema: "portal",
                table: "agreements",
                column: "agreement_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_country_long_names_short_name",
                schema: "portal",
                table: "country_long_names",
                column: "short_name");

            migrationBuilder.AddForeignKey(
                name: "fk_agreements_agreement_statuses_agreement_status_id",
                schema: "portal",
                table: "agreements",
                column: "agreement_status_id",
                principalSchema: "portal",
                principalTable: "agreement_statuses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("INSERT INTO portal.use_cases (id, name, shortname) values ('b3948771-3372-4568-9e0e-acca4e674098', 'Behavior Twin', 'BT') ON CONFLICT DO NOTHING");

            migrationBuilder.Sql("UPDATE portal.verified_credential_type_assigned_use_cases SET use_case_id = 'b3948771-3372-4568-9e0e-acca4e674098' WHERE verified_credential_type_id = 3 and use_case_id = 'c065a349-f649-47f8-94d5-1a504a855419'");

            migrationBuilder.Sql("UPDATE portal.agreements SET agreement_status_id = 2 WHERE id = 'aa0a0000-7fbc-1f2f-817f-bce0502c1090'");

            migrationBuilder.Sql(@"CREATE VIEW portal.agreement_view AS
                SELECT 
                    a.id as agreement_id, 
                    a.name as agreement_name, 
                    cr.label as agreement_company_role,
                    args.label as agreement_status,
                    a.mandatory as mandatory
                FROM 
                portal.agreements as a
                	INNER JOIN portal.agreement_assigned_company_roles as aacr on (a. id = aacr.agreement_id)
                 	INNER JOIN portal.company_roles as cr on (aacr.company_role_id = cr.id)
                	INNER JOIN portal.agreement_statuses as args on (a.agreement_status_id = args.id)");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM portal.verified_credential_type_assigned_use_cases WHERE use_case_id = 'c065a349-f649-47f8-94d5-1a504a855419'");
            migrationBuilder.Sql("DELETE FROM portal.verified_credential_type_assigned_external_types WHERE verified_credential_type_id = 6 and verified_credential_external_type_id = 6");
            migrationBuilder.Sql("DELETE FROM portal.verified_credential_type_assigned_kinds WHERE verified_credential_type_id = 6 and verified_credential_type_kind_id = 1");
            migrationBuilder.Sql("DELETE FROM portal.verified_credential_external_type_use_case_detail_versions WHERE verified_credential_external_type_id = 6");
            migrationBuilder.Sql("UPDATE portal.verified_credential_type_assigned_use_cases SET use_case_id = 'c065a349-f649-47f8-94d5-1a504a855419' WHERE verified_credential_type_id = 3 and use_case_id = 'b3948771-3372-4568-9e0e-acca4e674098'");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS portal.agreement_view");

            migrationBuilder.DropForeignKey(
                name: "fk_agreements_agreement_statuses_agreement_status_id",
                schema: "portal",
                table: "agreements");

            migrationBuilder.DropTable(
                name: "agreement_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "country_long_names",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_agreements_agreement_status_id",
                schema: "portal",
                table: "agreements");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "verified_credential_external_types",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "verified_credential_types",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "agreement_status_id",
                schema: "portal",
                table: "agreements");

            migrationBuilder.DropColumn(
                name: "mandatory",
                schema: "portal",
                table: "agreements");

            migrationBuilder.AlterColumn<string>(
                name: "template",
                schema: "portal",
                table: "verified_credential_external_type_use_case_detail_versions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "country_name_de",
                schema: "portal",
                table: "countries",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "country_name_en",
                schema: "portal",
                table: "countries",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "client_id",
                schema: "portal",
                table: "company_service_accounts",
                type: "character varying(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "agreement_type",
                schema: "portal",
                table: "agreements",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[] { 300, "SYNCHRONIZE_SERVICE_ACCOUNTS" });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_types",
                columns: new[] { "id", "label" },
                values: new object[] { 5, "SERVICE_ACCOUNT_SYNC" });
        }
    }
}
