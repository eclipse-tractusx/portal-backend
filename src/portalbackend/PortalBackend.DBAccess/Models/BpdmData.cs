﻿/********************************************************************************
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

/// <summary>
/// View model of an application's detailed data for the bpdm.
/// </summary>
/// <param name="CompanyId">Id of the company.</param>
/// <param name="CompanyName">Name of the company.</param>
/// <param name="ShortName">ShortName of the company.</param>
/// <param name="BusinessPartnerNumber">BusinessPartnerNumber of the company.</param>
/// <param name="AlphaCode2">AlphaCode 2 of the company.</param>
/// <param name="ZipCode">Zipcode of the company's address.</param>
/// <param name="City">City of the company's address.</param>
/// <param name="StreetName">Street of the company's address.</param>
/// <param name="StreetNumber">Number in Street of the company's address.</param>
/// <param name="Region">Region of the company's address.</param>
/// <param name="Identifiers">Unique-identifiers of the company mapped to Bpdm identifier keys</param>
public record BpdmData(
    string CompanyName,
    string? ShortName,
    string? BusinessPartnerNumber,
    string? Alpha2Code,
    string? ZipCode,
    string? City,
    string? StreetName,
    string? StreetNumber,
    string? Region,
    IEnumerable<(BpdmIdentifierId UniqueIdentifierId, string Value)> Identifiers
);
