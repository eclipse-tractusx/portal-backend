/********************************************************************************
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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
    public partial class adjust_migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            // Adjust CompaniesLinkedTechnicalUser

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
                UNION
                SELECT
                    tu.id AS technical_user_id,
                    o.provider_company_id AS owners,
                    o.provider_company_id AS provider
                FROM
                    portal.technical_users tu
                        JOIN portal.identities i ON tu.id = i.id
                        LEFT JOIN portal.offer_subscriptions os ON tu.offer_subscription_id = os.id
                        LEFT JOIN portal.offers o ON os.offer_id = o.id
                WHERE tu.technical_user_type_id = 3
                ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert adjusted changes in CompaniesLinkedTechnicalUser
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
        }
    }
}
