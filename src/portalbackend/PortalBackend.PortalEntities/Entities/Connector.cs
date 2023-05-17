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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.AuditEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Auditing;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Base;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

[AuditEntityV1(typeof(AuditConnector20230503))]
public class Connector : IAuditableV1, IBaseEntity
{
    public Connector(Guid id, string name, string locationId, string connectorUrl)
    {
        Id = id;
        Name = name;
        LocationId = locationId;
        ConnectorUrl = connectorUrl;
    }

    public Guid Id { get; private set; }

    [MaxLength(255)]
    public string Name { get; set; }

    [MaxLength(255)]
    public string ConnectorUrl { get; set; }

    public ConnectorTypeId TypeId { get; set; }

    public ConnectorStatusId StatusId { get; set; }

    public Guid ProviderId { get; set; }

    public Guid? HostId { get; set; }

    /// <summary>
    /// Link to the self description document
    /// </summary>
    public Guid? SelfDescriptionDocumentId { get; set; }

    [StringLength(2, MinimumLength = 2)]
    public string LocationId { get; set; }

    public bool? DapsRegistrationSuccessful { get; set; }

    public string? SelfDescriptionMessage { get; set; }

    public DateTimeOffset? DateLastChanged { get; set; }

    public Guid? CompanyServiceAccountId { get; set; }

    [AuditLastEditorV1]
    public Guid? LastEditorId { get; set; }

    // Navigation properties
    public virtual ConnectorType? Type { get; set; }
    public virtual ConnectorStatus? Status { get; set; }
    public virtual Company? Provider { get; set; }
    public virtual Company? Host { get; set; }
    public virtual Country? Location { get; set; }
    public virtual ConnectorClientDetail? ClientDetails { get; set; }
    public virtual CompanyServiceAccount? CompanyServiceAccount { get; set; }
    public virtual CompanyUser? LastEditor { get; set; }

    /// <summary>
    /// Mapping to the assigned document
    /// </summary>
    public virtual Document? SelfDescriptionDocument { get; set; }
}
