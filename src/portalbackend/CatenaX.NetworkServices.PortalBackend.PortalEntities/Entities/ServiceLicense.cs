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
/// Information to the service licenses
/// </summary>
public class ServiceLicense
{
    /// <summary>
    /// Only needed for ef
    /// </summary>
    private ServiceLicense()
    {
        LicenseText = null!;
        Services = new HashSet<Service>();
    }

    /// <summary>
    /// Creates a new instance of <see cref="ServiceLicense"/>
    /// </summary>
    /// <param name="id">Unique identifier</param>
    /// <param name="licenseText">The license text</param>
    public ServiceLicense(Guid id, string licenseText)
        :this()
    {
        Id = id;
        LicenseText = licenseText;
    }

    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The license tex
    /// </summary>
    public string LicenseText { get; set; }
    
    /// <summary>
    /// Mapping between services and service license
    /// </summary>
    public virtual ICollection<Service> Services { get; private set; }
}
