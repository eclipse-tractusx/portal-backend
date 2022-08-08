using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatenaX.NetworkServices.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1248appnotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "requester_id",
                schema: "portal",
                table: "company_assigned_apps",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"));

            migrationBuilder.AddColumn<Guid>(
                name: "sales_manager_id",
                schema: "portal",
                table: "apps",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"));

            migrationBuilder.AlterColumn<Guid>(
                name: "sales_manager_id",
                schema: "portal",
                table: "apps",
                type: "uuid",
                nullable: false);
            
            migrationBuilder.AlterColumn<Guid>(
                name: "requester_id",
                schema: "portal",
                table: "company_assigned_apps",
                type: "uuid",
                nullable: false);
            
            migrationBuilder.CreateIndex(
                name: "ix_apps_sales_manager_id",
                schema: "portal",
                table: "apps",
                column: "sales_manager_id");

            migrationBuilder.AddForeignKey(
                name: "fk_apps_company_users_sales_manager_id",
                schema: "portal",
                table: "apps",
                column: "sales_manager_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_apps_company_users_sales_manager_id",
                schema: "portal",
                table: "apps");

            migrationBuilder.DropIndex(
                name: "ix_apps_sales_manager_id",
                schema: "portal",
                table: "apps");

            migrationBuilder.DropColumn(
                name: "requester_id",
                schema: "portal",
                table: "company_assigned_apps");

            migrationBuilder.DropColumn(
                name: "sales_manager_id",
                schema: "portal",
                table: "apps");
        }
    }
}
