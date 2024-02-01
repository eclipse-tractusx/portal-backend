/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class Language
{
    private Language()
    {
        ShortName = null!;
        AppDescriptions = new HashSet<OfferDescription>();
        CompanyRoleDescriptions = new HashSet<CompanyRoleDescription>();
        UserRoleDescriptions = new HashSet<UserRoleDescription>();
        SupportingApps = new HashSet<Offer>();
        LanguageLongNames = new HashSet<LanguageLongName>();
        LanguageLongNameLanguages = new HashSet<LanguageLongName>();
        UseCases = new HashSet<UseCaseDescription>();
        CountryLongNames = new HashSet<CountryLongName>();
        CompanyCertificateTypeDescriptions=new HashSet<CompanyCertificateTypeDescription>();
    }

    public Language(string shortName) : this()
    {
        ShortName = shortName;
    }

    [Key]
    [StringLength(2, MinimumLength = 2)]
    public string ShortName { get; set; }

    // Navigation properties
    public virtual ICollection<OfferDescription> AppDescriptions { get; private set; }
    public virtual ICollection<CompanyRoleDescription> CompanyRoleDescriptions { get; private set; }
    public virtual ICollection<UserRoleDescription> UserRoleDescriptions { get; private set; }
    public virtual ICollection<Offer> SupportingApps { get; private set; }
    public virtual ICollection<LanguageLongName> LanguageLongNames { get; private set; }
    public virtual ICollection<LanguageLongName> LanguageLongNameLanguages { get; private set; }
    public virtual ICollection<UseCaseDescription> UseCases { get; private set; }
    public virtual ICollection<CountryLongName> CountryLongNames { get; private set; }

    public virtual ICollection<CompanyCertificateTypeDescription> CompanyCertificateTypeDescriptions { get; private set; } 
}
