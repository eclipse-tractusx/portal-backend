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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.AuditEntities;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1255AuditCompanyApplications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "id",
                schema: "portal",
                table: "company_user_assigned_roles",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "last_editor_id",
                schema: "portal",
                table: "company_user_assigned_roles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "last_editor_id",
                schema: "portal",
                table: "company_applications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_company_applications_cplp_1255_audit_company_applications",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_operation_id = table.Column<int>(type: "integer", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    application_status_id = table.Column<int>(type: "integer", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_applications_cplp_1255_audit_company_applicat", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_company_user_assigned_roles_cplp_1255_audit_company_applications",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_operation_id = table.Column<int>(type: "integer", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_user_assigned_roles_cplp_1255_audit_company_a", x => x.id);
                });

            migrationBuilder.Sql(
                "CREATE OR REPLACE FUNCTION portal.process_company_user_assigned_roles_audit() RETURNS TRIGGER AS $audit_company_user_assigned_roles$ " +
                "BEGIN " +
                "IF (TG_OP = 'DELETE') THEN " +
                "INSERT INTO portal.audit_company_user_assigned_roles_cplp_1255_audit_company_applications ( id, audit_id, company_user_id,user_role_id, last_editor_id, date_last_changed, audit_operation_id ) SELECT gen_random_uuid(), OLD.id, OLD.company_user_id,OLD.user_role_id, OLD.last_editor_id, CURRENT_DATE, 3 ; " +
                "ELSIF (TG_OP = 'UPDATE') THEN " +
                "INSERT INTO portal.audit_company_user_assigned_roles_cplp_1255_audit_company_applications ( id, audit_id, company_user_id,user_role_id, last_editor_id, date_last_changed, audit_operation_id ) SELECT gen_random_uuid(), NEW.id, NEW.company_user_id,NEW.user_role_id, NEW.last_editor_id, CURRENT_DATE, 2 ; " +
                "ELSIF (TG_OP = 'INSERT') THEN " +
                "INSERT INTO portal.audit_company_user_assigned_roles_cplp_1255_audit_company_applications ( id, audit_id, company_user_id,user_role_id, last_editor_id, date_last_changed, audit_operation_id ) SELECT gen_random_uuid(), NEW.id, NEW.company_user_id,NEW.user_role_id, NEW.last_editor_id, CURRENT_DATE, 1 ; " +
                "END IF; " +
                "RETURN NULL; " +
                "END; " +
                "$audit_company_user_assigned_roles$ LANGUAGE plpgsql; " +
                "CREATE OR REPLACE TRIGGER audit_company_user_assigned_roles " +
                "AFTER INSERT OR UPDATE OR DELETE ON portal.company_user_assigned_roles " +
                "FOR EACH ROW EXECUTE FUNCTION portal.process_company_user_assigned_roles_audit();");
            
            migrationBuilder.Sql(
                "CREATE OR REPLACE FUNCTION portal.process_company_applications_audit() RETURNS TRIGGER AS $audit_company_applications$ " +
                "BEGIN "+
                "IF (TG_OP = 'DELETE') THEN "+
                "INSERT INTO portal.audit_company_applications_cplp_1255_audit_company_applications ( id, audit_id, date_created,application_status_id,company_id, last_editor_id, date_last_changed, audit_operation_id ) SELECT gen_random_uuid(), OLD.id, OLD.date_created,OLD.application_status_id,OLD.company_id, OLD.last_editor_id, CURRENT_DATE, 3 ; "+
                "ELSIF (TG_OP = 'UPDATE') THEN "+
                "INSERT INTO portal.audit_company_applications_cplp_1255_audit_company_applications ( id, audit_id, date_created,application_status_id,company_id, last_editor_id, date_last_changed, audit_operation_id ) SELECT gen_random_uuid(), NEW.id, NEW.date_created,NEW.application_status_id,NEW.company_id, NEW.last_editor_id, CURRENT_DATE, 2 ; "+
                "ELSIF (TG_OP = 'INSERT') THEN "+
                "INSERT INTO portal.audit_company_applications_cplp_1255_audit_company_applications ( id, audit_id, date_created,application_status_id,company_id, last_editor_id, date_last_changed, audit_operation_id ) SELECT gen_random_uuid(), NEW.id, NEW.date_created,NEW.application_status_id,NEW.company_id, NEW.last_editor_id, CURRENT_DATE, 1 ; "+
                "END IF; "+
                "RETURN NULL; "+
                "END; "+
                "$audit_company_applications$ LANGUAGE plpgsql; "+
                "CREATE OR REPLACE TRIGGER audit_company_applications "+
                "AFTER INSERT OR UPDATE OR DELETE ON portal.company_applications "+
                "FOR EACH ROW EXECUTE FUNCTION portal.process_company_applications_audit(); ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS process_audit_company_user_assigned_roles_audit(); DROP TRIGGER audit_company_user_assigned_roles ON company_user_assigned_roles; ");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS process_audit_company_applications_audit(); DROP TRIGGER audit_company_applications ON company_applications;");

            migrationBuilder.DropTable(
                name: "audit_company_applications_cplp_1255_audit_company_applications",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_company_user_assigned_roles_cplp_1255_audit_company_applications",
                schema: "portal");

            migrationBuilder.DropColumn(
                name: "id",
                schema: "portal",
                table: "company_user_assigned_roles");

            migrationBuilder.DropColumn(
                name: "last_editor_id",
                schema: "portal",
                table: "company_user_assigned_roles");

            migrationBuilder.DropColumn(
                name: "last_editor_id",
                schema: "portal",
                table: "company_applications");
        }
    }
}
