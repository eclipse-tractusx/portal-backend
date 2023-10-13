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
    public partial class CPLP3341ChangeFunctionConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE portal.connector_assigned_offer_subscriptions DROP CONSTRAINT IF EXISTS CK_Connector_ConnectorType_IsManaged;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS portal.is_connector_managed;");

            migrationBuilder.Sql("ALTER TABLE portal.verified_credential_type_assigned_use_cases DROP CONSTRAINT IF EXISTS CK_VCTypeAssignedUseCase_VerifiedCredentialType_UseCase;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS portal.is_credential_type_use_case;");

            migrationBuilder.Sql("ALTER TABLE portal.company_ssi_details DROP CONSTRAINT IF EXISTS CK_VC_ExternalType_DetailId_UseCase;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS portal.is_external_type_use_case;");

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

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS ct_is_connector_managed ON portal.connector_assigned_offer_subscriptions;");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS portal.tr_is_connector_managed;");

            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS ct_is_credential_type_use_case ON portal.verified_credential_type_assigned_use_cases;");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS portal.tr_is_credential_type_use_case;");

            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS ct_is_external_type_use_case ON portal.company_ssi_details;");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS portal.tr_is_external_type_use_case;");

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
        }
    }
}
