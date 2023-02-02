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

using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.Template;
using Microsoft.Extensions.Options;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Mail
{
    public class UserEmail : IUserEmail
    {
        public static readonly string ProviderPosition = "MailingService:Provider";
        public static readonly string TemplatePosition = "MailingService:Templates";
        public static readonly string UserEmailPosition = "MailingService:UserEmail";
        private readonly UserEmailSettings _Settings;
        private readonly ITemplateManager _TemplateManager;
        private readonly ISendMail _SendMail;

        public UserEmail( ITemplateManager templateManager, ISendMail sendMail, IOptions<UserEmailSettings> settings)
        {
            _TemplateManager = templateManager;
            _SendMail = sendMail;
            _Settings = settings.Value;
        }

        public async Task SendMailAsync(string email, string firstName, string lastName, string realm)
        {
            var templateParams = new Dictionary<string, string> {
                { "firstname", firstName },
                { "lastname", lastName },
                { "realm", realm }
            };
            var inviteMail = await _TemplateManager.ApplyTemplateAsync(_Settings.Template, templateParams).ConfigureAwait(false);
            await _SendMail.Send(_Settings.SenderEmail, email, inviteMail.Subject, inviteMail.Body, inviteMail.isHtml).ConfigureAwait(false);
        }
    }
}
