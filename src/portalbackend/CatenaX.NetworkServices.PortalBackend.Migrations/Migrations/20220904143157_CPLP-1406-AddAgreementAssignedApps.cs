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
                name: "fk_agreements_apps_app_id",
                schema: "portal",
                table: "agreements");

            migrationBuilder.DropForeignKey(
                name: "fk_apps_app_type_app_type_id",
                schema: "portal",
                table: "apps");

            migrationBuilder.DropIndex(
                name: "ix_agreements_app_id",
                schema: "portal",
                table: "agreements");

            migrationBuilder.DropColumn(
                name: "app_id",
                schema: "portal",
                table: "agreements");

            migrationBuilder.AlterColumn<int>(
                name: "app_type_id",
                schema: "portal",
                table: "apps",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "agreement_assigned_apps",
                schema: "portal",
                columns: table => new
                {
                    agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agreement_assigned_apps", x => new { x.agreement_id, x.app_id });
                    table.ForeignKey(
                        name: "fk_agreement_assigned_apps_agreements_agreement_id",
                        column: x => x.agreement_id,
                        principalSchema: "portal",
                        principalTable: "agreements",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_agreement_assigned_apps_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_agreement_assigned_apps_app_id",
                schema: "portal",
                table: "agreement_assigned_apps",
                column: "app_id");

            migrationBuilder.AddForeignKey(
                name: "fk_apps_app_type_app_type_id",
                schema: "portal",
                table: "apps",
                column: "app_type_id",
                principalSchema: "portal",
                principalTable: "app_type",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_apps_app_type_app_type_id",
                schema: "portal",
                table: "apps");

            migrationBuilder.DropTable(
                name: "agreement_assigned_apps",
                schema: "portal");

            migrationBuilder.AlterColumn<int>(
                name: "app_type_id",
                schema: "portal",
                table: "apps",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "app_id",
                schema: "portal",
                table: "agreements",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_agreements_app_id",
                schema: "portal",
                table: "agreements",
                column: "app_id");

            migrationBuilder.AddForeignKey(
                name: "fk_agreements_apps_app_id",
                schema: "portal",
                table: "agreements",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "apps",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_apps_app_type_app_type_id",
                schema: "portal",
                table: "apps",
                column: "app_type_id",
                principalSchema: "portal",
                principalTable: "app_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
