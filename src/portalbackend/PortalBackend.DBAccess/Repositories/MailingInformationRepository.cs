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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class MailingInformationRepository : IMailingInformationRepository
{
    private readonly PortalDbContext _context;

    public MailingInformationRepository(PortalDbContext context)
    {
        _context = context;
    }

    public IAsyncEnumerable<(Guid Id, string EmailAddress, string Template, Dictionary<string, string> MailParameter)> GetMailingInformationForProcess(Guid processId) =>
        _context.MailingInformations.Where(x => x.ProcessId == processId && x.MailingStatusId == MailingStatusId.PENDING)
            .Select(x => new ValueTuple<Guid, string, string, Dictionary<string, string>>(
                    x.Id,
                    x.Email,
                    x.Template,
                    x.MailParameter
                ))
            .ToAsyncEnumerable();

    public MailingInformation CreateMailingInformation(Guid processId, string email, string template, Dictionary<string, string> mailParameter)
    {
        var mailingInformation = new MailingInformation(Guid.NewGuid(), processId, email, template, mailParameter, MailingStatusId.PENDING);
        return _context.Add(mailingInformation).Entity;
    }

    public void AttachAndModifyMailingInformation(Guid id, Action<MailingInformation> initialize, Action<MailingInformation> setFields)
    {
        var mailingInformation = new MailingInformation(id, Guid.Empty, null!, null!, null!, default);
        initialize.Invoke(mailingInformation);
        _context.Attach(mailingInformation);
        setFields.Invoke(mailingInformation);
    }
}
