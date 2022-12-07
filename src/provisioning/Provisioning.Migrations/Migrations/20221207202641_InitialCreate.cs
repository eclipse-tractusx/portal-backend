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

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Migrations.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "provisioning");

            migrationBuilder.CreateSequence<int>(
                name: "client_sequence_sequence_id_seq",
                schema: "provisioning");

            migrationBuilder.CreateSequence<int>(
                name: "identity_provider_sequence_sequence_id_seq",
                schema: "provisioning");

            migrationBuilder.CreateTable(
                name: "client_sequences",
                schema: "provisioning",
                columns: table => new
                {
                    sequence_id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('provisioning.client_sequence_sequence_id_seq'::regclass)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_client_sequences", x => x.sequence_id);
                });

            migrationBuilder.CreateTable(
                name: "identity_provider_sequences",
                schema: "provisioning",
                columns: table => new
                {
                    sequence_id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('provisioning.identity_provider_sequence_sequence_id_seq'::regclass)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_provider_sequences", x => x.sequence_id);
                });

            migrationBuilder.CreateTable(
                name: "user_password_resets",
                schema: "provisioning",
                columns: table => new
                {
                    user_entity_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    password_modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reset_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_password_resets", x => x.user_entity_id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "client_sequences",
                schema: "provisioning");

            migrationBuilder.DropTable(
                name: "identity_provider_sequences",
                schema: "provisioning");

            migrationBuilder.DropTable(
                name: "user_password_resets",
                schema: "provisioning");

            migrationBuilder.DropSequence(
                name: "client_sequence_sequence_id_seq",
                schema: "provisioning");

            migrationBuilder.DropSequence(
                name: "identity_provider_sequence_sequence_id_seq",
                schema: "provisioning");
        }
    }
}
