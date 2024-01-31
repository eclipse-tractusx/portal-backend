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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using Document = Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities.Document;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;

public class CompanyCertificate
{

    private CompanyCertificate()
    {

    }
    public CompanyCertificate(Guid guid, DateTimeOffset dateTimeOffset, CompanyCertificateTypeId companyCertificateTypeId, CompanyCertificateStatusId companyCertificateStatusId, Guid comapnyId, Guid documentId)
    {
        Id = guid;
        ValidFrom = dateTimeOffset;
        CompanyCertificateTypeId = companyCertificateTypeId;
        CompanyCertificateStatusId = companyCertificateStatusId;
        CompanyId = comapnyId;
        DocumentId = documentId;

    }
    public Guid Id { get; set; }
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset? ValidTill { get; set; }
    public CompanyCertificateTypeId CompanyCertificateTypeId { get; private set; }
    public CompanyCertificateStatusId CompanyCertificateStatusId { get; set; }
    public Guid CompanyId { get; private set; }
    public Guid DocumentId { get; private set; }

    // Navigation Properties
    public virtual Company? Company { get; private set; }
    public virtual Document? Document { get; private set; }
    public virtual CompanyCertificateType? CompanyCertificateType { get; private set; }
    public virtual CompanyCertificateStatus? CompanyCertificateStatus { get; private set; }
}