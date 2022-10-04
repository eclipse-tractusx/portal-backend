﻿/********************************************************************************
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

using System.Reflection;
using Laraue.EfCoreTriggers.Common.TriggerBuilders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json.Serialization;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
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
    public AuditEntityV1Attribute(Type auditEntityType)
    {
        if (!typeof(IAuditEntityV1).IsAssignableFrom(auditEntityType))
        {
            throw new ArgumentException($"Entity must derive from {nameof(IAuditEntityV1)}", nameof(auditEntityType));
        }
    }
}