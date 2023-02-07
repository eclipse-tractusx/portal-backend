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
    public partial class CPLP2077agreementdocumentids : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "document_id",
                schema: "portal",
                table: "agreements",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("UPDATE portal.agreements s1 SET document_id = (select document_id from portal.agreement_assigned_documents s2 where s1.id = s2.agreement_id);");

            migrationBuilder.DropTable(
                name: "agreement_assigned_documents",
                schema: "portal");

            migrationBuilder.CreateIndex(
                name: "ix_agreements_document_id",
                schema: "portal",
                table: "agreements",
                column: "document_id");

            migrationBuilder.AddForeignKey(
                name: "fk_agreements_documents_document_id",
                schema: "portal",
                table: "agreements",
                column: "document_id",
                principalSchema: "portal",
                principalTable: "documents",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agreement_assigned_documents",
                schema: "portal",
                columns: table => new
                {
                    agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agreement_assigned_documents", x => new { x.agreement_id, x.document_id });
                    table.ForeignKey(
                        name: "fk_agreement_assigned_documents_agreements_agreement_id",
                        column: x => x.agreement_id,
                        principalSchema: "portal",
                        principalTable: "agreements",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_agreement_assigned_documents_documents_document_id",
                        column: x => x.document_id,
                        principalSchema: "portal",
                        principalTable: "documents",
                        principalColumn: "id");
                });

            migrationBuilder.Sql("INSERT into portal.agreement_assigned_documents(agreement_id,document_id) SELECT id, document_id FROM portal.agreements WHERE document_id is not null ;");
            
            migrationBuilder.CreateIndex(
                name: "ix_agreement_assigned_documents_document_id",
                schema: "portal",
                table: "agreement_assigned_documents",
                column: "document_id");

            migrationBuilder.DropForeignKey(
                name: "fk_agreements_documents_document_id",
                schema: "portal",
                table: "agreements");

            migrationBuilder.DropIndex(
                name: "ix_agreements_document_id",
                schema: "portal",
                table: "agreements");

            migrationBuilder.DropColumn(
                name: "document_id",
                schema: "portal",
                table: "agreements");
        }
    }
}
