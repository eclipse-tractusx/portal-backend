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
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Base;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Auditing;

public static class EntityTypeBuilderV1Extension
{
    public static EntityTypeBuilder<TEntity> HasAuditV1Triggers<TEntity, TAuditEntity>(this EntityTypeBuilder<TEntity> builder) where TEntity : class, IAuditableV1 where TAuditEntity : class, IAuditEntityV1
    {
        var auditEntityAttribute = (AuditEntityV1Attribute?)Attribute.GetCustomAttribute(typeof(TEntity), typeof(AuditEntityV1Attribute));
        if (auditEntityAttribute == null)
        {
            throw new ConfigurationException($"{nameof(TEntity)} must be annotated with {nameof(AuditEntityV1Attribute)}");
        }

        var auditEntityType = auditEntityAttribute.AuditEntityType;
        if (auditEntityType.Name != typeof(TAuditEntity).Name)
        {
            throw new ConfigurationException($"{typeof(TEntity).Name} attribute {typeof(AuditEntityV1Attribute).Name} configured AuditEntityType {auditEntityType.Name} doesn't match {typeof(TAuditEntity).Name}");
        }

        var sourceProperties = new List<PropertyInfo>();
        if (typeof(IBaseEntity).IsAssignableFrom(typeof(TEntity)))
        {
            sourceProperties.AddRange(typeof(IBaseEntity).GetProperties());
        }
        sourceProperties.AddRange(typeof(TEntity).GetProperties(BindingFlags.Public |
                                                                BindingFlags.Instance |
                                                                BindingFlags.DeclaredOnly).Where(p => !(p.GetGetMethod()?.IsVirtual ?? false)));
        var auditProperties = typeof(IAuditEntityV1).GetProperties();
        var targetProperties = auditEntityType.GetProperties().ExceptBy(auditProperties.Select(x => x.Name), p => p.Name);

        var illegalProperties = sourceProperties.IntersectBy(auditProperties.Select(x => x.Name), p => p.Name);
        if (illegalProperties.Any())
        {
            throw new ConfigurationException($"{typeof(TEntity).Name} is must not declare any of the following properties: {string.Join(", ", illegalProperties.Select(x => x.Name))}");
        }

        var missingProperties = sourceProperties.ExceptBy(targetProperties.Select(x => x.Name), p => p.Name);
        if (missingProperties.Any())
        {
            throw new ArgumentException($"{auditEntityAttribute.AuditEntityType.Name} is missing the following properties: {string.Join(", ", missingProperties.Select(x => x.Name))}");
        }

        if (!Array.Exists(
            typeof(TAuditEntity).GetProperties(),
            p => p.Name == AuditPropertyV1Names.AuditV1Id.ToString() && p.CustomAttributes.Any(a => a.AttributeType == typeof(KeyAttribute))))
        {
            throw new ConfigurationException($"{typeof(TAuditEntity).Name}.{AuditPropertyV1Names.AuditV1Id} must be marked as primary key by attribute {typeof(KeyAttribute).Name}");
        }

        var insertEditorProperty = sourceProperties.SingleOrDefault(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(AuditInsertEditorV1Attribute)));
        var lastEditorProperty = sourceProperties.SingleOrDefault(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(AuditLastEditorV1Attribute)));

        return builder.AfterInsert(trigger => trigger
                            .Action(action => action
                                .Insert(CreateNewAuditEntityExpression<TEntity, TAuditEntity>(sourceProperties, insertEditorProperty ?? lastEditorProperty))))
                        .AfterUpdate(trigger => trigger
                            .Action(action => action
                                .Insert(CreateUpdateAuditEntityExpression<TEntity, TAuditEntity>(sourceProperties, lastEditorProperty))))
                        .AfterDelete(trigger => trigger
                            .Action(action => action
                                .Insert(CreateDeleteAuditEntityExpression<TEntity, TAuditEntity>(sourceProperties, lastEditorProperty))));
    }

    private static Expression<Func<TEntity, TAuditEntity>> CreateNewAuditEntityExpression<TEntity, TAuditEntity>(IEnumerable<PropertyInfo> sourceProperties, PropertyInfo? lastEditorProperty)
    {
        var newValue = Expression.Parameter(typeof(TEntity), "newEntity");

        return Expression.Lambda<Func<TEntity, TAuditEntity>>(
            CreateAuditEntityExpression<TAuditEntity>(sourceProperties, AuditOperationId.INSERT, newValue, lastEditorProperty),
            newValue);
    }

    private static Expression<Func<TEntity, TEntity, TAuditEntity>> CreateUpdateAuditEntityExpression<TEntity, TAuditEntity>(IEnumerable<PropertyInfo> sourceProperties, PropertyInfo? lastEditorProperty)
    {
        var oldEntity = Expression.Parameter(typeof(TEntity), "oldEntity");
        var newEntity = Expression.Parameter(typeof(TEntity), "newEntity");

        return Expression.Lambda<Func<TEntity, TEntity, TAuditEntity>>(
            CreateAuditEntityExpression<TAuditEntity>(sourceProperties, AuditOperationId.UPDATE, newEntity, lastEditorProperty),
            oldEntity,
            newEntity);
    }

    private static Expression<Func<TEntity, TAuditEntity>> CreateDeleteAuditEntityExpression<TEntity, TAuditEntity>(IEnumerable<PropertyInfo> sourceProperties, PropertyInfo? lastEditorProperty)
    {
        var deletedEntity = Expression.Parameter(typeof(TEntity), "deletedEntity");

        return Expression.Lambda<Func<TEntity, TAuditEntity>>(
            CreateAuditEntityExpression<TAuditEntity>(sourceProperties, AuditOperationId.DELETE, deletedEntity, lastEditorProperty),
            deletedEntity);
    }

    private static MemberInitExpression CreateAuditEntityExpression<TAuditEntity>(IEnumerable<PropertyInfo> sourceProperties, AuditOperationId auditOperationId, ParameterExpression entity, PropertyInfo? lastEditorProperty)
    {
        var memberBindings = sourceProperties.Select(p => CreateMemberAssignment(typeof(TAuditEntity).GetMember(p.Name)[0], Expression.Property(entity, p)))
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
