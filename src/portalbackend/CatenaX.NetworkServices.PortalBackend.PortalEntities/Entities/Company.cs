﻿/********************************************************************************
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
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class Company
{
    private Company()
    {
        Name = null!;
        Agreements = new HashSet<Agreement>();
        BoughtOffers = new HashSet<Offer>();
        ProvidedOffers = new HashSet<Offer>();
        CompanyApplications = new HashSet<CompanyApplication>();
        CompanyAssignedRoles = new HashSet<CompanyAssignedRole>();
        IdentityProviders = new HashSet<IdentityProvider>();
        OfferSubscriptions = new HashSet<OfferSubscription>();
        CompanyUsers = new HashSet<CompanyUser>();
        CompanyServiceAccounts = new HashSet<CompanyServiceAccount>();
        Consents = new HashSet<Consent>();
        CompanyRoles = new HashSet<CompanyRole>();
        UseCases = new HashSet<UseCase>();
        ProvidedConnectors = new HashSet<Connector>();
        HostedConnectors = new HashSet<Connector>();
    }
    
    public Company(Guid id, string name, CompanyStatusId companyStatusId, DateTimeOffset dateCreated) : this()
    {
        Id = id;
        Name = name;
        CompanyStatusId = companyStatusId;
        DateCreated = dateCreated;
    }

    public Guid Id { get; private set; }

    public DateTimeOffset DateCreated { get; private set; }

    [MaxLength(20)]
    public string? BusinessPartnerNumber { get; set; }

    [MaxLength(20)]
    public string? TaxId { get; set; }

    [MaxLength(255)]
    public string Name { get; set; }

    [MaxLength(255)]
    public string? Shortname { get; set; }

    public CompanyStatusId CompanyStatusId { get; set; }

    public Guid? AddressId { get; set; }

    // Navigation properties
    public virtual Address? Address { get; set; }
    public virtual ICollection<Agreement> Agreements { get; private set; }
    public virtual ICollection<Offer> BoughtOffers { get; private set; }
    public virtual ICollection<CompanyApplication> CompanyApplications { get; private set; }
    public virtual ICollection<OfferSubscription> OfferSubscriptions { get; private set; }
    public virtual CompanyStatus? CompanyStatus { get; set; }
    public virtual ICollection<CompanyAssignedRole> CompanyAssignedRoles { get; private set; }
    public virtual ICollection<CompanyServiceAccount> CompanyServiceAccounts { get; private set; }
    public virtual ICollection<CompanyUser> CompanyUsers { get; private set; }
    public virtual ICollection<CompanyRole> CompanyRoles { get; private set; }
    public virtual ICollection<Consent> Consents { get; private set; }
    public virtual ICollection<Connector> HostedConnectors { get; private set; }
    public virtual ICollection<IdentityProvider> IdentityProviders { get; private set; }
    public virtual ICollection<Offer> ProvidedOffers { get; private set; }
    public virtual ICollection<Connector> ProvidedConnectors { get; private set; }
    public virtual ICollection<UseCase> UseCases { get; private set; }
}