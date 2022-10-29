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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1422AddSelfDescriptionDocuments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "self_description_document_id",
                schema: "portal",
                table: "connectors",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "self_description_document_id",
                schema: "portal",
                table: "companies",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_connectors_self_description_document_id",
                schema: "portal",
                table: "connectors",
                column: "self_description_document_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_companies_self_description_document_id",
                schema: "portal",
                table: "companies",
                column: "self_description_document_id");

            migrationBuilder.AddForeignKey(
                name: "fk_companies_documents_self_description_document_id",
                schema: "portal",
                table: "companies",
                column: "self_description_document_id",
                principalSchema: "portal",
                principalTable: "documents",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_connectors_documents_self_description_document_id",
                schema: "portal",
                table: "connectors",
                column: "self_description_document_id",
                principalSchema: "portal",
                principalTable: "documents",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_companies_documents_self_description_document_id",
                schema: "portal",
                table: "companies");

            migrationBuilder.DropForeignKey(
                name: "fk_connectors_documents_self_description_document_id",
                schema: "portal",
                table: "connectors");

            migrationBuilder.DropIndex(
                name: "ix_connectors_self_description_document_id",
                schema: "portal",
                table: "connectors");

            migrationBuilder.DropIndex(
                name: "ix_companies_self_description_document_id",
                schema: "portal",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "self_description_document_id",
                schema: "portal",
                table: "connectors");

            migrationBuilder.DropColumn(
                name: "self_description_document_id",
                schema: "portal",
                table: "companies");
        }
    }
}
