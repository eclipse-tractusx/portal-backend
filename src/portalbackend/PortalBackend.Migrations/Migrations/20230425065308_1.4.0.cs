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

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class _140 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_OFFER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_OFFER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_OFFER() CASCADE;");

            migrationBuilder.DropColumn(
                name: "technical_user_needed",
                schema: "portal",
                table: "service_details");

            migrationBuilder.AddColumn<int>(
                name: "license_type_id",
                schema: "portal",
                table: "offers",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "done",
                schema: "portal",
                table: "notifications",
                type: "boolean",
                nullable: true);

            migrationBuilder.Sql("UPDATE portal.notification_type_assigned_topics SET notification_topic_id = 2 WHERE notification_type_id = 11 AND notification_topic_id = 3;");
            migrationBuilder.Sql("UPDATE portal.notification_type_assigned_topics SET notification_topic_id = 2 WHERE notification_type_id = 17 AND notification_topic_id = 3;");
            migrationBuilder.Sql("UPDATE portal.notifications SET DONE = false WHERE notification_type_id = 8 OR notification_type_id = 11 OR notification_type_id = 13 OR notification_type_id = 17");

            migrationBuilder.AddColumn<Guid>(
                name: "last_editor_id",
                schema: "portal",
                table: "consents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "date_last_changed",
                schema: "portal",
                table: "connectors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "last_editor_id",
                schema: "portal",
                table: "connectors",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "app_instance_assigned_service_accounts",
                schema: "portal",
                columns: table => new
                {
                    app_instance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_service_account_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_instance_assigned_service_accounts", x => new { x.app_instance_id, x.company_service_account_id });
                    table.ForeignKey(
                        name: "fk_app_instance_assigned_service_accounts_app_instances_app_in",
                        column: x => x.app_instance_id,
                        principalSchema: "portal",
                        principalTable: "app_instances",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_app_instance_assigned_service_accounts_company_service_acco",
                        column: x => x.company_service_account_id,
                        principalSchema: "portal",
                        principalTable: "company_service_accounts",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "app_instance_setups",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_single_instance = table.Column<bool>(type: "boolean", nullable: false),
                    instance_url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_instance_setups", x => x.id);
                    table.ForeignKey(
                        name: "fk_app_instance_setups_offers_app_id",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "audit_connector20230405",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    connector_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    type_id = table.Column<int>(type: "integer", nullable: false),
                    status_id = table.Column<int>(type: "integer", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    host_id = table.Column<Guid>(type: "uuid", nullable: true),
                    self_description_document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    location_id = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    daps_registration_successful = table.Column<bool>(type: "boolean", nullable: true),
                    self_description_message = table.Column<string>(type: "text", nullable: true),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_connector20230405", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "audit_consent20230412",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    comment = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    consent_status_id = table.Column<int>(type: "integer", nullable: false),
                    target = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_consent20230412", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "audit_offer20230406",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_released = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    marketing_url = table.Column<string>(type: "text", nullable: true),
                    contact_email = table.Column<string>(type: "text", nullable: true),
                    contact_number = table.Column<string>(type: "text", nullable: true),
                    provider = table.Column<string>(type: "text", nullable: false),
                    offer_type_id = table.Column<int>(type: "integer", nullable: false),
                    sales_manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    provider_company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    offer_status_id = table.Column<int>(type: "integer", nullable: false),
                    license_type_id = table.Column<int>(type: "integer", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_offer20230406", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "connector_client_details",
                schema: "portal",
                columns: table => new
                {
                    connector_id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_connector_client_details", x => x.connector_id);
                    table.ForeignKey(
                        name: "fk_connector_client_details_connectors_connector_id",
                        column: x => x.connector_id,
                        principalSchema: "portal",
                        principalTable: "connectors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "license_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_license_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "technical_user_profiles",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_technical_user_profiles", x => x.id);
                    table.ForeignKey(
                        name: "fk_technical_user_profiles_offers_offer_id",
                        column: x => x.offer_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "technical_user_profile_assigned_user_roles",
                schema: "portal",
                columns: table => new
                {
                    technical_user_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_technical_user_profile_assigned_user_roles", x => new { x.technical_user_profile_id, x.user_role_id });
                    table.ForeignKey(
                        name: "fk_technical_user_profile_assigned_user_roles_technical_user_p",
                        column: x => x.technical_user_profile_id,
                        principalSchema: "portal",
                        principalTable: "technical_user_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_technical_user_profile_assigned_user_roles_user_roles_user_r",
                        column: x => x.user_role_id,
                        principalSchema: "portal",
                        principalTable: "user_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "connector_statuses",
                columns: new[] { "id", "label" },
                values: new object[] { 3, "INACTIVE" });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "license_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "COTS" },
                    { 2, "FOSS" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_offers_license_type_id",
                schema: "portal",
                table: "offers",
                column: "license_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_connectors_last_editor_id",
                schema: "portal",
                table: "connectors",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_instance_assigned_service_accounts_company_service_acco",
                schema: "portal",
                table: "app_instance_assigned_service_accounts",
                column: "company_service_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_instance_setups_app_id",
                schema: "portal",
                table: "app_instance_setups",
                column: "app_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_technical_user_profile_assigned_user_roles_user_role_id",
                schema: "portal",
                table: "technical_user_profile_assigned_user_roles",
                column: "user_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_technical_user_profiles_offer_id",
                schema: "portal",
                table: "technical_user_profiles",
                column: "offer_id");

            migrationBuilder.AddForeignKey(
                name: "fk_connectors_company_users_last_editor_id",
                schema: "portal",
                table: "connectors",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_offers_license_types_license_type_id",
                schema: "portal",
                table: "offers",
                column: "license_type_id",
                principalSchema: "portal",
                principalTable: "license_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230405 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"daps_registration_successful\", \"self_description_message\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.name, \r\n  OLD.connector_url, \r\n  OLD.type_id, \r\n  OLD.status_id, \r\n  OLD.provider_id, \r\n  OLD.host_id, \r\n  OLD.self_description_document_id, \r\n  OLD.location_id, \r\n  OLD.daps_registration_successful, \r\n  OLD.self_description_message, \r\n  OLD.date_last_changed, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_CONNECTOR AFTER DELETE\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230405 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"daps_registration_successful\", \"self_description_message\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.connector_url, \r\n  NEW.type_id, \r\n  NEW.status_id, \r\n  NEW.provider_id, \r\n  NEW.host_id, \r\n  NEW.self_description_document_id, \r\n  NEW.location_id, \r\n  NEW.daps_registration_successful, \r\n  NEW.self_description_message, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_CONNECTOR AFTER INSERT\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230405 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"daps_registration_successful\", \"self_description_message\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.connector_url, \r\n  NEW.type_id, \r\n  NEW.status_id, \r\n  NEW.provider_id, \r\n  NEW.host_id, \r\n  NEW.self_description_document_id, \r\n  NEW.location_id, \r\n  NEW.daps_registration_successful, \r\n  NEW.self_description_message, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_CONNECTOR AFTER UPDATE\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_CONSENT() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_CONSENT$\r\nBEGIN\r\n  INSERT INTO portal.audit_consent20230412 (\"id\", \"date_created\", \"comment\", \"consent_status_id\", \"target\", \"agreement_id\", \"company_id\", \"document_id\", \"company_user_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.date_created, \r\n  OLD.comment, \r\n  OLD.consent_status_id, \r\n  OLD.target, \r\n  OLD.agreement_id, \r\n  OLD.company_id, \r\n  OLD.document_id, \r\n  OLD.company_user_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_CONSENT$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_CONSENT AFTER DELETE\r\nON portal.consents\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_CONSENT();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_CONSENT() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_CONSENT$\r\nBEGIN\r\n  INSERT INTO portal.audit_consent20230412 (\"id\", \"date_created\", \"comment\", \"consent_status_id\", \"target\", \"agreement_id\", \"company_id\", \"document_id\", \"company_user_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.comment, \r\n  NEW.consent_status_id, \r\n  NEW.target, \r\n  NEW.agreement_id, \r\n  NEW.company_id, \r\n  NEW.document_id, \r\n  NEW.company_user_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_CONSENT$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_CONSENT AFTER INSERT\r\nON portal.consents\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_CONSENT();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_CONSENT() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_CONSENT$\r\nBEGIN\r\n  INSERT INTO portal.audit_consent20230412 (\"id\", \"date_created\", \"comment\", \"consent_status_id\", \"target\", \"agreement_id\", \"company_id\", \"document_id\", \"company_user_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.comment, \r\n  NEW.consent_status_id, \r\n  NEW.target, \r\n  NEW.agreement_id, \r\n  NEW.company_id, \r\n  NEW.document_id, \r\n  NEW.company_user_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_CONSENT$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_CONSENT AFTER UPDATE\r\nON portal.consents\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_CONSENT();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_OFFER() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_OFFER$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer20230406 (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"provider\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"license_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.name, \r\n  OLD.date_created, \r\n  OLD.date_released, \r\n  OLD.marketing_url, \r\n  OLD.contact_email, \r\n  OLD.contact_number, \r\n  OLD.provider, \r\n  OLD.offer_type_id, \r\n  OLD.sales_manager_id, \r\n  OLD.provider_company_id, \r\n  OLD.offer_status_id, \r\n  OLD.license_type_id, \r\n  OLD.date_last_changed, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_OFFER AFTER DELETE\r\nON portal.offers\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_OFFER();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_OFFER() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_OFFER$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer20230406 (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"provider\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"license_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.date_created, \r\n  NEW.date_released, \r\n  NEW.marketing_url, \r\n  NEW.contact_email, \r\n  NEW.contact_number, \r\n  NEW.provider, \r\n  NEW.offer_type_id, \r\n  NEW.sales_manager_id, \r\n  NEW.provider_company_id, \r\n  NEW.offer_status_id, \r\n  NEW.license_type_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_OFFER AFTER INSERT\r\nON portal.offers\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_OFFER();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_OFFER() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_OFFER$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer20230406 (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"provider\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"license_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.date_created, \r\n  NEW.date_released, \r\n  NEW.marketing_url, \r\n  NEW.contact_email, \r\n  NEW.contact_number, \r\n  NEW.provider, \r\n  NEW.offer_type_id, \r\n  NEW.sales_manager_id, \r\n  NEW.provider_company_id, \r\n  NEW.offer_status_id, \r\n  NEW.license_type_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_OFFER AFTER UPDATE\r\nON portal.offers\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_OFFER();");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_CONSENT() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_CONSENT() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_CONSENT() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_OFFER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_OFFER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_OFFER() CASCADE;");

            migrationBuilder.DropForeignKey(
                name: "fk_connectors_company_users_last_editor_id",
                schema: "portal",
                table: "connectors");

            migrationBuilder.DropForeignKey(
                name: "fk_offers_license_types_license_type_id",
                schema: "portal",
                table: "offers");

            migrationBuilder.DropTable(
                name: "app_instance_assigned_service_accounts",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_instance_setups",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_connector20230405",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_consent20230412",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_offer20230406",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "connector_client_details",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "license_types",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "technical_user_profile_assigned_user_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "technical_user_profiles",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_offers_license_type_id",
                schema: "portal",
                table: "offers");

            migrationBuilder.DropIndex(
                name: "ix_connectors_last_editor_id",
                schema: "portal",
                table: "connectors");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "connector_statuses",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "license_type_id",
                schema: "portal",
                table: "offers");

            migrationBuilder.DropColumn(
                name: "done",
                schema: "portal",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "last_editor_id",
                schema: "portal",
                table: "consents");

            migrationBuilder.DropColumn(
                name: "date_last_changed",
                schema: "portal",
                table: "connectors");

            migrationBuilder.DropColumn(
                name: "last_editor_id",
                schema: "portal",
                table: "connectors");

            migrationBuilder.AddColumn<bool>(
                name: "technical_user_needed",
                schema: "portal",
                table: "service_details",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_OFFER() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_OFFER$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer20230119 (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"provider\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.name, \r\n  OLD.date_created, \r\n  OLD.date_released, \r\n  OLD.marketing_url, \r\n  OLD.contact_email, \r\n  OLD.contact_number, \r\n  OLD.provider, \r\n  OLD.offer_type_id, \r\n  OLD.sales_manager_id, \r\n  OLD.provider_company_id, \r\n  OLD.offer_status_id, \r\n  OLD.date_last_changed, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_OFFER AFTER DELETE\r\nON portal.offers\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_OFFER();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_OFFER() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_OFFER$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer20230119 (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"provider\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.date_created, \r\n  NEW.date_released, \r\n  NEW.marketing_url, \r\n  NEW.contact_email, \r\n  NEW.contact_number, \r\n  NEW.provider, \r\n  NEW.offer_type_id, \r\n  NEW.sales_manager_id, \r\n  NEW.provider_company_id, \r\n  NEW.offer_status_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_OFFER AFTER INSERT\r\nON portal.offers\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_OFFER();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_OFFER() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_OFFER$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer20230119 (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"provider\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.date_created, \r\n  NEW.date_released, \r\n  NEW.marketing_url, \r\n  NEW.contact_email, \r\n  NEW.contact_number, \r\n  NEW.provider, \r\n  NEW.offer_type_id, \r\n  NEW.sales_manager_id, \r\n  NEW.provider_company_id, \r\n  NEW.offer_status_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_OFFER AFTER UPDATE\r\nON portal.offers\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_OFFER();");
        }
    }
}
