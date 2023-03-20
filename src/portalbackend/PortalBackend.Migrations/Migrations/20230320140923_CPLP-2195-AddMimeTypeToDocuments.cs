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
    public partial class CPLP2195AddMimeTypeToDocuments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "media_type_id",
                schema: "portal",
                table: "documents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "media_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media_types", x => x.id);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "media_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "JPEG" },
                    { 2, "GIF" },
                    { 3, "PNG" },
                    { 4, "SVG" },
                    { 5, "TIFF" },
                    { 6, "PDF" },
                    { 7, "JSON" },
                    { 8, "PEM" },
                    { 9, "CA_CERT" },
                    { 10, "PKX_CER" },
                    { 11, "OCTET" }
                });

            migrationBuilder.Sql("UPDATE portal.documents SET media_type_id = 1 where document_name ILIKE '%.jpg'");
            migrationBuilder.Sql("UPDATE portal.documents SET media_type_id = 1 where document_name ILIKE '%.jpeg'");
            migrationBuilder.Sql("UPDATE portal.documents SET media_type_id = 2 where document_name ILIKE '%.gif'");
            migrationBuilder.Sql("UPDATE portal.documents SET media_type_id = 3 where document_name ILIKE '%.png'");
            migrationBuilder.Sql("UPDATE portal.documents SET media_type_id = 4 where document_name ILIKE '%.svg'");
            migrationBuilder.Sql("UPDATE portal.documents SET media_type_id = 5 where document_name ILIKE '%.tif'");
            migrationBuilder.Sql("UPDATE portal.documents SET media_type_id = 5 where document_name ILIKE '%.tiff'");
            migrationBuilder.Sql("UPDATE portal.documents SET media_type_id = 6 where document_name ILIKE '%.pdf'");
            migrationBuilder.Sql("UPDATE portal.documents SET media_type_id = 7 where document_name ILIKE '%.json'");
            migrationBuilder.Sql("UPDATE portal.documents SET media_type_id = 6 where media_type_id = 0 AND document_type_id IN (1,3,5)");
            migrationBuilder.Sql("UPDATE portal.documents SET media_type_id = 3 where media_type_id = 0 AND document_type_id IN (6,7)");

            migrationBuilder.CreateIndex(
                name: "ix_documents_media_type_id",
                schema: "portal",
                table: "documents",
                column: "media_type_id");

            migrationBuilder.AddForeignKey(
                name: "fk_documents_media_types_media_type_id",
                schema: "portal",
                table: "documents",
                column: "media_type_id",
                principalSchema: "portal",
                principalTable: "media_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_documents_media_types_media_type_id",
                schema: "portal",
                table: "documents");

            migrationBuilder.DropTable(
                name: "media_types",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_documents_media_type_id",
                schema: "portal",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "media_type_id",
                schema: "portal",
                table: "documents");
        }
    }
}
