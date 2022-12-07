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
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.Replication.PgOutput.Messages;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1212ChangeAppToOffer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_agreements_apps_app_id",
                schema: "portal",
                table: "agreements");

            migrationBuilder.DropForeignKey(
                name: "fk_app_assigned_use_cases_apps_app_id",
                schema: "portal",
                table: "app_assigned_use_cases");

            migrationBuilder.DropForeignKey(
                name: "fk_app_instances_apps_app_id",
                schema: "portal",
                table: "app_instances");

            migrationBuilder.DropForeignKey(
                name: "fk_app_languages_apps_app_id",
                schema: "portal",
                table: "app_languages");

            migrationBuilder.DropForeignKey(
                name: "fk_app_languages_languages_language_temp_id1",
                schema: "portal",
                table: "app_languages");

            migrationBuilder.DropForeignKey(
                name: "fk_company_role_descriptions_languages_language_temp_id2",
                schema: "portal",
                table: "company_role_descriptions");

            migrationBuilder.DropForeignKey(
                name: "fk_company_user_assigned_app_favourites_apps_app_id",
                schema: "portal",
                table: "company_user_assigned_app_favourites");

            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_apps_app_id",
                schema: "portal",
                table: "user_roles");

            migrationBuilder.DropTable(
                name: "audit_company_assigned_apps_cplp_1254_db_audit",
                schema: "portal");

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

            migrationBuilder.RenameTable(
                name: "app_assigned_documents",
                newName: "offer_assigned_documents",
                schema: "portal");
            
            migrationBuilder.RenameColumn(
                name: "app_id",
                schema: "portal",
                table: "offer_assigned_documents",
                newName: "offer_id");
            
            migrationBuilder.RenameTable(
                name: "app_assigned_licenses",
                newName: "offer_assigned_licenses",
                schema: "portal");

            migrationBuilder.RenameColumn(
                name: "app_license_id",
                schema: "portal",
                table: "offer_assigned_licenses",
                newName: "offer_license_id");

            migrationBuilder.RenameColumn(
                name: "app_id",
                schema: "portal",
                table: "offer_assigned_licenses",
                newName: "offer_id");

            migrationBuilder.RenameTable(
                name: "app_descriptions",
                newName: "offer_descriptions",
                schema: "portal");

            migrationBuilder.RenameColumn(
                name: "app_id",
                schema: "portal",
                table: "offer_descriptions",
                newName: "offer_id");

            migrationBuilder.RenameTable(
                name: "app_detail_images",
                newName: "offer_detail_images",
                schema: "portal");

            migrationBuilder.RenameColumn(
                name: "app_id",
                schema: "portal",
                table: "offer_detail_images",
                newName: "offer_id");

            migrationBuilder.RenameTable(
                name: "app_tags",
                newName: "offer_tags",
                schema: "portal");

            migrationBuilder.RenameColumn(
                name: "app_id",
                schema: "portal",
                table: "offer_tags",
                newName: "offer_id");

            migrationBuilder.DropPrimaryKey(
                name: "pk_company_assigned_apps",
                table: "company_assigned_apps",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "company_assigned_apps",
                newName: "offer_subscriptions",
                schema: "portal");

            migrationBuilder.RenameColumn(
                name: "app_id",
                schema: "portal",
                table: "offer_subscriptions",
                newName: "offer_id");

            migrationBuilder.RenameColumn(
                name: "app_subscription_status_id",
                schema: "portal",
                table: "offer_subscriptions",
                newName: "offer_subscription_status_id");
            
            migrationBuilder.AddPrimaryKey(
                name: "pk_offer_subscriptions",
                table: "offer_subscriptions",
                column: "id",
                schema: "portal"); 
            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "offer_subscriptions",
                schema: "portal",
                defaultValueSql: null,
                oldDefaultValue: "gen_random_uuid()"); 

            migrationBuilder.RenameTable(
                name: "app_licenses",
                newName: "offer_licenses",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "app_subscription_statuses",
                newName: "offer_subscription_statuses",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "apps",
                newName: "offers",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "app_statuses",
                newName: "offer_statuses",
                schema: "portal");

            migrationBuilder.RenameColumn(
                name: "app_id",
                schema: "portal",
                table: "user_roles",
                newName: "offer_id");
            
            migrationBuilder.RenameIndex(
                name: "ix_user_roles_app_id",
                schema: "portal",
                table: "user_roles",
                newName: "ix_user_roles_offer_id");

            migrationBuilder.RenameColumn(
                name: "app_id",
                schema: "portal",
                table: "agreements",
                newName: "offer_id");

            migrationBuilder.RenameIndex(
                name: "ix_agreements_app_id",
                schema: "portal",
                table: "agreements",
                newName: "ix_agreements_offer_id");

            migrationBuilder.AddColumn<string>(
                name: "display_name",
                table: "offer_subscriptions",
                schema: "portal",
                type: "character varying(255)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "offer_subscriptions",
                schema: "portal",
                type: "character varying(4096)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "offer_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_types", x => x.id);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "offer_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "APP" },
                    { 2, "CORE_COMPONENT" },
                    { 3, "SERVICE" }
                });

            migrationBuilder.AddColumn<int>(
                name: "offer_type_id",
                table: "offers",
                schema: "portal",
                type: "integer",
                nullable: false,
                defaultValue: 1
            );

            migrationBuilder.DropColumn(
                name: "is_core_component",
                table: "offers",
                schema: "portal"
            );

            migrationBuilder.RenameColumn(
                name: "app_status_id",
                table: "offers",
                newName: "offer_status_id",
                schema: "portal");
            
            migrationBuilder.CreateTable(
                name: "app_subscription_details",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_instance_id = table.Column<Guid>(type: "uuid", nullable: true),
                    app_subscription_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_subscription_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_app_subscription_details_app_instances_app_instance_id",
                        column: x => x.app_instance_id,
                        principalSchema: "portal",
                        principalTable: "app_instances",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_app_subscription_details_offer_subscriptions_offer_subscrip",
                        column: x => x.offer_subscription_id,
                        principalSchema: "portal",
                        principalTable: "offer_subscriptions",
                        principalColumn: "id");
                });

            migrationBuilder.Sql(
                "INSERT INTO portal.app_subscription_details (id, offer_subscription_id, app_instance_id, app_subscription_url) SELECT gen_random_uuid(), os.id, os.app_instance_id, os.app_url FROM portal.offer_subscriptions os");

            migrationBuilder.DropColumn(
                name: "app_instance_id",
                table: "offer_subscriptions",
                schema: "portal");
            
            migrationBuilder.DropColumn(
                name: "app_url",
                table: "offer_subscriptions",
                schema: "portal");
                
            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_roles",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 3, "SERVICE_PROVIDER" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_role_descriptions",
                columns: new[] { "company_role_id", "language_short_name", "description" },
                values: new object[,]
                {
                    { 3, "de", "Dienstanbieter" },
                    { 3, "en", "Service Provider" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_app_subscription_details_app_instance_id",
                schema: "portal",
                table: "app_subscription_details",
                column: "app_instance_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_subscription_details_offer_subscription_id",
                schema: "portal",
                table: "app_subscription_details",
                column: "offer_subscription_id",
                unique: true);
            
            migrationBuilder.CreateIndex(
                name: "ix_offer_detail_images_offer_id",
                schema: "portal",
                table: "offer_detail_images",
                column: "offer_id");
            
            migrationBuilder.CreateIndex(
                name: "ix_offer_subscriptions_company_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "company_id");
            
            migrationBuilder.CreateIndex(
                name: "ix_offer_subscriptions_offer_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "offer_id");
            
            migrationBuilder.CreateIndex(
                name: "ix_offer_subscriptions_offer_subscription_status_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "offer_subscription_status_id");
            
            migrationBuilder.CreateIndex(
                name: "ix_offers_offer_status_id",
                schema: "portal",
                table: "offers",
                column: "offer_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_offers_offer_type_id",
                schema: "portal",
                table: "offers",
                column: "offer_type_id");
            
            migrationBuilder.CreateIndex(
                name: "ix_offers_provider_company_id",
                schema: "portal",
                table: "offers",
                column: "provider_company_id");
            
            migrationBuilder.CreateIndex(
                name: "ix_offers_sales_manager_id",
                schema: "portal",
                table: "offers",
                column: "sales_manager_id");

            migrationBuilder.AddForeignKey(
                name: "fk_agreements_offers_offer_id",
                schema: "portal",
                table: "agreements",
                column: "offer_id",
                principalSchema: "portal",
                principalTable: "offers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_app_assigned_use_cases_offers_app_id",
                schema: "portal",
                table: "app_assigned_use_cases",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "offers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_app_instances_offers_app_id",
                schema: "portal",
                table: "app_instances",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "offers",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_app_languages_languages_language_temp_id",
                schema: "portal",
                table: "app_languages",
                column: "language_short_name",
                principalSchema: "portal",
                principalTable: "languages",
                principalColumn: "short_name");

            migrationBuilder.AddForeignKey(
                name: "fk_app_languages_offers_app_id",
                schema: "portal",
                table: "app_languages",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "offers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_role_descriptions_languages_language_temp_id1",
                schema: "portal",
                table: "company_role_descriptions",
                column: "language_short_name",
                principalSchema: "portal",
                principalTable: "languages",
                principalColumn: "short_name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_company_user_assigned_app_favourites_offers_app_id",
                schema: "portal",
                table: "company_user_assigned_app_favourites",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "offers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_offers_offer_id",
                schema: "portal",
                table: "user_roles",
                column: "offer_id",
                principalSchema: "portal",
                principalTable: "offers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("DROP TRIGGER IF EXISTS audit_company_assigned_apps ON portal.offer_subscriptions;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS portal.process_services_audit();");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS portal.process_company_assigned_apps_audit();");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_offer_detail_images_offer_id",
                schema: "portal",
                table: "offer_detail_images");

            migrationBuilder.DropIndex(
                name: "ix_offer_subscriptions_company_id",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.DropIndex(
                name: "ix_offer_subscriptions_offer_id",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.DropIndex(
                name: "ix_offer_subscriptions_offer_subscription_status_id",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.DropIndex(
                name: "ix_offers_offer_status_id",
                schema: "portal",
                table: "offers");

            migrationBuilder.DropIndex(
                name: "ix_offers_provider_company_id",
                schema: "portal",
                table: "offers");

            migrationBuilder.DropIndex(
                name: "ix_offers_sales_manager_id",
                schema: "portal",
                table: "offers");

            migrationBuilder.DropForeignKey(
                name: "fk_agreements_offers_offer_id",
                schema: "portal",
                table: "agreements");

            migrationBuilder.DropForeignKey(
                name: "fk_app_assigned_use_cases_offers_app_id",
                schema: "portal",
                table: "app_assigned_use_cases");

            migrationBuilder.DropForeignKey(
                name: "fk_app_instances_offers_app_id",
                schema: "portal",
                table: "app_instances");

            migrationBuilder.DropForeignKey(
                name: "fk_app_languages_languages_language_temp_id",
                schema: "portal",
                table: "app_languages");

            migrationBuilder.DropForeignKey(
                name: "fk_app_languages_offers_app_id",
                schema: "portal",
                table: "app_languages");

            migrationBuilder.DropForeignKey(
                name: "fk_company_role_descriptions_languages_language_temp_id1",
                schema: "portal",
                table: "company_role_descriptions");

            migrationBuilder.DropForeignKey(
                name: "fk_company_user_assigned_app_favourites_offers_app_id",
                schema: "portal",
                table: "company_user_assigned_app_favourites");

            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_offers_offer_id",
                schema: "portal",
                table: "user_roles");

            migrationBuilder.DropColumn(
                name: "display_name",
                table: "offer_subscriptions",
                schema: "portal");

            migrationBuilder.DropColumn(
                name: "description",
                table: "offer_subscriptions",
                schema: "portal");

            migrationBuilder.RenameColumn(
                name: "offer_id",
                schema: "portal",
                table: "user_roles",
                newName: "app_id");

            migrationBuilder.RenameIndex(
                name: "ix_user_roles_offer_id",
                schema: "portal",
                table: "user_roles",
                newName: "ix_user_roles_app_id");

            migrationBuilder.RenameColumn(
                name: "offer_id",
                schema: "portal",
                table: "agreements",
                newName: "app_id");

            migrationBuilder.RenameIndex(
                name: "ix_agreements_offer_id",
                schema: "portal",
                table: "agreements",
                newName: "ix_agreements_app_id");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "company_role_descriptions",
                keyColumns: new[] { "company_role_id", "language_short_name" },
                keyValues: new object[] { 3, "de" });

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "company_role_descriptions",
                keyColumns: new[] { "company_role_id", "language_short_name" },
                keyValues: new object[] { 3, "en" });

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "company_roles",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.CreateTable(
                name: "audit_company_assigned_apps_cplp_1254_db_audit",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_instance_id = table.Column<Guid>(type: "uuid", nullable: true),
                    app_subscription_status_id = table.Column<int>(type: "integer", nullable: false),
                    app_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    audit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_operation_id = table.Column<int>(type: "integer", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_assigned_apps_cplp_1254_db_audit", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_services_cplp_1213_add_services",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_operation_id = table.Column<int>(type: "integer", nullable: false),
                    contact_email = table.Column<string>(type: "text", nullable: true),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    provider_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sales_manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    service_status_id = table.Column<int>(type: "integer", nullable: false),
                    thumbnail_url = table.Column<string>(type: "text", nullable: true)
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
                    provider_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sales_manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    service_status_id = table.Column<int>(type: "integer", nullable: false),
                    contact_email = table.Column<string>(type: "text", nullable: true),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    thumbnail_url = table.Column<string>(type: "text", nullable: true)
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
                    requester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_subscription_status_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
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
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
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

            migrationBuilder.RenameTable(
                name: "offer_assigned_documents",
                newName: "app_assigned_documents",
                schema: "portal");
            
            migrationBuilder.RenameColumn(
                name: "offer_id",
                schema: "portal",
                table: "app_assigned_documents",
                newName: "app_id");
            
            migrationBuilder.RenameTable(
                name: "offer_assigned_licenses",
                newName: "app_assigned_licenses",
                schema: "portal");

            migrationBuilder.RenameColumn(
                name: "offer_license_id",
                schema: "portal",
                table: "app_assigned_licenses",
                newName: "app_license_id");

            migrationBuilder.RenameColumn(
                name: "offer_id",
                schema: "portal",
                table: "app_assigned_licenses",
                newName: "app_id");

            migrationBuilder.RenameTable(
                name: "offer_descriptions",
                newName: "app_descriptions",
                schema: "portal");

            migrationBuilder.RenameColumn(
                name: "offer_id",
                schema: "portal",
                table: "app_descriptions",
                newName: "app_id");

            migrationBuilder.RenameTable(
                name: "offer_detail_images",
                newName: "app_detail_images",
                schema: "portal");

            migrationBuilder.RenameColumn(
                name: "offer_id",
                schema: "portal",
                table: "app_detail_images",
                newName: "app_id");

            migrationBuilder.RenameTable(
                name: "offer_tags",
                newName: "app_tags",
                schema: "portal");

            migrationBuilder.RenameColumn(
                name: "offer_id",
                schema: "portal",
                table: "app_tags",
                newName: "app_id");

            migrationBuilder.RenameTable(
                name: "offer_subscriptions",
                newName: "company_assigned_apps",
                schema: "portal");

            migrationBuilder.RenameColumn(
                name: "offer_id",
                schema: "portal",
                table: "company_assigned_apps",
                newName: "app_id");

            migrationBuilder.RenameColumn(
                name: "offer_subscription_status_id",
                schema: "portal",
                table: "company_assigned_apps",
                newName: "app_subscription_status_id");

            migrationBuilder.RenameTable(
                name: "offer_licenses",
                newName: "app_licenses",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "offer_subscription_statuses",
                newName: "app_subscription_statuses",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "offers",
                newName: "apps",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "offer_statuses",
                newName: "app_statuses",
                schema: "portal");

            migrationBuilder.DropColumn(
                name: "offer_type_id",
                table: "apps",
                schema: "portal"
            );
                
            migrationBuilder.AddColumn<bool>(
                name: "is_core_component",
                table: "apps",
                schema: "portal",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.DropTable(
                name: "offer_types",
                schema: "portal");

            migrationBuilder.RenameColumn(
                name: "offer_status_id",
                table: "apps",
                newName: "app_status_id",
                schema: "portal");
                
            migrationBuilder.AddColumn<Guid>(
                name: "app_instance_id",
                table: "company_assigned_apps",
                schema: "portal",
                type: "uuid", 
                nullable: true
            );
            
            migrationBuilder.AddColumn<string>(
                name: "app_url",
                table: "company_assigned_apps",
                schema: "portal",
                type: "character varying(255)", 
                maxLength: 255,
                nullable: true);

            migrationBuilder.Sql("UPDATE portal.company_assigned_apps as ca SET app_instance_id = app.app_instance_id, app_url = app.app_subscription_url FROM portal.app_subscription_details app WHERE ca.id = app.offer_subscription_id;");

            migrationBuilder.DropTable(
                name: "app_subscription_details",
                schema: "portal");

            migrationBuilder.CreateIndex(
                name: "ix_company_assigned_apps_app_instance_id",
                schema: "portal",
                table: "company_assigned_apps",
                column: "app_instance_id");

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

            migrationBuilder.AddForeignKey(
                name: "fk_agreements_apps_app_id",
                schema: "portal",
                table: "agreements",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "apps",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_app_assigned_use_cases_apps_app_id",
                schema: "portal",
                table: "app_assigned_use_cases",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "apps",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_app_instances_apps_app_id",
                schema: "portal",
                table: "app_instances",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "apps",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_app_languages_apps_app_id",
                schema: "portal",
                table: "app_languages",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "apps",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_app_languages_languages_language_temp_id1",
                schema: "portal",
                table: "app_languages",
                column: "language_short_name",
                principalSchema: "portal",
                principalTable: "languages",
                principalColumn: "short_name");

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
                name: "fk_company_user_assigned_app_favourites_apps_app_id",
                schema: "portal",
                table: "company_user_assigned_app_favourites",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "apps",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_apps_app_id",
                schema: "portal",
                table: "user_roles",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "apps",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
            
            migrationBuilder.DropPrimaryKey(
                name: "pk_offer_subscriptions",
                table: "company_assigned_apps",
                schema: "portal"); 

            migrationBuilder.AddPrimaryKey(
                name: "pk_company_assigned_apps",
                table: "company_assigned_apps",
                columns: new [] { "company_id", "app_id" },
                schema: "portal"); 
        }
    }
}
