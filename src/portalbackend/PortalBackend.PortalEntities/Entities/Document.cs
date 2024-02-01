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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.AuditEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Auditing;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Base;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

[AuditEntityV1(typeof(AuditDocument20231115))]
public class Document : IAuditableV1, IBaseEntity
{
    private Document()
    {
        DocumentHash = null!;
        DocumentName = null!;
        DocumentContent = null!;
        Agreements = new HashSet<Agreement>();
        Consents = new HashSet<Consent>();
        Offers = new HashSet<Offer>();
        Companies = new HashSet<Company>();
        DocumentCompanyCertificate=new HashSet<CompanyCertificate>();
    }

    public Document(Guid id, byte[] documentContent, byte[] documentHash, string documentName, MediaTypeId mediaTypeId, DateTimeOffset dateCreated, DocumentStatusId documentStatusId, DocumentTypeId documentTypeId)
        : this()
    {
        Id = id;
        DocumentContent = documentContent;
        DocumentHash = documentHash;
        DocumentName = documentName;
        DateCreated = dateCreated;
        DocumentStatusId = documentStatusId;
        DocumentTypeId = documentTypeId;
        MediaTypeId = mediaTypeId;
    }

    public Guid Id { get; private set; }

    public DateTimeOffset DateCreated { get; private set; }

    public byte[] DocumentHash { get; set; }

    public byte[] DocumentContent { get; set; }

    [MaxLength(255)]
    public string DocumentName { get; set; }

    public MediaTypeId MediaTypeId { get; set; }

    public DocumentTypeId DocumentTypeId { get; set; }

    public DocumentStatusId DocumentStatusId { get; set; }

    public Guid? CompanyUserId { get; set; }

    [LastChangedV1]
    public DateTimeOffset? DateLastChanged { get; set; }

    [LastEditorV1]
    public Guid? LastEditorId { get; private set; }

    // Navigation properties
    public virtual CompanyUser? CompanyUser { get; set; }
    public virtual DocumentType? DocumentType { get; set; }
    public virtual MediaType? MediaType { get; set; }
    public virtual DocumentStatus? DocumentStatus { get; set; }

    /// <summary>
    /// Mapping to an optional the connector
    /// </summary>
    public virtual Connector? Connector { get; set; }

    public virtual CompanySsiDetail? CompanySsiDetail { get; set; }

    public virtual ICollection<Agreement> Agreements { get; private set; }
    public virtual ICollection<Consent> Consents { get; private set; }
    public virtual ICollection<Offer> Offers { get; private set; }
    public virtual ICollection<Company> Companies { get; private set; }
    public virtual ICollection<CompanyCertificate> DocumentCompanyCertificate { get; private set; }
}
