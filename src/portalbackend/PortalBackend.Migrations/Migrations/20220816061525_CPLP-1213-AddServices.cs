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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1213AddServices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_services_cplp_1213_add_services",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_operation_id = table.Column<int>(type: "integer", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    thumbnail_url = table.Column<string>(type: "text", nullable: true),
                    provider_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_status_id = table.Column<int>(type: "integer", nullable: false),
                    contact_email = table.Column<string>(type: "text", nullable: true),
                    sales_manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_services_cplp_1213_add_services", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "service_licenses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    license_text = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_licenses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "service_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "service_subscription_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_subscription_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "services",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    thumbnail_url = table.Column<string>(type: "text", nullable: true),
                    provider_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_status_id = table.Column<int>(type: "integer", nullable: false),
                    contact_email = table.Column<string>(type: "text", nullable: true),
                    sales_manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_services", x => x.id);
                    table.ForeignKey(
                        name: "fk_services_companies_provider_company_id",
                        column: x => x.provider_company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_services_company_users_sales_manager_id",
                        column: x => x.sales_manager_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_services_service_statuses_service_status_id",
                        column: x => x.service_status_id,
                        principalSchema: "portal",
                        principalTable: "service_statuses",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_assigned_services",
                schema: "portal",
                columns: table => new
                {
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_subscription_status_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_assigned_services", x => new { x.service_id, x.company_id });
                    table.ForeignKey(
                        name: "fk_company_assigned_services_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_assigned_services_company_users_requester_id",
                        column: x => x.requester_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_assigned_services_service_subscription_statuses_ser",
                        column: x => x.service_subscription_status_id,
                        principalSchema: "portal",
                        principalTable: "service_subscription_statuses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_assigned_services_services_service_id",
                        column: x => x.service_id,
                        principalSchema: "portal",
                        principalTable: "services",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "service_assigned_licenses",
                schema: "portal",
                columns: table => new
                {
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_license_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_assigned_licenses", x => new { x.service_id, x.service_license_id });
                    table.ForeignKey(
                        name: "fk_service_assigned_licenses_service_licenses_service_license_",
                        column: x => x.service_license_id,
                        principalSchema: "portal",
                        principalTable: "service_licenses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_service_assigned_licenses_services_service_id",
                        column: x => x.service_id,
                        principalSchema: "portal",
                        principalTable: "services",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "service_descriptions",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_short_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_descriptions", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_descriptions_services_service_id",
                        column: x => x.service_id,
                        principalSchema: "portal",
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_statuses",
                columns: new[] { "id", "label" },
                values: new object[] { 5, "DELETED" });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_user_statuses",
                columns: new[] { "id", "label" },
                values: new object[] { 3, "DELETED" });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "service_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "CREATED" },
                    { 2, "IN_REVIEW" },
                    { 3, "ACTIVE" },
                    { 4, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "service_subscription_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "PENDING" },
                    { 2, "ACTIVE" },
                    { 3, "INACTIVE" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_company_assigned_services_company_id",
                schema: "portal",
                table: "company_assigned_services",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_assigned_services_requester_id",
                schema: "portal",
                table: "company_assigned_services",
                column: "requester_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_assigned_services_service_subscription_status_id",
                schema: "portal",
                table: "company_assigned_services",
                column: "service_subscription_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_assigned_licenses_service_license_id",
                schema: "portal",
                table: "service_assigned_licenses",
                column: "service_license_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_descriptions_service_id",
                schema: "portal",
                table: "service_descriptions",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_services_provider_company_id",
                schema: "portal",
                table: "services",
                column: "provider_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_services_sales_manager_id",
                schema: "portal",
                table: "services",
                column: "sales_manager_id");

            migrationBuilder.CreateIndex(
                name: "ix_services_service_status_id",
                schema: "portal",
                table: "services",
                column: "service_status_id");
            
            // The audit trigger creation need to be reworked for classes that are not existing anymore
            // needs to be done for migration CPLP-1254-db-audit as well
            // migrationBuilder.AddAuditTrigger<AuditService>("audit_services_cplp_1213_add_services");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropAuditTrigger<AuditService>();

            migrationBuilder.DropTable(
                name: "audit_services_cplp_1213_add_services",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_assigned_services",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "service_assigned_licenses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "service_descriptions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "service_subscription_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "service_licenses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "services",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "service_statuses",
                schema: "portal");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "company_statuses",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "company_user_statuses",
                keyColumn: "id",
                keyValue: 3);
        }
    }
}
