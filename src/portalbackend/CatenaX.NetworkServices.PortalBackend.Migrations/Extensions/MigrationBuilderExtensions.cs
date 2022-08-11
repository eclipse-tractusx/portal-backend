using System.Text;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Auditing;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json.Serialization;

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
/// Extension methods for the migration builder
/// </summary>
public static class MigrationBuilderExtensions
{
    public static void AddAuditTrigger<TEntity>(this MigrationBuilder migrationBuilder, string version)
        where TEntity : class, IAuditEntity
    {
        var snakeCaseStrategy = new SnakeCaseNamingStrategy();
        var sb = new StringBuilder();
        var properties = typeof(TEntity).GetProperties()
            .Where(x => !x.GetAccessors().Any(a => a.IsVirtual) && x.Name != nameof(IAuditEntity.DateLastChanged))
            .Select(x => snakeCaseStrategy.GetPropertyName(x.Name, false))
            .ToList();
        var auditTableName = $"{snakeCaseStrategy.GetPropertyName(typeof(TEntity).Name, false)}s";
        var tableName = auditTableName.Replace("audit_", string.Empty);
        sb.AppendLine($"CREATE OR REPLACE FUNCTION portal.process_{tableName}_audit() RETURNS TRIGGER AS ${auditTableName}$");
        sb.AppendLine("BEGIN");
        sb.AppendLine("IF (TG_OP = 'DELETE') THEN");
        sb.AppendLine(GenerateInsertStatement(properties, snakeCaseStrategy, auditTableName, AuditOperationId.DELETE, version));
        sb.AppendLine("ELSIF (TG_OP = 'UPDATE') THEN");
        sb.AppendLine(GenerateInsertStatement(properties, snakeCaseStrategy, auditTableName, AuditOperationId.UPDATE, version));
        sb.AppendLine($"ELSIF (TG_OP = 'INSERT') THEN");
        sb.AppendLine(GenerateInsertStatement(properties, snakeCaseStrategy, auditTableName, AuditOperationId.INSERT, version));
        sb.AppendLine("END IF;");
        sb.AppendLine("RETURN NULL;");
        sb.AppendLine("END;");
        sb.AppendLine($"${auditTableName}$ LANGUAGE plpgsql;");
        sb.AppendLine($"CREATE TRIGGER {auditTableName}");
        sb.AppendLine($"AFTER INSERT OR UPDATE OR DELETE ON portal.{tableName}");
        sb.AppendLine($"FOR EACH ROW EXECUTE FUNCTION portal.process_{tableName}_audit();");

        migrationBuilder.Sql(sb.ToString());
    }

    public static void DropAuditTrigger<TEntity>(this MigrationBuilder migrationBuilder)
        where TEntity : class, IAuditEntity
    {
        var snakeCaseStrategy = new SnakeCaseNamingStrategy();
        var auditTableName = $"{snakeCaseStrategy.GetPropertyName(typeof(TEntity).Name, false)}s";
        var tableName = auditTableName.Replace("audit_", string.Empty);
        migrationBuilder.Sql($"DROP FUNCTION IF EXISTS process_{auditTableName}_audit();");
        migrationBuilder.Sql($"DROP TRIGGER {auditTableName} ON {tableName};");
    }

    private static string GenerateInsertStatement(
        IReadOnlyCollection<string> properties,
        NamingStrategy namingStrategy,
        string auditTableName, 
        AuditOperationId operation,
        string version)
    {
        var prefix = operation switch
        {
            AuditOperationId.INSERT => "NEW",
            AuditOperationId.UPDATE => "NEW",
            AuditOperationId.DELETE => "OLD",
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };

        return $"INSERT INTO portal.{auditTableName}_{version} ( " +
               $"{nameof(IAuditable.Id).ToLower()}, " +
               $"{namingStrategy.GetPropertyName(nameof(IAuditEntity.AuditId), false).ToLower()}, " +
               $"{string.Join(",", properties)}, " +
               $"{namingStrategy.GetPropertyName(nameof(IAuditable.LastEditorId), false)}, " +
               $"{namingStrategy.GetPropertyName(nameof(IAuditEntity.DateLastChanged), false)}, " +
               $"{namingStrategy.GetPropertyName(nameof(IAuditEntity.AuditOperationId), false)} ) " +
               "SELECT " +
               "gen_random_uuid(), " +
               $"{prefix}.{nameof(IAuditable.Id).ToLower()}, " +
               $"{string.Join(",", properties.Select(x => $"{prefix}.{x}"))}, " +
               $"{prefix}.{namingStrategy.GetPropertyName(nameof(IAuditable.LastEditorId), false)}, " +
               "CURRENT_DATE, " +
               $"{(int) operation} ;";
    }
}