/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class MailingInformation : IBaseEntity
{
    private MailingInformation()
    {
        Email = null!;
        Template = null!;
        MailParameter = null!;
    }

    public MailingInformation(Guid id, Guid processId, string email, string template, IReadOnlyDictionary<string, string> mailParameter, MailingStatusId mailingStatusId)
        : this()
    {
        Id = id;
        ProcessId = processId;
        Email = email;
        Template = template;
        MailParameter = mailParameter;
        MailingStatusId = mailingStatusId;
    }

    public Guid Id { get; }

    public Guid ProcessId { get; set; }

    public string Email { get; set; }

    public string Template { get; set; }

    public MailingStatusId MailingStatusId { get; set; }

    public IReadOnlyDictionary<string, string> MailParameter { get; set; }

    public virtual Process? Process { get; private set; }

    public virtual MailingStatus? MailingStatus { get; private set; }
}
