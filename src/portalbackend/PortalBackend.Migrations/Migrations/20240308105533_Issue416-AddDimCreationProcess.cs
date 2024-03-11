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
using System.Text.Json;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class Issue416AddDimCreationProcess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_onboarding_service_provider_details",
                schema: "portal",
                table: "onboarding_service_provider_details");

            migrationBuilder.DropPrimaryKey(
                name: "pk_offer_subscriptions_process_datas",
                schema: "portal",
                table: "offer_subscriptions_process_datas");

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                schema: "portal",
                table: "onboarding_service_provider_details",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("UPDATE portal.onboarding_service_provider_details SET id = gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                schema: "portal",
                table: "offer_subscriptions_process_datas",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("UPDATE portal.offer_subscriptions_process_datas SET id = gen_random_uuid()");

            migrationBuilder.AddPrimaryKey(
                name: "pk_onboarding_service_provider_details",
                schema: "portal",
                table: "onboarding_service_provider_details",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_offer_subscriptions_process_datas",
                schema: "portal",
                table: "offer_subscriptions_process_datas",
                column: "id");

            migrationBuilder.CreateTable(
                name: "company_wallet_datas",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    did = table.Column<string>(type: "text", nullable: false),
                    client_id = table.Column<string>(type: "text", nullable: false),
                    client_secret = table.Column<byte[]>(type: "bytea", nullable: false),
                    initialization_vector = table.Column<byte[]>(type: "bytea", nullable: true),
                    encryption_mode = table.Column<int>(type: "integer", nullable: false),
                    authentication_service_url = table.Column<string>(type: "text", nullable: false),
                    did_document = table.Column<JsonDocument>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_wallet_datas", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_wallet_datas_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 20, "CREATE_DIM_WALLET" },
                    { 21, "AWAIT_DIM_RESPONSE" },
                    { 22, "RETRIGGER_CREATE_DIM_WALLET" },
                    { 23, "VALIDATE_DID_DOCUMENT" },
                    { 24, "RETRIGGER_VALIDATE_DID_DOCUMENT" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_onboarding_service_provider_details_company_id",
                schema: "portal",
                table: "onboarding_service_provider_details",
                column: "company_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_offer_subscriptions_process_datas_offer_subscription_id",
                schema: "portal",
                table: "offer_subscriptions_process_datas",
                column: "offer_subscription_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_company_wallet_datas_company_id",
                schema: "portal",
                table: "company_wallet_datas",
                column: "company_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "company_wallet_datas",
                schema: "portal");

            migrationBuilder.DropPrimaryKey(
                name: "pk_onboarding_service_provider_details",
                schema: "portal",
                table: "onboarding_service_provider_details");

            migrationBuilder.DropIndex(
                name: "ix_onboarding_service_provider_details_company_id",
                schema: "portal",
                table: "onboarding_service_provider_details");

            migrationBuilder.DropPrimaryKey(
                name: "pk_offer_subscriptions_process_datas",
                schema: "portal",
                table: "offer_subscriptions_process_datas");

            migrationBuilder.DropIndex(
                name: "ix_offer_subscriptions_process_datas_offer_subscription_id",
                schema: "portal",
                table: "offer_subscriptions_process_datas");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 24);

            migrationBuilder.DropColumn(
                name: "id",
                schema: "portal",
                table: "onboarding_service_provider_details");

            migrationBuilder.DropColumn(
                name: "id",
                schema: "portal",
                table: "offer_subscriptions_process_datas");

            migrationBuilder.AddPrimaryKey(
                name: "pk_onboarding_service_provider_details",
                schema: "portal",
                table: "onboarding_service_provider_details",
                column: "company_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_offer_subscriptions_process_datas",
                schema: "portal",
                table: "offer_subscriptions_process_datas",
                column: "offer_subscription_id");
        }
    }
}
