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

public class AppReleaseErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = ImmutableDictionary.CreateRange<int, string>([
        new((int)AppReleaseErrors.APP_NOT_EXIST, "App {appId} does not exist"),
        new((int)AppReleaseErrors.APP_FORBIDDEN_COM_NOT_PROVIDER_COM_APP, "Company {companyId} is not the provider company of app {appId}"),
        new((int)AppReleaseErrors.APP_ARG_APP_ID_NOT_EMPTY, "AppId must not be empty"),
        new((int)AppReleaseErrors.APP_UNEXPECTED_USECASE_NOT_NULL, "usecase should never be null here"),
        new((int)AppReleaseErrors.APP_ARG_APP_ID_IN_CREATED_STATE, "AppId must be in Created State"),
        new((int)AppReleaseErrors.APP_NOT_ROLE_EXIST, "role {roleId} does not exist"),
        new((int)AppReleaseErrors.APP_ARG_LANG_CODE_NOT_EMPTY, "Language Codes must not be null or empty"),
        new((int)AppReleaseErrors.APP_ARG_USECASE_ID_NOT_EMPTY, "Use Case Ids must not be null or empty"),
        new((int)AppReleaseErrors.APP_ARG_INVALID_LANG_CODE_OR_USECASE_ID, "invalid language code or UseCaseId specified"),
        new((int)AppReleaseErrors.APP_CONFLICT_APP_STATE_CANNOT_UPDATED, "Apps in State {offerState} can't be updated"),
        new((int)AppReleaseErrors.APP_FORBIDDEN_COMPANY_NOT_APP_PROVIDER, "Company {companyId} is not the app provider."),
        new((int)AppReleaseErrors.APP_ARG_LANG_NOT_EXIST_IN_DB, "The language(s) {existingLanguageCodes} do not exist in the database."),
        new((int)AppReleaseErrors.APP_NOT_FOUND_OR_INCORRECT_STATUS, "App {appId} not found or Incorrect Status"),
        new((int)AppReleaseErrors.APP_CONFLICT_APP_NOT_CREATED_STATE, "App {appId} is not in Created State"),
        new((int)AppReleaseErrors.APP_CONFLICT_OFFER_APP_ID_NOT_OFFERTYPE_APP, "offer {appId} is not offerType APP"),
        new((int)AppReleaseErrors.APP_UNEXPECTED_APP_DATA_NOT_NULL, "appData should never be null here"),
        new((int)AppReleaseErrors.APP_ARG_MULTI_INSTANCE_APP_URL_SET, "Multi instance app must not have a instance url set {instanceUrl}"),
        new((int)AppReleaseErrors.APP_FORBIDDEN_COMP_ID_NOT_PROVIDER_COMPANY, "Company {companyId} is not the provider company"),
        new((int)AppReleaseErrors.APP_UNEXPECT_ROLE_DETAILS_NOT_NULL, "roleDetails should never be null here"),
        new((int)AppReleaseErrors.APP_CONFLICT_NOT_IN_CREATED_STATE, "App {appId} is not in Status {offerStatusId}"),
        new((int)AppReleaseErrors.APP_CONFLICT_ONLY_ONE_APP_INSTANCE_ALLOWED, "The must be at exactly one AppInstance")

    ]);

    public Type Type { get => typeof(AppReleaseErrors); }
    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum AppReleaseErrors
{
    APP_NOT_EXIST,
    APP_FORBIDDEN_COM_NOT_PROVIDER_COM_APP,
    APP_ARG_APP_ID_NOT_EMPTY,
    APP_UNEXPECTED_USECASE_NOT_NULL,
    APP_ARG_APP_ID_IN_CREATED_STATE,
    APP_NOT_ROLE_EXIST,
    APP_ARG_LANG_CODE_NOT_EMPTY,
    APP_ARG_USECASE_ID_NOT_EMPTY,
    APP_ARG_INVALID_LANG_CODE_OR_USECASE_ID,
    APP_CONFLICT_APP_STATE_CANNOT_UPDATED,
    APP_FORBIDDEN_COMPANY_NOT_APP_PROVIDER,
    APP_ARG_LANG_NOT_EXIST_IN_DB,
    APP_NOT_FOUND_OR_INCORRECT_STATUS,
    APP_CONFLICT_APP_NOT_CREATED_STATE,
    APP_CONFLICT_OFFER_APP_ID_NOT_OFFERTYPE_APP,
    APP_UNEXPECTED_APP_DATA_NOT_NULL,
    APP_ARG_MULTI_INSTANCE_APP_URL_SET,
    APP_FORBIDDEN_COMP_ID_NOT_PROVIDER_COMPANY,
    APP_UNEXPECT_ROLE_DETAILS_NOT_NULL,
    APP_CONFLICT_NOT_IN_CREATED_STATE,
    APP_CONFLICT_ONLY_ONE_APP_INSTANCE_ALLOWED

}
