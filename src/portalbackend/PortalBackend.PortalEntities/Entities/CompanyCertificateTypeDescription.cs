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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;

public class CompanyCertificateTypeDescription
{
    private CompanyCertificateTypeDescription()
    {
        ShortName = null!;
        Description = null!;
    }

    public CompanyCertificateTypeDescription(CompanyCertificateTypeId id, string shortName, string description)
    {
        Id = id;
        ShortName = shortName;
        Description = description;
    }
    public CompanyCertificateTypeId Id { get; private set; }

    [StringLength(2, MinimumLength = 2)]
    public string ShortName { get; private set; }

    public string Description { get; private set; }

    // Navigation Properties
    public virtual Language? Language { get; private set; }

    public virtual CompanyCertificateType? CompanyCertificateType { get; private set; }
}
