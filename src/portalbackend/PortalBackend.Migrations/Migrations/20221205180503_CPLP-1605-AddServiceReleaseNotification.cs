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
    public partial class CPLP1605AddServiceReleaseNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "portal",
                table: "notification_type",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 17, "SERVICE_RELEASE_REQUEST" },
                    { 18, "SERVICE_RELEASE_APPROVAL" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "notification_type_assigned_topic",
                columns: new[] { "notification_topic_id", "notification_type_id" },
                values: new object[] { 3, 15 });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "notification_type_assigned_topic",
                columns: new[] { "notification_topic_id", "notification_type_id" },
                values: new object[] { 3, 17 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "portal",
                table: "notification_type",
                keyColumn: "id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "notification_type_assigned_topic",
                keyColumns: new[] { "notification_topic_id", "notification_type_id" },
                keyValues: new object[] { 3, 15 });

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "notification_type_assigned_topic",
                keyColumns: new[] { "notification_topic_id", "notification_type_id" },
                keyValues: new object[] { 3, 17 });

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "notification_type",
                keyColumn: "id",
                keyValue: 17);
        }
    }
}
