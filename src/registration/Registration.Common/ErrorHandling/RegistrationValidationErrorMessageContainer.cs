/********************************************************************************
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Common.ErrorHandling;

public class RegistrationValidationErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = ImmutableDictionary.CreateRange<int, string>([
        new((int)RegistrationValidationErrors.BPN_INVALID, "BPN must contain exactly 16 characters and must be prefixed with BPNL"),
        new((int)RegistrationValidationErrors.NAME_NOT_EMPTY, "Name must not be empty"),
        new((int)RegistrationValidationErrors.CITY_NOT_EMPTY, "City must not be empty"),
        new((int)RegistrationValidationErrors.STREET_NOT_EMPTY, "Streetname must not be empty"),
        new((int)RegistrationValidationErrors.REGION_INVALID, "Region must not be empty, expected value as example: 'BY' for Bayern"),
        new((int)RegistrationValidationErrors.COUNTRY_CODE_MIN_LENGTH, "CountryAlpha2Code must be 2 chars"),
        new((int)RegistrationValidationErrors.UNIQUE_IDS_NO_EMPTY_VALUES, "uniqueIds must not contain empty values: {emptyValues}"),
        new((int)RegistrationValidationErrors.UNIQUE_IDS_NO_DUPLICATE_VALUES, "uniqueIds must not contain duplicate types: {duplicateValues}"),
        new((int)RegistrationValidationErrors.BPN_ALREADY_EXISTS, "The Bpn {businessPartnerNumber} already exists"),
        new((int)RegistrationValidationErrors.COUNTRY_CODE_DOES_NOT_EXIST, "Location {countryAlpha2Code} does not exist"),
        new((int)RegistrationValidationErrors.COUNTRY_CODE_NOT_VALID, "{countryAlpha2Code} is not a valid country-code"),
        new((int)RegistrationValidationErrors.UNIQUE_IDS_INVALID_FOR_COUNTRY, "invalid uniqueIds for country {country}: '{values}'")
   ]);

    public Type Type { get => typeof(RegistrationValidationErrors); }
    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum RegistrationValidationErrors
{
    BPN_INVALID,
    NAME_NOT_EMPTY,
    CITY_NOT_EMPTY,
    STREET_NOT_EMPTY,
    REGION_INVALID,
    COUNTRY_CODE_MIN_LENGTH,
    UNIQUE_IDS_NO_EMPTY_VALUES,
    UNIQUE_IDS_NO_DUPLICATE_VALUES,
    BPN_ALREADY_EXISTS,
    COUNTRY_CODE_DOES_NOT_EXIST,
    COUNTRY_CODE_NOT_VALID,
    UNIQUE_IDS_INVALID_FOR_COUNTRY
}
