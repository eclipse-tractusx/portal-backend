/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using CatenaX.NetworkServices.PortalBackend.PortalEntities.Auditing;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

/// <summary>
/// Entity for a service
/// </summary>
public class Service : IAuditable
{
    /// <summary>
    /// Only needed for EF Core and auditing
    /// </summary>
    protected Service()
    {
        Name = null!;
        ServiceLicenses = new HashSet<ServiceLicense>();
        ServiceDescriptions = new HashSet<ServiceDescription>();
        Companies = new HashSet<Company>();
    }
    
    public Service(Guid id, DateTimeOffset dateCreated, string name, Guid providerCompanyId, ServiceStatusId serviceStatusId)
        : this()
    {
        Id = id;
        DateCreated = dateCreated;
        Name = name;
        ProviderCompanyId = providerCompanyId;
        ServiceStatusId = serviceStatusId;
    }

    /// <summary>
    /// Id of the entity
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Date of the creation
    /// </summary>
    public DateTimeOffset DateCreated { get; set; }

    /// <summary>
    /// Name for the service
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Optional Thumbnail Url
    /// </summary>
    public string? ThumbnailUrl { get; set; }
    
    /// <summary>
    /// Id of the providing company
    /// </summary>
    public Guid ProviderCompanyId { get; set; }
    
    /// <summary>
    /// Id of the service status
    /// </summary>
    public ServiceStatusId ServiceStatusId { get; set; }
    
    /// <summary>
    /// Contact mail adress
    /// </summary>
    public string? ContactEmail { get; set; }
    
    /// <summary>
    /// Id of the sales manager for the service
    /// </summary>
    public Guid? SalesManagerId { get; set; }
    
    /// <inheritdoc />
    public Guid? LastEditorId { get; set; }

    /// <summary>
    /// Navigation property to the providing company
    /// </summary>
    public virtual Company? ProviderCompany { get; set; }

    /// <summary>
    /// Navigation property to the service status
    /// </summary>
    public virtual ServiceStatus? ServiceStatus { get; set; }

    /// <summary>
    /// Navigation property to the sales manager
    /// </summary>
    public virtual CompanyUser? SalesManager { get; set; }

    /// <summary>
    /// Mapping to the service licenses
    /// </summary>
    public virtual ICollection<ServiceLicense> ServiceLicenses { get; private set; }
    
    /// <summary>
    /// Mapping to the service descriptions
    /// </summary>
    public virtual ICollection<ServiceDescription> ServiceDescriptions { get; private set; }
    
    /// <summary>
    /// Mapping to the company assigned services
    /// </summary>
    public virtual ICollection<Company> Companies { get; private set; }
}