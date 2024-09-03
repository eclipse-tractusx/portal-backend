/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.AuditEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Auditing;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

[AuditEntityV1(typeof(AuditCertificateManagement20240416))]
public class CompanyCertificate : IAuditableV1, IBaseEntity
{
    private CompanyCertificate()
    {
        CompanyCertificateAssignedSites = new HashSet<CompanyCertificateAssignedSite>();
    }

    public CompanyCertificate(Guid id, CompanyCertificateTypeId companyCertificateTypeId, CompanyCertificateStatusId companyCertificateStatusId, Guid companyId, Guid documentId) : this()
    {
        Id = id;
        CompanyCertificateTypeId = companyCertificateTypeId;
        CompanyCertificateStatusId = companyCertificateStatusId;
        CompanyId = companyId;
        DocumentId = documentId;
    }

    public Guid Id { get; private set; }
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidTill { get; set; }
    public CompanyCertificateTypeId CompanyCertificateTypeId { get; set; }
    public CompanyCertificateStatusId CompanyCertificateStatusId { get; set; }
    public Guid CompanyId { get; private set; }
    public Guid DocumentId { get; private set; }
    public string? ExternalCertificateNumber { get; set; }
    public string? Issuer { get; set; }
    public string? TrustLevel { get; set; }
    public string? Validator { get; set; }

    [LastChangedV1]
    public DateTimeOffset? DateLastChanged { get; set; }

    [LastEditorV1]
    public Guid? LastEditorId { get; private set; }

    // Navigation Properties
    public virtual Company? Company { get; private set; }
    public virtual Document? Document { get; private set; }
    public virtual CompanyCertificateType? CompanyCertificateType { get; private set; }
    public virtual CompanyCertificateStatus? CompanyCertificateStatus { get; private set; }
    public virtual ICollection<CompanyCertificateAssignedSite> CompanyCertificateAssignedSites { get; private set; }
}
