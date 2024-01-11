/********************************************************************************
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Base;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class CompanyInvitation : IBaseEntity
{
    private CompanyInvitation()
    {
        FirstName = null!;
        LastName = null!;
        Email = null!;
        OrganisationName = null!;
    }

    public CompanyInvitation(Guid id, string firstName, string lastName, string email, string organisationName, Guid processId)
        : this()
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        OrganisationName = organisationName;
        ProcessId = processId;
    }

    public Guid Id { get; set; }

    public string? UserName { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public string OrganisationName { get; set; }

    public Guid ProcessId { get; set; }

    public Guid? ApplicationId { get; set; }

    public string? IdpName { get; set; }

    public byte[]? Password { get; set; }
    public string? ClientId { get; set; }
    public byte[]? ClientSecret { get; set; }
    public string? ServiceAccountUserId { get; set; }

    public virtual Process? Process { get; private set; }

    public virtual CompanyApplication? Application { get; private set; }
}
