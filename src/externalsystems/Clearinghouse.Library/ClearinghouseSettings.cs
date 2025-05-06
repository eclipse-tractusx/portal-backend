/********************************************************************************
 * Copyright (c) 2022 BMW Group AG
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library;

/// <summary>
/// Settings used in business logic concerning connectors.
/// </summary>
public class ClearinghouseSettings
{
    [Required(AllowEmptyStrings = false)]
    public string CallbackUrl { get; set; } = null!;

    [Required]
    public ClearinghouseCredentialsSettings DefaultClearinghouseCredentials { get; set; } = null!;
    public IEnumerable<ClearinghouseCredentialsSettings> RegionalClearinghouseCredentials { get; set; } = [];

    public bool Validate()
    {
        if (RegionalClearinghouseCredentials.IsNullOrEmpty())
            return true;

        RegionalClearinghouseCredentials.DuplicatesBy(x => x.CountryAlpha2Code)
            .IfAny(duplicate => throw new ConfigurationException($"CountryCodes {string.Join(", ", duplicate.Select(x => x.CountryAlpha2Code))} are ambiguous"));

        return RegionalClearinghouseCredentials.Any(c => c.Validate());
    }
}

public class ClearinghouseCredentialsSettings : KeyVaultAuthSettings
{
    [Required(AllowEmptyStrings = false)]
    public string BaseAddress { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    public string ValidationPath { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    public string CountryAlpha2Code { get; set; } = null!;

    /// <summary>
    ///  If <c>true</c> all sd factory calls are disabled and won't be called. The respective process steps will be skipped.
    /// </summary>
    public bool ClearinghouseConnectDisabled { get; set; }

    public bool Validate()
    {
        new ConfigurationValidation<ClearinghouseCredentialsSettings>()
            .NotNullOrWhiteSpace(BaseAddress, () => nameof(BaseAddress))
            .NotNullOrWhiteSpace(ValidationPath, () => nameof(ValidationPath))
            .NotNullOrWhiteSpace(CountryAlpha2Code, () => nameof(CountryAlpha2Code))
            .NotNullOrWhiteSpace(TokenAddress, () => nameof(TokenAddress));

        var hasUsernamePassword = !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        var hasClientIdSecret = !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);
        if (!hasUsernamePassword && !hasClientIdSecret)
        {
            throw new ConfigurationException("Either Username and Password, or ClientId and ClientSecret must be provided.");
        }

        return true;
    }
}
