/********************************************************************************
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

using Org.Eclipse.TractusX.Portal.Backend.Registration.Common;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;

public record CompanyDetailData(
    Guid CompanyId,
    string Name,
    string City,
    string StreetName,
    string CountryAlpha2Code,
    [property: JsonPropertyName("bpn")] string? BusinessPartnerNumber,
    string? ShortName,
    string Region,
    string? StreetAdditional,
    string? StreetNumber,
    string? ZipCode,
    IEnumerable<CompanyUniqueIdData> UniqueIds
) : RegistrationData(Name, City, StreetName, CountryAlpha2Code, BusinessPartnerNumber, ShortName, Region, StreetAdditional, StreetNumber, ZipCode, UniqueIds);
