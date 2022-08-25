using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatenaX.NetworkServices.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1378AddAppAssignedDocuments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "app_assigned_documents",
                schema: "portal",
                columns: table => new
                {
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_assigned_documents", x => new { x.app_id, x.document_id });
                    table.ForeignKey(
                        name: "fk_app_assigned_documents_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_app_assigned_documents_documents_document_id",
                        column: x => x.document_id,
                        principalSchema: "portal",
                        principalTable: "documents",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_app_assigned_documents_document_id",
                schema: "portal",
                table: "app_assigned_documents",
                column: "document_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_assigned_documents",
                schema: "portal");
        }
    }
}
