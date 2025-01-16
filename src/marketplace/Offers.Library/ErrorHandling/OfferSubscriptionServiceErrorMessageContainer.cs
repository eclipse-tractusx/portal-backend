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

public class OfferSubscriptionServiceErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = new Dictionary<OfferSubscriptionServiceErrors, string> {
                { OfferSubscriptionServiceErrors.OFFER_NOTFOUND, "Offer {offerId} does not exist." },
                { OfferSubscriptionServiceErrors.SUBSCRIPTION_NOTFOUND, "Subscription {subscriptionId} does not exist." },
                { OfferSubscriptionServiceErrors.NON_PROVIDER_IS_FORBIDDEN, "Only the providing company can decline the subscription request." },
                { OfferSubscriptionServiceErrors.OFFER_STATUS_CONFLICT_INCORR_OFFER_STATUS, "Subscription of offer {offerName} should be in {offerStatus} state." },
            }.ToImmutableDictionary(x => (int)x.Key, x => x.Value);

    public Type Type { get => typeof(OfferSubscriptionServiceErrors); }
    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum OfferSubscriptionServiceErrors
{
    OFFER_NOTFOUND,
    SUBSCRIPTION_NOTFOUND,
    NON_PROVIDER_IS_FORBIDDEN,
    OFFER_STATUS_CONFLICT_INCORR_OFFER_STATUS
}
