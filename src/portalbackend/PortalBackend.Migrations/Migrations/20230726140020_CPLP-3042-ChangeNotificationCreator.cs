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
    public partial class CPLP3042ChangeNotificationCreator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_notifications_company_users_creator_id",
                schema: "portal",
                table: "notifications");

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_identities_creator_id",
                schema: "portal",
                table: "notifications",
                column: "creator_user_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_notifications_identities_creator_id",
                schema: "portal",
                table: "notifications");

            migrationBuilder.Sql("UPDATE portal.notifications as n SET creator_user_id = null FROM (SELECT id FROM portal.identities where identity_type_id = 2) AS i WHERE i.id = n.creator_user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_company_users_creator_id",
                schema: "portal",
                table: "notifications",
                column: "creator_user_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");
        }
    }
}
