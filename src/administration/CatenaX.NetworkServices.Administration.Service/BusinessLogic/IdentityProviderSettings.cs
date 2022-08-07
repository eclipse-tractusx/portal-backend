/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using CatenaX.NetworkServices.Framework.ErrorHandling;
using System.Text;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class IdentityProviderSettings
{
    public IdentityProviderCsvSettings CsvSettings { get; init; } = null!;
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
}

public static class IdentityProviderSettingsExtension
{
    public static IServiceCollection ConfigureIdentityProviderSettings(
        this IServiceCollection services,
        IConfigurationSection section) =>
        services.Configure<IdentityProviderSettings>(x =>
            {
                section.Bind(x);
                var csvSettings = x.CsvSettings;
                ConfigurationValidation.ValidateNotNull<IdentityProviderSettings>(csvSettings,()=>nameof(x.CsvSettings));
                ConfigurationValidation.ValidateNotNull<IdentityProviderSettings>(csvSettings.Charset,()=>nameof(csvSettings.Charset));
                try
                {
                    csvSettings.Encoding = Encoding.GetEncoding(csvSettings.Charset);
                }
                catch(ArgumentException ae)
                {
                    throw new ConfigurationException($"'{nameof(IdentityProviderSettings)}': {nameof(csvSettings.Charset)} '{csvSettings.Charset}' is not a valid Encoding", ae);
                }
                ConfigurationValidation.ValidateNotNullOrWhiteSpace<IdentityProviderSettings>(csvSettings.FileName,()=>nameof(csvSettings.FileName));
                ConfigurationValidation.ValidateNotNullOrWhiteSpace<IdentityProviderSettings>(csvSettings.ContentType,()=>nameof(csvSettings.ContentType));
                ConfigurationValidation.ValidateNotNullOrWhiteSpace<IdentityProviderSettings>(csvSettings.Separator,()=>nameof(csvSettings.Separator));
                ConfigurationValidation.ValidateNotNullOrWhiteSpace<IdentityProviderSettings>(csvSettings.HeaderUserId,()=>nameof(csvSettings.HeaderUserId));
                ConfigurationValidation.ValidateNotNullOrWhiteSpace<IdentityProviderSettings>(csvSettings.HeaderFirstName,()=>nameof(csvSettings.HeaderFirstName));
                ConfigurationValidation.ValidateNotNullOrWhiteSpace<IdentityProviderSettings>(csvSettings.HeaderLastName,()=>nameof(csvSettings.HeaderLastName));
                ConfigurationValidation.ValidateNotNullOrWhiteSpace<IdentityProviderSettings>(csvSettings.HeaderEmail,()=>nameof(csvSettings.HeaderEmail));
                ConfigurationValidation.ValidateNotNullOrWhiteSpace<IdentityProviderSettings>(csvSettings.HeaderProviderAlias,()=>nameof(csvSettings.HeaderProviderAlias));
                ConfigurationValidation.ValidateNotNullOrWhiteSpace<IdentityProviderSettings>(csvSettings.HeaderProviderUserId,()=>nameof(csvSettings.HeaderProviderUserId));
                ConfigurationValidation.ValidateNotNullOrWhiteSpace<IdentityProviderSettings>(csvSettings.HeaderProviderUserName,()=>nameof(csvSettings.HeaderProviderUserName));
            });
}
