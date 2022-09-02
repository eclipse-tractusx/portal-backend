/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Keycloak.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Library.Enums;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using Microsoft.Extensions.Options;
using System.Text;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class IdentityProviderBusinessLogic : IIdentityProviderBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IdentityProviderSettings _settings;

    public IdentityProviderBusinessLogic(IPortalRepositories portalRepositories, IProvisioningManager provisioningManager, IOptions<IdentityProviderSettings> options)
    {
        _portalRepositories = portalRepositories;
        _provisioningManager = provisioningManager;
        _settings = options.Value;
    }

    public async IAsyncEnumerable<IdentityProviderDetails> GetOwnCompanyIdentityProvidersAsync(string iamUserId)
    {
        await foreach (var identityProviderData in _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderCategoryDataUntracked(iamUserId).ConfigureAwait(false))
        {
            switch(identityProviderData.CategoryId)
            {
                case IdentityProviderCategoryId.KEYCLOAK_SHARED:
                case IdentityProviderCategoryId.KEYCLOAK_OIDC:
                    yield return await GetIdentityProviderDetailsOidc(identityProviderData.IdentityProviderId, identityProviderData.Alias, identityProviderData.CategoryId).ConfigureAwait(false);
                    break;
                case IdentityProviderCategoryId.KEYCLOAK_SAML:
                    var identityProviderDataSAML = await _provisioningManager.GetCentralIdentityProviderDataSAMLAsync(identityProviderData.Alias).ConfigureAwait(false);
                    yield return new IdentityProviderDetails(
                        identityProviderData.IdentityProviderId,
                        identityProviderData.Alias,
                        identityProviderData.CategoryId,
                        identityProviderDataSAML.DisplayName,
                        identityProviderDataSAML.RedirectUrl,
                        identityProviderDataSAML.Enabled,
                        await _provisioningManager.GetIdentityProviderMappers(identityProviderData.Alias).ToListAsync().ConfigureAwait(false))
                        {
                            saml = new IdentityProviderDetailsSaml(
                                identityProviderDataSAML.EntityId,
                                identityProviderDataSAML.SingleSignOnServiceUrl)
                        };
                    break;
            }
        }
    }

    public ValueTask<IdentityProviderDetails> CreateOwnCompanyIdentityProviderAsync(IamIdentityProviderProtocol protocol, string iamUserId)
    {
        IdentityProviderCategoryId identityProviderCategory;
        switch (protocol)
        {
            case IamIdentityProviderProtocol.SAML:
                identityProviderCategory = IdentityProviderCategoryId.KEYCLOAK_SAML;
                break;
            case IamIdentityProviderProtocol.OIDC:
                identityProviderCategory = IdentityProviderCategoryId.KEYCLOAK_OIDC;
                break;
            default:
                throw new ControllerArgumentException($"unexcepted value of protocol: '{protocol.ToString()}'", nameof(protocol));
        }
        return CreateOwnCompanyIdentityProviderInternalAsync(identityProviderCategory, protocol, iamUserId);
    }

    private async ValueTask<IdentityProviderDetails> CreateOwnCompanyIdentityProviderInternalAsync(IdentityProviderCategoryId identityProviderCategory, IamIdentityProviderProtocol protocol, string iamUserId)
    {
        var identityProviderRepository = _portalRepositories.GetInstance<IIdentityProviderRepository>();

        var (companyName, companyId) = await _portalRepositories.GetInstance<ICompanyRepository>().GetCompanyNameIdUntrackedAsync(iamUserId).ConfigureAwait(false);
        if (companyName == null || companyId == Guid.Empty)
        {
            throw new ControllerArgumentException($"user {iamUserId} is not associated with a company", nameof(iamUserId));
        }
        var alias = await _provisioningManager.CreateOwnIdpAsync(companyName, protocol).ConfigureAwait(false);
        var identityProvider = identityProviderRepository.CreateIdentityProvider(identityProviderCategory);
        identityProvider.CompanyIdentityProviders.Add(identityProviderRepository.CreateCompanyIdentityProvider(companyId, identityProvider.Id));
        identityProviderRepository.CreateIamIdentityProvider(identityProvider, alias);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);

        switch (protocol)
        {
            case IamIdentityProviderProtocol.OIDC:
                return await GetIdentityProviderDetailsOidc(identityProvider.Id, alias, IdentityProviderCategoryId.KEYCLOAK_OIDC).ConfigureAwait(false);
            case IamIdentityProviderProtocol.SAML:
                return await GetIdentityProviderDetailsSaml(identityProvider.Id, alias).ConfigureAwait(false);
            default:
                throw new UnexpectedConditionException($"unexpected value of protocol: '{protocol.ToString()}'");
        }
    }

    public async ValueTask<IdentityProviderDetails> GetOwnCompanyIdentityProviderAsync(Guid identityProviderId, string iamUserId)
    {
        var (alias, category) = await ValidateGetOwnCompanyIdentityProviderArguments(identityProviderId, iamUserId).ConfigureAwait(false);

        switch(category)
        {
            case IdentityProviderCategoryId.KEYCLOAK_SHARED:
            case IdentityProviderCategoryId.KEYCLOAK_OIDC:
                return await GetIdentityProviderDetailsOidc(identityProviderId, alias, category).ConfigureAwait(false);
            case IdentityProviderCategoryId.KEYCLOAK_SAML:
                return await GetIdentityProviderDetailsSaml(identityProviderId, alias).ConfigureAwait(false);
            default:
                throw new ControllerArgumentException($"unexpected value for category '{category.ToString()}' of identityProvider '{identityProviderId}'");
        }
    }

    private async ValueTask<(string Alias, IdentityProviderCategoryId Category)> ValidateGetOwnCompanyIdentityProviderArguments(Guid identityProviderId, string iamUserId)
    {
        var (alias, category, isOwnCompany) = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderAliasUntrackedAsync(identityProviderId, iamUserId).ConfigureAwait(false);
        if (!isOwnCompany)
        {
            throw new ForbiddenException($"identityProvider {identityProviderId} is not associated with company of user {iamUserId}");
        }
        if (alias == null)
        {
            throw new NotFoundException($"identityProvider {identityProviderId} does not exist");
        }
        return new ValueTuple<string,IdentityProviderCategoryId>(alias, category);
    }

    public async ValueTask<IdentityProviderDetails> SetOwnCompanyIdentityProviderStatusAsync(Guid identityProviderId, bool enabled, string iamUserId)
    {
        var (category, alias) = await ValidateSetOwnCompanyIdentityProviderStatusArguments(identityProviderId, enabled, iamUserId).ConfigureAwait(false);

        switch(category)
        {
            case IdentityProviderCategoryId.KEYCLOAK_OIDC:
                await _provisioningManager.SetCentralIdentityProviderStatusAsync(alias, enabled).ConfigureAwait(false);
                return await GetIdentityProviderDetailsOidc(identityProviderId, alias, category).ConfigureAwait(false);
            case IdentityProviderCategoryId.KEYCLOAK_SAML:
                await _provisioningManager.SetCentralIdentityProviderStatusAsync(alias, enabled).ConfigureAwait(false);
                return await GetIdentityProviderDetailsSaml(identityProviderId, alias).ConfigureAwait(false);
            case IdentityProviderCategoryId.KEYCLOAK_SHARED:
                await _provisioningManager.SetSharedIdentityProviderStatusAsync(alias, enabled).ConfigureAwait(false);
                return await GetIdentityProviderDetailsOidc(identityProviderId, alias, category).ConfigureAwait(false);
            default:
                throw new ControllerArgumentException($"unexpected value for category '{category.ToString()}' of identityProvider '{identityProviderId}'");
        }
    }

    private async ValueTask<(IdentityProviderCategoryId Category, string Alias)> ValidateSetOwnCompanyIdentityProviderStatusArguments(Guid identityProviderId, bool enabled, string iamUserId)
    {
        var result = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(identityProviderId, iamUserId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"identityProvider {identityProviderId} does not exist");
        }
        if (!result.IsSameCompany)
        {
            throw new ForbiddenException($"identityProvider {identityProviderId} is not associated with company of user {iamUserId}");
        }
        if (!enabled)
        {
            await ValidateOtherActiveIdentityProvider(
                result.Alias,
                result.Aliase,
                () => throw new ControllerArgumentException($"cannot disable indentityProvider {identityProviderId} as no other active identityProvider exists for this company")
            ).ConfigureAwait(false);
        }
        return new ValueTuple<IdentityProviderCategoryId, string>(result.IdentityProviderCategory, result.Alias);
    }

    public async ValueTask<IdentityProviderDetails> UpdateOwnCompanyIdentityProviderAsync(Guid identityProviderId, IdentityProviderEditableDetails details, string iamUserId)
    {
        var (category, alias) = await ValidateUpdateOwnCompanyIdentityProviderArguments(identityProviderId, iamUserId).ConfigureAwait(false);

        switch(category)
        {
            case IdentityProviderCategoryId.KEYCLOAK_OIDC:
                await UpdateIdentityProviderOidc(alias, details).ConfigureAwait(false);
                return await GetIdentityProviderDetailsOidc(identityProviderId, alias, category).ConfigureAwait(false);
            case IdentityProviderCategoryId.KEYCLOAK_SAML:
                await UpdateIdentityProviderSaml(alias, details).ConfigureAwait(false);
                return await GetIdentityProviderDetailsSaml(identityProviderId, alias).ConfigureAwait(false);
            case IdentityProviderCategoryId.KEYCLOAK_SHARED:
                await UpdateIdentityProviderShared(alias, details).ConfigureAwait(false);
                return await GetIdentityProviderDetailsOidc(identityProviderId, alias, category).ConfigureAwait(false);
            default:
                throw new ControllerArgumentException($"unexpected value for category '{category.ToString()}' of identityProvider '{identityProviderId}'");
        }
    }

    private async ValueTask<(IdentityProviderCategoryId Category, string Alias)> ValidateUpdateOwnCompanyIdentityProviderArguments(Guid identityProviderId, string iamUserId)
    {
        var result = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(identityProviderId, iamUserId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"identityProvider {identityProviderId} does not exist");
        }
        if (!result.IsSameCompany)
        {
            throw new ForbiddenException($"identityProvider {identityProviderId} is not associated with company of user {iamUserId}");
        }
        return new ValueTuple<IdentityProviderCategoryId, string>(result.IdentityProviderCategory, result.Alias);
    }

    private async ValueTask UpdateIdentityProviderOidc(string alias, IdentityProviderEditableDetails details)
    {
        if(details.oidc == null)
        {
            throw new ControllerArgumentException("property 'oidc' must not be null", nameof(details.oidc));
        }
        if(details.saml != null)
        {
            throw new ControllerArgumentException("property 'saml' must be null", nameof(details.saml));
        }
        await _provisioningManager.UpdateCentralIdentityProviderDataOIDCAsync(
            new IdentityProviderEditableConfigOidc(
                alias,
                details.displayName,
                details.oidc.metadataUrl,
                details.oidc.clientAuthMethod,
                details.oidc.clientId,
                details.oidc.secret,
                details.oidc.signatureAlgorithm))
            .ConfigureAwait(false);
        await _provisioningManager.GetCentralIdentityProviderDataOIDCAsync(alias).ConfigureAwait(false);
    }

    private async ValueTask UpdateIdentityProviderSaml(string alias, IdentityProviderEditableDetails details)
    {
        if(details.saml == null)
        {
            throw new ControllerArgumentException("property 'saml' must not be null", nameof(details.saml));
        }
        if(details.oidc != null)
        {
            throw new ControllerArgumentException("property 'oidc' must be null", nameof(details.oidc));
        }
        await _provisioningManager.UpdateCentralIdentityProviderDataSAMLAsync(
            new IdentityProviderEditableConfigSaml(
                alias,
                details.displayName,
                details.saml.serviceProviderEntityId,
                details.saml.singleSignOnServiceUrl))
            .ConfigureAwait(false);
        await _provisioningManager.GetCentralIdentityProviderDataSAMLAsync(alias).ConfigureAwait(false);
    }

    private async ValueTask UpdateIdentityProviderShared(string alias, IdentityProviderEditableDetails details)
    {
        if(details.oidc != null)
        {
            throw new ControllerArgumentException("property 'oidc' must be null", nameof(details.oidc));
        }
        if(details.saml != null)
        {
            throw new ControllerArgumentException("property 'saml' must be null", nameof(details.saml));
        }
        await _provisioningManager.UpdateSharedIdentityProviderAsync(alias, details.displayName).ConfigureAwait(false);
    }

    private async ValueTask ValidateOtherActiveIdentityProvider(string alias, IEnumerable<string> aliase, Action noSuccessAction)
    {
        if (!await aliase
            .Where(_alias => _alias != alias)
            .ToAsyncEnumerable()
            .AnyAwaitAsync(alias => _provisioningManager.IsCentralIdentityProviderEnabled(alias)))
        {
            noSuccessAction();
        }
    }

    public async ValueTask DeleteOwnCompanyIdentityProviderAsync(Guid identityProviderId, string iamUserId)
    {
        var (companyId, companyCount, alias, category) = await ValidateDeleteOwnCompanyIdentityProviderArguments(identityProviderId, iamUserId).ConfigureAwait(false);

        _portalRepositories.Remove(new CompanyIdentityProvider(companyId, identityProviderId));
        if (companyCount == 1)
        {
            if (alias != null)
            {
                _portalRepositories.Remove(new IamIdentityProvider(alias, Guid.Empty));
                if (category == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                {
                    await _provisioningManager.DeleteSharedIdpRealmAsync(alias).ConfigureAwait(false);
                }
                await _provisioningManager.DeleteCentralIdentityProviderAsync(alias).ConfigureAwait(false);
            }
            _portalRepositories.Remove(_portalRepositories.Attach(new IdentityProvider(identityProviderId, default, default)));
        }
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private async ValueTask<(Guid CompanyId, int CompanyCount, string Alias, IdentityProviderCategoryId Category)> ValidateDeleteOwnCompanyIdentityProviderArguments(Guid identityProviderId, string iamUserId)
    {
        var result = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderDeletionDataUntrackedAsync(identityProviderId, iamUserId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"identityProvider {identityProviderId} does not exist");
        }
        var (companyId, companyCount, alias, category, aliase) = result;
        if (result.CompanyId == Guid.Empty)
        {
            throw new ForbiddenException($"identityProvider {identityProviderId} is not associated with company of user {iamUserId}");
        }

        if (await _provisioningManager.IsCentralIdentityProviderEnabled(alias).ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"cannot delete identityProvider {identityProviderId} as it is enabled");
        }

        await ValidateOtherActiveIdentityProvider(
            alias,
            aliase,
            () => throw new ControllerArgumentException($"cannot delete indentityProvider {identityProviderId} as no other active identityProvider exists for this company")
        ).ConfigureAwait(false);

        return new ValueTuple<Guid, int, string, IdentityProviderCategoryId>(companyId, companyCount, alias, category);
    }

    private async ValueTask<IdentityProviderDetails> GetIdentityProviderDetailsOidc(Guid identityProviderId, string alias, IdentityProviderCategoryId categoryId)
    {
        var identityProviderDataOIDC = await _provisioningManager.GetCentralIdentityProviderDataOIDCAsync(alias).ConfigureAwait(false);
        return new IdentityProviderDetails(
            identityProviderId,
            alias,
            categoryId,
            identityProviderDataOIDC.DisplayName,
            identityProviderDataOIDC.RedirectUrl,
            identityProviderDataOIDC.Enabled,
            await _provisioningManager.GetIdentityProviderMappers(alias).ToListAsync().ConfigureAwait(false))
            {
                oidc = new IdentityProviderDetailsOidc(
                    identityProviderDataOIDC.AuthorizationUrl,
                    identityProviderDataOIDC.ClientId,
                    identityProviderDataOIDC.ClientAuthMethod)
                    {
                        signatureAlgorithm = identityProviderDataOIDC.SignatureAlgorithm
                    }
            };
    }

    private async ValueTask<IdentityProviderDetails> GetIdentityProviderDetailsSaml(Guid identityProviderId, string alias)
    {
        var identityProviderDataSAML = await _provisioningManager.GetCentralIdentityProviderDataSAMLAsync(alias).ConfigureAwait(false);
        return new IdentityProviderDetails(
            identityProviderId,
            alias,
            IdentityProviderCategoryId.KEYCLOAK_SAML,
            identityProviderDataSAML.DisplayName,
            identityProviderDataSAML.RedirectUrl,
            identityProviderDataSAML.Enabled,
            await _provisioningManager.GetIdentityProviderMappers(alias).ToListAsync().ConfigureAwait(false))
            {
                saml = new IdentityProviderDetailsSaml(
                    identityProviderDataSAML.EntityId,
                    identityProviderDataSAML.SingleSignOnServiceUrl)
            };
    }

    public async ValueTask<UserIdentityProviderLinkData> CreateOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, UserIdentityProviderLinkData identityProviderLinkData, string iamUserId)
    {
        var (userEntityId, alias) = await GetUserAliasDataAsync(companyUserId, identityProviderLinkData.identityProviderId, iamUserId).ConfigureAwait(false);

        try
        {
            await _provisioningManager.AddProviderUserLinkToCentralUserAsync(
                userEntityId,
                new IdentityProviderLink(
                    alias,
                    identityProviderLinkData.userId,
                    identityProviderLinkData.userName))
                .ConfigureAwait(false);
        }
        catch(KeycloakEntityConflictException ce)
        {
            throw new ConflictException($"identityProviderLink for identityProvider {identityProviderLinkData.identityProviderId} already exists for user {companyUserId}", ce);
        }
        
        return new UserIdentityProviderLinkData(
            identityProviderLinkData.identityProviderId,
            identityProviderLinkData.userId,
            identityProviderLinkData.userName);
    }

    public async ValueTask<UserIdentityProviderLinkData> UpdateOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, Guid identityProviderId, UserLinkData userLinkData, string iamUserId)
    {
        var (userEntityId, alias) = await GetUserAliasDataAsync(companyUserId, identityProviderId, iamUserId).ConfigureAwait(false);

        try
        {
            await _provisioningManager.DeleteProviderUserLinkToCentralUserAsync(userEntityId, alias);
        }
        catch(KeycloakEntityNotFoundException e)
        {
            throw new NotFoundException($"identityProviderLink for identityProvider {identityProviderId} not found in keycloak for user {companyUserId}", e);
        }
        await _provisioningManager.AddProviderUserLinkToCentralUserAsync(
            userEntityId,
            new IdentityProviderLink(
                alias,
                userLinkData.userId,
                userLinkData.userName))
            .ConfigureAwait(false);
        
        return new UserIdentityProviderLinkData(
            identityProviderId,
            userLinkData.userId,
            userLinkData.userName);
    }

    public async ValueTask<UserIdentityProviderLinkData> GetOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, Guid identityProviderId, string iamUserId)
    {
        var (userEntityId, alias) = await GetUserAliasDataAsync(companyUserId, identityProviderId, iamUserId).ConfigureAwait(false);

        var result = await _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(userEntityId).FirstOrDefaultAsync(identityProviderLink => identityProviderLink.Alias == alias).ConfigureAwait(false);

        if (result == default)
        {
            throw new NotFoundException($"identityProviderLink for identityProvider {identityProviderId} not found in keycloak for user {companyUserId}");
        }
        return new UserIdentityProviderLinkData(
            identityProviderId,
            result.UserId,
            result.UserName);
    }

    public async ValueTask DeleteOwnCompanyUserIdentityProviderDataAsync(Guid companyUserId, Guid identityProviderId, string iamUserId)
    {
        var (userEntityId, alias) = await GetUserAliasDataAsync(companyUserId, identityProviderId, iamUserId).ConfigureAwait(false);
        try
        {
            await _provisioningManager.DeleteProviderUserLinkToCentralUserAsync(userEntityId, alias).ConfigureAwait(false);
        }
        catch(KeycloakEntityNotFoundException e)
        {
            throw new NotFoundException($"identityProviderLink for identityProvider {identityProviderId} not found in keycloak for user {companyUserId}", e);
        }
    }

    public async IAsyncEnumerable<UserIdentityProviderData> GetOwnCompanyUsersIdentityProviderDataAsync(IEnumerable<Guid> identityProviderIds, string iamUserId, bool unlinkedUsersOnly)
    {
        var identityProviderAliasDatas = await GetOwnCompanyUsersIdentityProviderAliasDataInternalAsync(identityProviderIds, iamUserId).ConfigureAwait(false);
        var idPerAlias = identityProviderAliasDatas.ToDictionary(item => item.Alias, item => item.IdentityProviderId);
        var aliase = identityProviderAliasDatas.Select(item => item.Alias).ToList();

        await foreach (var (companyUserId, userProfile, links) in GetOwnCompanyIdentityProviderLinkDataInternalAsync(iamUserId).ConfigureAwait(false))
        {
            var identityProviderLinks = await links.ToListAsync().ConfigureAwait(false);
            if (!unlinkedUsersOnly
                || aliase.Any(alias => identityProviderLinks.All(identityProviderLink => identityProviderLink.Alias != alias)))
            {
                yield return new UserIdentityProviderData(
                    companyUserId,
                    userProfile.FirstName,
                    userProfile.LastName,
                    userProfile.Email,
                    identityProviderLinks
                        .Where(identityProviderLink => aliase.Contains(identityProviderLink.Alias))
                        .Select(linkData => new UserIdentityProviderLinkData(
                            idPerAlias[linkData.Alias],
                            linkData.UserId,
                            linkData.UserName))
                );
            }
        }
    }

    public (Stream FileStream, string ContentType, string FileName, Encoding Encoding) GetOwnCompanyUsersIdentityProviderLinkDataStream(IEnumerable<Guid> identityProviderIds, string iamUserId, bool unlinkedUsersOnly)
    {
        var csvSettings = _settings.CsvSettings;
        return (new AsyncEnumerableStringStream(GetOwnCompanyUsersIdentityProviderDataLines(identityProviderIds, unlinkedUsersOnly, iamUserId), csvSettings.Encoding), csvSettings.ContentType, csvSettings.FileName, csvSettings.Encoding);
    }

    public ValueTask<IdentityProviderUpdateStats> UploadOwnCompanyUsersIdentityProviderLinkDataAsync(IFormFile document, string iamUserId)
    {
        if (!document.ContentType.Equals(_settings.CsvSettings.ContentType, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnsupportedMediaTypeException($"Only contentType {_settings.CsvSettings.ContentType} files are allowed.");
        }
        return UploadOwnCompanyUsersIdentityProviderLinkDataInternalAsync(document, iamUserId);
    }

    private async ValueTask<IdentityProviderUpdateStats> UploadOwnCompanyUsersIdentityProviderLinkDataInternalAsync(IFormFile document, string iamUserId)
    {
        var userRepository = _portalRepositories.GetInstance<IUserRepository>();
        var (companyId, creatorId) = await userRepository.GetOwnCompanAndCompanyUseryId(iamUserId).ConfigureAwait(false);
        if (companyId == Guid.Empty)
        {
            throw new ControllerArgumentException($"user {iamUserId} is not associated with a company");
        }
        var (sharedIdpAlias, existingAliase) = await GetOwnCompaniesAliasDataAsync(iamUserId).ConfigureAwait(false);

        using var stream = document.OpenReadStream();
        var reader = new StreamReader(stream, _settings.CsvSettings.Encoding);

        var numIdps = await ValidateHeadersReturningNumIdpsAsync(reader).ConfigureAwait(false);

        var nextLine = await reader.ReadLineAsync().ConfigureAwait(false);
        int numUpdates = 0;
        int numUnchanged = 0;
        var errors = new List<String>();
        int numLines = 0;
        while (nextLine != null)
        {
            numLines++;
            try
            {
                var (companyUserId, profile, identityProviderLinks) = ParseCSVLine(nextLine, numIdps, existingAliase);
                var (userEntityId, existingProfile, links) = await GetExistingUserAndLinkDataAsync(userRepository, companyUserId, companyId).ConfigureAwait(false);
                var existingLinks = await links.ToListAsync().ConfigureAwait(false);
                var updated = false;

                foreach (var identityProviderLink in identityProviderLinks)
                {
                    updated |= await UpdateIdentityProviderLinksAsync(userEntityId, companyUserId, identityProviderLink, existingLinks, sharedIdpAlias).ConfigureAwait(false);
                }

                if (existingProfile != profile)
                {
                    await UpdateUserProfileAsync(userEntityId, companyUserId, profile, existingLinks, sharedIdpAlias, creatorId).ConfigureAwait(false);
                    updated = true;
                }
                if (updated)
                {
                    numUpdates++;
                }
                else
                {
                    numUnchanged++;
                }
            }
            catch(Exception e)
            {
                errors.Add($"line: {numLines}, message: {e.Message}");
            }

            nextLine = await reader.ReadLineAsync().ConfigureAwait(false);
        }
        return new IdentityProviderUpdateStats(numUpdates, numUnchanged, errors.Count, numLines, errors);
    }

    private async ValueTask<int> ValidateHeadersReturningNumIdpsAsync(StreamReader reader)
    {
        var firstLine = await reader.ReadLineAsync().ConfigureAwait(false);
        if (firstLine == null)
        {
            throw new ControllerArgumentException("uploaded file contains no lines");
        }
        return ParseCSVFirstLineReturningNumIdps(firstLine);
    }

    private async ValueTask<(string? SharedIdpAlias, IEnumerable<string> ValidAliase)> GetOwnCompaniesAliasDataAsync(string iamUserId)
    {
        var identityProviderCategoryData = (await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderCategoryDataUntracked(iamUserId).ToListAsync().ConfigureAwait(false));
        var sharedIdpAlias = identityProviderCategoryData.Where(data => data.CategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED).Select(data => data.Alias).SingleOrDefault();
        var validAliase = identityProviderCategoryData.Select(data => data.Alias).ToList();
        return (sharedIdpAlias, validAliase);
    }

    private async ValueTask<(string UserEntityId, UserProfile ExistingProfile, IAsyncEnumerable<IdentityProviderLink> ExistingLinks)> GetExistingUserAndLinkDataAsync(IUserRepository userRepository, Guid companyUserId, Guid companyId)
    {
        var userEntityData = await userRepository.GetUserEntityDataAsync(companyUserId, companyId).ConfigureAwait(false);
        if (userEntityData == default)
        {
            throw new ControllerArgumentException($"unexpected value of {_settings.CsvSettings.HeaderUserId}: '{companyUserId}'");
        }
        var (userEntityId, existingFirstName, existingLastName, existingEmail) = userEntityData;

        return (
            userEntityId,
            new UserProfile(existingFirstName, existingLastName, existingEmail),
            _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(userEntityId)
        );
    }

    private async ValueTask<bool> UpdateIdentityProviderLinksAsync(string userEntityId, Guid companyUserId, IdentityProviderLink identityProviderLink, IEnumerable<IdentityProviderLink> existingLinks, string? sharedIdpAlias)
    {
        var (alias, userId, userName) = identityProviderLink;

        if (existingLinks.Contains(identityProviderLink))
        {
            return false;
        }
        if (alias == sharedIdpAlias)
        {
            throw new ControllerArgumentException($"unexpected update of shared identityProviderLink, alias '{alias}', companyUser '{companyUserId}', providerUserId: '{identityProviderLink.UserId}', providerUserName: '{identityProviderLink.UserName}'");
        }
        var updated = false;
        if (existingLinks.Any(link => link.Alias == alias))
        {
            await _provisioningManager.DeleteProviderUserLinkToCentralUserAsync(userEntityId, alias).ConfigureAwait(false);
            updated = true;
        }
        if (string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(userName))
        {
            return updated;
        }
        await _provisioningManager.AddProviderUserLinkToCentralUserAsync(userEntityId, identityProviderLink).ConfigureAwait(false);
        return true;
    }

    private async ValueTask UpdateUserProfileAsync(string userEntityId, Guid companyUserId, UserProfile profile, IEnumerable<IdentityProviderLink> existingLinks, string? sharedIdpAlias, Guid creatorId)
    {
        if (!await _provisioningManager.UpdateCentralUserAsync(userEntityId, profile.FirstName ?? "", profile.LastName ?? "", profile.Email ?? "").ConfigureAwait(false))
        {
            throw new UnexpectedConditionException($"error updating central keycloak-user {userEntityId}");
        }
        if (sharedIdpAlias != null)
        {
            var sharedIdpLink = existingLinks.FirstOrDefault(link => link.Alias == sharedIdpAlias);
            if (sharedIdpLink != default && !await _provisioningManager.UpdateSharedRealmUserAsync(sharedIdpAlias, sharedIdpLink.UserId, profile.FirstName ?? "", profile.LastName ?? "", profile.Email ?? "").ConfigureAwait(false))
            {
                throw new UnexpectedConditionException($"error updating central keycloak-user {userEntityId}");
            }
        }
        _portalRepositories.Attach(new CompanyUser(companyUserId, Guid.Empty, default, default, creatorId), companyUser =>
        {
            companyUser.Firstname = profile.FirstName;
            companyUser.Lastname = profile.LastName;
            companyUser.Email = profile.Email;
        });
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
   }

    private int ParseCSVFirstLineReturningNumIdps(string firstLine)
    {
        var headers = firstLine.Split(_settings.CsvSettings.Separator).GetEnumerator();
        foreach (var csvHeader in CSVHeaders())
        {
            if (!headers.MoveNext())
            {
                throw new ControllerArgumentException($"invalid format: expected '{csvHeader}', got ''");
            }
            if ((string)headers.Current != csvHeader)
            {
                throw new ControllerArgumentException($"invalid format: expected '{csvHeader}', got '{headers.Current}'");
            }
        }
        var numIdps = 0;
        var hasNext = headers.MoveNext();
        while (hasNext)
        {
            foreach (var csvHeader in CSVIdpHeaders())
            {
                if (!hasNext)
                {
                    throw new ControllerArgumentException($"invalid format: expected '{csvHeader}', got ''");
                }
                if ((string)headers.Current != csvHeader)
                {
                    throw new ControllerArgumentException($"invalid format: expected '{csvHeader}', got '{headers.Current}'");
                }
                hasNext = headers.MoveNext();
            }
            numIdps++;
        }
        return numIdps;
    }

    private (Guid CompanyUserId, UserProfile UserProfile, IEnumerable<IdentityProviderLink> IdentityProviderLinks) ParseCSVLine(string line, int numIdps, IEnumerable<string> existingAliase)
    {
        var items = line.Split(_settings.CsvSettings.Separator).AsEnumerable().GetEnumerator();
        if(!items.MoveNext())
        {
            throw new ControllerArgumentException($"value for {_settings.CsvSettings.HeaderUserId} type Guid expected");
        }
        if (!Guid.TryParse(items.Current, out var companyUserId))
        {
            throw new ControllerArgumentException($"invalid format for {_settings.CsvSettings.HeaderUserId} type Guid: '{items.Current}'");
        }
        if(!items.MoveNext())
        {
            throw new ControllerArgumentException($"value for {_settings.CsvSettings.HeaderFirstName} expected");
        }
        var firstName = items.Current;
        if(!items.MoveNext())
        {
            throw new ControllerArgumentException($"value for {_settings.CsvSettings.HeaderLastName} expected");
        }
        var lastName = items.Current;
        if(!items.MoveNext())
        {
            throw new ControllerArgumentException($"value for {_settings.CsvSettings.HeaderEmail} expected");
        }
        var email = items.Current;
        var identityProviderLinks = ParseCSVIdentityProviderLinks(items, numIdps, existingAliase).ToList();
        return (companyUserId, new UserProfile(firstName, lastName, email), identityProviderLinks);
    }

    private IEnumerable<IdentityProviderLink> ParseCSVIdentityProviderLinks(IEnumerator<string> items, int numIdps, IEnumerable<string> existingAliase)
    {
        var remaining = numIdps;
        while (remaining > 0)
        {
            if(!items.MoveNext())
            {
                throw new ControllerArgumentException($"value for {_settings.CsvSettings.HeaderProviderAlias} expected");
            }
            var identityProviderAlias = items.Current;
            if (!existingAliase.Contains(identityProviderAlias))
            {
                throw new ControllerArgumentException($"unexpected value for {_settings.CsvSettings.HeaderProviderAlias}: {identityProviderAlias}]");
            }
            if(!items.MoveNext())
            {
                throw new ControllerArgumentException($"value for {_settings.CsvSettings.HeaderProviderUserId} expected");
            }
            var identityProviderUserId = items.Current;
            if(!items.MoveNext())
            {
                throw new ControllerArgumentException($"value for {_settings.CsvSettings.HeaderProviderUserName} expected");
            }
            var identityProviderUserName = items.Current;
            yield return new IdentityProviderLink(identityProviderAlias, identityProviderUserId, identityProviderUserName);
            remaining--;
        }
    }

    private IEnumerable<string> CSVHeaders() {
        var csvSettings = _settings.CsvSettings;
        yield return csvSettings.HeaderUserId; 
        yield return csvSettings.HeaderFirstName;
        yield return csvSettings.HeaderLastName;
        yield return csvSettings.HeaderEmail;
    }

    private IEnumerable<string> CSVIdpHeaders() {
        var csvSettings = _settings.CsvSettings;
        yield return csvSettings.HeaderProviderAlias;
        yield return csvSettings.HeaderProviderUserId;
        yield return csvSettings.HeaderProviderUserName;
    }

    private async IAsyncEnumerable<string> GetOwnCompanyUsersIdentityProviderDataLines(IEnumerable<Guid> identityProviderIds, bool unlinkedUsersOnly, string iamUserId)
    {
        var idpAliasDatas = await GetOwnCompanyUsersIdentityProviderAliasDataInternalAsync(identityProviderIds, iamUserId).ConfigureAwait(false);
        var aliase = idpAliasDatas.Select(data => data.Alias).ToList();
        var csvSettings = _settings.CsvSettings;

        bool firstLine = true;

        await foreach (var (companyUserId, userProfile, identityProviderLinksAsync) in GetOwnCompanyIdentityProviderLinkDataInternalAsync(iamUserId).ConfigureAwait(false))
        {
            if (firstLine)
            {
                firstLine = false;
                yield return string.Join(csvSettings.Separator, CSVHeaders().Concat(idpAliasDatas.SelectMany(data => CSVIdpHeaders())));
            }
            var identityProviderLinks = await identityProviderLinksAsync.ToListAsync().ConfigureAwait(false);
            if (!unlinkedUsersOnly
                || aliase.Any(alias => identityProviderLinks.All(identityProviderLink => identityProviderLink.Alias != alias)))
            {
                yield return string.Join(
                    csvSettings.Separator,
                    companyUserId,
                    userProfile.FirstName,
                    userProfile.LastName,
                    userProfile.Email,
                    string.Join(csvSettings.Separator, aliase.SelectMany(alias =>
                        {
                            var identityProviderLink = identityProviderLinks.FirstOrDefault(linkData => linkData.Alias == alias);
                            return new [] { alias, identityProviderLink?.UserId, identityProviderLink?.UserName };
                        })));
            }
        }
    }

    private async ValueTask<IEnumerable<(Guid IdentityProviderId, string Alias)>> GetOwnCompanyUsersIdentityProviderAliasDataInternalAsync(IEnumerable<Guid> identityProviderIds, string iamUserId)
    {
        if (!identityProviderIds.Any())
        {
            throw new ControllerArgumentException("at least one identityProviderId must be specified", nameof(identityProviderIds));
        }
        var identityProviderData = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderAliasDataUntracked(iamUserId, identityProviderIds).ToListAsync().ConfigureAwait(false);

        var invalidIds = identityProviderIds.Except(identityProviderData.Select(data => data.IdentityProviderId));
        if (invalidIds.Any())
        {
            throw new ControllerArgumentException($"invalid identityProviders: [{String.Join(", ", invalidIds)}] for user {iamUserId}", nameof(identityProviderIds));
        }

        return identityProviderData;
    }

    private async IAsyncEnumerable<(Guid CompanyUserId, UserProfile UserProfile, IAsyncEnumerable<IdentityProviderLink> LinkDatas)> GetOwnCompanyIdentityProviderLinkDataInternalAsync(string iamUserId)
    {
        await foreach(var (companyUserId, firstName, lastName, email, userEntityId) in _portalRepositories.GetInstance<IUserRepository>()
            .GetOwnCompanyUserQuery(iamUserId)
            .Select(companyUser =>
                new ValueTuple<Guid, string?, string?, string?, string?>(
                    companyUser.Id,
                    companyUser.Firstname,
                    companyUser.Lastname,
                    companyUser.Email,
                    companyUser.IamUser!.UserEntityId))
            .ToAsyncEnumerable().ConfigureAwait(false))
        {
            if (userEntityId != null)
            {
                yield return (
                    companyUserId,
                    new UserProfile(firstName, lastName, email),
                    _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(userEntityId)
                );
            }
        }
    }

    private sealed class AsyncEnumerableStringStream : Stream
    {
        public AsyncEnumerableStringStream(IAsyncEnumerable<string> data, Encoding encoding) : base()
        {
            _enumerator = data.GetAsyncEnumerator();
            _stream = new MemoryStream();
            _writer = new StreamWriter(_stream, encoding);
        }

        private readonly IAsyncEnumerator<string> _enumerator;
        private readonly MemoryStream _stream;
        private readonly TextWriter _writer;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanTimeout => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }
        public override long Seek (long offset, System.IO.SeekOrigin origin) => throw new NotSupportedException();
        public override void Flush() => throw new NotSupportedException();
        public override int Read(byte [] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Write(byte [] buffer, int offset, int count) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();

        public override async ValueTask<int> ReadAsync (Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var written = _stream.Read(buffer.Span);
            while (buffer.Length - written > 0 && await _enumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
            {
                _stream.Position = 0;
                _stream.SetLength(0);
                _writer.WriteLine(_enumerator.Current);
                _writer.Flush();
                _stream.Position = 0;

                written += _stream.Read(buffer.Span.Slice(written));
            }
            return written;
        }
    }

    private async ValueTask<(string UserEntityId, string Alias)> GetUserAliasDataAsync(Guid companyUserId, Guid identityProviderId, string iamUserId)
    {
        var userAliasData = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, iamUserId).ConfigureAwait(false);
        if (userAliasData == default)
        {
            throw new NotFoundException($"companyUserId {companyUserId} does not exist");
        }
        if (userAliasData.UserEntityId == null)
        {
            throw new UnexpectedConditionException($"companyUserId {companyUserId} is not linked to keycloak");
        }
        if (userAliasData.Alias == null)
        {
            throw new NotFoundException($"identityProvider {identityProviderId} not found in company of user {companyUserId}" );
        }
        if (!userAliasData.IsSameCompany)
        {
            throw new ForbiddenException($"user {iamUserId} does not belong to company of companyUserId {companyUserId}");
        }
        return new ValueTuple<string, string>(
            userAliasData.UserEntityId,
            userAliasData.Alias);
    }

    private sealed record UserProfile(string? FirstName, string? LastName, string? Email);
}
