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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Base;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Auditing;

public static class AuditValidation
{
    public static void ValidateAuditEntities()
    {
        var auditableEntities = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetInterfaces().Contains(typeof(IAuditableV1)));
        foreach (var auditableEntity in auditableEntities)
        {
            auditableEntity.ValidateAuditV1();
        }
    }

    private static void ValidateAuditV1(this Type auditableEntityType)
    {
        var auditEntityAttribute = (AuditEntityV1Attribute?)Attribute.GetCustomAttribute(auditableEntityType, typeof(AuditEntityV1Attribute));
        if (auditEntityAttribute == null)
        {
            throw new ConfigurationException($"{auditableEntityType.Name} must be annotated with {nameof(AuditEntityV1Attribute)}");
        }

        var auditEntityType = auditEntityAttribute.AuditEntityType;
        if (!typeof(IAuditEntityV1).IsAssignableFrom(auditEntityType))
        {
            throw new ConflictException($"{auditEntityType} must inherit from {nameof(IAuditEntityV1)}");
        }

        var sourceProperties = new List<PropertyInfo>();
        if (typeof(IBaseEntity).IsAssignableFrom(auditableEntityType))
        {
            sourceProperties.AddRange(typeof(IBaseEntity).GetProperties());
        }
        sourceProperties.AddRange(auditableEntityType.GetProperties(BindingFlags.Public |
                                                                    BindingFlags.Instance |
                                                                    BindingFlags.DeclaredOnly).Where(p => !(p.GetGetMethod()?.IsVirtual ?? false)));
        var auditProperties = typeof(IAuditEntityV1).GetProperties();
        var targetProperties = auditEntityType.GetProperties().ExceptBy(auditProperties.Select(x => x.Name), p => p.Name);

        var illegalProperties = sourceProperties.IntersectBy(auditProperties.Select(x => x.Name), p => p.Name);
        if (illegalProperties.Any())
        {
            throw new ConfigurationException($"{auditableEntityType.Name} is must not declare any of the following properties: {string.Join(", ", illegalProperties.Select(x => x.Name))}");
        }

        var missingProperties = sourceProperties.ExceptBy(targetProperties.Select(x => x.Name), p => p.Name);
        if (missingProperties.Any())
        {
            throw new ArgumentException($"{auditEntityAttribute.AuditEntityType.Name} is missing the following properties: {string.Join(", ", missingProperties.Select(x => x.Name))}");
        }

        if (!Array.Exists(
                auditEntityType.GetProperties(),
            p => p.Name == AuditPropertyV1Names.AuditV1Id.ToString() && p.CustomAttributes.Any(a => a.AttributeType == typeof(KeyAttribute))))
        {
            throw new ConfigurationException($"{auditEntityType.Name}.{AuditPropertyV1Names.AuditV1Id} must be marked as primary key by attribute {nameof(KeyAttribute)}");
        }
    }
}
