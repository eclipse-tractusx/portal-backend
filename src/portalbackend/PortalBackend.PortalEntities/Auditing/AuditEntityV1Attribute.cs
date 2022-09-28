/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using Laraue.EfCoreTriggers.Common.TriggerBuilders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json.Serialization;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.AuditEntities;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Auditing;

/// <summary>
/// Attribute to Provide the needed methods to setup an audit trigger
/// </summary>
/// <remarks>
/// The implementation of this Attribute must not be changed.
/// When changes are needed create a V2 of it.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class AuditEntityV1Attribute : Attribute
{
    private readonly Type _auditEntityType;
    private const string _prefix = "NEW.";

    public AuditEntityV1Attribute(Type auditEntityType)
    {
        if (!typeof(IAuditEntity).IsAssignableFrom(auditEntityType))
        {
            throw new ArgumentException($"Entity must derive from {nameof(IAuditEntity)}", nameof(auditEntityType));
        }

        _auditEntityType = auditEntityType;
    }
    
    public string GetAuditSql<TEntity>(EntityTypeBuilder<TEntity> entity, TriggerEvent triggerEvent)
        where TEntity : class
    {
        var snakeCaseStrategy = new SnakeCaseNamingStrategy();
        var auditTableName = snakeCaseStrategy.GetPropertyName(_auditEntityType.Name, false);
        var propertiesToExcludeNames = typeof(IAuditable).GetProperties().Select(x => x.Name)
            .Concat(typeof(IAuditEntity).GetProperties().Select(x => x.Name))
            .ToArray();
        var tableName = entity.Metadata.GetTableName() ?? snakeCaseStrategy.GetPropertyName(entity.Metadata.Name, false);
        var baseEntityProperties = entity.Metadata.GetProperties().Where(x => !propertiesToExcludeNames.Contains(x.Name)).Select(x => x.GetColumnName(StoreObjectIdentifier.Table(tableName, entity.Metadata.GetSchema()))).ToList();
        var properties = string.Join(",", baseEntityProperties);
        var propertiesWithPrefix = string.Join(",", baseEntityProperties.Select(x => $"{_prefix}{x}"));

        return $"INSERT INTO portal.{auditTableName} ( " +
               $"{snakeCaseStrategy.GetPropertyName(nameof(IAuditEntity.AuditId), false).ToLower()}, " +
               $"{string.Join(",", properties)}, " +
               $"{snakeCaseStrategy.GetPropertyName(nameof(IAuditable.LastEditorId), false)}, " +
               $"{snakeCaseStrategy.GetPropertyName(nameof(IAuditEntity.DateLastChanged), false)}, " +
               $"{snakeCaseStrategy.GetPropertyName(nameof(IAuditEntity.AuditOperationId), false)} ) " +
               "SELECT " +
               "gen_random_uuid(), " +
               $"{string.Join(",", propertiesWithPrefix)}, " +
               $"{_prefix}{entity.GetColumnName(nameof(IAuditable.LastEditorId), tableName)}, " +
               "CURRENT_DATE, " +
               $"{(int)triggerEvent.GetOperationForTriggerEvent()} ;";
    }
}