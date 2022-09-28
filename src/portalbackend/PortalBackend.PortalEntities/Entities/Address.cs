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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class Address
{
    private Address()
    {
        City = null!;
        Streetname = null!;
        CountryAlpha2Code = null!;
        Companies = new HashSet<Company>();
    }

    public Address(Guid id, string city, string streetname, string countryAlpha2Code, DateTimeOffset dateCreated) : this()
    {
        Id = id;
        DateCreated = dateCreated;
        City = city;
        Streetname = streetname;
        CountryAlpha2Code = countryAlpha2Code;
    }

    public Guid Id { get; private set; }

    public DateTimeOffset DateCreated { get; private set; }

    public DateTimeOffset? DateLastChanged { get; set; }

    [MaxLength(255)]
    public string City { get; set; }

    [MaxLength(255)]
    public string? Region { get; set; }

    [MaxLength(255)]
    public string? Streetadditional { get; set; }

    [MaxLength(255)]
    public string Streetname { get; set; }

    [MaxLength(255)]
    public string? Streetnumber { get; set; }

    [MaxLength(12)]
    public string? Zipcode { get; set; }

    [StringLength(2, MinimumLength = 2)]
    public string CountryAlpha2Code { get; set; }

    // Navigation properties
    public virtual Country? Country { get; set; }
    public virtual ICollection<Company> Companies { get; private set; }
}
