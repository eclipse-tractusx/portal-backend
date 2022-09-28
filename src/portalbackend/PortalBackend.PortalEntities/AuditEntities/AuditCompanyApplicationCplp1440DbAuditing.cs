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

using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Auditing;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.AuditEntities;

/// <summary>
/// Audit entity for <see cref="CompanyApplication"/> only needed for configuration purposes
/// </summary>
public class AuditCompanyApplicationCplp1440DbAuditing : IAuditEntity
{
    /// <inheritdoc />
    public Guid AuditId { get; set; }

    public Guid Id { get; set; }

    public DateTimeOffset DateCreated { get; set; }

    public CompanyApplicationStatusId ApplicationStatusId { get; set; }
    
    public Guid CompanyId { get; set; }

    /// <inheritdoc />
    public Guid? LastEditorId { get; set; }
    
    /// <inheritdoc />
    public AuditOperationId AuditOperationId { get; set; }
    
    /// <inheritdoc />
    public DateTimeOffset DateLastChanged { get; set; }
}
