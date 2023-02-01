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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Base;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class Agreement : IBaseEntity
{
    private Agreement()
    {
        Name = null!;
        Consents = new HashSet<Consent>();
        AgreementAssignedCompanyRoles = new HashSet<AgreementAssignedCompanyRole>();
        AgreementAssignedOffers = new HashSet<AgreementAssignedOffer>();
        AgreementAssignedOfferTypes = new HashSet<AgreementAssignedOfferType>();
        Documents = new HashSet<Document>();
    }

    public Agreement(Guid id, AgreementCategoryId agreementCategoryId, string name, DateTimeOffset dateCreated) : this()
    {
        Id = id;
        AgreementCategoryId = agreementCategoryId;
        Name = name;
        DateCreated = dateCreated;
    }

    public AgreementCategoryId AgreementCategoryId { get; private set; }

    public Guid Id { get; private set; }

    public DateTimeOffset DateCreated { get; private set; }

    public DateTimeOffset? DateLastChanged { get; set; }

    [MaxLength(255)]
    public string? AgreementType { get; set; }

    [MaxLength(255)]
    public string Name { get; set; }

    public Guid IssuerCompanyId { get; set; }

    public Guid? UseCaseId { get; set; }

    // Navigation properties
    public virtual AgreementCategory? AgreementCategory { get; set; }
    public virtual Company? IssuerCompany { get; set; }
    public virtual UseCase? UseCase { get; set; }
    public virtual ICollection<Consent> Consents { get; private set; }
    public virtual ICollection<AgreementAssignedCompanyRole> AgreementAssignedCompanyRoles { get; private set; }
    public virtual ICollection<AgreementAssignedOffer> AgreementAssignedOffers { get; private set; }
    public virtual ICollection<AgreementAssignedOfferType> AgreementAssignedOfferTypes { get; private set; }
    public virtual ICollection<Document> Documents { get; private set; }
}
