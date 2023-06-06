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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class IdentityProviderSettings
{
    public IdentityProviderCsvSettings CsvSettings { get; init; } = null!;

    public bool Validate()
    {
        new ConfigurationValidation<IdentityProviderSettings>().NotNull(CsvSettings, () => nameof(CsvSettings));
        return CsvSettings.Validate();
    }
}

public class IdentityProviderCsvSettings
{
    public string Charset { get; init; } = null!;
    public Encoding Encoding { get; set; } = default!;
    public string FileName { get; init; } = null!;
    public string ContentType { get; init; } = null!;
    public string Separator { get; init; } = null!;
    public string HeaderUserId { get; init; } = null!;
    public string HeaderFirstName { get; init; } = null!;
    public string HeaderLastName { get; init; } = null!;
    public string HeaderEmail { get; init; } = null!;
    public string HeaderProviderAlias { get; init; } = null!;
    public string HeaderProviderUserId { get; init; } = null!;
    public string HeaderProviderUserName { get; init; } = null!;

    public bool Validate()
    {
        new ConfigurationValidation<IdentityProviderCsvSettings>()
            .NotNull(Charset, () => nameof(Charset))
            .NotNullOrWhiteSpace(FileName, () => nameof(FileName))
            .NotNullOrWhiteSpace(ContentType, () => nameof(ContentType))
            .NotNullOrWhiteSpace(Separator, () => nameof(Separator))
            .NotNullOrWhiteSpace(HeaderUserId, () => nameof(HeaderUserId))
            .NotNullOrWhiteSpace(HeaderFirstName, () => nameof(HeaderFirstName))
            .NotNullOrWhiteSpace(HeaderLastName, () => nameof(HeaderLastName))
            .NotNullOrWhiteSpace(HeaderEmail, () => nameof(HeaderEmail))
            .NotNullOrWhiteSpace(HeaderProviderAlias, () => nameof(HeaderProviderAlias))
            .NotNullOrWhiteSpace(HeaderProviderUserId, () => nameof(HeaderProviderUserId))
            .NotNullOrWhiteSpace(HeaderProviderUserName, () => nameof(HeaderProviderUserName));
        try
        {
            Encoding = Encoding.GetEncoding(Charset);
        }
        catch (ArgumentException ae)
        {
            throw new ConfigurationException($"'{nameof(IdentityProviderCsvSettings)}': {nameof(Charset)} '{Charset}' is not a valid Encoding", ae);
        }

        return true;
    }
}

public static class IdentityProviderSettingsExtension
{
    public static IServiceCollection ConfigureIdentityProviderSettings(
        this IServiceCollection services,
        IConfigurationSection section)
    {
        services.AddOptions<IdentityProviderSettings>()
            .Bind(section)
            .Validate(x => x.Validate())
            .ValidateOnStart();
        return services;
    }
}
