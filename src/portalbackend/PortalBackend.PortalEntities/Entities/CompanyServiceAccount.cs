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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Base;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Views;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class CompanyServiceAccount : IBaseEntity, ILockableEntity
{
    public CompanyServiceAccount(Guid id, string name, string description, CompanyServiceAccountTypeId companyServiceAccountTypeId, CompanyServiceAccountKindId companyServiceAccountKindId, Guid version)
    {
        Id = id;
        Name = name;
        Description = description;
        CompanyServiceAccountTypeId = companyServiceAccountTypeId;
        CompanyServiceAccountKindId = companyServiceAccountKindId;
        Version = version;
        AppInstances = new HashSet<AppInstanceAssignedCompanyServiceAccount>();
    }

    /// <inheritdoc />
    public Guid Id { get; set; }

    [StringLength(255)]
    public string? ClientClientId { get; set; }

    [MaxLength(255)]
    public string Name { get; set; }

    public string Description { get; set; }

    public CompanyServiceAccountTypeId CompanyServiceAccountTypeId { get; set; }
    public CompanyServiceAccountKindId CompanyServiceAccountKindId { get; set; }
    public Guid? OfferSubscriptionId { get; set; }

    [ConcurrencyCheck]
    public Guid Version { get; set; }

    public DateTimeOffset? LockExpiryDate { get; set; }

    // Navigation properties
    public virtual Identity? Identity { get; set; }
    public virtual CompanyServiceAccountType? CompanyServiceAccountType { get; set; }
    public virtual CompanyServiceAccountKind? CompanyServiceAccountKind { get; set; }
    public virtual OfferSubscription? OfferSubscription { get; set; }
    public virtual Connector? Connector { get; set; }
    public virtual CompaniesLinkedServiceAccount? CompaniesLinkedServiceAccount { get; private set; }
    public virtual DimCompanyServiceAccount? DimCompanyServiceAccount { get; private set; }
    public virtual DimUserCreationData? DimUserCreationData { get; set; }
    public virtual ICollection<AppInstanceAssignedCompanyServiceAccount> AppInstances { get; private set; }
}
