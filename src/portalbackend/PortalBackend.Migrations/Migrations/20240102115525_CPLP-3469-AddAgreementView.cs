/********************************************************************************
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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
    public partial class CPLP3469AddAgreementView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "agreement_type",
                schema: "portal",
                table: "agreements");

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

            migrationBuilder.InsertData(
                schema: "portal",
                table: "agreement_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "ACTIVE" },
                    { 2, "INACTIVE" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_agreements_agreement_status_id",
                schema: "portal",
                table: "agreements",
                column: "agreement_status_id");

            migrationBuilder.AddForeignKey(
                name: "fk_agreements_agreement_statuses_agreement_status_id",
                schema: "portal",
                table: "agreements",
                column: "agreement_status_id",
                principalSchema: "portal",
                principalTable: "agreement_statuses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

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
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS portal.agreement_view");

            migrationBuilder.DropForeignKey(
                name: "fk_agreements_agreement_statuses_agreement_status_id",
                schema: "portal",
                table: "agreements");

            migrationBuilder.DropTable(
                name: "agreement_statuses",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_agreements_agreement_status_id",
                schema: "portal",
                table: "agreements");

            migrationBuilder.DropColumn(
                name: "agreement_status_id",
                schema: "portal",
                table: "agreements");

            migrationBuilder.DropColumn(
                name: "mandatory",
                schema: "portal",
                table: "agreements");

            migrationBuilder.AddColumn<string>(
                name: "agreement_type",
                schema: "portal",
                table: "agreements",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
