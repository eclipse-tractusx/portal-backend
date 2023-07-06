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
    public partial class CPLP2841CompanyLinkedServiceAccountsView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.Sql(@"CREATE OR REPLACE VIEW CompanyLinkedServiceAccounts AS
               SELECT
               csa.id AS service_account_id,
               i.company_id as owners,
               CASE
               WHEN csa.offer_subscription_id IS NOT NULL THEN os.company_id
               WHEN EXISTS (SELECT 1 FROM portal.connectors cs WHERE cs.company_service_account_id = csa.id) THEN c.provider_id
               ELSE NULL
               END AS provider
               FROM
               portal.company_service_accounts csa
               JOIN portal.identities i ON csa.id = i.id
               LEFT JOIN portal.offer_subscriptions os ON csa.offer_subscription_id = os.id
               LEFT JOIN portal.connectors  c ON csa.id = c.company_service_account_id
               WHERE csa.company_service_account_type_id = 1 AND i.identity_type_id=2
               Union 
               SELECT
               csa.id AS service_account_id,
               i.company_id as owners,
               null as   provider
               FROM
               portal.company_service_accounts csa
               JOIN portal.identities i ON csa.id = i.id
               WHERE csa.company_service_account_type_id = 2
            ");
  
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW [IF EXISTS] CompanyLinkedServiceAccounts");
        }
    }
}
