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

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class CountryAssignedIdentifier
{
    private CountryAssignedIdentifier() 
    {
        CountryAlpha2Code = null!;
    }

    public CountryAssignedIdentifier(string countryAlpha2Code, UniqueIdentifierId uniqueIdentifierId)
     : this()
    {
        CountryAlpha2Code = countryAlpha2Code;
        UniqueIdentifierId = uniqueIdentifierId;
    }
    
    [JsonPropertyName("country_alpha2code")]
    [StringLength(2, MinimumLength = 2)]
    public string CountryAlpha2Code { get; private set; }
    public UniqueIdentifierId UniqueIdentifierId { get; private set; }

    // Navigation properties
    public virtual Country? Country { get; private set; }
    public virtual UniqueIdentifier? UniqueIdentifier { get; private set; }
}
