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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Base;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class CompanyServiceAccount : IBaseEntity
{
    private CompanyServiceAccount()
    {
        Name = default!;
        Description = default!;
        UserRoles = new HashSet<UserRole>();
        CompanyServiceAccountAssignedRoles = new HashSet<CompanyServiceAccountAssignedRole>();
        AppInstances = new HashSet<AppInstanceAssignedCompanyServiceAccount>();
    }
    
    public CompanyServiceAccount(Guid id, Guid serviceAccountOwnerId, CompanyServiceAccountStatusId companyServiceAccountStatusId, string name, string description, DateTimeOffset dateCreated, CompanyServiceAccountTypeId companyServiceAccountTypeId) : this()
    {
        Id = id;
        DateCreated = dateCreated;
        ServiceAccountOwnerId = serviceAccountOwnerId;
        CompanyServiceAccountStatusId = companyServiceAccountStatusId;
        Name = name;
        Description = description;
        CompanyServiceAccountTypeId = companyServiceAccountTypeId;
    }

    [Key]
    public Guid Id { get; private set; }

    public DateTimeOffset DateCreated { get; private set; }

    public Guid ServiceAccountOwnerId { get; private set; }

    [MaxLength(255)]
    public string Name { get; set; }

    public string Description { get; set; }

    public CompanyServiceAccountTypeId CompanyServiceAccountTypeId { get; set; }

    public CompanyServiceAccountStatusId CompanyServiceAccountStatusId { get; set; }

    public Guid? OfferSubscriptionId { get; set; }

    // Navigation properties
    public virtual Company? ServiceAccountOwner { get; private set; }
    public virtual IamServiceAccount? IamServiceAccount { get; set; }
    public virtual CompanyServiceAccountStatus? CompanyServiceAccountStatus { get; set; }
    public virtual CompanyServiceAccountType? CompanyServiceAccountType { get; set; }
    public virtual OfferSubscription? OfferSubscription { get; set; }
    public virtual Connector? Connector { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; private set; }
    public virtual ICollection<CompanyServiceAccountAssignedRole> CompanyServiceAccountAssignedRoles { get; private set; }

    public virtual ICollection<AppInstanceAssignedCompanyServiceAccount> AppInstances { get; private set; }
}
