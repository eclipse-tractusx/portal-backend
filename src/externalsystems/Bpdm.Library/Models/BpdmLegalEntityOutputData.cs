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

namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;

public record PageOutputResponseBpdmLegalEntityData(
    IEnumerable<BpdmLegalEntityOutputData>? Content
);

public record BpdmLegalEntityOutputData(
    string? ExternalId,
    string? Bpn,
    string? LegalShortName,
    string? LegalForm,
    IEnumerable<BpdmIdentifier> Identifiers,
    IEnumerable<BpdmStatus> States,
    IEnumerable<BpdmProfileClassification> Classifications,
    IEnumerable<string> LegalNameParts,
    IEnumerable<string> Roles,
    BpdmLegalAddressResponse LegalAddress
);

public record BpdmLegalAddressResponse(
    string ExternalId,
    string LegalEntityExternalId,
    string SiteExternalId,
    string Bpn,
    IEnumerable<string> NameParts,
    IEnumerable<BpdmAddressState> States,
    IEnumerable<BpdmAddressIdentifier> Identifiers,
    BpdmAddressPhysicalPostalAddress PhysicalPostalAddress,
    BpdmAddressAlternativePostalAddress AlternativePostalAddress,
    IEnumerable<string> Roles
);

public record BpdmCountry
(
    string TechnicalKey,
    string Name
);
