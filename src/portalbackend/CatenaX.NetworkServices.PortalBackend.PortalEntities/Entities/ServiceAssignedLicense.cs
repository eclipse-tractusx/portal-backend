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

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

/// <summary>
/// Mapping entity for <see cref="Service"/> and <see cref="ServiceLicense"/>
/// </summary>
public class ServiceAssignedLicense
{
    /// <summary>
    /// Only needed for ef
    /// </summary>
    private ServiceAssignedLicense() { }

    /// <summary>
    /// Creates a new instance of <see cref="ServiceAssignedLicense"/>
    /// </summary>
    /// <param name="serviceId">key of the service</param>
    /// <param name="serviceLicenseId">key of the service license</param>
    public ServiceAssignedLicense(Guid serviceId, Guid serviceLicenseId)
        :this()
    {
        ServiceId = serviceId;
        ServiceLicenseId = serviceLicenseId;
    }

    /// <summary>
    /// key of the service
    /// </summary>
    public Guid ServiceId { get; set; }

    /// <summary>
    /// key of the service license
    /// </summary>
    public Guid ServiceLicenseId { get; set; }
    
    /// <summary>
    /// Navigation to the service
    /// </summary>
    public virtual Service? Service { get; private set; }
    
    /// <summary>
    /// Navigation to the service
    /// </summary>
    public virtual ServiceLicense? ServiceLicense { get; private set; }
}