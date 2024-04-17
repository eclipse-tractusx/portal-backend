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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class MailBusinessLogic(IPortalRepositories portalRepositories, IMailingProcessCreation mailingProcessCreation)
    : IMailBusinessLogic
{
    private static readonly IEnumerable<string> ValidTemplates =
        [
            "CredentialExpiry",
            "CredentialRejected",
            "CredentialApproval"
        ];

    public async Task SendMail(MailData mailData)
    {
        if (!ValidTemplates.Contains(mailData.Template))
        {
            throw ConflictException.Create(AdministrationMailErrors.INVALID_TEMPLATE, [new("template", mailData.Template)]);
        }

        var data = await portalRepositories.GetInstance<IUserRepository>().GetUserMailData(mailData.Requester).ConfigureAwait(false);
        if (!data.Exists)
        {
            throw NotFoundException.Create(AdministrationMailErrors.USER_NOT_FOUND, [new("userId", mailData.Requester.ToString())]);
        }

        if (data.RecipientMail is not null)
        {
            mailingProcessCreation.CreateMailProcess(data.RecipientMail, mailData.Template, mailData.MailParameters.ToImmutableDictionary(x => x.Key, x => x.Value));
        }
    }
}
