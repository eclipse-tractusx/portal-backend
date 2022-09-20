﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatenaX.NetworkServices.PortalBackend.Migrations.Migrations
{
    public partial class CPLP831AddConsentAssignedOffer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agreement_assigned_offer_types",
                schema: "portal",
                columns: table => new
                {
                    agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_type_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agreement_assigned_offer_types", x => new { x.agreement_id, x.offer_type_id });
                    table.ForeignKey(
                        name: "fk_agreement_assigned_offer_types_agreements_agreement_id",
                        column: x => x.agreement_id,
                        principalSchema: "portal",
                        principalTable: "agreements",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_agreement_assigned_offer_types_offer_types_offer_type_id",
                        column: x => x.offer_type_id,
                        principalSchema: "portal",
                        principalTable: "offer_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "consent_assigned_offers",
                schema: "portal",
                columns: table => new
                {
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consent_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consent_assigned_offers", x => new { x.consent_id, x.offer_id });
                    table.ForeignKey(
                        name: "fk_consent_assigned_offers_consents_consent_id",
                        column: x => x.consent_id,
                        principalSchema: "portal",
                        principalTable: "consents",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_consent_assigned_offers_offers_offer_id",
                        column: x => x.offer_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_agreement_assigned_offer_types_offer_type_id",
                schema: "portal",
                table: "agreement_assigned_offer_types",
                column: "offer_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_consent_assigned_offers_offer_id",
                schema: "portal",
                table: "consent_assigned_offers",
                column: "offer_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agreement_assigned_offer_types",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "consent_assigned_offers",
                schema: "portal");
        }
    }
}
