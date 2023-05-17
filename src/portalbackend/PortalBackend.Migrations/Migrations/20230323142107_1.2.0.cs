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
using System;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
	public partial class _120 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<DateTimeOffset>(
				name: "lock_expiry_date",
				schema: "portal",
				table: "processes",
				type: "timestamp with time zone",
				nullable: true);

			migrationBuilder.AddColumn<Guid>(
				name: "version",
				schema: "portal",
				table: "processes",
				type: "uuid",
				nullable: false,
				defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

			migrationBuilder.AddColumn<string>(
				name: "message",
				schema: "portal",
				table: "process_steps",
				type: "text",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "media_type_id",
				schema: "portal",
				table: "documents",
				type: "integer",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.AddColumn<Guid>(
				name: "last_editor_id",
				schema: "portal",
				table: "company_assigned_roles",
				type: "uuid",
				nullable: true);

			migrationBuilder.CreateTable(
				name: "audit_company_assigned_role2023316",
				schema: "portal",
				columns: table => new
				{
					audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
					company_id = table.Column<Guid>(type: "uuid", nullable: false),
					company_role_id = table.Column<int>(type: "integer", nullable: false),
					date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
					last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
					audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
					audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
					audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_audit_company_assigned_role2023316", x => x.audit_v1id);
				});

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

			migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_assigned_role2023316 (\"company_id\", \"company_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.company_id, \r\n  OLD.company_role_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYASSIGNEDROLE AFTER DELETE\r\nON portal.company_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_COMPANYASSIGNEDROLE();");

			migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_assigned_role2023316 (\"company_id\", \"company_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.company_id, \r\n  NEW.company_role_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYASSIGNEDROLE AFTER INSERT\r\nON portal.company_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_COMPANYASSIGNEDROLE();");

			migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_assigned_role2023316 (\"company_id\", \"company_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.company_id, \r\n  NEW.company_role_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYASSIGNEDROLE AFTER UPDATE\r\nON portal.company_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_COMPANYASSIGNEDROLE();");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYASSIGNEDROLE() CASCADE;");

			migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYASSIGNEDROLE() CASCADE;");

			migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYASSIGNEDROLE() CASCADE;");

			migrationBuilder.DropForeignKey(
				name: "fk_documents_media_types_media_type_id",
				schema: "portal",
				table: "documents");

			migrationBuilder.DropTable(
				name: "audit_company_assigned_role2023316",
				schema: "portal");

			migrationBuilder.DropTable(
				name: "media_types",
				schema: "portal");

			migrationBuilder.DropIndex(
				name: "ix_documents_media_type_id",
				schema: "portal",
				table: "documents");

			migrationBuilder.DropColumn(
				name: "lock_expiry_date",
				schema: "portal",
				table: "processes");

			migrationBuilder.DropColumn(
				name: "version",
				schema: "portal",
				table: "processes");

			migrationBuilder.DropColumn(
				name: "message",
				schema: "portal",
				table: "process_steps");

			migrationBuilder.DropColumn(
				name: "media_type_id",
				schema: "portal",
				table: "documents");

			migrationBuilder.DropColumn(
				name: "last_editor_id",
				schema: "portal",
				table: "company_assigned_roles");
		}
	}
}
