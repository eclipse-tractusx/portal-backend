/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.ErrorHandling;

public class OfferServiceErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = new Dictionary<OfferServiceErrors, string> {
                { OfferServiceErrors.OFFER_NOTFOUND, "Offer {offerId} does not exist" },
                { OfferServiceErrors.COMPANY_NOT_PROVIDER, "Company {companyId} is not the providing company" },
                { OfferServiceErrors.NOT_EMPTY_ROLES_AND_PROFILES, "Technical User Profiles and Role IDs both should not be empty." },
                { OfferServiceErrors.TECHNICAL_USERS_FOR_CONSULTANCY, "Technical User Profiles can't be set for CONSULTANCY_SERVICE" },
                { OfferServiceErrors.ROLES_DOES_NOT_EXIST, "Roles {roleIds} do not exist" },
                { OfferServiceErrors.ROLES_MISSMATCH, "Roles must either be provider only or visible for provider and subscriber" }
            }.ToImmutableDictionary(x => (int)x.Key, x => x.Value);

    public Type Type { get => typeof(OfferServiceErrors); }
    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum OfferServiceErrors
{
    OFFER_NOTFOUND,
    COMPANY_NOT_PROVIDER,
    NOT_EMPTY_ROLES_AND_PROFILES,
    TECHNICAL_USERS_FOR_CONSULTANCY,
    ROLES_DOES_NOT_EXIST,
    ROLES_MISSMATCH
}
