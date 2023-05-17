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

public record BpdmLegalEntityDto(
	string Bpn,
	IEnumerable<BpdmIdentifierDto> Identifiers,
	IEnumerable<BpdmNameDto> Names,
	BpdmLegalFormDto LegalForm,
	BpdmStatusDto Status,
	IEnumerable<BpdmProfileClassificationDto> ProfileClassifications,
	IEnumerable<BpdmUrlDataDto> Types,
	IEnumerable<BpdmBankAccountDto> BankAccounts,
	IEnumerable<BpdmDataDto> Roles,
	IEnumerable<BpdmRelationDto> Relations,
	DateTimeOffset Currentness
);

public record BpdmIdentifierDto(
	string Value,
	BpdmUrlDataDto Type,
	BpdmUrlDataDto IssuingBody,
	BpdmDataDto Status
);

public record BpdmNameDto(
	string Value,
	string ShortName,
	BpdmUrlDataDto Type,
	BpdmDataDto Language
);

public record BpdmLegalFormDto(
	string TechnicalKey,
	string Name,
	string Url,
	string MainAbbreviation,
	BpdmDataDto Language,
	IEnumerable<BpdmNameUrlDto> Categories
);

public record BpdmStatusDto(
	string OfficialDenotation,
	DateTimeOffset ValidFrom,
	DateTimeOffset ValidUntil,
	BpdmUrlDataDto Type
);

public record BpdmProfileClassificationDto(
	string Value,
	string Code,
	BpdmNameUrlDto Type
);

public record BpdmDataDto(
	string TechnicalKey,
	string Name
);

public record BpdmUrlDataDto(
	string TechnicalKey,
	string Name,
	string Url
);

public record BpdmNameUrlDto(
	string Name,
	string Url
);

public record BpdmBankAccountDto(
	IEnumerable<int> TrustScores,
	BpdmDataDto Currency,
	string InternationalBankAccountIdentifier,
	string InternationalBankIdentifier,
	string NationalBankAccountIdentifier,
	string NationalBankIdentifier
);

public record BpdmRelationDto(
	BpdmDataDto RelationClass,
	BpdmDataDto Type,
	string StartNode,
	string EndNode,
	DateTimeOffset StartedAt,
	DateTimeOffset EndedAt
);
