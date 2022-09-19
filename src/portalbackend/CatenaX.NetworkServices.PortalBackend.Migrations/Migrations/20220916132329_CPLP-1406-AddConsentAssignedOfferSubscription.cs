using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatenaX.NetworkServices.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1406AddConsentAssignedOfferSubscription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_offer_subscriptions_consents_consent_id",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.DropIndex(
                name: "ix_offer_subscriptions_consent_id",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.DropColumn(
                name: "consent_id",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.CreateTable(
                name: "consent_assigned_offer_subscriptions",
                schema: "portal",
                columns: table => new
                {
                    offer_subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consent_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consent_assigned_offer_subscriptions", x => new { x.consent_id, x.offer_subscription_id });
                    table.ForeignKey(
                        name: "fk_consent_assigned_offer_subscriptions_consents_consent_id",
                        column: x => x.consent_id,
                        principalSchema: "portal",
                        principalTable: "consents",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_consent_assigned_offer_subscriptions_offer_subscriptions_of",
                        column: x => x.offer_subscription_id,
                        principalSchema: "portal",
                        principalTable: "offer_subscriptions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_consent_assigned_offer_subscriptions_offer_subscription_id",
                schema: "portal",
                table: "consent_assigned_offer_subscriptions",
                column: "offer_subscription_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "consent_assigned_offer_subscriptions",
                schema: "portal");

            migrationBuilder.AddColumn<Guid>(
                name: "consent_id",
                schema: "portal",
                table: "offer_subscriptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_offer_subscriptions_consent_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "consent_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_offer_subscriptions_consents_consent_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "consent_id",
                principalSchema: "portal",
                principalTable: "consents",
                principalColumn: "id");
        }
    }
}
