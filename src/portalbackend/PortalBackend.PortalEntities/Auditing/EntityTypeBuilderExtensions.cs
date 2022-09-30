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
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Auditing;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.AuditEntities;

public static class EntityTypeBuilderExtensions
{
    public static string GetSql<TEntity>(this EntityTypeBuilder<TEntity> entity, TriggerEvent triggerEvent)
        where TEntity : class, IAuditableV1
    {
        var auditAttr = (AuditEntityV1Attribute?) Attribute.GetCustomAttribute(entity.Metadata.ClrType, typeof(AuditEntityV1Attribute));
        if (auditAttr is null)
        {
            throw new ConfigurationException("The given Entity has no Auditable Entity");
        }

        return auditAttr.GetAuditSql(entity, triggerEvent);
    }

    /// <summary>
    /// Get Column Name for the given property
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="propertyName"></param>
    /// <param name="tableName"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public static string GetColumnName<TEntity>(this EntityTypeBuilder<TEntity> entity, string propertyName, string tableName)
        where TEntity : class =>
        entity.Metadata.GetProperty(propertyName)
            .GetColumnName(StoreObjectIdentifier.Table(tableName, entity.Metadata.GetSchema())) ??
        entity.Metadata.GetProperty(propertyName).Name;
}