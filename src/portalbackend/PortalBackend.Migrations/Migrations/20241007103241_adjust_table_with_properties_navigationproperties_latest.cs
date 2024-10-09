/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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
    public partial class adjust_table_with_properties_navigationproperties_latest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"() CASCADE;");

            // drop foreign keys AppInstanceAssignedCompanyServiceAccount

            migrationBuilder.DropForeignKey(
                name: "fk_app_instance_assigned_service_accounts_app_instances_app_in",
                schema: "portal",
                table: "app_instance_assigned_service_accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_app_instance_assigned_service_accounts_company_service_acco",
                schema: "portal",
                table: "app_instance_assigned_service_accounts");

            // drop foreign keys CompanyServiceAccount

            migrationBuilder.DropForeignKey(
                name: "fk_company_service_accounts_company_service_account_kindes_com",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_company_service_accounts_company_service_account_types_comp",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_company_service_accounts_identities_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_company_service_accounts_offer_subscriptions_offer_subscrip",
                schema: "portal",
                table: "company_service_accounts");

            // drop foreign keys Connector

            migrationBuilder.DropForeignKey(
                name: "fk_connectors_company_service_accounts_company_service_account",
                schema: "portal",
                table: "connectors");

            // drop foreign keys DimCompanyServiceAccount

            migrationBuilder.DropForeignKey(
                name: "fk_dim_company_service_accounts_company_service_accounts_id",
                schema: "portal",
                table: "dim_company_service_accounts");

            // drop foreign keys DimUserCreationData

            migrationBuilder.DropForeignKey(
                name: "fk_dim_user_creation_data_processes_process_id",
                schema: "portal",
                table: "dim_user_creation_data");

            migrationBuilder.DropForeignKey(
                name: "fk_dim_user_creation_data_company_service_accounts_service_acc",
                schema: "portal",
                table: "dim_user_creation_data");

            // as company_linked_technical_users is a view the autocreated constraint ain't work
            // the respective navigational property nevertheless works without.
            // the autocreated statement is left here for documentation purpose.

            // migrationBuilder.DropForeignKey(
            //     name: "fk_company_linked_service_accounts_company_service_accounts_co",
            //     schema: "portal",
            //     table: "company_linked_service_accounts");

            // AppInstanceAssignedCompanyServiceAccount -> AppInstanceAssignedTechnicalUser

            migrationBuilder.DropPrimaryKey(
                name: "pk_app_instance_assigned_service_accounts",
                table: "app_instance_assigned_service_accounts",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_app_instance_assigned_service_accounts_company_service_acco",
                table: "app_instance_assigned_service_accounts",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "app_instance_assigned_service_accounts",
                schema: "portal",
                newName: "app_instance_assigned_technical_users");

            migrationBuilder.RenameColumn(
                name: "company_service_account_id",
                table: "app_instance_assigned_technical_users",
                schema: "portal",
                newName: "technical_user_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_app_instance_assigned_technical_users",
                table: "app_instance_assigned_technical_users",
                schema: "portal",
                columns: ["app_instance_id", "technical_user_id"]);

            migrationBuilder.CreateIndex(
                name: "ix_app_instance_assigned_technical_users_technical_user_id",
                table: "app_instance_assigned_technical_users",
                schema: "portal",
                column: "technical_user_id");

            // CompanyServiceAccount -> TechnicalUser

            migrationBuilder.DropIndex(
                name: "ix_company_service_accounts_client_client_id",
                table: "company_service_accounts",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_company_service_accounts_company_service_account_kind_id",
                table: "company_service_accounts",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_company_service_accounts_company_service_account_type_id",
                table: "company_service_accounts",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "company_service_accounts",
                schema: "portal",
                newName: "technical_users");

            migrationBuilder.RenameColumn(
                name: "company_service_account_kind_id",
                table: "technical_users",
                schema: "portal",
                newName: "technical_user_kind_id");

            migrationBuilder.RenameColumn(
                name: "company_service_account_type_id",
                table: "technical_users",
                schema: "portal",
                newName: "technical_user_type_id");

            migrationBuilder.RenameIndex(
                name: "pk_company_service_accounts",
                table: "technical_users",
                schema: "portal",
                newName: "pk_technical_users");

            migrationBuilder.CreateIndex(
                name: "ix_technical_users_client_client_id",
                table: "technical_users",
                schema: "portal",
                column: "client_client_id",
                filter: "client_client_id is not null AND technical_user_kind_id = 1");

            migrationBuilder.RenameIndex(
                name: "ix_company_service_accounts_offer_subscription_id",
                table: "technical_users",
                schema: "portal",
                newName: "ix_technical_users_offer_subscription_id");

            migrationBuilder.CreateIndex(
                name: "ix_technical_users_technical_user_kind_id",
                table: "technical_users",
                schema: "portal",
                column: "technical_user_kind_id");

            migrationBuilder.CreateIndex(
                name: "ix_technical_users_technical_user_type_id",
                table: "technical_users",
                schema: "portal",
                column: "technical_user_type_id");

            // CompanyServiceAccountKind -> TechnicalUserKind

            migrationBuilder.RenameTable(
                name: "company_service_account_kindes",
                schema: "portal",
                newName: "technical_user_kinds");

            migrationBuilder.RenameIndex(
                name: "pk_company_service_account_kindes",
                table: "technical_user_kinds",
                schema: "portal",
                newName: "pk_technical_user_kinds");

            // CompanyServiceAccountType -> TechnicalUserType

            migrationBuilder.RenameTable(
                name: "company_service_account_types",
                schema: "portal",
                newName: "technical_user_types");

            migrationBuilder.RenameIndex(
                name: "pk_company_service_account_types",
                table: "technical_user_types",
                schema: "portal",
                newName: "pk_technical_user_types");

            // Connector

            migrationBuilder.DropIndex(
                name: "ix_connectors_company_service_account_id",
                table: "connectors",
                schema: "portal");

            migrationBuilder.RenameColumn(
                name: "company_service_account_id",
                schema: "portal",
                table: "connectors",
                newName: "technical_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_connectors_technical_user_id",
                table: "connectors",
                schema: "portal",
                column: "technical_user_id");

            // DimCompanyServiceAccount -> ExternalTechnicalUser

            migrationBuilder.RenameTable(
                name: "dim_company_service_accounts",
                schema: "portal",
                newName: "external_technical_users");

            migrationBuilder.RenameIndex(
                name: "pk_dim_company_service_accounts",
                table: "external_technical_users",
                schema: "portal",
                newName: "pk_external_technical_users");

            // DimUserCreationData -> ExternalTechnicalUserCreationData

            migrationBuilder.DropIndex(
                name: "ix_dim_user_creation_data_service_account_id",
                table: "dim_user_creation_data",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "dim_user_creation_data",
                schema: "portal",
                newName: "external_technical_user_creation_data");

            migrationBuilder.RenameColumn(
                name: "service_account_id",
                table: "external_technical_user_creation_data",
                schema: "portal",
                newName: "technical_user_id");

            migrationBuilder.RenameIndex(
                name: "pk_dim_user_creation_data",
                table: "external_technical_user_creation_data",
                schema: "portal",
                newName: "pk_external_technical_user_creation_data");

            migrationBuilder.RenameIndex(
                name: "ix_dim_user_creation_data_process_id",
                table: "external_technical_user_creation_data",
                schema: "portal",
                newName: "ix_external_technical_user_creation_data_process_id");

            migrationBuilder.CreateIndex(
                name: "ix_external_technical_user_creation_data_technical_user_id",
                table: "external_technical_user_creation_data",
                schema: "portal",
                column: "technical_user_id");

            // CompaniesLinkedServiceAccount -> CompaniesLinkedTechnicalUser

            migrationBuilder.Sql(@"DROP VIEW IF EXISTS portal.company_linked_service_accounts");

            migrationBuilder.Sql(@"CREATE OR REPLACE VIEW portal.company_linked_technical_users AS
                SELECT
                    tu.id AS technical_user_id,
                    i.company_id AS owners,
                    CASE
                        WHEN tu.offer_subscription_id IS NOT NULL THEN o.provider_company_id
                        WHEN EXISTS (SELECT 1 FROM portal.connectors cs WHERE cs.technical_user_id = tu.id) THEN c.host_id
                        END AS provider
                FROM portal.technical_users tu
                    JOIN portal.identities i ON tu.id = i.id
                    LEFT JOIN portal.offer_subscriptions os ON tu.offer_subscription_id = os.id
                    LEFT JOIN portal.offers o ON os.offer_id = o.id
                    LEFT JOIN portal.connectors c ON tu.id = c.technical_user_id
                WHERE tu.technical_user_type_id = 1 AND i.identity_type_id = 2
                UNION
                SELECT
                    tu.id AS technical_user_id,
                    i.company_id AS owners,
                    null AS provider
                FROM
                    portal.technical_users tu
                        JOIN portal.identities i ON tu.id = i.id
                WHERE tu.technical_user_type_id = 2
                ");

            // re-add foreign keys AppInstanceAssignedTechnicalUser (AppInstanceAssignedCompanyServiceAccount)

            migrationBuilder.AddForeignKey(
                name: "fk_app_instance_assigned_technical_users_app_instances_app_ins",
                table: "app_instance_assigned_technical_users",
                column: "app_instance_id",
                schema: "portal",
                principalSchema: "portal",
                principalTable: "app_instances",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_app_instance_assigned_technical_users_technical_users_techn",
                table: "app_instance_assigned_technical_users",
                column: "technical_user_id",
                schema: "portal",
                principalSchema: "portal",
                principalTable: "technical_users",
                principalColumn: "id");

            // re-add foreign keys TechnicalUser (CompanyServiceAccount)

            migrationBuilder.AddForeignKey(
                name: "fk_technical_users_identities_id",
                table: "technical_users",
                column: "id",
                schema: "portal",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_technical_users_offer_subscriptions_offer_subscription_id",
                table: "technical_users",
                column: "offer_subscription_id",
                schema: "portal",
                principalSchema: "portal",
                principalTable: "offer_subscriptions",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
               name: "fk_technical_users_technical_user_kinds_technical_user_kind_id",
               table: "technical_users",
               column: "technical_user_kind_id",
               schema: "portal",
               principalSchema: "portal",
               principalTable: "technical_user_kinds",
               principalColumn: "id",
               onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_technical_users_technical_user_types_technical_user_type_id",
                table: "technical_users",
                column: "technical_user_type_id",
                schema: "portal",
                principalSchema: "portal",
                principalTable: "technical_user_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            // re-add foreign keys Connector

            migrationBuilder.AddForeignKey(
                name: "fk_connectors_technical_users_technical_user_id",
                schema: "portal",
                table: "connectors",
                column: "technical_user_id",
                principalSchema: "portal",
                principalTable: "technical_users",
                principalColumn: "id");

            // re-add foreign keys ExternalTechnicalUser (DimCompanyServiceAccount)

            migrationBuilder.AddForeignKey(
               name: "fk_external_technical_users_external_technical_users_id",
               table: "external_technical_users",
               column: "id",
               schema: "portal",
               principalSchema: "portal",
               principalTable: "technical_users",
               principalColumn: "id",
               onDelete: ReferentialAction.Cascade);

            // re-add foreign keys ExternalTechnicalUserCreationData (DimUserCreationData)

            migrationBuilder.AddForeignKey(
                name: "fk_external_technical_user_creation_data_processes_process_id",
                table: "external_technical_user_creation_data",
                column: "process_id",
                schema: "portal",
                principalSchema: "portal",
                principalTable: "processes",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_external_technical_user_creation_data_technical_users_techn",
                table: "external_technical_user_creation_data",
                column: "technical_user_id",
                schema: "portal",
                principalSchema: "portal",
                principalTable: "technical_users",
                principalColumn: "id");

            // as company_linked_technical_users is a view the autocreated constraint ain't work
            // the respective navigational property nevertheless works without.
            // the autocreated statement is left here for documentation purpose.

            // migrationBuilder.AddForeignKey(
            //     name: "fk_company_linked_technical_users_technical_users_technical_us",
            //     table: "company_linked_technical_users",
            //     column: "technical_user_id",
            //     schema: "portal",
            //     principalSchema: "portal",
            //     principalTable: "technical_users",
            //     principalColumn: "id",
            //     onDelete: ReferentialAction.Cascade);

            // AuditConnector20241008

            migrationBuilder.CreateTable(
                name: "audit_connector20241008",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    connector_url = table.Column<string>(type: "text", nullable: true),
                    type_id = table.Column<int>(type: "integer", nullable: true),
                    status_id = table.Column<int>(type: "integer", nullable: true),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: true),
                    host_id = table.Column<Guid>(type: "uuid", nullable: true),
                    self_description_document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    location_id = table.Column<string>(type: "text", nullable: true),
                    self_description_message = table.Column<string>(type: "text", nullable: true),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    technical_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sd_creation_process_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_connector20241008", x => x.audit_v1id);
                });

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_connector20241008\" (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"technical_user_id\", \"sd_creation_process_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"connector_url\", \r\n  NEW.\"type_id\", \r\n  NEW.\"status_id\", \r\n  NEW.\"provider_id\", \r\n  NEW.\"host_id\", \r\n  NEW.\"self_description_document_id\", \r\n  NEW.\"location_id\", \r\n  NEW.\"self_description_message\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"technical_user_id\", \r\n  NEW.\"sd_creation_process_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_CONNECTOR AFTER INSERT\r\nON \"portal\".\"connectors\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_connector20241008\" (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"technical_user_id\", \"sd_creation_process_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"connector_url\", \r\n  NEW.\"type_id\", \r\n  NEW.\"status_id\", \r\n  NEW.\"provider_id\", \r\n  NEW.\"host_id\", \r\n  NEW.\"self_description_document_id\", \r\n  NEW.\"location_id\", \r\n  NEW.\"self_description_message\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"technical_user_id\", \r\n  NEW.\"sd_creation_process_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_CONNECTOR AFTER UPDATE\r\nON \"portal\".\"connectors\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"() CASCADE;");

            migrationBuilder.DropTable(
                name: "audit_connector20241008",
                schema: "portal");

            // drop foreign keys AppInstanceAssignedTechnicalUser (AppInstanceAssignedCompanyServiceAccount)

            migrationBuilder.DropForeignKey(
                name: "fk_app_instance_assigned_technical_users_app_instances_app_ins",
                schema: "portal",
                table: "app_instance_assigned_technical_users");

            migrationBuilder.DropForeignKey(
                name: "fk_app_instance_assigned_technical_users_technical_users_techn",
                schema: "portal",
                table: "app_instance_assigned_technical_users");

            // drop foreign keys TechnicalUsers (CompanyServiceAccount)

            migrationBuilder.DropForeignKey(
                name: "fk_technical_users_identities_id",
                schema: "portal",
                table: "technical_users");

            migrationBuilder.DropForeignKey(
                name: "fk_technical_users_offer_subscriptions_offer_subscription_id",
                schema: "portal",
                table: "technical_users");

            migrationBuilder.DropForeignKey(
                name: "fk_technical_users_technical_user_kinds_technical_user_kind_id",
                schema: "portal",
                table: "technical_users");

            migrationBuilder.DropForeignKey(
                name: "fk_technical_users_technical_user_types_technical_user_type_id",
                schema: "portal",
                table: "technical_users");

            // drop foreign keys Connector

            migrationBuilder.DropForeignKey(
                name: "fk_connectors_technical_users_technical_user_id",
                schema: "portal",
                table: "connectors");

            // drop foreign keys ExternalTechnicalUser (DimCompanyServiceAccount)

            migrationBuilder.DropForeignKey(
                name: "fk_external_technical_users_external_technical_users_id",
                schema: "portal",
                table: "external_technical_users");

            // drop foreign keys ExternalTechnicalUserCreationData (DimUserCreationData)

            migrationBuilder.DropForeignKey(
                name: "fk_external_technical_user_creation_data_processes_process_id",
                schema: "portal",
                table: "external_technical_user_creation_data");

            migrationBuilder.DropForeignKey(
                name: "fk_external_technical_user_creation_data_technical_users_techn",
                schema: "portal",
                table: "external_technical_user_creation_data");

            // as company_linked_technical_users is a view the autocreated constraint ain't work
            // the respective navigational property nevertheless works without.
            // the autocreated statement is left here for documentation purpose.

            // migrationBuilder.DropForeignKey(
            //     name: "fk_company_linked_technical_users_technical_users_technical_us",
            //     schema: "portal",
            //     table: "company_linked_technical_users");

            // AppInstanceAssignedTechnicalUser -> AppInstanceAssignedCompanyServiceAccount

            migrationBuilder.DropPrimaryKey(
                name: "pk_app_instance_assigned_technical_users",
                table: "app_instance_assigned_technical_users",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_app_instance_assigned_technical_users_technical_user_id",
                table: "app_instance_assigned_technical_users",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "app_instance_assigned_technical_users",
                schema: "portal",
                newName: "app_instance_assigned_service_accounts");

            migrationBuilder.RenameColumn(
                name: "technical_user_id",
                table: "app_instance_assigned_service_accounts",
                schema: "portal",
                newName: "company_service_account_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_app_instance_assigned_service_accounts",
                table: "app_instance_assigned_service_accounts",
                schema: "portal",
                columns: ["app_instance_id", "company_service_account_id"]);

            migrationBuilder.CreateIndex(
                name: "ix_app_instance_assigned_service_accounts_company_service_acco",
                table: "app_instance_assigned_service_accounts",
                schema: "portal",
                column: "company_service_account_id");

            // TechnicalUser -> CompanyServiceAccount

            migrationBuilder.DropIndex(
                name: "ix_technical_users_client_client_id",
                table: "technical_users",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_technical_users_technical_user_kind_id",
                table: "technical_users",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_technical_users_technical_user_type_id",
                table: "technical_users",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "technical_users",
                schema: "portal",
                newName: "company_service_accounts");

            migrationBuilder.RenameColumn(
                name: "technical_user_kind_id",
                table: "company_service_accounts",
                schema: "portal",
                newName: "company_service_account_kind_id");

            migrationBuilder.RenameColumn(
                name: "technical_user_type_id",
                table: "company_service_accounts",
                schema: "portal",
                newName: "company_service_account_type_id");

            migrationBuilder.RenameIndex(
                name: "pk_technical_users",
                table: "company_service_accounts",
                schema: "portal",
                newName: "pk_company_service_accounts");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_client_client_id",
                table: "company_service_accounts",
                schema: "portal",
                column: "client_client_id",
                filter: "client_client_id is not null AND company_service_account_kind_id = 1");

            migrationBuilder.RenameIndex(
                name: "ix_technical_users_offer_subscription_id",
                table: "company_service_accounts",
                schema: "portal",
                newName: "ix_company_service_accounts_offer_subscription_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_company_service_account_kind_id",
                table: "company_service_accounts",
                schema: "portal",
                column: "company_service_account_kind_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_company_service_account_type_id",
                table: "company_service_accounts",
                schema: "portal",
                column: "company_service_account_type_id");

            // TechnicalUserKind -> CompanyServiceAccountKind

            migrationBuilder.RenameTable(
                name: "technical_user_kinds",
                schema: "portal",
                newName: "company_service_account_kindes");

            migrationBuilder.RenameIndex(
                name: "pk_technical_user_kinds",
                table: "company_service_account_kindes",
                schema: "portal",
                newName: "pk_company_service_account_kindes");

            // TechnicalUserType -> CompanyServiceAccountType

            migrationBuilder.RenameTable(
                name: "technical_user_types",
                schema: "portal",
                newName: "company_service_account_types");

            migrationBuilder.RenameIndex(
                name: "pk_technical_user_types",
                table: "company_service_account_types",
                schema: "portal",
                newName: "pk_company_service_account_types");

            // Connector

            migrationBuilder.DropIndex(
                name: "ix_connectors_technical_user_id",
                table: "connectors",
                schema: "portal");

            migrationBuilder.RenameColumn(
                name: "technical_user_id",
                schema: "portal",
                table: "connectors",
                newName: "company_service_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_connectors_company_service_account_id",
                table: "connectors",
                schema: "portal",
                column: "company_service_account_id");

            // ExternalTechnicalUser -> DimCompanyServiceAccount

            migrationBuilder.RenameTable(
                name: "external_technical_users",
                schema: "portal",
                newName: "dim_company_service_accounts"
            );

            migrationBuilder.RenameIndex(
                name: "pk_external_technical_users",
                table: "dim_company_service_accounts",
                schema: "portal",
                newName: "pk_dim_company_service_accounts");

            // ExternalTechnicalUserCreationData -> DimUserCreationData

            migrationBuilder.DropIndex(
                name: "ix_external_technical_user_creation_data_technical_user_id",
                table: "external_technical_user_creation_data",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "external_technical_user_creation_data",
                schema: "portal",
                newName: "dim_user_creation_data");

            migrationBuilder.RenameColumn(
                name: "technical_user_id",
                table: "dim_user_creation_data",
                schema: "portal",
                newName: "service_account_id");

            migrationBuilder.RenameIndex(
                name: "pk_external_technical_user_creation_data",
                table: "dim_user_creation_data",
                schema: "portal",
                newName: "pk_dim_user_creation_data");

            migrationBuilder.RenameIndex(
                name: "ix_external_technical_user_creation_data_process_id",
                table: "dim_user_creation_data",
                schema: "portal",
                newName: "ix_dim_user_creation_data_process_id");

            migrationBuilder.CreateIndex(
                name: "ix_dim_user_creation_data_service_account_id",
                table: "dim_user_creation_data",
                schema: "portal",
                column: "service_account_id");

            // CompaniesLinkedTechnicalUser -> CompaniesLinkedServiceAccount

            migrationBuilder.Sql(@"DROP VIEW IF EXISTS portal.company_linked_technical_users");

            migrationBuilder.Sql(@"CREATE OR REPLACE VIEW portal.company_linked_service_accounts AS
                SELECT
                    csa.id AS service_account_id,
                    i.company_id AS owners,
                    CASE
                        WHEN csa.offer_subscription_id IS NOT NULL THEN o.provider_company_id
                        WHEN EXISTS (SELECT 1 FROM portal.connectors cs WHERE cs.company_service_account_id = csa.id) THEN c.host_id
                        END AS provider
                FROM portal.company_service_accounts csa
                    JOIN portal.identities i ON csa.id = i.id
                    LEFT JOIN portal.offer_subscriptions os ON csa.offer_subscription_id = os.id
                    LEFT JOIN portal.offers o ON os.offer_id = o.id
                    LEFT JOIN portal.connectors c ON csa.id = c.company_service_account_id
                WHERE csa.company_service_account_type_id = 1 AND i.identity_type_id = 2
                UNION
                SELECT
                    csa.id AS service_account_id,
                    i.company_id AS owners,
                    null AS provider
                FROM
                    portal.company_service_accounts csa
                        JOIN portal.identities i ON csa.id = i.id
                WHERE csa.company_service_account_type_id = 2
                ");

            // re-add foreign keys AppInstanceAssignedCompanyServiceAccount (AppInstanceAssignedTechnicalUser)

            migrationBuilder.AddForeignKey(
                name: "fk_app_instance_assigned_service_accounts_app_instances_app_in",
                schema: "portal",
                table: "app_instance_assigned_service_accounts",
                column: "app_instance_id",
                principalSchema: "portal",
                principalTable: "app_instances",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_app_instance_assigned_service_accounts_company_service_acco",
                schema: "portal",
                table: "app_instance_assigned_service_accounts",
                column: "company_service_account_id",
                principalSchema: "portal",
                principalTable: "company_service_accounts",
                principalColumn: "id");

            // re-add foreign keys CompanyServiceAccount (TechnicalUser)

            migrationBuilder.AddForeignKey(
                name: "fk_company_service_accounts_identities_id",
                table: "company_service_accounts",
                column: "id",
                schema: "portal",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_company_service_accounts_offer_subscriptions_offer_subscrip",
                table: "company_service_accounts",
                column: "offer_subscription_id",
                schema: "portal",
                principalSchema: "portal",
                principalTable: "offer_subscriptions",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_service_accounts_company_service_account_kindes_com",
                table: "company_service_accounts",
                column: "company_service_account_kind_id",
                schema: "portal",
                principalSchema: "portal",
                principalTable: "company_service_account_kindes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_company_service_accounts_company_service_account_types_comp",
                table: "company_service_accounts",
                column: "company_service_account_type_id",
                schema: "portal",
                principalSchema: "portal",
                principalTable: "company_service_account_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            // re-add foreign keys Connector

            migrationBuilder.AddForeignKey(
                name: "fk_connectors_company_service_accounts_company_service_account",
                table: "connectors",
                column: "company_service_account_id",
                schema: "portal",
                principalSchema: "portal",
                principalTable: "company_service_accounts",
                principalColumn: "id");

            // re-add foreign keys DimCompanyServiceAccount (ExternalTechnicalUser)

            migrationBuilder.AddForeignKey(
               name: "fk_dim_company_service_accounts_company_service_accounts_id",
               table: "dim_company_service_accounts",
               column: "id",
               schema: "portal",
               principalSchema: "portal",
               principalTable: "company_service_accounts",
               principalColumn: "id",
               onDelete: ReferentialAction.Cascade);

            // re-add foreign keys DimUserCreationData (ExternalTechnicalUserCreationData)

            migrationBuilder.AddForeignKey(
                name: "fk_dim_user_creation_data_processes_process_id",
                table: "dim_user_creation_data",
                column: "process_id",
                schema: "portal",
                principalSchema: "portal",
                principalTable: "processes",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_dim_user_creation_data_company_service_accounts_service_acc",
                table: "dim_user_creation_data",
                column: "service_account_id",
                schema: "portal",
                principalSchema: "portal",
                principalTable: "company_service_accounts",
                principalColumn: "id");

            // as company_linked_technical_users is a view the autocreated constraint ain't work
            // the respective navigational property nevertheless works without.
            // the autocreated statement is left here for documentation purpose.

            // migrationBuilder.AddForeignKey(
            //     name: "fk_company_linked_service_accounts_company_service_accounts_co",
            //     table: "company_linked_service_accounts",
            //     column: "company_service_account",
            //     schema: "portal",
            //     principalSchema: "portal",
            //     principalTable: "company_service_accounts",
            //     principalColumn: "id",
            //     onDelete: ReferentialAction.Cascade);

            // AuditConnector20240814

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_connector20240814\" (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"sd_creation_process_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"connector_url\", \r\n  NEW.\"type_id\", \r\n  NEW.\"status_id\", \r\n  NEW.\"provider_id\", \r\n  NEW.\"host_id\", \r\n  NEW.\"self_description_document_id\", \r\n  NEW.\"location_id\", \r\n  NEW.\"self_description_message\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"company_service_account_id\", \r\n  NEW.\"sd_creation_process_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_CONNECTOR AFTER INSERT\r\nON \"portal\".\"connectors\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_connector20240814\" (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"sd_creation_process_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"connector_url\", \r\n  NEW.\"type_id\", \r\n  NEW.\"status_id\", \r\n  NEW.\"provider_id\", \r\n  NEW.\"host_id\", \r\n  NEW.\"self_description_document_id\", \r\n  NEW.\"location_id\", \r\n  NEW.\"self_description_message\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"company_service_account_id\", \r\n  NEW.\"sd_creation_process_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_CONNECTOR AFTER UPDATE\r\nON \"portal\".\"connectors\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"();");
        }
    }
}
