/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1634AddNotificationTopics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "notification_topic_id",
                schema: "portal",
                table: "notifications",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "notification_topic_id",
                schema: "portal",
                table: "notifications",
                type: "integer",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "notification_topic",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_topic", x => x.id);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "notification_topic",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "INFO" },
                    { 2, "ACTION" },
                    { 3, "OFFER" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_notification_topic_id",
                schema: "portal",
                table: "notifications",
                column: "notification_topic_id");

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_notification_topic_notification_topic_id",
                schema: "portal",
                table: "notifications",
                column: "notification_topic_id",
                principalSchema: "portal",
                principalTable: "notification_topic",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_notifications_notification_topic_notification_topic_id",
                schema: "portal",
                table: "notifications");

            migrationBuilder.DropTable(
                name: "notification_topic",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_notifications_notification_topic_id",
                schema: "portal",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "notification_topic_id",
                schema: "portal",
                table: "notifications");
        }
    }
}
