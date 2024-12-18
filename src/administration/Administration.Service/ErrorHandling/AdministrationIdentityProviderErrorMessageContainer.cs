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

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;

public class AdministrationIdentityProviderErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = ImmutableDictionary.CreateRange<int, string>([
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_UNEXPECT_VAL_FOR_CATEGORY_ID, "unexpected value for category {categoryId}"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_UNEXPECT_VAL_OF_PROTOCOL, "unexcepted value of protocol: {protocol}"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_PROVIDER_TYPE_CREATION_NOT_SUPPORTED, "creation of identityProviderType {typeId} is not supported"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_DISPLAY_NAME_CHAR_BET_TWO_TO_THIRTY, "displayName length must be 2-30 characters"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_ALLO_CHAR_AS_PER_REG_EX, "allowed characters in displayName: 'a-zA-Z0-9!?@&#'\"()_-=/*.,;: '"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_COMPANY_NOT_EXIST, "company {companyId} does not exist"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_FORBIDDEN_NOT_ALLOW_CREATE_PROVIDER_TYPE, "Not allowed to create an identityProvider of type {typeId}"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_UNEXPECT_VAL_FOR_CATEGORY_OF_PROVIDER, "unexpected value for category {category} of identityProvider {identityProviderId}"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_CONFLICT_PROVIDER_NOT_ASSOCIATE_WITH_COMPANY, "identityProvider {identityProviderId} is not associated with company {companyId}"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_NOT_PROVIDER_NOT_EXIST, "identityProvider {identityProviderId} does not exist"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_CONFLICT_SHARED_IDP_NOT_USE_SAML, "Shared Idps must not use SAML"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_FORBIDDEN_COMP_NOT_OWNER_PROVIDER, "company {companyId} is not the owner of identityProvider {identityProviderId}"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_CONFLICT_PROVIDER_NOT_HAVE_IAMIDENTITY_PROVIDER_ALIAS, "identityprovider {identityProviderId} does not have an iamIdentityProvider.alias"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_UNEXPECT_COMPANYID_ALIAS_NOT_NULL, "CompanyIdAliase should never be null here"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_NOT_DISABLE_PROVIDER, "cannot disable indentityProvider {identityProviderId} as no other active identityProvider exists for this company"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_FORBIDDEN_USER_NOT_ALLOW_CHANGE_PROVIDER, "User not allowed to run the change for identity provider {identityProviderId}"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_OIDC_NOT_NULL, "property oidc must not be null"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_SAML_NOT_NULL, "property 'saml' must be null"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_CANNOT_DEL_ENABLE_PROVIDERID, "cannot delete identityProvider {identityProviderId} as it is enabled"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_NOT_DELETE_PROVIDER, "cannot delete indentityProvider {identityProviderId} as no other active identityProvider exists for this company"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_NOT_COMP_USERID_NO_KEYLOCK_LINK_FOUND, "identityProviderLink for identityProvider {identityProviderId} not found in keycloak for user {companyUserId}"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_UNSUPPORTEDMEDIA_CONTENT_TYPE_ALLOWED, "Only contentType {contentType} files are allowed."),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_UNEXPECT_USER_PROFILE_LINK_DATA_NEVER_DEFAULT, "userProfileLinkData should never be default here"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_UNEXPECT_CSV_COMPAY_USERID, "unexpected value of {headerUserId}: {companyUserId}"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_UNEXPECT_SHARED_IDENTITY_PROVIDER_LINK, "unexpected update of shared identityProviderLink, alias {alias}, companyUser {companyUserId}, providerUserId: {userId}, providerUserName: {userName}"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_INVALID_FORMAT_CSVHEADER, "invalid format: expected {csvHeader}, got ''"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_INVALID_FORMAT_CSVHEADER_WITH_CURR, "invalid format: expected {csvHeader}, got {current}"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_VAL_HEADER_USERID, "value for {headerUserId} type Guid expected"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_VAL_HEADER_USERID_WITH_CURRENT_ITEMS, "invalid format for {headerUserId} type Guid: {current}"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_VAL_FOR_HEADER_FIRSTNAME, "value for {headerFirstName} expected"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_VAL_FOR_HEADER_LASTNAME, "value for {headerLastName} expected"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_VAL_FOR_HEADER_EMAIL, "value for {headerEmail} expected"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_VAL_HEADER_PROVIDERALIAS, "value for {headerProviderAlias} expected"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_VAL_HEADER_PROVIDERALIAS_WITH_IDENTITY_ALIAS, "unexpected value for {headerProviderAlias}: {identityProviderAlias}"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_VAL_HEADER_PROVIDER_USERID, "value for {headerProviderUserId} expected"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_VAL_HEADER_PROVIDER_USERNAME, "value for {headerProviderUserName} expected"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_ATLEAST_ONE_PROVIDERID_SPECIFIED, "at least one identityProviderId must be specified"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_INVALID_IDENTITY_PROVIDER_IDS, "invalid identityProviders: {invalidIds)} for company {companyId}"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_NOT_COMPANY_USERID_NOT_EXIST, "companyUserId {companyUserId} does not exist"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_NOT_FOUND_COMPANY_OF_COMPANY_USER_ID, "identityProvider {identityProviderId} not found in company of user {companyUserId}"),
        new((int)AdministrationIdentityProviderErrors.IDENTITY_UNEXPECT_COMPANY_USERID_NOT_LINKED_KEYCLOAK, "companyUserId {companyUserId} is not linked to keycloak")
    ]);

    public Type Type { get => typeof(AdministrationIdentityProviderErrors); }

    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum AdministrationIdentityProviderErrors
{
    IDENTITY_ARGUMENT_UNEXPECT_VAL_FOR_CATEGORY_ID,
    IDENTITY_ARGUMENT_UNEXPECT_VAL_OF_PROTOCOL,
    IDENTITY_ARGUMENT_PROVIDER_TYPE_CREATION_NOT_SUPPORTED,
    IDENTITY_ARGUMENT_DISPLAY_NAME_CHAR_BET_TWO_TO_THIRTY,
    IDENTITY_ARGUMENT_ALLO_CHAR_AS_PER_REG_EX,
    IDENTITY_ARGUMENT_COMPANY_NOT_EXIST,
    IDENTITY_FORBIDDEN_NOT_ALLOW_CREATE_PROVIDER_TYPE,
    IDENTITY_ARGUMENT_UNEXPECT_VAL_FOR_CATEGORY_OF_PROVIDER,
    IDENTITY_CONFLICT_PROVIDER_NOT_ASSOCIATE_WITH_COMPANY,
    IDENTITY_NOT_PROVIDER_NOT_EXIST,
    IDENTITY_CONFLICT_SHARED_IDP_NOT_USE_SAML,
    IDENTITY_FORBIDDEN_COMP_NOT_OWNER_PROVIDER,
    IDENTITY_CONFLICT_PROVIDER_NOT_HAVE_IAMIDENTITY_PROVIDER_ALIAS,
    IDENTITY_UNEXPECT_COMPANYID_ALIAS_NOT_NULL,
    IDENTITY_ARGUMENT_NOT_DISABLE_PROVIDER,
    IDENTITY_FORBIDDEN_USER_NOT_ALLOW_CHANGE_PROVIDER,
    IDENTITY_ARGUMENT_OIDC_NOT_NULL,
    IDENTITY_ARGUMENT_SAML_NOT_NULL,
    IDENTITY_ARGUMENT_CANNOT_DEL_ENABLE_PROVIDERID,
    IDENTITY_ARGUMENT_NOT_DELETE_PROVIDER,
    IDENTITY_NOT_COMP_USERID_NO_KEYLOCK_LINK_FOUND,
    IDENTITY_UNSUPPORTEDMEDIA_CONTENT_TYPE_ALLOWED,
    IDENTITY_UNEXPECT_USER_PROFILE_LINK_DATA_NEVER_DEFAULT,
    IDENTITY_ARGUMENT_UNEXPECT_CSV_COMPAY_USERID,
    IDENTITY_ARGUMENT_UNEXPECT_SHARED_IDENTITY_PROVIDER_LINK,
    IDENTITY_ARGUMENT_INVALID_FORMAT_CSVHEADER,
    IDENTITY_ARGUMENT_INVALID_FORMAT_CSVHEADER_WITH_CURR,
    IDENTITY_ARGUMENT_VAL_HEADER_USERID,
    IDENTITY_ARGUMENT_VAL_HEADER_USERID_WITH_CURRENT_ITEMS,
    IDENTITY_ARGUMENT_VAL_FOR_HEADER_FIRSTNAME,
    IDENTITY_ARGUMENT_VAL_FOR_HEADER_LASTNAME,
    IDENTITY_ARGUMENT_VAL_FOR_HEADER_EMAIL,
    IDENTITY_ARGUMENT_VAL_HEADER_PROVIDERALIAS,
    IDENTITY_ARGUMENT_VAL_HEADER_PROVIDERALIAS_WITH_IDENTITY_ALIAS,
    IDENTITY_ARGUMENT_VAL_HEADER_PROVIDER_USERID,
    IDENTITY_ARGUMENT_VAL_HEADER_PROVIDER_USERNAME,
    IDENTITY_ARGUMENT_ATLEAST_ONE_PROVIDERID_SPECIFIED,
    IDENTITY_ARGUMENT_INVALID_IDENTITY_PROVIDER_IDS,
    IDENTITY_NOT_COMPANY_USERID_NOT_EXIST,
    IDENTITY_NOT_FOUND_COMPANY_OF_COMPANY_USER_ID,
    IDENTITY_UNEXPECT_COMPANY_USERID_NOT_LINKED_KEYCLOAK

}
