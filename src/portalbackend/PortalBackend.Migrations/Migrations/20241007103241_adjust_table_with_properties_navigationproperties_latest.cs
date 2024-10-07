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

            migrationBuilder.DropForeignKey(
                name: "fk_app_instance_assigned_service_accounts_company_service_acco",
                schema: "portal",
                table: "app_instance_assigned_service_accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_connectors_company_service_accounts_company_service_account",
                schema: "portal",
                table: "connectors");

            migrationBuilder.DropForeignKey(
                name: "fk_dim_user_creation_data_company_service_accounts_service_acc",
                schema: "portal",
                table: "dim_user_creation_data");

            //external_technical_user table creation starts

            migrationBuilder.DropForeignKey(
                name: "fk_dim_company_service_accounts_company_service_accounts_id",
                schema: "portal",
                table: "dim_company_service_accounts");

            migrationBuilder.DropPrimaryKey(
                name: "pk_dim_company_service_accounts",
                table: "dim_company_service_accounts",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "dim_company_service_accounts",
                schema: "portal",
                newName: "external_technical_users"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_external_technical_users",
                table: "external_technical_users",
                column: "id");

            //end external_technical_user table creation    

            // technical_users creation starts            

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

            migrationBuilder.DropPrimaryKey(
                name: "pk_company_service_accounts",
                table: "company_service_accounts",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "company_service_accounts",
                schema: "portal",
                newName: "technical_users");

            migrationBuilder.RenameColumn(
                name: "company_service_account_kind_id",
                table: "technical_users",
                newName: "technical_user_kind_id");

            migrationBuilder.RenameColumn(
                name: "company_service_account_type_id",
                table: "technical_users",
                newName: "technical_user_type_id");

            migrationBuilder.RenameIndex(
                name: "ix_company_service_accounts_client_client_id",
                newName: "ix_technical_users_client_client_id",
                schema: "portal",
                table: "technical_users");

            migrationBuilder.RenameIndex(
                name: "ix_company_service_accounts_company_service_account_kind_id",
                newName: "ix_technical_users_company_service_account_kind_id",
                schema: "portal",
                table: "technical_users");

            migrationBuilder.RenameIndex(
                name: "ix_company_service_accounts_company_service_account_type_id",
                newName: "ix_technical_users_company_service_account_type_id",
                schema: "portal",
                table: "technical_users");

            migrationBuilder.RenameIndex(
                name: "ix_company_service_accounts_offer_subscription_id",
                newName: "ix_technical_users_offer_subscription_id",
                schema: "portal",
                table: "technical_users");

            migrationBuilder.AddPrimaryKey(
                name: "pk_technical_users",
                table: "technical_users",
                column: "id"
            );

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
               name: "fk_external_technical_users_external_technical_users_id",
               table: "external_technical_users",
               column: "id",
               schema: "portal",
               principalSchema: "portal",
               principalTable: "technical_users",
               principalColumn: "id",
               onDelete: ReferentialAction.Cascade);

            //end technical_users creation

            //rename company_service_account_kindes to technical_user_kinds starts

            migrationBuilder.DropPrimaryKey(
                name: "pk_company_service_account_kindes",
                table: "company_service_account_kindes",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "company_service_account_kindes",
                schema: "portal",
                newName: "technical_user_kinds");

            migrationBuilder.AddPrimaryKey(
                name: "pk_technical_user_kinds",
                table: "technical_user_kinds",
                column: "id"
            );

            migrationBuilder.AddForeignKey(
               name: "fk_technical_users_technical_user_kinds_technical_user_kind_id",
               table: "technical_users",
               column: "technical_user_kind_id",
               schema: "portal",
               principalSchema: "portal",
               principalTable: "technical_user_kinds",
               principalColumn: "id",
               onDelete: ReferentialAction.Cascade);

            //end rename company_service_account_kindes to technical_user_kinds

            //rename company_service_account_types to technical_user_types starts

            migrationBuilder.DropPrimaryKey(
                name: "pk_company_service_account_types",
                table: "company_service_account_types",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "company_service_account_types",
                schema: "portal",
                newName: "technical_user_types");

            migrationBuilder.AddPrimaryKey(
                name: "pk_technical_user_types",
                table: "technical_user_types",
                column: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_technical_users_technical_user_types_technical_user_type_id",
                table: "technical_users",
                column: "technical_user_type_id",
                schema: "portal",
                principalSchema: "portal",
                principalTable: "technical_user_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            //end rename company_service_account_types to technical_user_types starts            

            migrationBuilder.RenameColumn(
                name: "service_account_id",
                schema: "portal",
                table: "dim_user_creation_data",
                newName: "technical_user_id");

            migrationBuilder.RenameIndex(
                name: "ix_dim_user_creation_data_service_account_id",
                schema: "portal",
                table: "dim_user_creation_data",
                newName: "ix_dim_user_creation_data_technical_user_id");

            migrationBuilder.RenameColumn(
                name: "company_service_account_id",
                schema: "portal",
                table: "connectors",
                newName: "technical_user_id");

            migrationBuilder.RenameIndex(
                name: "ix_connectors_company_service_account_id",
                schema: "portal",
                table: "connectors",
                newName: "ix_connectors_technical_user_id");

            migrationBuilder.RenameColumn(
                name: "company_service_account_id",
                schema: "portal",
                table: "audit_connector20240814",
                newName: "technical_user_id");

            migrationBuilder.RenameColumn(
                name: "company_service_account_id",
                schema: "portal",
                table: "app_instance_assigned_service_accounts",
                newName: "technical_user_id");

            migrationBuilder.RenameIndex(
                name: "ix_app_instance_assigned_service_accounts_company_service_acco",
                schema: "portal",
                table: "app_instance_assigned_service_accounts",
                newName: "ix_app_instance_assigned_service_accounts_technical_user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_app_instance_assigned_service_accounts_technical_users_tech",
                schema: "portal",
                table: "app_instance_assigned_service_accounts",
                column: "technical_user_id",
                principalSchema: "portal",
                principalTable: "technical_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_connectors_technical_users_technical_user_id",
                schema: "portal",
                table: "connectors",
                column: "technical_user_id",
                principalSchema: "portal",
                principalTable: "technical_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_dim_user_creation_data_technical_users_technical_user_id",
                schema: "portal",
                table: "dim_user_creation_data",
                column: "technical_user_id",
                principalSchema: "portal",
                principalTable: "technical_users",
                principalColumn: "id");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_connector20240814\" (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"technical_user_id\", \"sd_creation_process_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"connector_url\", \r\n  NEW.\"type_id\", \r\n  NEW.\"status_id\", \r\n  NEW.\"provider_id\", \r\n  NEW.\"host_id\", \r\n  NEW.\"self_description_document_id\", \r\n  NEW.\"location_id\", \r\n  NEW.\"self_description_message\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"technical_user_id\", \r\n  NEW.\"sd_creation_process_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_CONNECTOR AFTER INSERT\r\nON \"portal\".\"connectors\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_connector20240814\" (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"technical_user_id\", \"sd_creation_process_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"connector_url\", \r\n  NEW.\"type_id\", \r\n  NEW.\"status_id\", \r\n  NEW.\"provider_id\", \r\n  NEW.\"host_id\", \r\n  NEW.\"self_description_document_id\", \r\n  NEW.\"location_id\", \r\n  NEW.\"self_description_message\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"technical_user_id\", \r\n  NEW.\"sd_creation_process_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_CONNECTOR AFTER UPDATE\r\nON \"portal\".\"connectors\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"() CASCADE;");

            migrationBuilder.DropForeignKey(
                name: "fk_app_instance_assigned_service_accounts_technical_users_tech",
                schema: "portal",
                table: "app_instance_assigned_service_accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_connectors_technical_users_technical_user_id",
                schema: "portal",
                table: "connectors");

            migrationBuilder.DropForeignKey(
                name: "fk_dim_user_creation_data_technical_users_technical_user_id",
                schema: "portal",
                table: "dim_user_creation_data");

            //remane external_technical_user to dim_company_service_accounts table creation starts

            migrationBuilder.DropForeignKey(
                name: "fk_external_technical_users_external_technical_users_id",
                schema: "portal",
                table: "external_technical_users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_external_technical_users",
                table: "external_technical_users",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "external_technical_users",
                schema: "portal",
                newName: "dim_company_service_accounts"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_dim_company_service_accounts",
                table: "dim_company_service_accounts",
                column: "id");

            //end remane external_technical_user to dim_company_service_accounts table creation  

            // rename technical_users to company_service_accounts creation starts         

            migrationBuilder.DropForeignKey(
                name: "fk_technical_users_technical_user_kinds_technical_user_kind_id",
                schema: "portal",
                table: "technical_users");

            migrationBuilder.DropForeignKey(
                name: "fk_technical_users_technical_user_types_technical_user_type_id",
                schema: "portal",
                table: "technical_users");

            migrationBuilder.DropForeignKey(
                name: "fk_technical_users_identities_id",
                schema: "portal",
                table: "technical_users");

            migrationBuilder.DropForeignKey(
                name: "fk_technical_users_offer_subscriptions_offer_subscription_id",
                schema: "portal",
                table: "technical_users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_technical_users",
                table: "technical_users",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "technical_users",
                schema: "portal",
                newName: "company_service_accounts");

            migrationBuilder.RenameColumn(
                name: "technical_user_kind_id",
                table: "company_service_accounts",
                newName: "company_service_account_kind_id");

            migrationBuilder.RenameColumn(
                name: "technical_user_type_id",
                table: "company_service_accounts",
                newName: "company_service_account_type_id");

            migrationBuilder.RenameIndex(
                name: "ix_technical_users_client_client_id",
                newName: "ix_company_service_accounts_client_client_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.RenameIndex(
                name: "ix_technical_users_company_service_account_kind_id",
                newName: "ix_company_service_accounts_company_service_account_kind_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.RenameIndex(
                name: "ix_technical_users_company_service_account_type_id",
                newName: "ix_company_service_accounts_company_service_account_type_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.RenameIndex(
                name: "ix_technical_users_offer_subscription_id",
                newName: "ix_company_service_accounts_offer_subscription_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.AddPrimaryKey(
                name: "pk_company_service_accounts",
                table: "company_service_accounts",
                column: "id"
            );

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
               name: "fk_dim_company_service_accounts_company_service_accounts_id",
               table: "dim_company_service_accounts",
               column: "id",
               schema: "portal",
               principalSchema: "portal",
               principalTable: "company_service_accounts",
               principalColumn: "id",
               onDelete: ReferentialAction.Cascade);

            //end rename technical_users to company_service_accounts creation

            //rename technical_user_kinds to company_service_account_kindes starts

            migrationBuilder.DropPrimaryKey(
                name: "pk_technical_user_kinds",
                table: "technical_user_kinds",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "technical_user_kinds",
                schema: "portal",
                newName: "company_service_account_kindes");

            migrationBuilder.AddPrimaryKey(
                name: "pk_company_service_account_kindes",
                table: "company_service_account_kindes",
                column: "id"
            );

            migrationBuilder.AddForeignKey(
               name: "fk_company_service_accounts_company_service_account_kindes_com",
               table: "company_service_accounts",
               column: "company_service_account_kind_id",
               schema: "portal",
               principalSchema: "portal",
               principalTable: "company_service_account_kindes",
               principalColumn: "id",
               onDelete: ReferentialAction.Cascade);

            //end rename technical_user_kinds to company_service_account_kindes

            //rename technical_user_types to company_service_account_types starts

            migrationBuilder.DropPrimaryKey(
                name: "pk_technical_user_types",
                table: "technical_user_types",
                schema: "portal");

            migrationBuilder.RenameTable(
                name: "technical_user_types",
                schema: "portal",
                newName: "company_service_account_types");

            migrationBuilder.AddPrimaryKey(
                name: "pk_company_service_account_types",
                table: "company_service_account_types",
                column: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_company_service_accounts_company_service_account_types_comp",
                table: "technical_users",
                column: "company_service_account_type_id",
                schema: "portal",
                principalSchema: "portal",
                principalTable: "company_service_account_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            //rename technical_user_types to company_service_account_types starts   

            migrationBuilder.RenameColumn(
                name: "technical_user_id",
                schema: "portal",
                table: "dim_user_creation_data",
                newName: "service_account_id");

            migrationBuilder.RenameIndex(
                name: "ix_dim_user_creation_data_technical_user_id",
                schema: "portal",
                table: "dim_user_creation_data",
                newName: "ix_dim_user_creation_data_service_account_id");

            migrationBuilder.RenameColumn(
                name: "technical_user_id",
                schema: "portal",
                table: "connectors",
                newName: "company_service_account_id");

            migrationBuilder.RenameIndex(
                name: "ix_connectors_technical_user_id",
                schema: "portal",
                table: "connectors",
                newName: "ix_connectors_company_service_account_id");

            migrationBuilder.RenameColumn(
                name: "technical_user_id",
                schema: "portal",
                table: "audit_connector20240814",
                newName: "company_service_account_id");

            migrationBuilder.RenameColumn(
                name: "technical_user_id",
                schema: "portal",
                table: "app_instance_assigned_service_accounts",
                newName: "company_service_account_id");

            migrationBuilder.RenameIndex(
                name: "ix_app_instance_assigned_service_accounts_technical_user_id",
                schema: "portal",
                table: "app_instance_assigned_service_accounts",
                newName: "ix_app_instance_assigned_service_accounts_company_service_acco");

            migrationBuilder.AddForeignKey(
                name: "fk_app_instance_assigned_service_accounts_company_service_acco",
                schema: "portal",
                table: "app_instance_assigned_service_accounts",
                column: "company_service_account_id",
                principalSchema: "portal",
                principalTable: "company_service_accounts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_connectors_company_service_accounts_company_service_account",
                schema: "portal",
                table: "connectors",
                column: "company_service_account_id",
                principalSchema: "portal",
                principalTable: "company_service_accounts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_dim_user_creation_data_company_service_accounts_service_acc",
                schema: "portal",
                table: "dim_user_creation_data",
                column: "service_account_id",
                principalSchema: "portal",
                principalTable: "company_service_accounts",
                principalColumn: "id");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_connector20240814\" (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"sd_creation_process_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"connector_url\", \r\n  NEW.\"type_id\", \r\n  NEW.\"status_id\", \r\n  NEW.\"provider_id\", \r\n  NEW.\"host_id\", \r\n  NEW.\"self_description_document_id\", \r\n  NEW.\"location_id\", \r\n  NEW.\"self_description_message\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"company_service_account_id\", \r\n  NEW.\"sd_creation_process_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_CONNECTOR AFTER INSERT\r\nON \"portal\".\"connectors\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_connector20240814\" (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"sd_creation_process_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"connector_url\", \r\n  NEW.\"type_id\", \r\n  NEW.\"status_id\", \r\n  NEW.\"provider_id\", \r\n  NEW.\"host_id\", \r\n  NEW.\"self_description_document_id\", \r\n  NEW.\"location_id\", \r\n  NEW.\"self_description_message\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"company_service_account_id\", \r\n  NEW.\"sd_creation_process_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_CONNECTOR AFTER UPDATE\r\nON \"portal\".\"connectors\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"();");
        }
    }
}
