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
using System;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP2831AddConnectorAssignedOfferSubscriptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "connector_assigned_offer_subscriptions",
                schema: "portal",
                columns: table => new
                {
                    connector_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_subscription_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_connector_assigned_offer_subscriptions", x => new { x.connector_id, x.offer_subscription_id });
                    table.ForeignKey(
                        name: "fk_connector_assigned_offer_subscriptions_connectors_connector",
                        column: x => x.connector_id,
                        principalSchema: "portal",
                        principalTable: "connectors",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_connector_assigned_offer_subscriptions_offer_subscriptions_",
                        column: x => x.offer_subscription_id,
                        principalSchema: "portal",
                        principalTable: "offer_subscriptions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_connector_assigned_offer_subscriptions_offer_subscription_id",
                schema: "portal",
                table: "connector_assigned_offer_subscriptions",
                column: "offer_subscription_id");

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

            migrationBuilder.Sql(@"CREATE VIEW portal.offer_subscription_view AS
                SELECT
                  os.id AS subscription_id,
                  o.offer_type_id AS offer_type_id,
                  tu.id AS technical_user,
                  asd.app_instance_id AS app_instance,
                  caos.connector_id AS connector
                FROM
                  portal.offer_subscriptions AS os
                  JOIN portal.offers AS o ON os.offer_id = o.id
                  LEFT JOIN portal.company_service_accounts AS tu ON os.id = tu.offer_subscription_id
                  LEFT JOIN portal.app_subscription_details AS asd ON os.id = asd.offer_subscription_id
                  LEFT JOIN portal.connector_assigned_offer_subscriptions AS caos ON os.id = caos.offer_subscription_id;");

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW [IF EXISTS] portal.subscription_view");
            migrationBuilder.Sql("ALTER TABLE portal.connector_assigned_offer_subscriptions DROP CONSTRAINT IF EXISTS CK_Connector_ConnectorType_IsManaged;");
            migrationBuilder.Sql("DROP FUNCTION portal.is_connector_managed;");

            migrationBuilder.Sql(@"CREATE OR REPLACE VIEW portal.company_linked_service_accounts AS
               SELECT
               csa.id AS service_account_id,
               i.company_id AS owners,
               CASE
               WHEN csa.offer_subscription_id IS NOT NULL THEN os.company_id
               WHEN EXISTS (SELECT 1 FROM portal.connectors cs WHERE cs.company_service_account_id = csa.id) THEN c.provider_id
               ELSE NULL
               END AS provider
               FROM
               portal.company_service_accounts csa
               JOIN portal.identities i ON csa.id = i.id
               LEFT JOIN portal.offer_subscriptions os ON csa.offer_subscription_id = os.id
               LEFT JOIN portal.connectors c ON csa.id = c.company_service_account_id
               WHERE csa.company_service_account_type_id = 1 AND i.identity_type_id=2
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

            migrationBuilder.DropTable(
                name: "connector_assigned_offer_subscriptions",
                schema: "portal");
        }
    }
}
