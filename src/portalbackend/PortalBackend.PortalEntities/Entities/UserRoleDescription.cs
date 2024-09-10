/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

public class UserRoleDescription
{
    private UserRoleDescription()
    {
        UserRoleId = Guid.Empty;
        LanguageShortName = null!;
        Description = null!;
    }

    public UserRoleDescription(Guid userRoleId, string languageShortName, string description)
    {
        UserRoleId = userRoleId;
        LanguageShortName = languageShortName;
        Description = description;
    }

    public Guid UserRoleId { get; private set; }

    [StringLength(2, MinimumLength = 2)]
    public string LanguageShortName { get; private set; }

    [MaxLength(255)]
    public string Description { get; set; }

    public virtual UserRole? UserRole { get; private set; }
    public virtual Language? Language { get; private set; }
}
