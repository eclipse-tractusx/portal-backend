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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;

namespace Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail
{
    public class MailSettings
    {
        public const string Position = "MailingService:Mail";
        public string SmtpHost { get; set; } = null!;
        public string SmtpUser { get; set; } = null!;
        public string SmtpPassword { get; set; } = null!;
        public int SmtpPort { get; set; } = 0;
        public string? HttpProxy { get; set; }
        public int HttpProxyPort { get; set; }
        public string SenderEmail { get; set; } = null!;

        public bool Validate()
        {
            var validation = new ConfigurationValidation<MailSettings>()
                .NotNullOrWhiteSpace(SmtpHost, () => nameof(SmtpHost))
                .NotNullOrWhiteSpace(SmtpUser, () => nameof(SmtpUser))
                .NotNullOrWhiteSpace(SmtpPassword, () => nameof(SmtpPassword))
                .NotNullOrWhiteSpace(SenderEmail, () => nameof(SenderEmail));

            if (HttpProxy != null)
            {
                validation.NotNullOrWhiteSpace(HttpProxy, () => nameof(HttpProxy));
            }
            return true;
        }
    }

    public static class MailSettingsExtension
    {
        public static IServiceCollection ConfigureMailSettings(
            this IServiceCollection services,
            IConfigurationSection section
            )
        {
            services.AddOptions<MailSettings>()
                .Bind(section)
                .Validate(x => x.Validate())
                .ValidateOnStart();
            return services;
        }
    }
}
