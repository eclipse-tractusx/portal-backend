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
    public partial class CPLP1634AddNotificationTopics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "notification_type_assigned_topic",
                schema: "portal",
                columns: table => new
                {
                    notification_type_id = table.Column<int>(type: "integer", nullable: false),
                    notification_topic_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_type_assigned_topic", x => new { x.notification_type_id, x.notification_topic_id });
                    table.ForeignKey(
                        name: "fk_notification_type_assigned_topic_notification_topic_notific",
                        column: x => x.notification_topic_id,
                        principalSchema: "portal",
                        principalTable: "notification_topic",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_notification_type_assigned_topic_notification_type_notifica",
                        column: x => x.notification_type_id,
                        principalSchema: "portal",
                        principalTable: "notification_type",
                        principalColumn: "id");
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

            migrationBuilder.InsertData(
                schema: "portal",
                table: "notification_type_assigned_topic",
                columns: new[] { "notification_topic_id", "notification_type_id" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 1, 3 },
                    { 1, 4 },
                    { 1, 5 },
                    { 1, 6 },
                    { 1, 7 },
                    { 2, 8 },
                    { 3, 9 },
                    { 1, 10 },
                    { 3, 11 },
                    { 1, 12 },
                    { 2, 13 },
                    { 3, 14 }
                });

            migrationBuilder.CreateIndex(
                name: "ix_notification_type_assigned_topic_notification_topic_id",
                schema: "portal",
                table: "notification_type_assigned_topic",
                column: "notification_topic_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_type_assigned_topic_notification_type_id",
                schema: "portal",
                table: "notification_type_assigned_topic",
                column: "notification_type_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification_type_assigned_topic",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "notification_topic",
                schema: "portal");
        }
    }
}
