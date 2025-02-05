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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ErrorHandling;

public class AppChangeErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = ImmutableDictionary.CreateRange<int, string>([
        new((int)AppChangeErrors.APP_NOT_EXIST, "App {appId} does not exist"),
        new((int)AppChangeErrors.APP_CONFLICT_PROVIDER_COMPANY_NOT_SET, "App {appId} providing company is not yet set."),
        new((int)AppChangeErrors.APP_FORBIDDEN_COM_NOT_PROVIDER_COM_APP, "Company {companyId} is not the provider company of app {appId}"),
        new((int)AppChangeErrors.APP_CONFLICT_STATUS_INCORRECT, "App {appId} is in InCorrect Status"),
        new((int)AppChangeErrors.APP_UNEXPECT_OFFER_SUBSCRIPTION_DATA_SHOULD_NOT_NULL, "offerDescriptionDatas should never be null here"),
        new((int)AppChangeErrors.APP_CONFLICT_OFFER_STATUS_INCORRECT_STATE, "offerStatus is in incorrect State"),
        new((int)AppChangeErrors.APP_NOT_OFFER_OR_SUBSCRIPTION_EXISTS, "Offer {offerId} or subscription {subscriptionId} do not exists"),
        new((int)AppChangeErrors.APP_CONFLICT_SUBSCRIPTION_URL_NOT_CHANGED, "Subscription url of single instance apps can't be changed"),
        new((int)AppChangeErrors.APP_FORBIDDEN_COMPANY_NOT_APP_PROVIDER_COMPANY, "Company {companyId} is not the app's providing company"),
        new((int)AppChangeErrors.APP_CONFLICT_SUBSCRIPTION_STATUS_BE_ACTIVE, "Subscription {subscriptionId} must be in status {OfferSubscriptionStatusId}"),
        new((int)AppChangeErrors.APP_CONFLICT_NO_SUBSCRIPTION_DATA_CONFIGURED, "There is no subscription detail data configured for subscription {subscriptionId}"),
        new((int)AppChangeErrors.APP_DOCUMENT_FOR_APP_NOT_EXIST, "Document {documentId} for App {appId} does not exist."),
        new((int)AppChangeErrors.APP_CONFLICT_NOT_VALID_DOCUMENT_TYPE, "document {documentId} does not have a valid documentType"),
        new((int)AppChangeErrors.APP_CONFLICT_APP_NOT_ACTIVE, "App {appId} is not Active")
    ]);

    public Type Type { get => typeof(AppChangeErrors); }
    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum AppChangeErrors
{
    APP_NOT_EXIST,
    APP_CONFLICT_PROVIDER_COMPANY_NOT_SET,
    APP_FORBIDDEN_COM_NOT_PROVIDER_COM_APP,
    APP_CONFLICT_STATUS_INCORRECT,
    APP_UNEXPECT_OFFER_SUBSCRIPTION_DATA_SHOULD_NOT_NULL,
    APP_CONFLICT_OFFER_STATUS_INCORRECT_STATE,
    APP_NOT_OFFER_OR_SUBSCRIPTION_EXISTS,
    APP_CONFLICT_SUBSCRIPTION_URL_NOT_CHANGED,
    APP_FORBIDDEN_COMPANY_NOT_APP_PROVIDER_COMPANY,
    APP_CONFLICT_SUBSCRIPTION_STATUS_BE_ACTIVE,
    APP_CONFLICT_NO_SUBSCRIPTION_DATA_CONFIGURED,
    APP_DOCUMENT_FOR_APP_NOT_EXIST,
    APP_CONFLICT_NOT_VALID_DOCUMENT_TYPE,
    APP_CONFLICT_APP_NOT_ACTIVE

}
