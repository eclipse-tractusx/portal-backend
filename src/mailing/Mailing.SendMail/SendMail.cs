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

using MailKit.Net.Proxy;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail
{
    public class SendMail : ISendMail
    {
        private readonly MailSettings _MailSettings;

        public SendMail(IOptions<MailSettings> mailSettings)
        {
            _MailSettings = mailSettings.Value;
        }

        Task ISendMail.Send(string sender, string recipient, string subject, string body, bool useHtml)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(sender));
            message.To.Add(MailboxAddress.Parse(recipient));
            message.Subject = subject;
            if (useHtml)
            {
                message.Body = new TextPart("html") { Text = body };
            }
            else
            {
                message.Body = new TextPart("plain") { Text = body };
            }
            return _send(message);
        }

        private async Task _send(MimeMessage message)
        {
            using (var client = new SmtpClient())
            {
                if (_MailSettings.HttpProxy != null)
                {
                    client.ProxyClient = new HttpProxyClient(_MailSettings.HttpProxy, _MailSettings.HttpProxyPort);
                }
                await client.ConnectAsync(_MailSettings.SmtpHost, _MailSettings.SmtpPort);
                await client.AuthenticateAsync(_MailSettings.SmtpUser, _MailSettings.SmtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
