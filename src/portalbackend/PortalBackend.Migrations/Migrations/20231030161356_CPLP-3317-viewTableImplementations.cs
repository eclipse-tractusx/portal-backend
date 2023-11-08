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
    public partial class CPLP3317viewTableImplementations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE VIEW portal.company_users_view AS
                SELECT
                    c.id as company_id,
                    c.name as company_name,
                    cu.firstname as firstname,
                    cu.lastname as lastname,
                    cu.email as user_email,
                	ius.label as user_status
                FROM portal.identities as i
                     INNER JOIN portal.companies as c on (i.company_id = c.Id)
                     INNER JOIN portal.company_users as cu on (i.id = cu.id)
                	 INNER JOIN portal.identity_user_statuses as ius on (i.user_status_id = ius.id)
                WHERE identity_type_id = 1");
            migrationBuilder.Sql(@"CREATE VIEW company_idp_view AS
                SELECT
                    c.id as company_id,
                    c.name as company_name,
                    iip.iam_idp_alias as idp_alias
                FROM portal.identity_providers as ip
                    INNER JOIN portal.iam_identity_providers as iip on (ip.id = iip.identity_provider_id)
                    INNER JOIN portal.companies as c on (c.id = ip.owner_id)");
            migrationBuilder.Sql(@"CREATE VIEW portal.company_connector_view AS
                SELECT
                    c.id as company_id,
                    c.name as company_name,
                    con.connector_url as connector_url,
                    cs.label as connector_status
                FROM portal.connectors as con
                    INNER JOIN portal.companies as c on (c.Id = con.provider_id)
                	INNER JOIN portal.connector_statuses as cs on (con.status_id = cs.id)");
            migrationBuilder.Sql(@"CREATE VIEW portal.companyRole_collectionRoles_view AS
                SELECT
                    urc.name as collection_name,
                    ur.user_role as user_role,
                    iamC.client_client_id as client_Name
                FROM portal.company_roles as cr
                    INNER JOIN portal.company_role_assigned_role_collections as crarc on (cr.id = crarc.company_role_id)
                    INNER JOIN portal.user_role_assigned_collections as urac on (crarc.user_role_collection_id = urac.user_role_collection_id)
                    INNER JOIN portal.user_role_collections as urc on (urac.user_role_collection_id = urc.id)
                    INNER JOIN portal.user_roles as ur on (urac.user_role_id = ur.id)
                    INNER JOIN portal.app_instances as ai on (ai.app_id = ur.offer_id)
                    INNER JOIN portal.iam_clients as iamC on (ai.iam_client_id = iamC.Id)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS portal.Company_Users_view");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS portal.Company_Idp_View");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS portal.Company_Connector_View");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS portal.CompanyRole_CollectionRoles_View");
        }
    }
}
