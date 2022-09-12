using System;
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agreement_assigned_offers",
                schema: "portal");

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
