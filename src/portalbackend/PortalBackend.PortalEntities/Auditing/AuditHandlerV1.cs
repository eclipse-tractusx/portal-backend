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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Base;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using System.Reflection;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Auditing;

public class AuditHandlerV1 : IAuditHandler
{
    private readonly IIdentityService _identityService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AuditHandlerV1(IIdentityService identityService, IDateTimeProvider dateTimeProvider)
    {
        _identityService = identityService;
        _dateTimeProvider = dateTimeProvider;
    }

    public void HandleAuditForChangedEntries(IEnumerable<EntityEntry> changedEntries, DbContext context)
    {
        foreach (var entry in changedEntries)
        {
            // Set LastEditor
            foreach (var prop in entry.Properties.IntersectBy(
                                                 entry.Entity.GetType().GetProperties()
                                                     .Where(x => Attribute.IsDefined(x, typeof(AuditLastEditorV1Attribute)))
                                                     .Select(x => x.Name),
                                                 property => property.Metadata.Name))
            {
                prop.CurrentValue = _identityService.IdentityData.UserId;
            }

            // If existing try to set DateLastChanged
            foreach (var prop in entry.Properties.Where(x => x.Metadata.Name == "DateLastChanged"))
            {
                prop.CurrentValue = _dateTimeProvider.OffsetNow;
            }

            AddAuditEntry(entry, entry.Metadata.ClrType, context);
        }
    }

    private void AddAuditEntry(EntityEntry entityEntry, Type entityType, DbContext context)
    {
        var auditEntityAttribute = (AuditEntityV1Attribute?)Attribute.GetCustomAttribute(entityType, typeof(AuditEntityV1Attribute));
        if (auditEntityAttribute == null)
        {
            throw new ConfigurationException($"{entityType} must be annotated with {nameof(AuditEntityV1Attribute)}");
        }

        var auditEntityType = auditEntityAttribute.AuditEntityType;
        var sourceProperties = new List<PropertyInfo>();
        if (typeof(IBaseEntity).IsAssignableFrom(entityType))
        {
            sourceProperties.AddRange(typeof(IBaseEntity).GetProperties());
        }
        sourceProperties.AddRange(entityType.GetProperties(BindingFlags.Public |
                                                           BindingFlags.Instance |
                                                           BindingFlags.DeclaredOnly).Where(p => !(p.GetGetMethod()?.IsVirtual ?? false)));
        var auditProperties = typeof(IAuditEntityV1).GetProperties();
        var targetProperties = auditEntityType.GetProperties().ExceptBy(auditProperties.Select(x => x.Name), p => p.Name);

        if (Activator.CreateInstance(auditEntityType) is not IAuditEntityV1 newAuditEntity)
            return;

        foreach (var targetProperty in targetProperties)
        {
            var sourceProperty = sourceProperties.FirstOrDefault(p => p.Name == targetProperty.Name);
            if (sourceProperty == null)
                continue;

            var sourceValue = sourceProperty.GetValue(entityEntry.Entity);
            targetProperty.SetValue(newAuditEntity, sourceValue);
        }

        newAuditEntity.AuditV1Id = Guid.NewGuid();
        newAuditEntity.AuditV1OperationId = entityEntry.State.ToAuditOperation();
        newAuditEntity.AuditV1DateLastChanged = _dateTimeProvider.OffsetNow;
        newAuditEntity.AuditV1LastEditorId = _identityService.IdentityData.UserId;

        context.Add(newAuditEntity);
    }
}
