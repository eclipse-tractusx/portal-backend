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
    public partial class CPLP2950ChangeConnectorLastEditor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_connectors_company_users_last_editor_id",
                schema: "portal",
                table: "connectors");

            migrationBuilder.AddForeignKey(
                name: "fk_connectors_identities_last_editor_id",
                schema: "portal",
                table: "connectors",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_connectors_identities_last_editor_id",
                schema: "portal",
                table: "connectors");

            migrationBuilder.AddForeignKey(
                name: "fk_connectors_company_users_last_editor_id",
                schema: "portal",
                table: "connectors",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");
        }
    }
}
