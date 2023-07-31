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
    public partial class _160rc4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_notifications_company_users_creator_id",
                schema: "portal",
                table: "notifications");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_last_editor_id",
                schema: "portal",
                table: "user_roles",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_provider_company_details_last_editor_id",
                schema: "portal",
                table: "provider_company_details",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_offers_last_editor_id",
                schema: "portal",
                table: "offers",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_offer_subscriptions_last_editor_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_assigned_roles_last_editor_id",
                schema: "portal",
                table: "identity_assigned_roles",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_identities_last_editor_id",
                schema: "portal",
                table: "identities",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_consents_last_editor_id",
                schema: "portal",
                table: "consents",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_users_last_editor_id",
                schema: "portal",
                table: "company_users",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_ssi_details_last_editor_id",
                schema: "portal",
                table: "company_ssi_details",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_assigned_roles_last_editor_id",
                schema: "portal",
                table: "company_assigned_roles",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_applications_last_editor_id",
                schema: "portal",
                table: "company_applications",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_subscription_details_last_editor_id",
                schema: "portal",
                table: "app_subscription_details",
                column: "last_editor_id");

            migrationBuilder.AddForeignKey(
                name: "fk_app_subscription_details_identities_last_editor_id",
                schema: "portal",
                table: "app_subscription_details",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_applications_identities_last_editor_id",
                schema: "portal",
                table: "company_applications",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_assigned_roles_identities_last_editor_id",
                schema: "portal",
                table: "company_assigned_roles",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_ssi_details_identities_last_editor_id",
                schema: "portal",
                table: "company_ssi_details",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_users_identities_last_editor_id",
                schema: "portal",
                table: "company_users",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_consents_identities_last_editor_id",
                schema: "portal",
                table: "consents",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_identities_identities_last_editor_id",
                schema: "portal",
                table: "identities",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_identity_assigned_roles_identities_last_editor_id",
                schema: "portal",
                table: "identity_assigned_roles",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_identities_creator_id",
                schema: "portal",
                table: "notifications",
                column: "creator_user_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_offer_subscriptions_identities_last_editor_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_offers_identities_last_editor_id",
                schema: "portal",
                table: "offers",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_provider_company_details_identities_last_editor_id",
                schema: "portal",
                table: "provider_company_details",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_identities_last_editor_id",
                schema: "portal",
                table: "user_roles",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_app_subscription_details_identities_last_editor_id",
                schema: "portal",
                table: "app_subscription_details");

            migrationBuilder.DropForeignKey(
                name: "fk_company_applications_identities_last_editor_id",
                schema: "portal",
                table: "company_applications");

            migrationBuilder.DropForeignKey(
                name: "fk_company_assigned_roles_identities_last_editor_id",
                schema: "portal",
                table: "company_assigned_roles");

            migrationBuilder.DropForeignKey(
                name: "fk_company_ssi_details_identities_last_editor_id",
                schema: "portal",
                table: "company_ssi_details");

            migrationBuilder.DropForeignKey(
                name: "fk_company_users_identities_last_editor_id",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropForeignKey(
                name: "fk_consents_identities_last_editor_id",
                schema: "portal",
                table: "consents");

            migrationBuilder.DropForeignKey(
                name: "fk_identities_identities_last_editor_id",
                schema: "portal",
                table: "identities");

            migrationBuilder.DropForeignKey(
                name: "fk_identity_assigned_roles_identities_last_editor_id",
                schema: "portal",
                table: "identity_assigned_roles");

            migrationBuilder.DropForeignKey(
                name: "fk_notifications_identities_creator_id",
                schema: "portal",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "fk_offer_subscriptions_identities_last_editor_id",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.DropForeignKey(
                name: "fk_offers_identities_last_editor_id",
                schema: "portal",
                table: "offers");

            migrationBuilder.DropForeignKey(
                name: "fk_provider_company_details_identities_last_editor_id",
                schema: "portal",
                table: "provider_company_details");

            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_identities_last_editor_id",
                schema: "portal",
                table: "user_roles");

            migrationBuilder.DropIndex(
                name: "ix_user_roles_last_editor_id",
                schema: "portal",
                table: "user_roles");

            migrationBuilder.DropIndex(
                name: "ix_provider_company_details_last_editor_id",
                schema: "portal",
                table: "provider_company_details");

            migrationBuilder.DropIndex(
                name: "ix_offers_last_editor_id",
                schema: "portal",
                table: "offers");

            migrationBuilder.DropIndex(
                name: "ix_offer_subscriptions_last_editor_id",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.DropIndex(
                name: "ix_identity_assigned_roles_last_editor_id",
                schema: "portal",
                table: "identity_assigned_roles");

            migrationBuilder.DropIndex(
                name: "ix_identities_last_editor_id",
                schema: "portal",
                table: "identities");

            migrationBuilder.DropIndex(
                name: "ix_consents_last_editor_id",
                schema: "portal",
                table: "consents");

            migrationBuilder.DropIndex(
                name: "ix_company_users_last_editor_id",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropIndex(
                name: "ix_company_ssi_details_last_editor_id",
                schema: "portal",
                table: "company_ssi_details");

            migrationBuilder.DropIndex(
                name: "ix_company_assigned_roles_last_editor_id",
                schema: "portal",
                table: "company_assigned_roles");

            migrationBuilder.DropIndex(
                name: "ix_company_applications_last_editor_id",
                schema: "portal",
                table: "company_applications");

            migrationBuilder.DropIndex(
                name: "ix_app_subscription_details_last_editor_id",
                schema: "portal",
                table: "app_subscription_details");

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
