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

using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class Document
{
    private Document()
    {
        DocumentHash = null!;
        DocumentName = null!;
        DocumentContent = null!;
        Consents = new HashSet<Consent>();
        Offers = new HashSet<Offer>();
    }
    
    /// <summary>
    /// Please only use when attaching the Document to the database
    /// </summary>
    /// <param name="id"></param>
    public Document(Guid id) : this()
    {
        Id = id;
    }

    public Document(Guid id, byte[] documentContent, byte[] documentHash, string documentName, DateTimeOffset dateCreated, DocumentStatusId documentStatusId) : this()
    {
        Id = id;
        DocumentContent = documentContent;
        DocumentHash = documentHash;
        DocumentName = documentName;
        DateCreated = dateCreated;
        DocumentStatusId = documentStatusId;
    }

    public Guid Id { get; private set; }

    public DateTimeOffset DateCreated { get; private set; }

    public byte[] DocumentHash { get; set; }

    public byte[] DocumentContent { get; set; }

    [MaxLength(255)]
    public string DocumentName { get; set; }

    public DocumentTypeId DocumentTypeId { get; set; }

    public DocumentStatusId DocumentStatusId { get; set; }

    public Guid? CompanyUserId { get; set; }

    // Navigation properties
    public virtual CompanyUser? CompanyUser { get; set; }
    public virtual DocumentType? DocumentType { get; set; }
    public virtual DocumentStatus? DocumentStatus { get; set; }
    public virtual ICollection<Consent> Consents { get; private set; }
    public virtual ICollection<Offer> Offers { get; private set; }
}
