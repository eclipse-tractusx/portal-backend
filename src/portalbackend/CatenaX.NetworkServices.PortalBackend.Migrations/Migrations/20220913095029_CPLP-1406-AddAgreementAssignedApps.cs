﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatenaX.NetworkServices.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1406AddAgreementAssignedApps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_agreements_offers_offer_id",
                schema: "portal",
                table: "agreements");

            migrationBuilder.DropIndex(
                name: "ix_agreements_offer_id",
                schema: "portal",
                table: "agreements");

            migrationBuilder.DropColumn(
                name: "offer_id",
                schema: "portal",
                table: "agreements");

            migrationBuilder.AddColumn<Guid>(
                name: "consent_id",
                schema: "portal",
                table: "offer_subscriptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "agreement_assigned_offers",
                schema: "portal",
                columns: table => new
                {
                    agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agreement_assigned_offers", x => new { x.agreement_id, x.offer_id });
                    table.ForeignKey(
                        name: "fk_agreement_assigned_offers_agreements_agreement_id",
                        column: x => x.agreement_id,
                        principalSchema: "portal",
                        principalTable: "agreements",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_agreement_assigned_offers_offers_offer_id",
                        column: x => x.offer_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "agreement_categories",
                columns: new[] { "id", "label" },
                values: new object[] { 4, "SERVICE_CONTRACT" });

            migrationBuilder.CreateIndex(
                name: "ix_offer_subscriptions_consent_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "consent_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_agreement_assigned_offers_offer_id",
                schema: "portal",
                table: "agreement_assigned_offers",
                column: "offer_id");

            migrationBuilder.AddForeignKey(
                name: "fk_offer_subscriptions_consents_consent_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "consent_id",
                principalSchema: "portal",
                principalTable: "consents",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_offer_subscriptions_consents_consent_id",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.DropTable(
                name: "agreement_assigned_offers",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_offer_subscriptions_consent_id",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "agreement_categories",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "consent_id",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.AddColumn<Guid>(
                name: "offer_id",
                schema: "portal",
                table: "agreements",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_agreements_offer_id",
                schema: "portal",
                table: "agreements",
                column: "offer_id");

            migrationBuilder.AddForeignKey(
                name: "fk_agreements_offers_offer_id",
                schema: "portal",
                table: "agreements",
                column: "offer_id",
                principalSchema: "portal",
                principalTable: "offers",
                principalColumn: "id");
        }
    }
}
