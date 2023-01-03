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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class Language
{
    private Language()
    {
        ShortName = null!;
        LongNameDe = null!;
        LongNameEn = null!;
        AppDescriptions = new HashSet<OfferDescription>();
        CompanyRoleDescriptions = new HashSet<CompanyRoleDescription>();
        UserRoleDescriptions = new HashSet<UserRoleDescription>();
        SupportingApps = new HashSet<Offer>();
    }

    public Language(string shortName, string longNameDe, string longNameEn) : this()
    {
        ShortName = shortName;
        LongNameDe = longNameDe;
        LongNameEn = longNameEn;
    }

    [Key]
    [StringLength(2, MinimumLength = 2)]
    public string ShortName { get; set; }

    [MaxLength(255)]
    public string LongNameDe { get; set; }

    [MaxLength(255)]
    public string LongNameEn { get; set; }

    // Navigation properties
    public virtual ICollection<OfferDescription> AppDescriptions { get; private set; }
    public virtual ICollection<CompanyRoleDescription> CompanyRoleDescriptions { get; private set; }
    public virtual ICollection<UserRoleDescription> UserRoleDescriptions { get; private set; }
    public virtual ICollection<Offer> SupportingApps { get; private set; }
}
