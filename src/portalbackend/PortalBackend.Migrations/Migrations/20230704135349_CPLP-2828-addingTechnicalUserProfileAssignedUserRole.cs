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
    public partial class CPLP2828addingTechnicalUserProfileAssignedUserRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_technical_user_profile_assigned_user_roles_technical_user_p",
                schema: "portal",
                table: "technical_user_profile_assigned_user_roles");

            migrationBuilder.DropForeignKey(
                name: "fk_technical_user_profile_assigned_user_roles_user_roles_user_r",
                schema: "portal",
                table: "technical_user_profile_assigned_user_roles");

            migrationBuilder.AddForeignKey(
                name: "fk_technical_user_profile_assigned_user_roles_technical_user_p",
                schema: "portal",
                table: "technical_user_profile_assigned_user_roles",
                column: "technical_user_profile_id",
                principalSchema: "portal",
                principalTable: "technical_user_profiles",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_technical_user_profile_assigned_user_roles_user_roles_user_r",
                schema: "portal",
                table: "technical_user_profile_assigned_user_roles",
                column: "user_role_id",
                principalSchema: "portal",
                principalTable: "user_roles",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_technical_user_profile_assigned_user_roles_technical_user_p",
                schema: "portal",
                table: "technical_user_profile_assigned_user_roles");

            migrationBuilder.DropForeignKey(
                name: "fk_technical_user_profile_assigned_user_roles_user_roles_user_r",
                schema: "portal",
                table: "technical_user_profile_assigned_user_roles");

            migrationBuilder.AddForeignKey(
                name: "fk_technical_user_profile_assigned_user_roles_technical_user_p",
                schema: "portal",
                table: "technical_user_profile_assigned_user_roles",
                column: "technical_user_profile_id",
                principalSchema: "portal",
                principalTable: "technical_user_profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_technical_user_profile_assigned_user_roles_user_roles_user_r",
                schema: "portal",
                table: "technical_user_profile_assigned_user_roles",
                column: "user_role_id",
                principalSchema: "portal",
                principalTable: "user_roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
