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

using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

/// <summary>
/// Mapping table for services, company and service subscription state
/// </summary>
public class CompanyAssignedService
{
    /// <summary>
    /// Only needed for ef
    /// </summary>
    private CompanyAssignedService()
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="CompanyAssignedApp"/>
    /// </summary>
    /// <param name="serviceId">Id of the service</param>
    /// <param name="companyId">Id of the company</param>
    /// <param name="serviceSubscriptionStatusId">Id of the service Subscription status</param>
    /// <param name="requesterId">Id of the requester</param>
    public CompanyAssignedService(Guid serviceId, Guid companyId, ServiceSubscriptionStatusId serviceSubscriptionStatusId, Guid requesterId)
        :this()
    {
        ServiceId = serviceId;
        CompanyId = companyId;
        ServiceSubscriptionStatusId = serviceSubscriptionStatusId;
        RequesterId = requesterId;
    }
    
    /// <summary>
    /// Id of the service
    /// </summary>
    public Guid ServiceId { get; set; }
    
    /// <summary>
    /// Id of the company
    /// </summary>
    public Guid CompanyId { get; set; }
    
    /// <summary>
    /// Id of the  service Subscription status
    /// </summary>
    public ServiceSubscriptionStatusId ServiceSubscriptionStatusId { get; set; }
    
    /// <summary>
    /// Id of the requester
    /// </summary>
    public Guid RequesterId { get; set; }

    /// <summary>
    /// Navigation property for the service
    /// </summary>
    public virtual Service? Service { get; set; }

    /// <summary>
    /// Navigation property for the service
    /// </summary>
    public virtual Company? Company { get; set; }

    /// <summary>
    /// Navigation property for the service
    /// </summary>
    public virtual ServiceSubscriptionStatus? ServiceSubscriptionStatus { get; set; }

    /// <summary>
    /// Navigation property for the service
    /// </summary>
    public virtual CompanyUser? Requester { get; set; }
}