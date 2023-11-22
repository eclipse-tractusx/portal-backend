/********************************************************************************
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

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class CountryLongNames
{
    private CountryLongNames()
    {
        Alpha2Code = null!;
        ShortName = null!;
        CountryLongName = null!;
    }

    public CountryLongNames(string alpha2Code, string shortName, string countryLongName)
    {
        Alpha2Code = alpha2Code;
        ShortName = shortName;
        CountryLongName = countryLongName;
    }
    [StringLength(2, MinimumLength = 2)]
    [JsonPropertyName("alpha2code")]
    public string Alpha2Code { get; private set; }

    [StringLength(2, MinimumLength = 2)]
    public string ShortName { get; private set; }
    public string CountryLongName { get; private set; }

    // Navigation Properties
    public virtual Language? Language { get; private set; }
    public virtual Country? Country { get; private set; }
}
