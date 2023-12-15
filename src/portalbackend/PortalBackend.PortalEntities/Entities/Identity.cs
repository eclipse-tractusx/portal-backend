/********************************************************************************
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
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

[AuditEntityV1(typeof(AuditIdentity20231115))]
public class Identity : IBaseEntity, IAuditableV1
{
    public Identity(Guid id, DateTimeOffset dateCreated, Guid companyId, UserStatusId userStatusId, IdentityTypeId identityTypeId)
    {
        Id = id;
        DateCreated = dateCreated;
        CompanyId = companyId;
        UserStatusId = userStatusId;
        IdentityTypeId = identityTypeId;

        IdentityAssignedRoles = new HashSet<IdentityAssignedRole>();
        CreatedNotifications = new HashSet<Notification>();
    }

    public Guid Id { get; set; }

    public DateTimeOffset DateCreated { get; set; }

    public Guid CompanyId { get; set; }

    [JsonPropertyName("user_status_id")]
    public UserStatusId UserStatusId { get; set; }

    [Obsolete("remove as soon test-data has been cleaned up")]
    [StringLength(36)]
    public string? UserEntityId { get; set; }

    public IdentityTypeId IdentityTypeId { get; set; }

    [LastChangedV1]
    public DateTimeOffset? DateLastChanged { get; set; }

    [LastEditorV1]
    public Guid? LastEditorId { get; private set; }

    // Navigation properties
    public virtual CompanyUser? CompanyUser { get; set; }
    public virtual CompanyServiceAccount? CompanyServiceAccount { get; set; }
    public virtual Company? Company { get; set; }
    public virtual IdentityUserStatus? IdentityStatus { get; set; }
    public virtual IdentityType? IdentityType { get; set; }
    public virtual ICollection<Notification> CreatedNotifications { get; private set; }
    public virtual ICollection<IdentityAssignedRole> IdentityAssignedRoles { get; private set; }
    public virtual Identity? LastEditor { get; private set; }
}
