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
                     .GroupBy(entry => entry.Metadata.ClrType)
                     .Where(group => (AuditEntityV1Attribute?)Attribute.GetCustomAttribute(group.Key, typeof(AuditEntityV1Attribute)) != null))
        {
            var lastEditorNames = groupedEntries.Key.GetProperties()
                .Where(x => Attribute.IsDefined(x, typeof(AuditLastEditorV1Attribute))).Select(x => x.Name)
                .ToList();
            var lastChangedNames = groupedEntries.Key.GetProperties()
                .Where(x => Attribute.IsDefined(x, typeof(AuditLastChangedV1Attribute))).Select(x => x.Name)
                .ToList();

            foreach (var entry in groupedEntries.Where(entry => entry.State != EntityState.Deleted))
            {
                // Set LastEditor
                foreach (var prop in entry.Properties.Where(x => lastEditorNames.Any(lcn => lcn == x.Metadata.Name)))
                {
                    prop.CurrentValue = _identityService.IdentityData.UserId;
                }

                // If existing try to set DateLastChanged
                foreach (var prop in entry.Properties.Where(x => lastChangedNames.Any(lcn => lcn == x.Metadata.Name)))
                {
                    prop.CurrentValue = now;
                }
            }

            foreach (var entry in groupedEntries.Where(entry => entry.State == EntityState.Deleted))
            {
                AddAuditEntry(entry, entry.Metadata.ClrType, context);
            }
        }
    }

    private void AddAuditEntry(EntityEntry entityEntry, Type entityType, DbContext context)
    {
        var (auditEntityType, sourceProperties, _, targetProperties) = entityType.GetAuditPropertyInformation();
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
