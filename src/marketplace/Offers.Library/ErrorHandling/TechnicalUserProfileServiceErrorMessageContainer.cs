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

public class TechnicalUserProfileServiceErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = new Dictionary<TechnicalUserProfileServiceErrors, string> {
        { TechnicalUserProfileServiceErrors.OFFER_NOT_FOUND, "Offer {offerTypeId} {offerId} does not exists" },
        { TechnicalUserProfileServiceErrors.OFFER_SUBSCRIPTION_NOT_FOUND, "Offer Subscription {subscriptionId} does not exists" },
        { TechnicalUserProfileServiceErrors.OFFER_NAME_NOT_SET, "Offer name needs to be set here" },
    }.ToImmutableDictionary(x => (int)x.Key, x => x.Value);

    public Type Type { get => typeof(TechnicalUserProfileServiceErrors); }

    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum TechnicalUserProfileServiceErrors
{
    OFFER_NOT_FOUND,
    OFFER_SUBSCRIPTION_NOT_FOUND,
    OFFER_NAME_NOT_SET
}
