/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class CompanyServiceAccount
{
    private CompanyServiceAccount()
    {
        Name = default!;
        Description = default!;
        UserRoles = new HashSet<UserRole>();
        CompanyServiceAccountAssignedRoles = new HashSet<CompanyServiceAccountAssignedRole>();
    }
    
    public CompanyServiceAccount(Guid id, Guid companyId, CompanyServiceAccountStatusId companyServiceAccountStatusId, string name, string description, DateTimeOffset dateCreated) : this()
    {
        Id = id;
        DateCreated = dateCreated;
        CompanyId = companyId;
        CompanyServiceAccountStatusId = companyServiceAccountStatusId;
        Name = name;
        Description = description;
    }

    [Key]
    public Guid Id { get; private set; }

    public DateTimeOffset DateCreated { get; private set; }

    public Guid CompanyId { get; private set; }

    [MaxLength(255)]
    public string Name { get; set; }

    public string Description { get; set; }

    public CompanyServiceAccountStatusId CompanyServiceAccountStatusId { get; set; }

    // Navigation properties
    public virtual Company? Company { get; private set; }
    public virtual IamServiceAccount? IamServiceAccount { get; set; }
    public virtual CompanyServiceAccountStatus? CompanyServiceAccountStatus { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; private set; }
    public virtual ICollection<CompanyServiceAccountAssignedRole> CompanyServiceAccountAssignedRoles { get; private set; }
}
