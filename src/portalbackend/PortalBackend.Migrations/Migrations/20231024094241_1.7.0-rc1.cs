/********************************************************************************
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
    /// <inheritdoc />
    public partial class _170rc1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION\"() CASCADE;");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "date_created",
                schema: "portal",
                table: "offer_subscriptions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateTable(
                name: "audit_offer_subscription20231013",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_subscription_status_id = table.Column<int>(type: "integer", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    process_id = table.Column<Guid>(type: "uuid", nullable: true),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_offer_subscription20231013", x => x.audit_v1id);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[] { 300, "SYNCHRONIZE_SERVICE_ACCOUNTS" });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_types",
                columns: new[] { "id", "label" },
                values: new object[] { 5, "SERVICE_ACCOUNT_SYNC" });

            migrationBuilder.Sql("ALTER TABLE portal.connector_assigned_offer_subscriptions DROP CONSTRAINT IF EXISTS CK_Connector_ConnectorType_IsManaged;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS portal.is_connector_managed;");

            migrationBuilder.Sql("ALTER TABLE portal.verified_credential_type_assigned_use_cases DROP CONSTRAINT IF EXISTS CK_VCTypeAssignedUseCase_VerifiedCredentialType_UseCase;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS portal.is_credential_type_use_case;");

            migrationBuilder.Sql("ALTER TABLE portal.company_ssi_details DROP CONSTRAINT IF EXISTS CK_VC_ExternalType_DetailId_UseCase;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS portal.is_external_type_use_case;");

            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS ct_is_connector_managed ON portal.connector_assigned_offer_subscriptions;");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS portal.tr_is_connector_managed;");

            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS ct_is_credential_type_use_case ON portal.verified_credential_type_assigned_use_cases;");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS portal.tr_is_credential_type_use_case;");

            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS ct_is_external_type_use_case ON portal.company_ssi_details;");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS portal.tr_is_external_type_use_case;");

            migrationBuilder.Sql(@"CREATE FUNCTION portal.tr_is_connector_managed()
                RETURNS trigger
                VOLATILE
                COST 100
                AS $$
                BEGIN
                IF EXISTS(
                    SELECT 1
                        FROM portal.connectors
                        WHERE Id = NEW.connector_id
                        AND type_id = 2
                )
                THEN RETURN NEW;
                END IF;
                RAISE EXCEPTION 'the connector % is not managed', NEW.connector_id;
                END
                $$ LANGUAGE plpgsql;");

            migrationBuilder.Sql(@"CREATE CONSTRAINT TRIGGER ct_is_connector_managed
                AFTER INSERT
                ON portal.connector_assigned_offer_subscriptions
                INITIALLY DEFERRED
                FOR EACH ROW
                EXECUTE PROCEDURE portal.tr_is_connector_managed();");

            migrationBuilder.Sql(@"CREATE FUNCTION portal.tr_is_credential_type_use_case()
                RETURNS trigger
                VOLATILE
                COST 100
                AS $$
                BEGIN
                IF EXISTS (
                    SELECT 1
                        FROM portal.verified_credential_types
                        WHERE Id = NEW.verified_credential_type_id
                            AND NEW.verified_credential_type_id IN (
                                SELECT verified_credential_type_id
                                FROM portal.verified_credential_type_assigned_kinds
                                WHERE verified_credential_type_kind_id = '1'
                            )
                )
                THEN RETURN NEW;
                END IF;
                RAISE EXCEPTION 'The credential % is not a use case', NEW.verified_credential_type_id;
                END
                $$ LANGUAGE plpgsql;");

            migrationBuilder.Sql(@"CREATE CONSTRAINT TRIGGER ct_is_credential_type_use_case
                AFTER INSERT
                ON portal.verified_credential_type_assigned_use_cases
                INITIALLY DEFERRED
                FOR EACH ROW
                EXECUTE PROCEDURE portal.tr_is_credential_type_use_case();");

            migrationBuilder.Sql(@"CREATE FUNCTION portal.tr_is_external_type_use_case()
                RETURNS trigger
                VOLATILE
                COST 100
                AS $$
                BEGIN
                IF NEW.verified_credential_external_type_use_case_detail_id IS NULL
                    THEN RETURN NEW;
                END IF;
                IF EXISTS (
                    SELECT 1
                            FROM portal.verified_credential_external_type_use_case_detail_versions
                            WHERE Id = NEW.verified_credential_external_type_use_case_detail_id
                                AND verified_credential_external_type_id IN (
                                SELECT verified_credential_external_type_id
                                FROM portal.verified_credential_type_assigned_external_types
                                WHERE verified_credential_type_id IN (
                                    SELECT verified_credential_type_id
                                    FROM portal.verified_credential_type_assigned_kinds
                                    WHERE verified_credential_type_kind_id = '1'
                                )
                            )
                )
                THEN RETURN NEW;
                END IF;
                RAISE EXCEPTION 'the detail % is not an use case', NEW.verified_credential_external_type_use_case_detail_id;
                END
                $$ LANGUAGE plpgsql;");

            migrationBuilder.Sql(@"CREATE CONSTRAINT TRIGGER ct_is_external_type_use_case
                AFTER INSERT
                ON portal.company_ssi_details
                INITIALLY DEFERRED
                FOR EACH ROW
                EXECUTE PROCEDURE portal.tr_is_external_type_use_case();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer_subscription20231013\" (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"process_id\", \"date_created\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"offer_id\", \r\n  NEW.\"offer_subscription_status_id\", \r\n  NEW.\"display_name\", \r\n  NEW.\"description\", \r\n  NEW.\"requester_id\", \r\n  NEW.\"last_editor_id\", \r\n  NEW.\"process_id\", \r\n  NEW.\"date_created\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION AFTER INSERT\r\nON \"portal\".\"offer_subscriptions\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer_subscription20231013\" (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"process_id\", \"date_created\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"offer_id\", \r\n  NEW.\"offer_subscription_status_id\", \r\n  NEW.\"display_name\", \r\n  NEW.\"description\", \r\n  NEW.\"requester_id\", \r\n  NEW.\"last_editor_id\", \r\n  NEW.\"process_id\", \r\n  NEW.\"date_created\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION AFTER UPDATE\r\nON \"portal\".\"offer_subscriptions\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION\"() CASCADE;");

            migrationBuilder.DropTable(
                name: "audit_offer_subscription20231013",
                schema: "portal");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 300);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_types",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "date_created",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS ct_is_connector_managed ON portal.connector_assigned_offer_subscriptions;");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS portal.tr_is_connector_managed;");

            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS ct_is_credential_type_use_case ON portal.verified_credential_type_assigned_use_cases;");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS portal.tr_is_credential_type_use_case;");

            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS ct_is_external_type_use_case ON portal.company_ssi_details;");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS portal.tr_is_external_type_use_case;");

            migrationBuilder.Sql("ALTER TABLE portal.connector_assigned_offer_subscriptions DROP CONSTRAINT IF EXISTS CK_Connector_ConnectorType_IsManaged;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS portal.is_connector_managed;");

            migrationBuilder.Sql("ALTER TABLE portal.verified_credential_type_assigned_use_cases DROP CONSTRAINT IF EXISTS CK_VCTypeAssignedUseCase_VerifiedCredentialType_UseCase;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS portal.is_credential_type_use_case;");

            migrationBuilder.Sql("ALTER TABLE portal.company_ssi_details DROP CONSTRAINT IF EXISTS CK_VC_ExternalType_DetailId_UseCase;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS portal.is_external_type_use_case;");

            migrationBuilder.Sql(@"CREATE FUNCTION portal.is_connector_managed(connectorId uuid)
                RETURNS BOOLEAN
                LANGUAGE plpgsql
                AS
                $$
                BEGIN
                    RETURN EXISTS (
                        SELECT 1
                        FROM portal.connectors
                        WHERE Id = connectorId
                        AND type_id = 2
                    );
                END;
                $$");

            migrationBuilder.Sql(@"
                ALTER TABLE portal.connector_assigned_offer_subscriptions
                ADD CONSTRAINT CK_Connector_ConnectorType_IsManaged
                    CHECK (portal.is_connector_managed(connector_id))");

            migrationBuilder.Sql(@"CREATE FUNCTION portal.is_credential_type_use_case(vc_type_id integer)
                RETURNS BOOLEAN
                LANGUAGE plpgsql
                AS
                $$
                BEGIN
                    RETURN EXISTS (
                        SELECT 1
                        FROM portal.verified_credential_types
                        WHERE Id = vc_type_id
                            AND vc_type_id IN (
                                SELECT verified_credential_type_id
                                FROM portal.verified_credential_type_assigned_kinds
                                WHERE verified_credential_type_kind_id = '1'
                            )
                    );
                END;
                $$");

            migrationBuilder.Sql(@"
                ALTER TABLE portal.verified_credential_type_assigned_use_cases
                ADD CONSTRAINT CK_VCTypeAssignedUseCase_VerifiedCredentialType_UseCase 
                    CHECK (portal.is_credential_type_use_case(verified_credential_type_id))");

            migrationBuilder.Sql(@"CREATE FUNCTION portal.is_external_type_use_case(verified_credential_external_type_use_case_detail_id UUID)
                RETURNS BOOLEAN
                LANGUAGE plpgsql
                AS
                $$
                BEGIN
                    IF verified_credential_external_type_use_case_detail_id IS NULL THEN
                        RETURN TRUE;
                    END IF;
                    RETURN EXISTS (
                        SELECT 1
                            FROM portal.verified_credential_external_type_use_case_detail_versions
                            WHERE Id = verified_credential_external_type_use_case_detail_id
                                AND verified_credential_external_type_id IN (
                                SELECT verified_credential_external_type_id
                                FROM portal.verified_credential_type_assigned_external_types
                                WHERE verified_credential_type_id IN (
                                    SELECT verified_credential_type_id
                                    FROM portal.verified_credential_type_assigned_kinds
                                    WHERE verified_credential_type_kind_id = '1'
                                )
                            )
                    );
                END;
                $$");

            migrationBuilder.Sql(@"
                ALTER TABLE portal.company_ssi_details
                ADD CONSTRAINT CK_VC_ExternalType_DetailId_UseCase 
                    CHECK (portal.is_external_type_use_case(verified_credential_external_type_use_case_detail_id))");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer_subscription20230317\" (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"process_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"offer_id\", \r\n  NEW.\"offer_subscription_status_id\", \r\n  NEW.\"display_name\", \r\n  NEW.\"description\", \r\n  NEW.\"requester_id\", \r\n  NEW.\"last_editor_id\", \r\n  NEW.\"process_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION AFTER INSERT\r\nON \"portal\".\"offer_subscriptions\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer_subscription20230317\" (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"process_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"offer_id\", \r\n  NEW.\"offer_subscription_status_id\", \r\n  NEW.\"display_name\", \r\n  NEW.\"description\", \r\n  NEW.\"requester_id\", \r\n  NEW.\"last_editor_id\", \r\n  NEW.\"process_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION AFTER UPDATE\r\nON \"portal\".\"offer_subscriptions\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION\"();");
        }
    }
}
