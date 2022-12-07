/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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
    public partial class RenameEnumEntityKeyColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "invitation_status_id",
                schema: "portal",
                table: "invitation_statuses",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "identity_provider_category_id",
                schema: "portal",
                table: "identity_provider_categories",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "document_type_id",
                schema: "portal",
                table: "document_types",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "consent_status_id",
                schema: "portal",
                table: "consent_statuses",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "company_user_status_id",
                schema: "portal",
                table: "company_user_statuses",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "company_status_id",
                schema: "portal",
                table: "company_statuses",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "company_role_id",
                schema: "portal",
                table: "company_roles",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "application_status_id",
                schema: "portal",
                table: "company_application_statuses",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "app_status_id",
                schema: "portal",
                table: "app_statuses",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "agreement_category_id",
                schema: "portal",
                table: "agreement_categories",
                newName: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                schema: "portal",
                table: "invitation_statuses",
                newName: "invitation_status_id");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "portal",
                table: "identity_provider_categories",
                newName: "identity_provider_category_id");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "portal",
                table: "document_types",
                newName: "document_type_id");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "portal",
                table: "consent_statuses",
                newName: "consent_status_id");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "portal",
                table: "company_user_statuses",
                newName: "company_user_status_id");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "portal",
                table: "company_statuses",
                newName: "company_status_id");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "portal",
                table: "company_roles",
                newName: "company_role_id");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "portal",
                table: "company_application_statuses",
                newName: "application_status_id");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "portal",
                table: "app_statuses",
                newName: "app_status_id");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "portal",
                table: "agreement_categories",
                newName: "agreement_category_id");
        }
    }
}
