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

using System.ComponentModel.DataAnnotations;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class DocumentTemplate
{
    private DocumentTemplate()
    {
        Documenttemplatename = null!;
        Documenttemplateversion = null!;
    }

    public DocumentTemplate(Guid id, string documenttemplatename, string documenttemplateversion, DateTimeOffset dateCreated)
    {
        Id = id;
        Documenttemplatename = documenttemplatename;
        Documenttemplateversion = documenttemplateversion;
        DateCreated = dateCreated;
    }

    public Guid Id { get; private set; }

    public DateTimeOffset DateCreated { get; private set; }

    public DateTimeOffset? DateLastChanged { get; set; }

    [MaxLength(255)]
    public string Documenttemplatename { get; set; }

    [MaxLength(255)]
    public string Documenttemplateversion { get; set; }

    // Navigation properties
    public virtual AgreementAssignedDocumentTemplate? AgreementAssignedDocumentTemplate { get; set; }
}
