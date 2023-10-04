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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

/// <summary>
/// Type of a identity type
/// </summary>
public class IdentityProviderType
{
    /// <summary>
    /// Internal constructor, only for EF
    /// </summary>
    private IdentityProviderType()
    {
        Label = null!;
        IdentityProviders = new HashSet<IdentityProvider>();
    }

    /// <summary>
    /// Creates a new instance of <see cref="IdentityProviderType"/> and initializes the id and label 
    /// </summary>
    /// <param name="identityProviderTypeId">The IdentityProviderTypeId</param>
    public IdentityProviderType(IdentityProviderTypeId identityProviderTypeId) : this()
    {
        Id = identityProviderTypeId;
        Label = identityProviderTypeId.ToString();
    }

    /// <summary>
    /// Id of the type
    /// </summary>
    public IdentityProviderTypeId Id { get; private set; }

    /// <summary>
    /// The type as string 
    /// </summary>
    [MaxLength(255)]
    public string Label { get; private set; }

    public virtual ICollection<IdentityProvider> IdentityProviders { get; private set; }
}
