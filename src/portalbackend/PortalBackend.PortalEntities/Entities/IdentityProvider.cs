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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Base;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class IdentityProvider : IBaseEntity
{
    private IdentityProvider()
    {
        Companies = new HashSet<Company>();
        CompanyIdentityProviders = new HashSet<CompanyIdentityProvider>();
    }

    public IdentityProvider(Guid id, IdentityProviderCategoryId identityProviderCategoryId, IdentityProviderTypeId identityProviderTypeId, DateTimeOffset dateCreated) : this()
    {
        Id = id;
        IdentityProviderCategoryId = identityProviderCategoryId;
        IdentityProviderTypeId = identityProviderTypeId;
        DateCreated = dateCreated;
    }

    public Guid Id { get; private set; }

    public DateTimeOffset DateCreated { get; private set; }

    public IdentityProviderCategoryId IdentityProviderCategoryId { get; private set; }

    public IdentityProviderTypeId IdentityProviderTypeId { get; private set; }

    public Guid? OwnerId { get; set; }

    // Navigation properties
    public virtual IdentityProviderCategory? IdentityProviderCategory { get; private set; }
    public virtual IamIdentityProvider? IamIdentityProvider { get; set; }
    public virtual IdentityProviderType? IdentityProviderType { get; set; }
    public virtual Company? Owner { get; set; }
    public virtual ICollection<Company> Companies { get; private set; }
    public virtual ICollection<CompanyIdentityProvider> CompanyIdentityProviders { get; private set; }
}
