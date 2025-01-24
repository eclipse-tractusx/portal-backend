/********************************************************************************
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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

public class AgreementDescription
{
    public AgreementDescription(Guid agreementId, string languageShortName, string description)
    {
        AgreementId = agreementId;
        LanguageShortName = languageShortName;
        Description = description;
    }

    public Guid AgreementId { get; private set; }

    [MaxLength(2)]
    public string LanguageShortName { get; private set; }

    [MaxLength(255)]
    public string Description { get; set; }

    // Navigation properties
    public virtual Agreement? Agreement { get; private set; }
    public virtual Language? Language { get; private set; }
}
