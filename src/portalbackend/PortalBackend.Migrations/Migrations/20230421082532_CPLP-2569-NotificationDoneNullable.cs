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
    public partial class CPLP2569NotificationDoneNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "done",
                schema: "portal",
                table: "notifications",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.Sql("UPDATE portal.notification_type_assigned_topics SET notification_topic_id = 2 WHERE notification_type_id = 11 AND notification_topic_id = 3;");
            migrationBuilder.Sql("UPDATE portal.notification_type_assigned_topics SET notification_topic_id = 2 WHERE notification_type_id = 17 AND notification_topic_id = 3;");
            migrationBuilder.Sql("UPDATE portal.notifications SET DONE = null WHERE notification_type_id != 8 AND notification_type_id != 11 AND notification_type_id != 13 AND notification_type_id != 17");
            migrationBuilder.Sql("UPDATE portal.notifications SET DONE = false WHERE DONE != true AND (notification_type_id = 8 OR notification_type_id = 11 OR notification_type_id = 13 OR notification_type_id = 17)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE portal.notifications SET DONE = false WHERE DONE IS NULL");

            migrationBuilder.AlterColumn<bool>(
                name: "done",
                schema: "portal",
                table: "notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);
        }
    }
}
