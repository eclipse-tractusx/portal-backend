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

using Org.Eclipse.TractusX.Portal.Backend.Mailing.Template;

namespace Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;

public class MailingService : IMailingService
{
    private readonly ITemplateManager _templateManager;
    private readonly ISendMail _sendMail;

    public MailingService( ITemplateManager templateManager, ISendMail sendMail)
    {
        _templateManager = templateManager;
        _sendMail = sendMail;
    }

    public async Task SendMails(string recipient, IDictionary<string, string> parameters, IEnumerable<string> templates)
    {
        foreach(var temp in templates)
        {
            var email = await _templateManager.ApplyTemplateAsync(temp, parameters).ConfigureAwait(false);
            await _sendMail.Send("Notifications@catena-x.net", recipient, email.Subject, email.Body, email.isHtml).ConfigureAwait(false);
        }
    }
}
