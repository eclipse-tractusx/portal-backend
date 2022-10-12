using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1422AddSelfDescriptionDocuments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "self_description_document_id",
                schema: "portal",
                table: "connectors",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "self_description_document_id",
                schema: "portal",
                table: "companies",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_connectors_self_description_document_id",
                schema: "portal",
                table: "connectors",
                column: "self_description_document_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_companies_self_description_document_id",
                schema: "portal",
                table: "companies",
                column: "self_description_document_id");

            migrationBuilder.AddForeignKey(
                name: "fk_companies_documents_self_description_document_id",
                schema: "portal",
                table: "companies",
                column: "self_description_document_id",
                principalSchema: "portal",
                principalTable: "documents",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_connectors_documents_self_description_document_id",
                schema: "portal",
                table: "connectors",
                column: "self_description_document_id",
                principalSchema: "portal",
                principalTable: "documents",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_companies_documents_self_description_document_id",
                schema: "portal",
                table: "companies");

            migrationBuilder.DropForeignKey(
                name: "fk_connectors_documents_self_description_document_id",
                schema: "portal",
                table: "connectors");

            migrationBuilder.DropIndex(
                name: "ix_connectors_self_description_document_id",
                schema: "portal",
                table: "connectors");

            migrationBuilder.DropIndex(
                name: "ix_companies_self_description_document_id",
                schema: "portal",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "self_description_document_id",
                schema: "portal",
                table: "connectors");

            migrationBuilder.DropColumn(
                name: "self_description_document_id",
                schema: "portal",
                table: "companies");
        }
    }
}
