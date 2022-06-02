using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatenaX.NetworkServices.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1047BusinessPartners : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "bpn",
                schema: "portal",
                table: "companies",
                newName: "business_partner_number");

            migrationBuilder.CreateTable(
                name: "business_partners",
                schema: "portal",
                columns: table => new
                {
                    business_partner_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    parent_business_partner_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_business_partners", x => x.business_partner_number);
                    table.ForeignKey(
                        name: "fk_business_partners_business_partners_parent_business_partner",
                        column: x => x.parent_business_partner_number,
                        principalSchema: "portal",
                        principalTable: "business_partners",
                        principalColumn: "business_partner_number");
                });

            migrationBuilder.Sql("INSERT INTO portal.business_partners (business_partner_number, date_created) SELECT DISTINCT business_partner_number, date_created FROM portal.companies;");

            migrationBuilder.CreateTable(
                name: "company_user_assigned_business_partners",
                schema: "portal",
                columns: table => new
                {
                    business_partner_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_user_assigned_business_partners", x => new { x.business_partner_number, x.company_user_id });
                    table.ForeignKey(
                        name: "fk_company_user_assigned_business_partners_business_partners_b",
                        column: x => x.business_partner_number,
                        principalSchema: "portal",
                        principalTable: "business_partners",
                        principalColumn: "business_partner_number");
                    table.ForeignKey(
                        name: "fk_company_user_assigned_business_partners_company_users_compa",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_companies_business_partner_number",
                schema: "portal",
                table: "companies",
                column: "business_partner_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_business_partners_parent_business_partner_number",
                schema: "portal",
                table: "business_partners",
                column: "parent_business_partner_number");

            migrationBuilder.CreateIndex(
                name: "ix_company_user_assigned_business_partners_company_user_id",
                schema: "portal",
                table: "company_user_assigned_business_partners",
                column: "company_user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_companies_business_partners_business_partner_number",
                schema: "portal",
                table: "companies",
                column: "business_partner_number",
                principalSchema: "portal",
                principalTable: "business_partners",
                principalColumn: "business_partner_number");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_companies_business_partners_business_partner_number",
                schema: "portal",
                table: "companies");

            migrationBuilder.DropTable(
                name: "company_user_assigned_business_partners",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "business_partners",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_companies_business_partner_number",
                schema: "portal",
                table: "companies");

            migrationBuilder.RenameColumn(
                name: "business_partner_number",
                schema: "portal",
                table: "companies",
                newName: "bpn");
        }
    }
}
