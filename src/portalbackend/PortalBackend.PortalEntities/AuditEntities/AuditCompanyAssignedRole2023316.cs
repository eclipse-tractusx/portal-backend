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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Auditing;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.AuditEntities;

/// <summary>
/// Audit entity for <see cref="CompanyAssignedRole"/> only needed for configuration purposes
/// </summary>

public class AuditCompanyAssignedRole2023316 : IAuditEntityV1
{
    /// <inheritdoc />
    [Key]
    public Guid AuditV1Id { get; set; }
    
    public Guid CompanyId { get; private set; }

    public CompanyRoleId CompanyRoleId { get; private set; }
    
    public DateTimeOffset? DateLastChanged { get; set; }

    public Guid? LastEditorId { get; set; }

    /// <inheritdoc />
    public Guid? AuditV1LastEditorId { get; set; }
    
    /// <inheritdoc />
    public AuditOperationId AuditV1OperationId { get; set; }
    
    /// <inheritdoc />
    public DateTimeOffset AuditV1DateLastChanged { get; set; }
}
