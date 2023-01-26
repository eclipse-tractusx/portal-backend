/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.Template.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Org.Eclipse.TractusX.Portal.Backend.Mailing.Template;

public class TemplateSettings
{
    /// <summary>
    /// Default path for the mailing template section in the configuration file
    /// </summary>        
    public const string Position = "MailingService";

    public Dictionary<string, TemplateSetting> Templates { get; set; } = null!;

    public bool Validate()
    {
        var validation = new ConfigurationValidation<TemplateSettings>()
            .NotNull(Templates, () => nameof(Templates));

        return Templates.Values.All(t =>
        {
            validation.NotNullOrWhiteSpace(t.Subject, () => nameof(t.Subject));
            if (t.Body == null)
            {
                validation.NotNull(t.EmailTemplateType, () => nameof(t.EmailTemplateType));
            }
            if (!t.EmailTemplateType.HasValue)
            {
                validation.NotNullOrWhiteSpace(t.Body, () => nameof(t.Body));
            }
            return true;
        });
    }
}

/// <summary>
/// Configuration for templated emails that a service can send.
/// </summary>
public class TemplateSetting
{
    /// <summary>
    /// Subject of the email to be sent.
    /// </summary>
    public string Subject { get; set; } = null!;

    /// <summary>
    /// Body of the email to be sent (in case of non-html-templated emails)
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Template type to be used for the email (in case of html template).
    /// </summary>
    public EmailTemplateType? EmailTemplateType { get; set; }
}

public static class TemplateSettingsExtention
{
    public static IServiceCollection ConfigureTemplateSettings(
        this IServiceCollection services,
        IConfigurationSection section)
    {
        services.AddOptions<TemplateSettings>()
            .Bind(section)
            .Validate(x => x.Validate())
            .ValidateOnStart();
        return services;
    }
}
