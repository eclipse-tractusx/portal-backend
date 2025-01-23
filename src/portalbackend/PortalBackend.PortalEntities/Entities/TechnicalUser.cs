/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Views;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class TechnicalUser : IBaseEntity, IVersionedEntity
{
    public TechnicalUser(Guid id, Guid version, string name, string description, TechnicalUserTypeId technicalUserTypeId, TechnicalUserKindId technicalUserKindId)
    {
        Id = id;
        Name = name;
        Description = description;
        TechnicalUserTypeId = technicalUserTypeId;
        TechnicalUserKindId = technicalUserKindId;
        Version = version;
        AppInstanceAssignedTechnicalUsers = new HashSet<AppInstanceAssignedTechnicalUser>();
    }

    /// <inheritdoc />
    public Guid Id { get; set; }

    [StringLength(255)]
    public string? ClientClientId { get; set; }

    [MaxLength(255)]
    public string Name { get; set; }

    public string Description { get; set; }

    public TechnicalUserTypeId TechnicalUserTypeId { get; set; }
    public TechnicalUserKindId TechnicalUserKindId { get; set; }
    public Guid? OfferSubscriptionId { get; set; }

    [ConcurrencyCheck]
    public Guid Version { get; set; }

    // Navigation properties
    public virtual Identity? Identity { get; set; }
    public virtual TechnicalUserType? TechnicalUserType { get; set; }
    public virtual TechnicalUserKind? TechnicalUserKind { get; set; }
    public virtual OfferSubscription? OfferSubscription { get; set; }
    public virtual Connector? Connector { get; set; }
    public virtual CompaniesLinkedTechnicalUser? CompaniesLinkedTechnicalUser { get; private set; }
    public virtual ExternalTechnicalUser? ExternalTechnicalUser { get; private set; }
    public virtual ExternalTechnicalUserCreationData? ExternalTechnicalUserCreationData { get; set; }
    public virtual ICollection<AppInstanceAssignedTechnicalUser> AppInstanceAssignedTechnicalUsers { get; private set; }
}
