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
    /// <inheritdoc />
    public partial class CPLP3165SetIdpOwnerMandatory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // set the owner of shared idps to the using company (if not ambigious)
            migrationBuilder.Sql(@"UPDATE portal.identity_providers AS ip
                SET owner_id = 
                    (
                        SELECT cip.company_id
                        FROM portal.company_identity_providers AS cip
                        WHERE cip.identity_provider_id = ip.id
                    )
                WHERE ip.identity_provider_type_id = 3
                AND (
                        SELECT COUNT(*)
                        FROM portal.company_identity_providers
                        WHERE identity_provider_id = ip.id
                    ) = 1;");

            // ensure any leftover unassigned idps (there shouln't be any but data might be inconsistent) will finally have a valid owner
            migrationBuilder.Sql(@"UPDATE portal.identity_providers
                SET owner_id =
                    (
                        SELECT c.id
                        FROM portal.companies AS c
                        JOIN portal.company_assigned_roles AS cr ON c.id = cr.company_id
                        WHERE cr.company_role_id = 4
                        LIMIT 1
                    )
                WHERE owner_id IS null;");

            // define trigger function for validation only a single company may be assigned to an identity-provider that is not managed.
            migrationBuilder.Sql(@"CREATE FUNCTION portal.tr_company_identity_providers_is_valid_identity_provider_type()
                RETURNS trigger
                VOLATILE
                COST 100
                AS $$
                DECLARE idp_type integer;
                BEGIN
                SELECT identity_provider_type_id
                    INTO idp_type
                    FROM portal.identity_providers AS ip
                    WHERE ip.id = NEW.identity_provider_id;
                IF idp_type = 2
                    OR
                    (
                        SELECT COUNT(*)
                        FROM portal.company_identity_providers AS cip
                        WHERE cip.identity_provider_id = NEW.identity_provider_id
                    ) = 1
                THEN RETURN NEW;
                END IF;
                RAISE EXCEPTION 'identity_provider % of type % is already assigned to a different company_id', NEW.identity_provider_id, idp_type;
                END
                $$ LANGUAGE plpgsql;");

            // create the constraint trigger based on the previously defined trigger function. To ensure it does not depend on the order of inserts it runs deferred at the end of the transaction.
            migrationBuilder.Sql(@"CREATE CONSTRAINT TRIGGER ct_company_identity_providers_is_valid_identity_provider_type
                AFTER INSERT
                ON portal.company_identity_providers
                INITIALLY DEFERRED
                FOR EACH ROW
                EXECUTE PROCEDURE portal.tr_company_identity_providers_is_valid_identity_provider_type();");

            migrationBuilder.AlterColumn<Guid>(
                name: "owner_id",
                schema: "portal",
                table: "identity_providers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER ct_company_identity_providers_is_valid_identity_provider_type ON portal.company_identity_providers;");
            migrationBuilder.Sql(@"DROP FUNCTION portal.tr_company_identity_providers_is_valid_identity_provider_type;");

            migrationBuilder.AlterColumn<Guid>(
                name: "owner_id",
                schema: "portal",
                table: "identity_providers",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }
    }
}
