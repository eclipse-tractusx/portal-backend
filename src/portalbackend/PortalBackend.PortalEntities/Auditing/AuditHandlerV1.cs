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
        var now = _dateTimeProvider.OffsetNow;
        foreach (var groupedEntries in changedEntries
                     .GroupBy(entry => entry.Metadata.ClrType))
        {
            var lastEditorNames = groupedEntries.Key.GetProperties()
                .Where(x => Attribute.IsDefined(x, typeof(LastEditorV1Attribute)))
                .Select(x => x.Name)
                .Distinct()
                .ToHashSet();
            var lastChangedNames = groupedEntries.Key.GetProperties()
                .Where(x => Attribute.IsDefined(x, typeof(LastChangedV1Attribute)))
                .Select(x => x.Name)
                .Distinct()
                .ToHashSet();

            foreach (var entry in groupedEntries.Where(entry => entry.State != EntityState.Deleted))
            {
                foreach (var prop in entry.Properties.IntersectBy(
                             lastEditorNames,
                             property => property.Metadata.Name))
                {
                    prop.CurrentValue = _identityService.IdentityData.UserId;
                }

                foreach (var prop in entry.Properties.IntersectBy(
                             lastChangedNames,
                             property => property.Metadata.Name))
                {
                    prop.CurrentValue = now;
                }
            }

            if ((AuditEntityV1Attribute?)Attribute.GetCustomAttribute(groupedEntries.Key, typeof(AuditEntityV1Attribute)) == null)
                continue;

            var (auditEntityType, sourceProperties, _, targetProperties) = groupedEntries.Key.GetAuditPropertyInformation();
            foreach (var entry in groupedEntries.Where(entry => entry.State == EntityState.Deleted))
            {
                AddAuditEntry(entry, entry.Metadata.ClrType, context, auditEntityType, sourceProperties, targetProperties);
            }
        }
    }

    private void AddAuditEntry(EntityEntry entityEntry, Type entityType, DbContext context, Type auditEntityType, IEnumerable<PropertyInfo> sourceProperties, IEnumerable<PropertyInfo> targetProperties)
    {
        if (Activator.CreateInstance(auditEntityType) is not IAuditEntityV1 newAuditEntity)
            throw new UnexpectedConditionException($"AuditEntityV1Attribute can only be used on types implementing IAuditEntityV1 but Type {entityType} isn't");

        var propertyValues = entityEntry.CurrentValues;
        foreach (var joined in targetProperties.Join(
                     sourceProperties,
                     t => t.Name,
                     s => s.Name,
                     (t, s) => (Target: t, Value: propertyValues?[s.Name])))
        {
            joined.Target.SetValue(newAuditEntity, joined.Value);
        }

        newAuditEntity.AuditV1Id = Guid.NewGuid();
        newAuditEntity.AuditV1OperationId = entityEntry.State.ToAuditOperation();
        newAuditEntity.AuditV1DateLastChanged = _dateTimeProvider.OffsetNow;
        newAuditEntity.AuditV1LastEditorId = _identityService.IdentityData.UserId;

        context.Add(newAuditEntity);
    }
}
