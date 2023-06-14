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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.Template.Attributes;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.Template.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.Template.Model;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Org.Eclipse.TractusX.Portal.Backend.Mailing.Template;

public class TemplateManager : ITemplateManager
{
    private readonly TemplateSettings _settings;

    private static readonly Regex _templateMatcherExpression = new Regex(@"\{(\w+)\}", RegexOptions.None, TimeSpan.FromSeconds(1)); // to replace any text surrounded by { and }

    public TemplateManager(IOptions<TemplateSettings> templateSettings)
    {
        _settings = templateSettings.Value;
    }

    public async Task<Mail> ApplyTemplateAsync(string id, IDictionary<string, string> parameters)
    {
        if (!_settings.Templates.Any(x => x.Name == id))
        {
            throw new NoSuchTemplateException(id);
        }

        var template = _settings.Templates.Single(x => x.Name == id).Setting;
        var body = template.EmailTemplateType.HasValue
            ? await GetTemplateStringFromType(template.EmailTemplateType.Value).ConfigureAwait(false)
            : template.Body;
        if (body == null)
        {
            throw new NoSuchTemplateException(id);
        }

        return new Mail(
            ReplaceValues(template.Subject, parameters),
            ReplaceValues(body, parameters),
            template.EmailTemplateType.HasValue
        );
    }

    private static async Task<string> GetTemplateStringFromType(EmailTemplateType type)
    {
        var path = typeof(EmailTemplateType)?
            .GetMember(type.ToString())?
            .FirstOrDefault(m => m.DeclaringType == typeof(EmailTemplateType))?
            .GetCustomAttribute<PathAttribute>()?.Path;

        if (path == null)
        {
            throw new NoSuchTemplateException(type.ToString());
        }

        try
        {
            return await File.ReadAllTextAsync(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/EmailTemplates/" + path).ConfigureAwait(false);
        }
        catch (IOException ioe)
        {
            throw new NoSuchTemplateException(path, ioe);
        }
    }

    private static string ReplaceValues(string template, IDictionary<string, string> parameters) =>
        _templateMatcherExpression.Replace(
            template,
            m => parameters.TryGetValue(m.Groups[1].Value, out var value) ? value : "null");
}
