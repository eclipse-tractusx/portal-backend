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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

[AuditEntityV1(typeof(AuditUserRole20231115))]
public class UserRole : IAuditableV1, IBaseEntity
{
    private UserRole()
    {
        UserRoleText = null!;
        IdentityAssignedRoles = new HashSet<IdentityAssignedRole>();
        UserRoleCollections = new HashSet<UserRoleCollection>();
        UserRoleDescriptions = new HashSet<UserRoleDescription>();
        TechnicalUserProfileAssignedUserRole = new HashSet<TechnicalUserProfileAssignedUserRole>();
    }

    public UserRole(Guid id, string userRoleText, Guid offerId) : this()
    {
        Id = id;
        UserRoleText = userRoleText;
        OfferId = offerId;
    }

    public Guid Id { get; private set; }

    [MaxLength(255)]
    [Column("user_role")]
    [JsonPropertyName("user_role")]
    public string UserRoleText { get; set; }

    public Guid OfferId { get; set; }

    [LastEditorV1]
    public Guid? LastEditorId { get; private set; }
    // Navigation properties
    public virtual Offer? Offer { get; set; }
    public virtual Identity? LastEditor { get; private set; }
    public virtual ICollection<IdentityAssignedRole> IdentityAssignedRoles { get; private set; }
    public virtual ICollection<UserRoleCollection> UserRoleCollections { get; private set; }
    public virtual ICollection<UserRoleDescription> UserRoleDescriptions { get; private set; }
    public virtual ICollection<TechnicalUserProfileAssignedUserRole> TechnicalUserProfileAssignedUserRole { get; private set; }
}
