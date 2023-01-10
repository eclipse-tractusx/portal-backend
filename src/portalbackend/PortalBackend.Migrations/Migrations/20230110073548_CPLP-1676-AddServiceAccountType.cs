using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1676AddServiceAccountType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_company_service_accounts_companies_company_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.RenameColumn(
                name: "company_id",
                schema: "portal",
                table: "company_service_accounts",
                newName: "service_account_owner_id");

            migrationBuilder.RenameIndex(
                name: "ix_company_service_accounts_company_id",
                schema: "portal",
                table: "company_service_accounts",
                newName: "ix_company_service_accounts_service_account_owner_id");

            migrationBuilder.AddColumn<int>(
                name: "company_service_account_type_id",
                schema: "portal",
                table: "company_service_accounts",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "offer_subscription_id",
                schema: "portal",
                table: "company_service_accounts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "company_service_account_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_service_account_types", x => x.id);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_service_account_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "MANAGED" },
                    { 2, "OWN" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_company_service_account_type_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "company_service_account_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_offer_subscription_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "offer_subscription_id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_service_accounts_companies_service_account_owner_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "service_account_owner_id",
                principalSchema: "portal",
                principalTable: "companies",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_service_accounts_company_service_account_types_comp",
                schema: "portal",
                table: "company_service_accounts",
                column: "company_service_account_type_id",
                principalSchema: "portal",
                principalTable: "company_service_account_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_company_service_accounts_offer_subscriptions_offer_subscrip",
                schema: "portal",
                table: "company_service_accounts",
                column: "offer_subscription_id",
                principalSchema: "portal",
                principalTable: "offer_subscriptions",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_company_service_accounts_companies_service_account_owner_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_company_service_accounts_company_service_account_types_comp",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_company_service_accounts_offer_subscriptions_offer_subscrip",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropTable(
                name: "company_service_account_types",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_company_service_accounts_company_service_account_type_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropIndex(
                name: "ix_company_service_accounts_offer_subscription_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropColumn(
                name: "company_service_account_type_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropColumn(
                name: "offer_subscription_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.RenameColumn(
                name: "service_account_owner_id",
                schema: "portal",
                table: "company_service_accounts",
                newName: "company_id");

            migrationBuilder.RenameIndex(
                name: "ix_company_service_accounts_service_account_owner_id",
                schema: "portal",
                table: "company_service_accounts",
                newName: "ix_company_service_accounts_company_id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_service_accounts_companies_company_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "company_id",
                principalSchema: "portal",
                principalTable: "companies",
                principalColumn: "id");
        }
    }
}
