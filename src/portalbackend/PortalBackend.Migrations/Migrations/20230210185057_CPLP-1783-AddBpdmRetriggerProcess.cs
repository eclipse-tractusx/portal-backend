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

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1783AddBpdmRetriggerProcess : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "date_created",
                schema: "portal",
                table: "process_steps",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "date_last_changed",
                schema: "portal",
                table: "process_steps",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 10,
                column: "label",
                value: "START_SELF_DESCRIPTION_LP");

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 13, "RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH" },
                    { 14, "RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL" },
                    { 15, "OVERRIDE_BUSINESS_PARTNER_NUMBER" },
                    { 16, "TRIGGER_OVERRIDE_CLEARING_HOUSE" },
                    { 17, "START_OVERRIDE_CLEARING_HOUSE" },
                    { 18, "FINISH_SELF_DESCRIPTION_LP" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 18);

            migrationBuilder.DropColumn(
                name: "date_created",
                schema: "portal",
                table: "process_steps");

            migrationBuilder.DropColumn(
                name: "date_last_changed",
                schema: "portal",
                table: "process_steps");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 10,
                column: "label",
                value: "CREATE_SELF_DESCRIPTION_LP");
        }
    }
}
