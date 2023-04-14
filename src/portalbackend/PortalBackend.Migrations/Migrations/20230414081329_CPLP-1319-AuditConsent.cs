﻿/********************************************************************************
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
    public partial class CPLP1319AuditConsent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "last_editor_id",
                schema: "portal",
                table: "consents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_consent20230412",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    comment = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    consent_status_id = table.Column<int>(type: "integer", nullable: false),
                    target = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_consent20230412", x => x.audit_v1id);
                });

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_CONSENT() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_CONSENT$\r\nBEGIN\r\n  INSERT INTO portal.audit_consent20230412 (\"id\", \"date_created\", \"comment\", \"consent_status_id\", \"target\", \"agreement_id\", \"company_id\", \"document_id\", \"company_user_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.date_created, \r\n  OLD.comment, \r\n  OLD.consent_status_id, \r\n  OLD.target, \r\n  OLD.agreement_id, \r\n  OLD.company_id, \r\n  OLD.document_id, \r\n  OLD.company_user_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_CONSENT$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_CONSENT AFTER DELETE\r\nON portal.consents\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_CONSENT();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_CONSENT() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_CONSENT$\r\nBEGIN\r\n  INSERT INTO portal.audit_consent20230412 (\"id\", \"date_created\", \"comment\", \"consent_status_id\", \"target\", \"agreement_id\", \"company_id\", \"document_id\", \"company_user_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.comment, \r\n  NEW.consent_status_id, \r\n  NEW.target, \r\n  NEW.agreement_id, \r\n  NEW.company_id, \r\n  NEW.document_id, \r\n  NEW.company_user_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_CONSENT$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_CONSENT AFTER INSERT\r\nON portal.consents\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_CONSENT();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_CONSENT() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_CONSENT$\r\nBEGIN\r\n  INSERT INTO portal.audit_consent20230412 (\"id\", \"date_created\", \"comment\", \"consent_status_id\", \"target\", \"agreement_id\", \"company_id\", \"document_id\", \"company_user_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.comment, \r\n  NEW.consent_status_id, \r\n  NEW.target, \r\n  NEW.agreement_id, \r\n  NEW.company_id, \r\n  NEW.document_id, \r\n  NEW.company_user_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_CONSENT$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_CONSENT AFTER UPDATE\r\nON portal.consents\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_CONSENT();");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_CONSENT() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_CONSENT() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_CONSENT() CASCADE;");

            migrationBuilder.DropTable(
                name: "audit_consent20230412",
                schema: "portal");

            migrationBuilder.DropColumn(
                name: "last_editor_id",
                schema: "portal",
                table: "consents");
        }
    }
}
