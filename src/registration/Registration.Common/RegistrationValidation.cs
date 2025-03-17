/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Common.ErrorHandling;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Common;

public static class RegistrationValidation
{
    private static readonly Regex BpnRegex = new(ValidationExpressions.Bpn, RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    private static readonly Regex RegionRegex = new(ValidationExpressions.Region, RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    private static readonly IReadOnlyDictionary<(UniqueIdentifierId, string), Regex> UniqueIdentifierExpressions = ImmutableDictionary.CreateRange(
        new (UniqueIdentifierId Identifier, IEnumerable<(string Country, string Expression)> Expressions)[]
        {
            (UniqueIdentifierId.COMMERCIAL_REG_NUMBER, ValidationExpressions.COMMERCIAL_REG_NUMBER),
            (UniqueIdentifierId.VAT_ID, ValidationExpressions.VAT_ID),
            (UniqueIdentifierId.VIES, ValidationExpressions.VIES),
            (UniqueIdentifierId.EORI, ValidationExpressions.EORI),
            (UniqueIdentifierId.LEI_CODE, ValidationExpressions.LEI_CODE)
        }.SelectMany(x => x.Expressions.Select(y => KeyValuePair.Create((x.Identifier, y.Country), new Regex(y.Expression, RegexOptions.Compiled, TimeSpan.FromSeconds(1))))));

    public static void ValidateData(this RegistrationData data)
    {
        if (!string.IsNullOrEmpty(data.BusinessPartnerNumber) && !BpnRegex.IsMatch(data.BusinessPartnerNumber))
        {
            throw ControllerArgumentException.Create(RegistrationValidationErrors.BPN_INVALID);
        }

        if (string.IsNullOrWhiteSpace(data.Name))
        {
            throw ControllerArgumentException.Create(RegistrationValidationErrors.NAME_NOT_EMPTY);
        }

        if (string.IsNullOrWhiteSpace(data.City))
        {
            throw ControllerArgumentException.Create(RegistrationValidationErrors.CITY_NOT_EMPTY);
        }

        if (string.IsNullOrWhiteSpace(data.StreetName))
        {
            throw ControllerArgumentException.Create(RegistrationValidationErrors.STREET_NOT_EMPTY);
        }

        if (data.CountryAlpha2Code.Length != 2)
        {
            throw ControllerArgumentException.Create(RegistrationValidationErrors.COUNTRY_CODE_MIN_LENGTH);
        }

        if (!RegionRegex.IsMatch(data.Region))
        {
            throw ControllerArgumentException.Create(RegistrationValidationErrors.REGION_INVALID);
        }

        var emptyIds = data.UniqueIds.Where(uniqueId => string.IsNullOrWhiteSpace(uniqueId.Value));
        if (emptyIds.Any())
        {
            throw ControllerArgumentException.Create(
                RegistrationValidationErrors.UNIQUE_IDS_NO_EMPTY_VALUES,
                Enumerable.Repeat(new ErrorParameter("emptyValues", string.Join(", ", emptyIds.Select(uniqueId => uniqueId.UniqueIdentifierId))), 1));
        }

        var distinctIds = data.UniqueIds.DistinctBy(uniqueId => uniqueId.UniqueIdentifierId);
        if (distinctIds.Count() < data.UniqueIds.Count())
        {
            var duplicateIds = data.UniqueIds.Except(distinctIds);
            throw ControllerArgumentException.Create(
                RegistrationValidationErrors.UNIQUE_IDS_NO_DUPLICATE_VALUES,
                Enumerable.Repeat(new ErrorParameter("duplicateValues", string.Join(", ", duplicateIds.Select(uniqueId => uniqueId.UniqueIdentifierId))), 1));
        }

        data.UniqueIds.Where(uniqueId => !IsValidUniqueIdentifier(uniqueId.Value, uniqueId.UniqueIdentifierId, data.CountryAlpha2Code))
            .IfAny(invalidUniqueIdentifiersValues =>
                {
                    throw new ControllerArgumentException(
                        $"Invalid value of uniqueIds: '{string.Join(", ", invalidUniqueIdentifiersValues.Select(uniqueId => uniqueId.UniqueIdentifierId))}'",
                        nameof(data.UniqueIds));
                });
    }

    public static async Task ValidateDatabaseData(this RegistrationData data, Func<string, Task<bool>> checkBpn, Func<string, Task<bool>> checkCountryExistByAlpha2Code, Func<string, IEnumerable<UniqueIdentifierId>, Task<(bool IsValidCountry, IEnumerable<UniqueIdentifierId> UniqueIdentifierIds)>> getCountryAssignedIdentifiers, bool checkBpnAlreadyExists)
    {
        if (data.BusinessPartnerNumber != null && checkBpnAlreadyExists && await checkBpn(data.BusinessPartnerNumber.ToUpper()).ConfigureAwait(ConfigureAwaitOptions.None))
        {
            throw ControllerArgumentException.Create(
                RegistrationValidationErrors.BPN_ALREADY_EXISTS,
                Enumerable.Repeat(new ErrorParameter("businessPartnerNumber", data.BusinessPartnerNumber), 1));
        }

        if (!await checkCountryExistByAlpha2Code(data.CountryAlpha2Code).ConfigureAwait(ConfigureAwaitOptions.None))
        {
            throw ControllerArgumentException.Create(
                RegistrationValidationErrors.COUNTRY_CODE_DOES_NOT_EXIST,
                Enumerable.Repeat(new ErrorParameter("countryAlpha2Code", data.CountryAlpha2Code), 1));
        }

        if (data.UniqueIds.Any())
        {
            var assignedIdentifiers = await getCountryAssignedIdentifiers(
                    data.CountryAlpha2Code,
                    data.UniqueIds.Select(uniqueId => uniqueId.UniqueIdentifierId))
                .ConfigureAwait(ConfigureAwaitOptions.None);

            if (!assignedIdentifiers.IsValidCountry)
            {
                throw ControllerArgumentException.Create(
                    RegistrationValidationErrors.COUNTRY_CODE_NOT_VALID,
                    Enumerable.Repeat(new ErrorParameter("countryAlpha2Code", data.CountryAlpha2Code), 1));
            }

            if (assignedIdentifiers.UniqueIdentifierIds.Count() < data.UniqueIds.Count())
            {
                var invalidIds = data.UniqueIds.ExceptBy(assignedIdentifiers.UniqueIdentifierIds, uniqueId => uniqueId.UniqueIdentifierId);
                throw ControllerArgumentException.Create(RegistrationValidationErrors.UNIQUE_IDS_INVALID_FOR_COUNTRY,
                    new ErrorParameter[]
                    {
                        new("country", data.CountryAlpha2Code),
                        new("values", string.Join(", ", invalidIds.Select(uniqueId => uniqueId.UniqueIdentifierId)))
                    });
            }
        }
    }

    private static bool IsValidUniqueIdentifier(string value, UniqueIdentifierId uniqueIdentifierId, string countryCode)
    {
        if (!UniqueIdentifierExpressions.TryGetValue((uniqueIdentifierId, countryCode), out var regex))
        {
            regex = UniqueIdentifierExpressions.GetValueOrDefault((uniqueIdentifierId, ValidationExpressions.Worldwide)) ?? throw new ControllerArgumentException($"Unique identifier: {uniqueIdentifierId} is not available in the system", nameof(uniqueIdentifierId));
        }
        return regex.IsMatch(value);
    }
}
