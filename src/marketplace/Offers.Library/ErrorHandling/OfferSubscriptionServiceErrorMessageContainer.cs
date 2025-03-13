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
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = ImmutableDictionary.CreateRange<int, string>([
        new((int)OfferSubscriptionServiceErrors.OFFER_NOTFOUND, "Offer {offerId} does not exist."),
        new((int)OfferSubscriptionServiceErrors.SUBSCRIPTION_NOTFOUND, "Subscription {subscriptionId} does not exist."),
        new((int)OfferSubscriptionServiceErrors.NON_PROVIDER_IS_FORBIDDEN, "Only the providing company can decline the subscription request."),
        new((int)OfferSubscriptionServiceErrors.OFFER_STATUS_CONFLICT_INCORR_OFFER_STATUS, "Subscription of offer {offerName} should be in {offerStatus} state."),
        new((int)OfferSubscriptionServiceErrors.INVALID_CONFIGURATION_ROLES_NOT_EXIST, "invalid configuration, at least one of the configured roles does not exist in the database: {userRoles}"),
        new((int)OfferSubscriptionServiceErrors.PROVIDING_COMPANY_NOT_SET, "{offerTypeId} providing company is not set"),
        new((int)OfferSubscriptionServiceErrors.OFFER_NAME_NOT_CONFIGURED, "The offer name has not been configured properly"),
        new((int)OfferSubscriptionServiceErrors.AGREEMENTS_NOT_VALID, "agreements {agreementId} are not valid for offer {offerId}"),
        new((int)OfferSubscriptionServiceErrors.CONSENT_TO_AGREEMENTS_REQUIRED, "Consent to agreements {agreementId} must be given for offer {offerId}"),
        new((int)OfferSubscriptionServiceErrors.COMPANY_NOT_EXIST, "Company {companyId} does not exist"),
        new((int)OfferSubscriptionServiceErrors.COMPANY_NO_BUSINESS_PARTNER_NUMBER, "company {organizationName} has no BusinessPartnerNumber assigned"),
        new((int)OfferSubscriptionServiceErrors.COMPANY_ALREADY_SUBSCRIBED, "company {companyId} is already subscribed to {offerId}")
    ]);

    public Type Type { get => typeof(OfferSubscriptionServiceErrors); }
    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum OfferSubscriptionServiceErrors
{
    OFFER_NOTFOUND,
    SUBSCRIPTION_NOTFOUND,
    NON_PROVIDER_IS_FORBIDDEN,
    OFFER_STATUS_CONFLICT_INCORR_OFFER_STATUS,
    INVALID_CONFIGURATION_ROLES_NOT_EXIST,
    PROVIDING_COMPANY_NOT_SET,
    OFFER_NAME_NOT_CONFIGURED,
    AGREEMENTS_NOT_VALID,
    CONSENT_TO_AGREEMENTS_REQUIRED,
    COMPANY_NOT_EXIST,
    COMPANY_NO_BUSINESS_PARTNER_NUMBER,
    COMPANY_ALREADY_SUBSCRIBED
}
