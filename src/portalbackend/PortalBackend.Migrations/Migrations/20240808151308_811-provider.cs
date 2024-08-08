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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class _811provider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFER\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFER\"() CASCADE;");

            migrationBuilder.DropColumn(
                name: "provider",
                schema: "portal",
                table: "offers");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFER\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_OFFER$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer20231115\" (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"license_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"date_created\", \r\n  NEW.\"date_released\", \r\n  NEW.\"marketing_url\", \r\n  NEW.\"contact_email\", \r\n  NEW.\"contact_number\", \r\n  NEW.\"offer_type_id\", \r\n  NEW.\"sales_manager_id\", \r\n  NEW.\"provider_company_id\", \r\n  NEW.\"offer_status_id\", \r\n  NEW.\"license_type_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_OFFER AFTER INSERT\r\nON \"portal\".\"offers\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFER\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFER\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_OFFER$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer20231115\" (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"license_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"date_created\", \r\n  NEW.\"date_released\", \r\n  NEW.\"marketing_url\", \r\n  NEW.\"contact_email\", \r\n  NEW.\"contact_number\", \r\n  NEW.\"offer_type_id\", \r\n  NEW.\"sales_manager_id\", \r\n  NEW.\"provider_company_id\", \r\n  NEW.\"offer_status_id\", \r\n  NEW.\"license_type_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_OFFER AFTER UPDATE\r\nON \"portal\".\"offers\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFER\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFER\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFER\"() CASCADE;");

            migrationBuilder.AddColumn<string>(
                name: "provider",
                schema: "portal",
                table: "offers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFER\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_OFFER$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer20231115\" (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"provider\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"license_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"date_created\", \r\n  NEW.\"date_released\", \r\n  NEW.\"marketing_url\", \r\n  NEW.\"contact_email\", \r\n  NEW.\"contact_number\", \r\n  NEW.\"provider\", \r\n  NEW.\"offer_type_id\", \r\n  NEW.\"sales_manager_id\", \r\n  NEW.\"provider_company_id\", \r\n  NEW.\"offer_status_id\", \r\n  NEW.\"license_type_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_OFFER AFTER INSERT\r\nON \"portal\".\"offers\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFER\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFER\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_OFFER$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer20231115\" (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"provider\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"license_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"date_created\", \r\n  NEW.\"date_released\", \r\n  NEW.\"marketing_url\", \r\n  NEW.\"contact_email\", \r\n  NEW.\"contact_number\", \r\n  NEW.\"provider\", \r\n  NEW.\"offer_type_id\", \r\n  NEW.\"sales_manager_id\", \r\n  NEW.\"provider_company_id\", \r\n  NEW.\"offer_status_id\", \r\n  NEW.\"license_type_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_OFFER AFTER UPDATE\r\nON \"portal\".\"offers\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFER\"();");
        }
    }
}
