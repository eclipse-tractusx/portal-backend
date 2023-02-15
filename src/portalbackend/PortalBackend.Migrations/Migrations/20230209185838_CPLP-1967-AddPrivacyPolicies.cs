/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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
    public partial class CPLP1967AddPrivacyPolicies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "privacy_policies",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_privacy_policies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "offer_assigned_privacy_policies",
                schema: "portal",
                columns: table => new
                {
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    privacy_policy_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_assigned_privacy_policies", x => new { x.offer_id, x.privacy_policy_id });
                    table.ForeignKey(
                        name: "fk_offer_assigned_privacy_policies_offers_offer_id",
                        column: x => x.offer_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_offer_assigned_privacy_policies_privacy_policies_privacy_po",
                        column: x => x.privacy_policy_id,
                        principalSchema: "portal",
                        principalTable: "privacy_policies",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "privacy_policies",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "COMPANY_DATA" },
                    { 2, "USER_DATA" },
                    { 3, "LOCATION" },
                    { 4, "BROWSER_HISTORY" },
                    { 5, "NONE" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_offer_assigned_privacy_policies_privacy_policy_id",
                schema: "portal",
                table: "offer_assigned_privacy_policies",
                column: "privacy_policy_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "offer_assigned_privacy_policies",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "privacy_policies",
                schema: "portal");
        }
    }
}
