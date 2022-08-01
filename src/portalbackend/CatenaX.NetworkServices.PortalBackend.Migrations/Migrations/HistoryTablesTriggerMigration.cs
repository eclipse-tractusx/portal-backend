using CatenaX.NetworkServices.PortalBackend.Migrations.Extensions;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CatenaX.NetworkServices.PortalBackend.Migrations.Migrations;

[DbContext(typeof(PortalDbContext))]
[Migration("99999999999999_ManualAddAuditTrigger")]
public class HistoryTablesTriggerMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateAuditTriggerOnCompanyUser();
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteAuditTriggerOnCompanyUser();
    }
}