using System.Text;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Auditing;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.Migrations.Extensions;

/// <summary>
/// Extension methods for the migration builder
/// </summary>
public static class MigrationBuilderExtensions
{
    public static void CreateAuditTriggerOnCompanyUser(this MigrationBuilder migrationBuilder)
    {
        var snakeCaseStrategy = new SnakeCaseNamingStrategy();
        var auditEntities = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
            .Where(x => typeof(IAuditEntity).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
            .Select(x => x).ToList();

        var sb = new StringBuilder();
        foreach (var auditEntity in auditEntities)
        {
            var properties = auditEntity.GetProperties()
                .Where(x => !x.GetAccessors().Any(a => a.IsVirtual))
                .Select(x => snakeCaseStrategy.GetPropertyName(x.Name, false))
                .ToList();
            var auditTableName = $"{snakeCaseStrategy.GetPropertyName(auditEntity.Name, false)}s";
            var tableName = auditTableName.Replace("audit_", string.Empty);
            sb.AppendLine($"CREATE OR REPLACE FUNCTION process_{tableName}_audit() RETURNS TRIGGER AS ${auditTableName}$");
            sb.AppendLine("BEGIN");
            sb.AppendLine("IF (TG_OP = 'DELETE') THEN");
            sb.AppendLine(GenerateInsertStatement(properties, snakeCaseStrategy, auditTableName, AuditOperationId.DELETE));
            sb.AppendLine("ELSIF (TG_OP = 'UPDATE') THEN");
            sb.AppendLine(GenerateInsertStatement(properties, snakeCaseStrategy, auditTableName, AuditOperationId.UPDATE));
            sb.AppendLine($"ELSIF (TG_OP = 'INSERT') THEN");
            sb.AppendLine(GenerateInsertStatement(properties, snakeCaseStrategy, auditTableName, AuditOperationId.INSERT));
            sb.AppendLine("END IF;");
            sb.AppendLine("RETURN NULL;");
            sb.AppendLine("END;");
            sb.AppendLine($"${auditTableName}$ LANGUAGE plpgsql;");
            sb.AppendLine($"CREATE TRIGGER {auditTableName}");
            sb.AppendLine($"AFTER INSERT OR UPDATE OR DELETE ON {tableName}");
            sb.AppendLine($"FOR EACH ROW EXECUTE FUNCTION process_{tableName}_audit();");
        }

        migrationBuilder.Sql(sb.ToString());
    }

    private static string GenerateInsertStatement(
        IReadOnlyCollection<string> properties,
        NamingStrategy namingStrategy,
        string auditTableName, 
        AuditOperationId operation)
    {
        var prefix = operation switch
        {
            AuditOperationId.INSERT => "NEW",
            AuditOperationId.UPDATE => "NEW",
            AuditOperationId.DELETE => "OLD",
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };

        return
            $"INSERT INTO {auditTableName} ({nameof(IAuditable.Id).ToLower()}, {namingStrategy.GetPropertyName(nameof(IAuditEntity.AuditId), false).ToLower()}, {string.Join(",", properties)}, {namingStrategy.GetPropertyName(nameof(IAuditable.LastEditorId), false)}, {namingStrategy.GetPropertyName(nameof(IAuditable.DateLastChanged), false)}, {nameof(IAuditEntity.AuditOperationId)}) SELECT gen_random_uuid(), {prefix}.{nameof(IAuditable.Id).ToLower()}, {string.Join(",", properties.Select(x => $"{prefix}.{x}"))}, {prefix}.{namingStrategy.GetPropertyName(nameof(IAuditable.LastEditorId), false)}, getdate(), {(int) operation}";
    }

    public static void DeleteAuditTriggerOnCompanyUser(this MigrationBuilder migrationBuilder)
    {
        var snakeCaseStrategy = new SnakeCaseNamingStrategy();
        var auditEntities = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
            .Where(x => typeof(IAuditEntity).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
            .Select(x => x).ToList();

        foreach (var auditEntity in auditEntities)
        {
            var auditTableName = $"{snakeCaseStrategy.GetPropertyName(auditEntity.Name, false)}s";
            var tableName = auditTableName.Replace("audit_", string.Empty);
            migrationBuilder.Sql($"DROP FUNCTION IF EXISTS process_{auditTableName}_audit();");
            migrationBuilder.Sql($"DROP TRIGGER {auditTableName} ON {tableName};");
        }
    }
}