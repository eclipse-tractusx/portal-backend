/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
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

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1047CompanyUserBusinessPartners : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "parent",
                schema: "portal",
                table: "companies");

            migrationBuilder.RenameColumn(
                name: "bpn",
                schema: "portal",
                table: "companies",
                newName: "business_partner_number");

            migrationBuilder.AlterColumn<string>(
                name: "zipcode",
                schema: "portal",
                table: "addresses",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(19,2)",
                oldPrecision: 19,
                oldScale: 2);

            migrationBuilder.CreateTable(
                name: "company_user_assigned_business_partners",
                schema: "portal",
                columns: table => new
                {
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    business_partner_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_user_assigned_business_partners", x => new { x.company_user_id, x.business_partner_number });
                    table.ForeignKey(
                        name: "fk_company_user_assigned_business_partners_company_users_compa",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "company_user_assigned_business_partners",
                schema: "portal");

            migrationBuilder.RenameColumn(
                name: "business_partner_number",
                schema: "portal",
                table: "companies",
                newName: "bpn");

            migrationBuilder.AddColumn<string>(
                name: "parent",
                schema: "portal",
                table: "companies",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "zipcode",
                schema: "portal",
                table: "addresses",
                type: "numeric(19,2)",
                precision: 19,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(string),
                oldType: "character varying(12)",
                oldMaxLength: 12,
                oldNullable: true);
        }
    }
}
