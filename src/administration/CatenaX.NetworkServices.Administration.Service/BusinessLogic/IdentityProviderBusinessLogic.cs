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

    public async IAsyncEnumerable<IdentityProviderDetails> GetOwnCompanyIdentityProviders(string iamUserId)
    {
        await foreach ( var identityProviderData in _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderCategoryDataUntracked(iamUserId).ConfigureAwait(false))
        {
            switch(identityProviderData.CategoryId)
            {
                case IdentityProviderCategoryId.KEYCLOAK_SHARED:
                case IdentityProviderCategoryId.KEYCLOAK_OIDC:
                    yield return await GetCentralIdentityProviderDetailsOIDCAsync(identityProviderData.IdentityProviderId, identityProviderData.Alias, identityProviderData.CategoryId).ConfigureAwait(false);
                    break;
                case IdentityProviderCategoryId.KEYCLOAK_SAML:
                    var identityProviderDataSAML = await _provisioningManager.GetCentralIdentityProviderDataSAMLAsync(identityProviderData.Alias).ConfigureAwait(false);
                    yield return new IdentityProviderDetails(
                        identityProviderData.IdentityProviderId,
                        identityProviderData.Alias,
                        identityProviderData.CategoryId,
                        identityProviderDataSAML.DisplayName,
                        identityProviderDataSAML.RedirectUrl,
                        identityProviderDataSAML.Enabled)
                        {
                            saml = new IdentityProviderDetailsSaml(
                                identityProviderDataSAML.EntityId,
                                identityProviderDataSAML.SingleSignOnServiceUrl)
                        };
                    break;
            }
        }
    }

    public Task<IdentityProviderDetails> CreateOwnCompanyIdentityProviderAsync(IamIdentityProviderProtocol protocol, string iamUserId)
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

    private async Task<IdentityProviderDetails> CreateOwnCompanyIdentityProviderInternalAsync(IdentityProviderCategoryId identityProviderCategory, IamIdentityProviderProtocol protocol, string iamUserId)
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
                return await GetCentralIdentityProviderDetailsOIDCAsync(identityProvider.Id, alias, IdentityProviderCategoryId.KEYCLOAK_OIDC).ConfigureAwait(false);
            case IamIdentityProviderProtocol.SAML:
                return await GetCentralIdentityProviderDetailsSAMLAsync(identityProvider.Id, alias).ConfigureAwait(false);
            default:
                throw new UnexpectedConditionException($"unexcepted value of protocol: '{protocol.ToString()}'");
        }
    }

    public async Task<IdentityProviderDetails> GetOwnCompanyIdentityProvider(Guid identityProviderId, string iamUserId)
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
        switch(category)
        {
            case IdentityProviderCategoryId.KEYCLOAK_OIDC:
                return await GetCentralIdentityProviderDetailsOIDCAsync(identityProviderId, alias, category).ConfigureAwait(false);
            case IdentityProviderCategoryId.KEYCLOAK_SAML:
                return await GetCentralIdentityProviderDetailsSAMLAsync(identityProviderId, alias).ConfigureAwait(false);
            default:
                throw new ControllerArgumentException($"identityProvider '{identityProviderId}' category '{category.ToString()}' cannot be updated");
        }
    }

    public async Task<IdentityProviderDetails> UpdateOwnCompanyIdentityProvider(Guid identityProviderId, IdentityProviderEditableDetails details, string iamUserId)
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
        switch(category)
        {
            case IdentityProviderCategoryId.KEYCLOAK_OIDC:
                if(details.oidc == null)
                {
                    throw new ControllerArgumentException("property 'oidc' must not be null", nameof(details.oidc));
                }
                await _provisioningManager.UpdateCentralIdentityProviderDataOIDCAsync(
                    new IdentityProviderEditableConfigOidc(
                        alias,
                        details.displayName,
                        details.enabled,
                        details.oidc.metadataUrl,
                        details.oidc.clientAuthMethod,
                        details.oidc.clientId,
                        details.oidc.secret,
                        details.oidc.signatureAlgorithm))
                    .ConfigureAwait(false);
                await _provisioningManager.GetCentralIdentityProviderDataOIDCAsync(alias).ConfigureAwait(false);
                return await GetCentralIdentityProviderDetailsOIDCAsync(identityProviderId, alias, category).ConfigureAwait(false);
            case IdentityProviderCategoryId.KEYCLOAK_SAML:
                if(details.saml == null)
                {
                    throw new ControllerArgumentException("property 'saml' must not be null", nameof(details.saml));
                }
                await _provisioningManager.UpdateCentralIdentityProviderDataSAMLAsync(
                    new IdentityProviderEditableConfigSaml(
                        alias,
                        details.displayName,
                        details.enabled,
                        details.saml.serviceProviderEntityId,
                        details.saml.singleSignOnServiceUrl))
                    .ConfigureAwait(false);
                await _provisioningManager.GetCentralIdentityProviderDataSAMLAsync(alias).ConfigureAwait(false);
                return await GetCentralIdentityProviderDetailsSAMLAsync(identityProviderId, alias).ConfigureAwait(false);
            default:
                throw new ControllerArgumentException($"identityProvider '{identityProviderId}' category '{category.ToString()}' cannot be updated");
        }
    }

    public async Task DeleteOwnCompanyIdentityProvider(Guid identityProviderId, string iamUserId)
    {
        var result = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderDeletionDataUntrackedAsync(identityProviderId, iamUserId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"identityProvider {identityProviderId} does not exist");
        }
        var (companyId, alias, companyCount) = result;
        if (companyId == Guid.Empty)
        {
            throw new ForbiddenException($"identityProvider {identityProviderId} is not associated with company of user {iamUserId}");
        }
        _portalRepositories.Remove(new CompanyIdentityProvider(companyId, identityProviderId));
        if (companyCount == 1)
        {
            if (alias != null)
            {
                _portalRepositories.Remove(new IamIdentityProvider(alias, Guid.Empty));
                await _provisioningManager.DeleteCentralIdentityProviderAsync(alias).ConfigureAwait(false);
            }
            _portalRepositories.Remove(_portalRepositories.Attach(new IdentityProvider(identityProviderId, default, default)));
        }
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private async Task<IdentityProviderDetails> GetCentralIdentityProviderDetailsOIDCAsync(Guid identityProviderId, string alias, IdentityProviderCategoryId categoryId)
    {
        var identityProviderDataOIDC = await _provisioningManager.GetCentralIdentityProviderDataOIDCAsync(alias).ConfigureAwait(false);
        return new IdentityProviderDetails(
            identityProviderId,
            alias,
            categoryId,
            identityProviderDataOIDC.DisplayName,
            identityProviderDataOIDC.RedirectUrl,
            identityProviderDataOIDC.Enabled)
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

    private async Task<IdentityProviderDetails> GetCentralIdentityProviderDetailsSAMLAsync(Guid identityProviderId, string alias)
    {
        var identityProviderDataSAML = await _provisioningManager.GetCentralIdentityProviderDataSAMLAsync(alias).ConfigureAwait(false);
        return new IdentityProviderDetails(
            identityProviderId,
            alias,
            IdentityProviderCategoryId.KEYCLOAK_SAML,
            identityProviderDataSAML.DisplayName,
            identityProviderDataSAML.RedirectUrl,
            identityProviderDataSAML.Enabled)
            {
                saml = new IdentityProviderDetailsSaml(
                    identityProviderDataSAML.EntityId,
                    identityProviderDataSAML.SingleSignOnServiceUrl)
            };
    }

    public async Task<UserIdentityProviderLinkData> CreateOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, UserIdentityProviderLinkData identityProviderLinkData, string iamUserId)
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

    public async Task<UserIdentityProviderLinkData> UpdateOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, Guid identityProviderId, UserLinkData userLinkData, string iamUserId)
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

    public async Task<UserIdentityProviderLinkData> GetOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, Guid identityProviderId, string iamUserId)
    {
        var (userEntityId, alias) = await GetUserAliasDataAsync(companyUserId, identityProviderId, iamUserId).ConfigureAwait(false);

        var result = (await _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(Enumerable.Repeat(alias,1), userEntityId).ConfigureAwait(false)).SingleOrDefault();
        if (result == default)
        {
            throw new NotFoundException($"identityProviderLink for identityProvider {identityProviderId} not found in keycloak for user {companyUserId}");
        }
        return new UserIdentityProviderLinkData(
            identityProviderId,
            result.UserId,
            result.UserName);
    }

    public async Task DeleteOwnCompanyUserIdentityProviderDataAsync(Guid companyUserId, Guid identityProviderId, string iamUserId)
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

    public async IAsyncEnumerable<UserIdentityProviderData> GetOwnCompanyUsersIdentityProviderDataAsync(IEnumerable<Guid> identityProviderIds, string iamUserId)
    {
        var idpAliasDatas = await GetOwnCompanyUsersIdentityProviderAliasDataInternalAsync(identityProviderIds, iamUserId).ConfigureAwait(false);
        var idPerAlias = idpAliasDatas.ToDictionary(item => item.Alias, item => item.IdentityProviderId);

        await foreach (var (companyUserId, userProfile, identityProviderLinks) in GetOwnCompanyIdentityProviderLinkDataInternalAsync(iamUserId, idpAliasDatas).ConfigureAwait(false))
        {
            yield return new UserIdentityProviderData(
                companyUserId,
                userProfile.FirstName,
                userProfile.LastName,
                userProfile.Email,
                identityProviderLinks.Select(linkData => new UserIdentityProviderLinkData(
                    idPerAlias[linkData.Alias],
                    linkData.UserId,
                    linkData.UserName))
            );
        }
    }

    public (Stream FileStream, string ContentType, string FileName, Encoding Encoding) GetOwnCompanyUsersIdentityProviderLinkDataStream(IEnumerable<Guid> identityProviderIds, string iamUserId)
    {
        var csvSettings = _settings.CsvSettings;
        return (new AsyncEnumerableStringStream(GetOwnCompanyUsersIdentityProviderDataLines(identityProviderIds, iamUserId), csvSettings.Encoding), csvSettings.ContentType, csvSettings.FileName, csvSettings.Encoding);
    }

    public Task<IdentityProviderUpdateStats> UploadOwnCompanyUsersIdentityProviderLinkDataAsync(IFormFile document, string iamUserId)
    {
        if (!document.ContentType.Equals(_settings.CsvSettings.ContentType, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnsupportedMediaTypeException($"Only contentType {_settings.CsvSettings.ContentType} files are allowed.");
        }
        return UploadOwnCompanyUsersIdentityProviderLinkDataInternalAsync(document, iamUserId);
    }

    private async Task<IdentityProviderUpdateStats> UploadOwnCompanyUsersIdentityProviderLinkDataInternalAsync(IFormFile document, string iamUserId)
    {
        var userRepository = _portalRepositories.GetInstance<IUserRepository>();
        var companyId = await userRepository.GetOwnCompanyId(iamUserId).ConfigureAwait(false);
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
                var (userEntityId, existingProfile, existingLinks) = await GetExistingUserAndLinkDataAsync(userRepository, companyUserId, companyId, existingAliase).ConfigureAwait(false);
                var updated = false;

                foreach (var identityProviderLink in identityProviderLinks)
                {
                    updated |= await UpdateIdentityProviderLinksAsync(userEntityId, companyUserId, identityProviderLink, existingLinks, sharedIdpAlias).ConfigureAwait(false);
                }

                if (existingProfile != profile)
                {
                    await UpdateUserProfileAsync(userEntityId, companyUserId, profile, existingLinks, sharedIdpAlias).ConfigureAwait(false);
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

    private async Task<int> ValidateHeadersReturningNumIdpsAsync(StreamReader reader)
    {
        var firstLine = await reader.ReadLineAsync().ConfigureAwait(false);
        if (firstLine == null)
        {
            throw new ControllerArgumentException("uploaded file contains no lines");
        }
        return ParseCSVFirstLineReturningNumIdps(firstLine);
    }

    private async Task<(string? SharedIdpAlias, IEnumerable<string> ValidAliase)> GetOwnCompaniesAliasDataAsync(string iamUserId)
    {
        var identityProviderCategoryData = (await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderCategoryDataUntracked(iamUserId).ToListAsync().ConfigureAwait(false));
        var sharedIdpAlias = identityProviderCategoryData.Where(data => data.CategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED).Select(data => data.Alias).SingleOrDefault();
        var validAliase = identityProviderCategoryData.Select(data => data.Alias).ToList();
        return (sharedIdpAlias, validAliase);
    }

    private async Task<(string UserEntityId, UserProfile ExistingProfile, IEnumerable<IdentityProviderLink> ExistingLinks)> GetExistingUserAndLinkDataAsync(IUserRepository userRepository, Guid companyUserId, Guid companyId, IEnumerable<string> existingAliase)
    {
        var userEntityData = await userRepository.GetUserEntityDataAsync(companyUserId, companyId).ConfigureAwait(false);
        if (userEntityData == default)
        {
            throw new ControllerArgumentException($"unexpected value of {_settings.CsvSettings.HeaderUserId}: '{companyUserId}'");
        }
        var (userEntityId, existingFirstName, existingLastName, existingEmail) = userEntityData;

        var existingLinks = await _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(existingAliase, userEntityId).ConfigureAwait(false);
        return (userEntityId, new UserProfile(existingFirstName, existingLastName, existingEmail), existingLinks);
    }

    private async Task<bool> UpdateIdentityProviderLinksAsync(string userEntityId, Guid companyUserId, IdentityProviderLink identityProviderLink, IEnumerable<IdentityProviderLink> existingLinks, string? sharedIdpAlias)
    {
        var (alias, userId, userName) = identityProviderLink;
        var existingLink = existingLinks.SingleOrDefault(link => link.Alias == alias);
        if (existingLink == identityProviderLink)
        {
            return false;
        }
        if (alias == sharedIdpAlias)
        {
            throw new ControllerArgumentException($"unexpected update of shared identityProviderLink, alias '{alias}', companyUser '{companyUserId}', providerUserId: '{identityProviderLink.UserId}', providerUserName: '{identityProviderLink.UserName}'");
        }
        var updated = false;
        if (existingLink != null)
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

    private async Task UpdateUserProfileAsync(string userEntityId, Guid companyUserId, UserProfile profile, IEnumerable<IdentityProviderLink> existingLinks, string? sharedIdpAlias)
    {
        if (!await _provisioningManager.UpdateCentralUserAsync(userEntityId, profile.FirstName ?? "", profile.LastName ?? "", profile.Email ?? "").ConfigureAwait(false))
        {
            throw new UnexpectedConditionException($"error updating central keycloak-user {userEntityId}");
        }
        if (sharedIdpAlias != null)
        {
            var sharedIdpLink = existingLinks.SingleOrDefault(link => link.Alias == sharedIdpAlias);
            if (sharedIdpLink != default && !await _provisioningManager.UpdateSharedRealmUserAsync(sharedIdpAlias, sharedIdpLink.UserId, profile.FirstName ?? "", profile.LastName ?? "", profile.Email ?? "").ConfigureAwait(false))
            {
                throw new UnexpectedConditionException($"error updating central keycloak-user {userEntityId}");
            }
        }
        _portalRepositories.Attach(new CompanyUser(companyUserId, Guid.Empty, default, default), companyUser =>
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

    private async IAsyncEnumerable<string> GetOwnCompanyUsersIdentityProviderDataLines(IEnumerable<Guid> identityProviderIds, string iamUserId)
    {
        var idpAliasDatas = await GetOwnCompanyUsersIdentityProviderAliasDataInternalAsync(identityProviderIds, iamUserId).ConfigureAwait(false);
        var aliase = idpAliasDatas.Select(data => data.Alias).ToList();
        var csvSettings = _settings.CsvSettings;

        bool firstLine = true;

        await foreach (var (companyUserId, userProfile, identityProviderLinks) in GetOwnCompanyIdentityProviderLinkDataInternalAsync(iamUserId, idpAliasDatas).ConfigureAwait(false))
        {
            if (firstLine)
            {
                firstLine = false;
                yield return string.Join(csvSettings.Separator, CSVHeaders().Concat(idpAliasDatas.SelectMany(data => CSVIdpHeaders())));
            }
            yield return string.Join(
                csvSettings.Separator,
                companyUserId,
                userProfile.FirstName,
                userProfile.LastName,
                userProfile.Email,
                string.Join(csvSettings.Separator, aliase.SelectMany(alias =>
                    {
                        var identityProviderLink = identityProviderLinks.SingleOrDefault(linkData => linkData.Alias == alias);
                        return new [] { alias, identityProviderLink?.UserId, identityProviderLink?.UserName };
                    })));
        }
    }

    private async Task<IEnumerable<(Guid IdentityProviderId, string Alias)>> GetOwnCompanyUsersIdentityProviderAliasDataInternalAsync(IEnumerable<Guid> identityProviderIds, string iamUserId)
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

    private async IAsyncEnumerable<(Guid CompanyUserId, UserProfile UserProfile, IEnumerable<IdentityProviderLink> LinkDatas)> GetOwnCompanyIdentityProviderLinkDataInternalAsync(string iamUserId, IEnumerable<(Guid IdentityProviderId, string Alias)> identityProviderAliasDatas)
    {
        var aliase = identityProviderAliasDatas.Select(item => item.Alias).ToList();

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
                    await _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(aliase, userEntityId).ConfigureAwait(false));
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

    private async Task<(string UserEntityId, string Alias)> GetUserAliasDataAsync(Guid companyUserId, Guid identityProviderId, string iamUserId)
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
