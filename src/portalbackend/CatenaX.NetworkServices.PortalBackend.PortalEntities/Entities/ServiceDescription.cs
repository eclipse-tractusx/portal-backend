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
/// Entity for service descriptions
/// </summary>
public class ServiceDescription
{
    /// <summary>
    /// Only needed for ef
    /// </summary>
    private ServiceDescription()
    {
        Description = null!;
        LanguageShortName = null!;
    }


    /// <summary>
    /// Creates a new instance of <see cref="ServiceDescription"/>
    /// </summary>
    /// <param name="description">the description for the service</param>
    /// <param name="serviceId">Id of the service</param>
    /// <param name="languageShortName">the language short name</param>
    public ServiceDescription(string description, Guid serviceId, string languageShortName)
        :this()
    {
        Description = description;
        ServiceId = serviceId;
        LanguageShortName = languageShortName;
    }

    /// <summary>
    /// the description for the service
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// Id of the service
    /// </summary>
    public Guid ServiceId { get; set; }
    
    /// <summary>
    /// the language short name
    /// </summary>
    public string LanguageShortName { get; set; }

    /// <summary>
    /// Mapping between <see cref="ServiceDescription"/> and <see cref="Service"/>
    /// </summary>
    public virtual Service? Service { get; set; }

}