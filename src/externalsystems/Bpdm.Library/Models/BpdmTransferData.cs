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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;

/// <summary>
/// View model of an application's detailed data for the bpdm.
/// </summary>
/// <param name="CompanyName">Name of the company.</param>
/// <param name="AlphaCode2">AlphaCode 2 of the company.</param>
/// <param name="ZipCode">Zipcode of the company's address.</param>
/// <param name="City">City of the company's address.</param>
/// <param name="Street">Street of the company's address.</param>
public record BpdmTransferData(
    string ExternalId,
    string CompanyName,
    string? ShortName,
    string AlphaCode2,
    string? ZipCode,
    string City,
    string StreetName,
    string? StreetNumber,
    string? Region,
    IEnumerable<(BpdmIdentifierId BpdmIdentifierId, string Value)> Identifiers
);
