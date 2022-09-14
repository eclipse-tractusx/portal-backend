using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatenaX.NetworkServices.PortalBackend.Migrations.Migrations
{
    public partial class CPLP831AddConsentAssignedOffer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "ix_consent_assigned_offers_offer_id",
                schema: "portal",
                table: "consent_assigned_offers",
                column: "offer_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "consent_assigned_offers",
                schema: "portal");
        }
    }
}
