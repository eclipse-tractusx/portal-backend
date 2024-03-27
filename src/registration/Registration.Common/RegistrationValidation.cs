/********************************************************************************
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Text.RegularExpressions;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Common;

public static class RegistrationValidation
{
    private static readonly Regex BpnRegex = new(ValidationExpressions.Bpn, RegexOptions.Compiled, TimeSpan.FromSeconds(1));

    public static void ValidateData(this RegistrationData data)
    {
        if (data.BusinessPartnerNumber != null && !BpnRegex.IsMatch(data.BusinessPartnerNumber))
        {
            throw new ControllerArgumentException("BPN must contain exactly 16 characters and must be prefixed with BPNL", nameof(data.BusinessPartnerNumber));
        }

        if (string.IsNullOrWhiteSpace(data.Name))
        {
            throw new ControllerArgumentException("Name must not be empty", nameof(data.Name));
        }

        if (string.IsNullOrWhiteSpace(data.City))
        {
            throw new ControllerArgumentException("City must not be empty", nameof(data.City));
        }

        if (string.IsNullOrWhiteSpace(data.StreetName))
        {
            throw new ControllerArgumentException("Streetname must not be empty", nameof(data.StreetName));
        }

        if (data.CountryAlpha2Code.Length != 2)
        {
            throw new ControllerArgumentException("CountryAlpha2Code must be 2 chars",
                nameof(data.CountryAlpha2Code));
        }

        var emptyIds = data.UniqueIds.Where(uniqueId => string.IsNullOrWhiteSpace(uniqueId.Value));
        if (emptyIds.Any())
        {
            throw new ControllerArgumentException(
                $"uniqueIds must not contain empty values: '{string.Join(", ", emptyIds.Select(uniqueId => uniqueId.UniqueIdentifierId))}'",
                nameof(data.UniqueIds));
        }

        var distinctIds = data.UniqueIds.DistinctBy(uniqueId => uniqueId.UniqueIdentifierId);
        if (distinctIds.Count() < data.UniqueIds.Count())
        {
            var duplicateIds = data.UniqueIds.Except(distinctIds);
            throw new ControllerArgumentException(
                $"uniqueIds must not contain duplicate types: '{string.Join(", ", duplicateIds.Select(uniqueId => uniqueId.UniqueIdentifierId))}'",
                nameof(data.UniqueIds));
        }
    }

    public static async Task ValidateDatabaseData(this RegistrationData data, Func<string, Task<bool>> checkBpn, Func<string, Task<bool>> checkCountryExistByAlpha2Code, Func<string, IEnumerable<UniqueIdentifierId>, Task<(bool IsValidCountry, IEnumerable<UniqueIdentifierId> UniqueIdentifierIds)>> getCountryAssignedIdentifiers, bool checkBpnAlreadyExists)
    {
        if (data.BusinessPartnerNumber != null && checkBpnAlreadyExists && await checkBpn(data.BusinessPartnerNumber.ToUpper()).ConfigureAwait(ConfigureAwaitOptions.None))
        {
            throw new ControllerArgumentException($"The Bpn {data.BusinessPartnerNumber} already exists", nameof(data.BusinessPartnerNumber));
        }

        if (!await checkCountryExistByAlpha2Code(data.CountryAlpha2Code).ConfigureAwait(ConfigureAwaitOptions.None))
        {
            throw new ControllerArgumentException($"Location {data.CountryAlpha2Code} does not exist", nameof(data.CountryAlpha2Code));
        }

        if (data.UniqueIds.Any())
        {
            var assignedIdentifiers = await getCountryAssignedIdentifiers(
                    data.CountryAlpha2Code,
                    data.UniqueIds.Select(uniqueId => uniqueId.UniqueIdentifierId))
                .ConfigureAwait(ConfigureAwaitOptions.None);

            if (!assignedIdentifiers.IsValidCountry)
            {
                throw new ControllerArgumentException($"{data.CountryAlpha2Code} is not a valid country-code", nameof(data.UniqueIds));
            }

            if (assignedIdentifiers.UniqueIdentifierIds.Count() < data.UniqueIds.Count())
            {
                var invalidIds = data.UniqueIds.ExceptBy(assignedIdentifiers.UniqueIdentifierIds, uniqueId => uniqueId.UniqueIdentifierId);
                throw new ControllerArgumentException($"invalid uniqueIds for country {data.CountryAlpha2Code}: '{string.Join(", ", invalidIds.Select(uniqueId => uniqueId.UniqueIdentifierId))}'", nameof(data.UniqueIds));
            }
        }
    }
}
