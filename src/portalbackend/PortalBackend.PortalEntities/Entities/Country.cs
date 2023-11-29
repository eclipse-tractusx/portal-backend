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

public class Country
{
    private Country()
    {
        Alpha2Code = null!;
        Addresses = new HashSet<Address>();
        Connectors = new HashSet<Connector>();
        CountryAssignedIdentifiers = new HashSet<CountryAssignedIdentifier>();
        CountryLongNames = new HashSet<CountryLongName>();
    }

    public Country(string alpha2Code) : this()
    {
        Alpha2Code = alpha2Code;
    }

    [Key]
    [StringLength(2, MinimumLength = 2)]
    [JsonPropertyName("alpha2code")]
    public string Alpha2Code { get; private set; }

    [StringLength(3, MinimumLength = 3)]
    [JsonPropertyName("alpha3code")]
    public string? Alpha3Code { get; set; }

    // Navigation properties
    public virtual ICollection<Address> Addresses { get; private set; }
    public virtual ICollection<Connector> Connectors { get; private set; }
    public virtual ICollection<CountryAssignedIdentifier> CountryAssignedIdentifiers { get; private set; }
    public virtual ICollection<CountryLongName> CountryLongNames { get; private set; }
}
