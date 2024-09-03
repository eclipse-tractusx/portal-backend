/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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
    public partial class _240CreateProcessPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_processes_process_types_process_type_id",
                schema: "portal",
                table: "processes");

            migrationBuilder.DropIndex(
                name: "ix_offer_subscriptions_process_id",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.DropIndex(
                name: "ix_identity_provider_assigned_processes_process_id",
                schema: "portal",
                table: "identity_provider_assigned_processes");

            migrationBuilder.DropIndex(
                name: "ix_company_user_assigned_processes_process_id",
                schema: "portal",
                table: "company_user_assigned_processes");

            migrationBuilder.DropIndex(
                name: "ix_company_applications_checklist_process_id",
                schema: "portal",
                table: "company_applications");

            migrationBuilder.CreateIndex(
                name: "ix_offer_subscriptions_process_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "process_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_provider_assigned_processes_process_id",
                schema: "portal",
                table: "identity_provider_assigned_processes",
                column: "process_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_user_assigned_processes_process_id",
                schema: "portal",
                table: "company_user_assigned_processes",
                column: "process_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_applications_checklist_process_id",
                schema: "portal",
                table: "company_applications",
                column: "checklist_process_id");

            migrationBuilder.AddForeignKey(
                name: "fk_processes_process_types_process_type_id",
                schema: "portal",
                table: "processes",
                column: "process_type_id",
                principalSchema: "portal",
                principalTable: "process_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_processes_process_types_process_type_id",
                schema: "portal",
                table: "processes");

            migrationBuilder.DropIndex(
                name: "ix_offer_subscriptions_process_id",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.DropIndex(
                name: "ix_identity_provider_assigned_processes_process_id",
                schema: "portal",
                table: "identity_provider_assigned_processes");

            migrationBuilder.DropIndex(
                name: "ix_company_user_assigned_processes_process_id",
                schema: "portal",
                table: "company_user_assigned_processes");

            migrationBuilder.DropIndex(
                name: "ix_company_applications_checklist_process_id",
                schema: "portal",
                table: "company_applications");

            migrationBuilder.CreateIndex(
                name: "ix_offer_subscriptions_process_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "process_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_identity_provider_assigned_processes_process_id",
                schema: "portal",
                table: "identity_provider_assigned_processes",
                column: "process_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_company_user_assigned_processes_process_id",
                schema: "portal",
                table: "company_user_assigned_processes",
                column: "process_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_company_applications_checklist_process_id",
                schema: "portal",
                table: "company_applications",
                column: "checklist_process_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_processes_process_types_process_type_id",
                schema: "portal",
                table: "processes",
                column: "process_type_id",
                principalSchema: "portal",
                principalTable: "process_types",
                principalColumn: "id");
        }
    }
}
