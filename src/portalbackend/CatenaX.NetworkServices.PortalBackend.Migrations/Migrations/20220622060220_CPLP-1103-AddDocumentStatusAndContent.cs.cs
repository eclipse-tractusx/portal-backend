﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatenaX.NetworkServices.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1103AddDocumentStatusAndContentcs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "document",
                schema: "portal",
                table: "documents");

            migrationBuilder.RenameColumn(
                name: "documentname",
                schema: "portal",
                table: "documents",
                newName: "document_name");

            migrationBuilder.RenameColumn(
                name: "documenthash",
                schema: "portal",
                table: "documents",
                newName: "document_hash");

            migrationBuilder.Sql("ALTER TABLE portal.documents ALTER COLUMN document_hash TYPE bytea USING document_hash::TEXT::BYTEA");

            migrationBuilder.AddColumn<byte[]>(
                name: "document_content",
                schema: "portal",
                table: "documents",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<int>(
                name: "document_status_id",
                schema: "portal",
                table: "documents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "document_status",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_status", x => x.id);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "document_status",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "PENDING" },
                    { 2, "LOCKED" },
                    { 3, "INACTIVE" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_documents_document_status_id",
                schema: "portal",
                table: "documents",
                column: "document_status_id");

            migrationBuilder.Sql("UPDATE portal.documents SET document_status_id = 2");

            migrationBuilder.AddForeignKey(
                name: "fk_documents_document_status_document_status_id",
                schema: "portal",
                table: "documents",
                column: "document_status_id",
                principalSchema: "portal",
                principalTable: "document_status",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_documents_document_status_document_status_id",
                schema: "portal",
                table: "documents");

            migrationBuilder.DropTable(
                name: "document_status",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_documents_document_status_id",
                schema: "portal",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "document_content",
                schema: "portal",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "document_status_id",
                schema: "portal",
                table: "documents");

            migrationBuilder.RenameColumn(
                name: "document_name",
                schema: "portal",
                table: "documents",
                newName: "documentname");

            migrationBuilder.RenameColumn(
                name: "document_hash",
                schema: "portal",
                table: "documents",
                newName: "documenthash");

            migrationBuilder.AlterColumn<string>(
                name: "documenthash",
                schema: "portal",
                table: "documents",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "bytea");

            migrationBuilder.AddColumn<uint>(
                name: "document",
                schema: "portal",
                table: "documents",
                type: "oid",
                nullable: false,
                defaultValue: 0u);
        }
    }
}
