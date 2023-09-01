/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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

using Laraue.EfCoreTriggers.Common.Extensions;
using Laraue.EfCoreTriggers.Common.TriggerBuilders.TableRefs;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Auditing;

public static class EntityTypeBuilderV1Extension
{
    public static EntityTypeBuilder<TEntity> HasAuditV1Triggers<TEntity, TAuditEntity>(this EntityTypeBuilder<TEntity> builder) where TEntity : class, IAuditableV1 where TAuditEntity : class, IAuditEntityV1
    {
        var (auditEntityType, sourceProperties, auditProperties, targetProperties) = typeof(TEntity).GetAuditPropertyInformation() ?? throw new ConfigurationException($"{typeof(TEntity)} must be annotated with {nameof(AuditEntityV1Attribute)}");
        if (typeof(TAuditEntity) != auditEntityType)
        {
            throw new ConfigurationException($"{typeof(TEntity).Name} is annotated with {nameof(AuditEntityV1Attribute)} referring to a different audit entity type {auditEntityType.Name} then {typeof(TAuditEntity).Name}");
        }

        var illegalProperties = sourceProperties.IntersectBy(auditProperties.Select(x => x.Name), p => p.Name);
        illegalProperties.IfAny(
            illegal => throw new ConfigurationException($"{typeof(TEntity).Name} is must not declare any of the following properties: {string.Join(", ", illegal.Select(x => x.Name))}"));

        var missingProperties = sourceProperties.ExceptBy(targetProperties.Select(x => x.Name), p => p.Name);
        missingProperties.IfAny(
            missing => throw new ArgumentException($"{typeof(TAuditEntity).Name} is missing the following properties: {string.Join(", ", missing.Select(x => x.Name))}"));

        if (!Array.Exists(
            typeof(TAuditEntity).GetProperties(),
            p => p.Name == AuditPropertyV1Names.AuditV1Id.ToString() && p.CustomAttributes.Any(a => a.AttributeType == typeof(KeyAttribute))))
        {
            throw new ConfigurationException($"{typeof(TAuditEntity).Name}.{AuditPropertyV1Names.AuditV1Id} must be marked as primary key by attribute {typeof(KeyAttribute).Name}");
        }

        var insertEditorProperty = sourceProperties.SingleOrDefault(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(AuditInsertEditorV1Attribute)));
        var lastEditorProperty = sourceProperties.SingleOrDefault(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(LastEditorV1Attribute)));

        return builder
            .AfterInsert(trigger => trigger
            .Action(action => action
                .Insert(CreateNewAuditEntityExpression<TEntity, TAuditEntity>(sourceProperties, insertEditorProperty ?? lastEditorProperty))))
            .AfterUpdate(trigger => trigger
                .Action(action => action
                    .Insert(CreateUpdateAuditEntityExpression<TEntity, TAuditEntity>(sourceProperties, lastEditorProperty))));
    }

    private static Expression<Func<NewTableRef<TEntity>, TAuditEntity>> CreateNewAuditEntityExpression<TEntity, TAuditEntity>(IEnumerable<PropertyInfo> sourceProperties, PropertyInfo? lastEditorProperty) where TEntity : class
    {
        var entity = Expression.Parameter(typeof(NewTableRef<TEntity>), "entity");

        var newPropertyInfo = typeof(NewTableRef<TEntity>).GetProperty("New");
        if (newPropertyInfo == null)
        {
            throw new UnexpectedConditionException($"{nameof(NewTableRef<TEntity>)} must have property New");
        }

        var propertyExpression = Expression.Property(entity, newPropertyInfo);
        return Expression.Lambda<Func<NewTableRef<TEntity>, TAuditEntity>>(
            CreateAuditEntityExpression<TAuditEntity>(sourceProperties, AuditOperationId.INSERT, propertyExpression, lastEditorProperty),
            entity);
    }

    private static Expression<Func<OldAndNewTableRefs<TEntity>, TAuditEntity>> CreateUpdateAuditEntityExpression<TEntity, TAuditEntity>(IEnumerable<PropertyInfo> sourceProperties, PropertyInfo? lastEditorProperty) where TEntity : class
    {
        var entity = Expression.Parameter(typeof(OldAndNewTableRefs<TEntity>), "entity");

        var newPropertyInfo = typeof(OldAndNewTableRefs<TEntity>).GetProperty("New");
        if (newPropertyInfo == null)
        {
            throw new UnexpectedConditionException($"{nameof(OldAndNewTableRefs<TEntity>)} must have property New");
        }

        var propertyExpression = Expression.Property(entity, newPropertyInfo);
        return Expression.Lambda<Func<OldAndNewTableRefs<TEntity>, TAuditEntity>>(
            CreateAuditEntityExpression<TAuditEntity>(sourceProperties, AuditOperationId.UPDATE, propertyExpression, lastEditorProperty),
            entity);
    }

    private static MemberInitExpression CreateAuditEntityExpression<TAuditEntity>(IEnumerable<PropertyInfo> sourceProperties, AuditOperationId auditOperationId, Expression entity, PropertyInfo? lastEditorProperty)
    {
        var memberBindings = sourceProperties.Select(p =>
                CreateMemberAssignment(typeof(TAuditEntity).GetMember(p.Name)[0], Expression.Property(entity, p)))
                    .Append(CreateMemberAssignment(typeof(TAuditEntity).GetMember(AuditPropertyV1Names.AuditV1Id.ToString())[0], Expression.New(typeof(Guid))))
                    .Append(CreateMemberAssignment(typeof(TAuditEntity).GetMember(AuditPropertyV1Names.AuditV1OperationId.ToString())[0], Expression.Constant(auditOperationId)))
                    .Append(CreateMemberAssignment(typeof(TAuditEntity).GetMember(AuditPropertyV1Names.AuditV1DateLastChanged.ToString())[0], Expression.New(typeof(DateTimeOffset))));

        if (lastEditorProperty != null)
        {
            memberBindings = memberBindings.Append(CreateMemberAssignment(typeof(TAuditEntity).GetMember(AuditPropertyV1Names.AuditV1LastEditorId.ToString())[0], Expression.Property(entity, lastEditorProperty)));
        }

        return Expression.MemberInit(
            Expression.New(typeof(TAuditEntity)),
            memberBindings);
    }

    private static MemberAssignment CreateMemberAssignment(MemberInfo member, Expression expression)
    {
        try
        {
            return Expression.Bind(member, expression);
        }
        catch (Exception e)
        {
            throw new ArgumentException($"{member.DeclaringType?.Name}.{member.Name} is not assignable from {expression}, {e.Message}", e);
        }
    }
}
