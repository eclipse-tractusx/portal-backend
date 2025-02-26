/********************************************************************************
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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

public class OfferSetupServiceErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = new Dictionary<OfferSetupServiceErrors, string> {
        { OfferSetupServiceErrors.OFFERURL_NOT_CONTAIN, "OfferUrl {offerUrl} must not contain #" },
        { OfferSetupServiceErrors.APP_INSTANCE_ALREADY_EXISTS, "The app instance for offer {offerId} already exist" },
        { OfferSetupServiceErrors.APP_INSTANCE_ASSOCIATED_WITH_SUBSCRIPTIONS, "The app instance {appInstanceId} is associated with exiting subscriptions" },
        { OfferSetupServiceErrors.APP_DOES_NOT_EXIST, "App {offerId} does not exist." },
        { OfferSetupServiceErrors.OFFER_SUBCRIPTION_NOT_EXIST, "OfferSubscription {requestId} does not exist" },
        { OfferSetupServiceErrors.OFFER_NOT_SINGLE_INSTANCE, "offer {offerId} is not set up as single instance app" },
        { OfferSetupServiceErrors.SINGLE_INSTANCE_OFFER_MUST_HAVE_ONE_INSTANCE, "There should always be exactly one instance defined for a single instance offer {offerId}" },
        { OfferSetupServiceErrors.CLIENTID_EMPTY_FOR_SINGLE_INSTANCE, "clientId must not be empty for single instance offer {offerId}" },
        { OfferSetupServiceErrors.OFFERSUBSCRIPTION_NOT_EXIST, "Offer Subscription {offerSubscriptionId} does not exist" },
        { OfferSetupServiceErrors.OFFER_SUBSCRIPTION_PENDING, "Status of the offer subscription must be pending" },
        { OfferSetupServiceErrors.ONLY_PROVIDER_CAN_SETUP_SERVICE, "Only the providing company can setup the service" },
        { OfferSetupServiceErrors.ONLY_ONE_APP_INSTANCE_FOR_SINGLE_INSTANCE, "There must only be one app instance for single instance apps" },
        { OfferSetupServiceErrors.STEP_NOT_ELIGIBLE_FOR_SINGLE_INSTANCE, "This step is not eligible to run for single instance apps" },
        { OfferSetupServiceErrors.PROCESS_STEP_ONLY_FOR_SINGLE_INSTANCE, "The process step is only executable for single instance apps" },
        { OfferSetupServiceErrors.SUBSCRIPTION_ONLY_ACTIVATED_BY_PROVIDER, "Subscription can only be activated by the provider of the offer" },
        { OfferSetupServiceErrors.OFFERURL_SHOULD_BE_SET, "OfferUrl should be set" },
        { OfferSetupServiceErrors.OFFERS_WITHOUT_TYPE_NOT_ELIGIBLE, "Offers without type {OfferTypeId.APP} are not eligible to run" },
        { OfferSetupServiceErrors.TECHNICAL_USER_NOT_NEEDED, "Technical user is not needed" },
        { OfferSetupServiceErrors.BPN_MUST_BE_SET, "Bpn must be set" },
        { OfferSetupServiceErrors.OFFER_NAME_MUST_BE_SET, "Offer Name must be set for subscription {offerSubscriptionId}" },
        { OfferSetupServiceErrors.OFFERSUBSCRIPTION_MUST_BE_LINKED_TO_PROCESS, "OfferSubscription {offerSubscriptionId} must be linked to a process" },
        { OfferSetupServiceErrors.COMPANY_MUST_BE_PROVIDER, "Company {companyId} must be provider of the offer for offerSubscription {offerSubscriptionId}" },
        { OfferSetupServiceErrors.CLIENTID_EMPTY_FOR_OFFERSUBSCRIPTION, "clientId must not be empty for offerSubscription {offerSubscriptionId}" },
   }.ToImmutableDictionary(x => (int)x.Key, x => x.Value);

    public Type Type { get => typeof(OfferSetupServiceErrors); }

    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum OfferSetupServiceErrors
{
    OFFERURL_NOT_CONTAIN,
    APP_INSTANCE_ALREADY_EXISTS,
    APP_INSTANCE_ASSOCIATED_WITH_SUBSCRIPTIONS,
    APP_DOES_NOT_EXIST,
    OFFER_NOT_SINGLE_INSTANCE,
    OFFER_SUBCRIPTION_NOT_EXIST,
    SINGLE_INSTANCE_OFFER_MUST_HAVE_ONE_INSTANCE,
    CLIENTID_EMPTY_FOR_SINGLE_INSTANCE,
    OFFERSUBSCRIPTION_NOT_EXIST,
    OFFER_SUBSCRIPTION_PENDING,
    ONLY_PROVIDER_CAN_SETUP_SERVICE,
    ONLY_ONE_APP_INSTANCE_FOR_SINGLE_INSTANCE,
    STEP_NOT_ELIGIBLE_FOR_SINGLE_INSTANCE,
    PROCESS_STEP_ONLY_FOR_SINGLE_INSTANCE,
    SUBSCRIPTION_ONLY_ACTIVATED_BY_PROVIDER,
    OFFERURL_SHOULD_BE_SET,
    OFFERS_WITHOUT_TYPE_NOT_ELIGIBLE,
    TECHNICAL_USER_NOT_NEEDED,
    BPN_MUST_BE_SET,
    OFFER_NAME_MUST_BE_SET,
    OFFERSUBSCRIPTION_MUST_BE_LINKED_TO_PROCESS,
    COMPANY_MUST_BE_PROVIDER,
    CLIENTID_EMPTY_FOR_OFFERSUBSCRIPTION
}

