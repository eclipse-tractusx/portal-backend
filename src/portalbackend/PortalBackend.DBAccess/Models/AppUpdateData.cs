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

public record AppUpdateData(
    OfferStatusId OfferState,
    bool IsUserOfProvider,
    IEnumerable<LocalizedDescription> OfferDescriptions,
    IEnumerable<(string Shortname, bool IsMatch)> Languages,
    IEnumerable<Guid> MatchingUseCases, 
    ValueTuple<Guid, string, bool> OfferLicense,
    IEnumerable<PrivacyPolicyId> MatchingPrivacyPolicies,
    string? Name,
    string Provider,
    Guid? SalesManagerId,
    string? ContactEmail,
    string? ContactNumber,
    string? MarketingUrl
);
