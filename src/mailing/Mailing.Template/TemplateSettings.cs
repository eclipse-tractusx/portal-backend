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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.Template.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Mailing.Template;

public class TemplateSettings
{
    /// <summary>
    /// Default path for the mailing template section in the configuration file
    /// </summary>        
    public const string Position = "MailingService";

    public IEnumerable<TemplateInfo> Templates { get; set; } = null!;

    public bool Validate()
    {
        var validation = new ConfigurationValidation<TemplateSettings>()
            .NotNull(Templates, () => nameof(Templates));

        if (Templates.GroupBy(x => x.Name).Any(x => x.Count() > 1))
        {
            throw new ConfigurationException($"{nameof(Templates)}: The name of the tempalte must be unique");
        }

        return Templates.Select(x => x.Setting).All(t =>
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
public record TemplateInfo(string Name, TemplateSetting Setting);

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
