/********************************************************************************
 * Copyright (c) 2021, 2023 Microsoft and BMW Group AG
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

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Bpn.Model;

public record BpdmLegalEntityAddressDto(
    string LegalEntity,
    BpdmLegalAddressDto LegalAddress
);

public record BpdmLegalAddressDto(
    BpdmAddressVersionDto Version,
    string CareOf,
    IEnumerable<string> Contexts,
    BpdmDataDto Country,
    IEnumerable<BpdmAdministrativeAreaDto> AdministrativeAreas,
    IEnumerable<BpdmPostCodeDto> PostCodes,
    IEnumerable<BpdmLocalityDto> Localities,
    IEnumerable<BpdmThoroughfareDto> Thoroughfares,
    IEnumerable<BpdmPremiseDto> Premises,
    IEnumerable<BpdmPostalDeliveryPointDto> PostalDeliveryPoints,
    BpdmGeographicCoordinatesDto GeographicCoordinates,
    IEnumerable<BpdmUrlDataDto> Types
);

public record BpdmAddressVersionDto(
    BpdmDataDto CharacterSet,
    BpdmDataDto Language
);

public record BpdmAdministrativeAreaDto(
    string Value,
    string ShortName,
    string FipsCode,
    BpdmUrlDataDto Type,
    BpdmDataDto Language
);

public record BpdmPostCodeDto(
    string Value,
    BpdmUrlDataDto Type
);

public record BpdmLocalityDto(
    string Value,
    string ShortName,
    BpdmUrlDataDto Type,
    BpdmDataDto Language
);

public record BpdmThoroughfareDto(
    string Value,
    string Name,
    string ShortName,
    string Number,
    string Direction,
    BpdmUrlDataDto Type,
    BpdmDataDto Language
);

public record BpdmPremiseDto(
    string Value,
    string ShortName,
    string Number,
    BpdmUrlDataDto Type,
    BpdmDataDto Language
);

public record BpdmPostalDeliveryPointDto(
    string Value,
    string ShortName,
    string Number,
    BpdmUrlDataDto Type,
    BpdmDataDto Language
);

public record BpdmGeographicCoordinatesDto(
    int Longitude,
    int Latitude,
    int Altitude
);
