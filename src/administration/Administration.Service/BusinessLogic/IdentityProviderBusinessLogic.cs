/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class IdentityProviderBusinessLogic : IIdentityProviderBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IdentityProviderSettings _settings;

    private static readonly Regex _displayNameValidationExpression = new Regex(@"^[a-zA-Z0-9\!\?\@\&\#\'\x22\(\)_\-\=\/\*\.\,\;\: ]+$", RegexOptions.None, TimeSpan.FromSeconds(1));

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
            switch (identityProviderData.CategoryId)
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

    public ValueTask<IdentityProviderDetails> CreateOwnCompanyIdentityProviderAsync(IamIdentityProviderProtocol protocol, string? displayName, string iamUserId)
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
        if (displayName != null)
        {
            ValidateDisplayName(displayName);
        }

        return CreateOwnCompanyIdentityProviderInternalAsync(identityProviderCategory, protocol, displayName, iamUserId);
    }

    private static void ValidateDisplayName(string displayName)
    {
        if (displayName.Length < 2 || displayName.Length > 30)
        {
            throw new ControllerArgumentException("displayName length must be 2-30 characters");
        }
        if (!_displayNameValidationExpression.IsMatch(displayName))
        {
            throw new ControllerArgumentException("allowed characters in displayName: 'a-zA-Z0-9!?@&#'\"()_-=/*.,;: '");
        }
    }

    private async ValueTask<IdentityProviderDetails> CreateOwnCompanyIdentityProviderInternalAsync(IdentityProviderCategoryId identityProviderCategory, IamIdentityProviderProtocol protocol, string? displayName, string iamUserId)
    {
        var identityProviderRepository = _portalRepositories.GetInstance<IIdentityProviderRepository>();

        var result = await _portalRepositories.GetInstance<ICompanyRepository>().GetCompanyNameIdUntrackedAsync(iamUserId).ConfigureAwait(false);
        if (result == default)
        {
            throw new ControllerArgumentException($"user {iamUserId} is not associated with a company", nameof(iamUserId));
        }
        var alias = await _provisioningManager.CreateOwnIdpAsync(displayName ?? result.CompanyName, result.CompanyName, protocol).ConfigureAwait(false);
        var identityProvider = identityProviderRepository.CreateIdentityProvider(identityProviderCategory);
        identityProvider.CompanyIdentityProviders.Add(identityProviderRepository.CreateCompanyIdentityProvider(result.CompanyId, identityProvider.Id));
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

    public async ValueTask<IdentityProviderDetails> GetOwnCompanyIdentityProviderAsync(Guid identityProviderId, IdentityData identity)
    {
        var (alias, category) = await ValidateGetOwnCompanyIdentityProviderArguments(identityProviderId, identity).ConfigureAwait(false);

        switch (category)
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

    private async ValueTask<(string Alias, IdentityProviderCategoryId Category)> ValidateGetOwnCompanyIdentityProviderArguments(Guid identityProviderId, IdentityData identity)
    {
        var (alias, category, isOwnCompany) = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderAliasUntrackedAsync(identityProviderId, identity.CompanyId).ConfigureAwait(false);
        if (!isOwnCompany)
        {
            throw new ForbiddenException($"identityProvider {identityProviderId} is not associated with company of user {identity.UserEntityId}");
        }
        if (alias == null)
        {
            throw new NotFoundException($"identityProvider {identityProviderId} does not exist");
        }
        return new ValueTuple<string, IdentityProviderCategoryId>(alias, category);
    }

    public async ValueTask<IdentityProviderDetails> SetOwnCompanyIdentityProviderStatusAsync(Guid identityProviderId, bool enabled, IdentityData identity)
    {
        var (category, alias) = await ValidateSetOwnCompanyIdentityProviderStatusArguments(identityProviderId, enabled, identity).ConfigureAwait(false);

        switch (category)
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

    private async ValueTask<(IdentityProviderCategoryId Category, string Alias)> ValidateSetOwnCompanyIdentityProviderStatusArguments(Guid identityProviderId, bool enabled, IdentityData identity)
    {
        var result = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(identityProviderId, identity.CompanyId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"identityProvider {identityProviderId} does not exist");
        }
        if (!result.IsSameCompany)
        {
            throw new ForbiddenException($"identityProvider {identityProviderId} is not associated with company of user {identity.UserEntityId}");
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

    public async ValueTask<IdentityProviderDetails> UpdateOwnCompanyIdentityProviderAsync(Guid identityProviderId, IdentityProviderEditableDetails details, IdentityData identity)
    {
        var (category, alias) = await ValidateUpdateOwnCompanyIdentityProviderArguments(identityProviderId, details, identity).ConfigureAwait(false);

        switch (category)
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

    private async ValueTask<(IdentityProviderCategoryId Category, string Alias)> ValidateUpdateOwnCompanyIdentityProviderArguments(Guid identityProviderId, IdentityProviderEditableDetails details, IdentityData identity)
    {
        ValidateDisplayName(details.displayName);

        var result = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(identityProviderId, identity.CompanyId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"identityProvider {identityProviderId} does not exist");
        }
        if (!result.IsSameCompany)
        {
            throw new ForbiddenException($"identityProvider {identityProviderId} is not associated with company of user {identity.UserEntityId}");
        }
        return new ValueTuple<IdentityProviderCategoryId, string>(result.IdentityProviderCategory, result.Alias);
    }

    private async ValueTask UpdateIdentityProviderOidc(string alias, IdentityProviderEditableDetails details)
    {
        if (details.oidc == null)
        {
            throw new ControllerArgumentException("property 'oidc' must not be null", nameof(details.oidc));
        }
        if (details.saml != null)
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
    }

    private async ValueTask UpdateIdentityProviderSaml(string alias, IdentityProviderEditableDetails details)
    {
        if (details.saml == null)
        {
            throw new ControllerArgumentException("property 'saml' must not be null", nameof(details.saml));
        }
        if (details.oidc != null)
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
    }

    private async ValueTask UpdateIdentityProviderShared(string alias, IdentityProviderEditableDetails details)
    {
        if (details.oidc != null)
        {
            throw new ControllerArgumentException("property 'oidc' must be null", nameof(details.oidc));
        }
        if (details.saml != null)
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

    public async ValueTask<UserIdentityProviderLinkData> CreateOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, UserIdentityProviderLinkData identityProviderLinkData, IdentityData identity)
    {
        var (userEntityId, alias) = await GetUserAliasDataAsync(companyUserId, identityProviderLinkData.identityProviderId, identity).ConfigureAwait(false);

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
        catch (KeycloakEntityConflictException ce)
        {
            throw new ConflictException($"identityProviderLink for identityProvider {identityProviderLinkData.identityProviderId} already exists for user {companyUserId}", ce);
        }

        return new UserIdentityProviderLinkData(
            identityProviderLinkData.identityProviderId,
            identityProviderLinkData.userId,
            identityProviderLinkData.userName);
    }

    public async ValueTask<UserIdentityProviderLinkData> CreateOrUpdateOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, Guid identityProviderId, UserLinkData userLinkData, IdentityData identity)
    {
        var (userEntityId, alias) = await GetUserAliasDataAsync(companyUserId, identityProviderId, identity).ConfigureAwait(false);

        try
        {
            await _provisioningManager.DeleteProviderUserLinkToCentralUserAsync(userEntityId, alias);
        }
        catch (KeycloakEntityNotFoundException)
        {
            // for create-and-update semantics this is expected and not an error
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

    public async ValueTask<UserIdentityProviderLinkData> GetOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, Guid identityProviderId, IdentityData identity)
    {
        var (userEntityId, alias) = await GetUserAliasDataAsync(companyUserId, identityProviderId, identity).ConfigureAwait(false);

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

    public async ValueTask DeleteOwnCompanyUserIdentityProviderDataAsync(Guid companyUserId, Guid identityProviderId, IdentityData identity)
    {
        var (userEntityId, alias) = await GetUserAliasDataAsync(companyUserId, identityProviderId, identity).ConfigureAwait(false);
        try
        {
            await _provisioningManager.DeleteProviderUserLinkToCentralUserAsync(userEntityId, alias).ConfigureAwait(false);
        }
        catch (KeycloakEntityNotFoundException e)
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

    public ValueTask<IdentityProviderUpdateStats> UploadOwnCompanyUsersIdentityProviderLinkDataAsync(IFormFile document, string iamUserId, CancellationToken cancellationToken)
    {
        if (!document.ContentType.Equals(_settings.CsvSettings.ContentType, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnsupportedMediaTypeException($"Only contentType {_settings.CsvSettings.ContentType} files are allowed.");
        }
        return UploadOwnCompanyUsersIdentityProviderLinkDataInternalAsync(document, iamUserId, cancellationToken);
    }

    private async ValueTask<IdentityProviderUpdateStats> UploadOwnCompanyUsersIdentityProviderLinkDataInternalAsync(IFormFile document, string iamUserId, CancellationToken cancellationToken)
    {
        var userRepository = _portalRepositories.GetInstance<IUserRepository>();
        var (companyId, creatorId) = await userRepository.GetOwnCompanyAndCompanyUserId(iamUserId).ConfigureAwait(false);
        if (companyId == Guid.Empty)
        {
            throw new ControllerArgumentException($"user {iamUserId} is not associated with a company");
        }
        var (sharedIdpAlias, existingAliase) = await GetCompanyAliasDataAsync(companyId).ConfigureAwait(false);

        using var stream = document.OpenReadStream();

        int numIdps = default;

        var (numProcessed, numLines, errors) = await CsvParser.ProcessCsvAsync(
            stream,
            line =>
            {
                numIdps = ParseCSVFirstLineReturningNumIdps(line);
            },
            line => ParseCSVLine(line, numIdps, existingAliase),
            lines => ProcessOwnCompanyUsersIdentityProviderLinkDataInternalAsync(lines, userRepository, companyId, sharedIdpAlias, creatorId, cancellationToken),
            cancellationToken
        ).ConfigureAwait(false);

        var numErrors = errors.Count();
        var numUnchanged = numLines - numProcessed - numErrors;

        return new IdentityProviderUpdateStats(numProcessed, numUnchanged, numErrors, numLines, errors.Select(x => $"line: {x.Line}, message: {x.Error.Message}"));
    }

    private async IAsyncEnumerable<(bool, Exception?)> ProcessOwnCompanyUsersIdentityProviderLinkDataInternalAsync(
        IAsyncEnumerable<(Guid CompanyUserId, UserProfile UserProfile, IEnumerable<IdentityProviderLink> IdentityProviderLinks)> userProfileLinkDatas,
        IUserRepository userRepository,
        Guid companyId,
        string? sharedIdpAlias,
        Guid creatorId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var (companyUserId, profile, identityProviderLinks) in userProfileLinkDatas)
        {
            Exception? error = null;
            var success = false;
            try
            {
                var (userEntityId, existingProfile, links) = await GetExistingUserAndLinkDataAsync(userRepository, companyUserId, companyId).ConfigureAwait(false);
                var existingLinks = await links.ToListAsync(cancellationToken).ConfigureAwait(false);
                var updated = false;

                cancellationToken.ThrowIfCancellationRequested();

                foreach (var identityProviderLink in identityProviderLinks)
                {
                    updated |= await UpdateIdentityProviderLinksAsync(userEntityId, companyUserId, identityProviderLink, existingLinks, sharedIdpAlias).ConfigureAwait(false);
                }

                if (existingProfile != profile)
                {
                    await UpdateUserProfileAsync(userRepository, userEntityId, companyUserId, profile, existingLinks, sharedIdpAlias, creatorId).ConfigureAwait(false);
                    updated = true;
                }
                success = updated;
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    throw;
                }
                error = e;
            }
            yield return (success, error);
        }
    }

    private async ValueTask<(string? SharedIdpAlias, IEnumerable<string> ValidAliase)> GetCompanyAliasDataAsync(Guid companyId)
    {
        var identityProviderCategoryData = (await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetCompanyIdentityProviderCategoryDataUntracked(companyId).ToListAsync().ConfigureAwait(false));
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

        var existingLink = existingLinks.SingleOrDefault(link => link.Alias == alias);

        if ((existingLink == null && string.IsNullOrWhiteSpace(userName) && string.IsNullOrWhiteSpace(userId)) ||
            (existingLink != null && existingLink.UserName == userName && existingLink.UserId == userId))
        {
            return false;
        }

        if (alias == sharedIdpAlias)
        {
            throw new ControllerArgumentException($"unexpected update of shared identityProviderLink, alias '{alias}', companyUser '{companyUserId}', providerUserId: '{userId}', providerUserName: '{userName}'");
        }

        if (existingLink != null)
        {
            await _provisioningManager.DeleteProviderUserLinkToCentralUserAsync(userEntityId, alias).ConfigureAwait(false);
        }
        await _provisioningManager.AddProviderUserLinkToCentralUserAsync(userEntityId, identityProviderLink).ConfigureAwait(false);
        return true;
    }

    private async ValueTask UpdateUserProfileAsync(IUserRepository userRepository, string userEntityId, Guid companyUserId, UserProfile profile, IEnumerable<IdentityProviderLink> existingLinks, string? sharedIdpAlias, Guid creatorId)
    {
        var (firstName, lastName, email) = (profile.FirstName ?? "", profile.LastName ?? "", profile.Email ?? "");

        await _provisioningManager.UpdateCentralUserAsync(userEntityId, firstName, lastName, email).ConfigureAwait(false);

        if (sharedIdpAlias != null)
        {
            var sharedIdpLink = existingLinks.FirstOrDefault(link => link.Alias == sharedIdpAlias);
            if (sharedIdpLink != default)
            {
                await _provisioningManager.UpdateSharedRealmUserAsync(sharedIdpAlias, sharedIdpLink.UserId, firstName, lastName, email).ConfigureAwait(false);
            }
        }
        userRepository.AttachAndModifyCompanyUser(companyUserId, null, companyUser =>
            {
                companyUser.Firstname = profile.FirstName;
                companyUser.Lastname = profile.LastName;
                companyUser.Email = profile.Email;
                companyUser.LastEditorId = creatorId;
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

    private ValueTask<(Guid CompanyUserId, UserProfile UserProfile, IEnumerable<IdentityProviderLink> IdentityProviderLinks)> ParseCSVLine(string line, int numIdps, IEnumerable<string> existingAliase)
    {
        var items = line.Split(_settings.CsvSettings.Separator).AsEnumerable().GetEnumerator();
        if (!items.MoveNext())
        {
            throw new ControllerArgumentException($"value for {_settings.CsvSettings.HeaderUserId} type Guid expected");
        }
        if (!Guid.TryParse(items.Current, out var companyUserId))
        {
            throw new ControllerArgumentException($"invalid format for {_settings.CsvSettings.HeaderUserId} type Guid: '{items.Current}'");
        }
        if (!items.MoveNext())
        {
            throw new ControllerArgumentException($"value for {_settings.CsvSettings.HeaderFirstName} expected");
        }
        var firstName = items.Current;
        if (!items.MoveNext())
        {
            throw new ControllerArgumentException($"value for {_settings.CsvSettings.HeaderLastName} expected");
        }
        var lastName = items.Current;
        if (!items.MoveNext())
        {
            throw new ControllerArgumentException($"value for {_settings.CsvSettings.HeaderEmail} expected");
        }
        var email = items.Current;
        var identityProviderLinks = ParseCSVIdentityProviderLinks(items, numIdps, existingAliase).ToList();
        return ValueTask.FromResult((companyUserId, new UserProfile(firstName, lastName, email), identityProviderLinks.AsEnumerable()));
    }

    private IEnumerable<IdentityProviderLink> ParseCSVIdentityProviderLinks(IEnumerator<string> items, int numIdps, IEnumerable<string> existingAliase)
    {
        var remaining = numIdps;
        while (remaining > 0)
        {
            if (!items.MoveNext())
            {
                throw new ControllerArgumentException($"value for {_settings.CsvSettings.HeaderProviderAlias} expected");
            }
            var identityProviderAlias = items.Current;
            if (!existingAliase.Contains(identityProviderAlias))
            {
                throw new ControllerArgumentException($"unexpected value for {_settings.CsvSettings.HeaderProviderAlias}: {identityProviderAlias}]");
            }
            if (!items.MoveNext())
            {
                throw new ControllerArgumentException($"value for {_settings.CsvSettings.HeaderProviderUserId} expected");
            }
            var identityProviderUserId = items.Current;
            if (!items.MoveNext())
            {
                throw new ControllerArgumentException($"value for {_settings.CsvSettings.HeaderProviderUserName} expected");
            }
            var identityProviderUserName = items.Current;
            yield return new IdentityProviderLink(identityProviderAlias, identityProviderUserId, identityProviderUserName);
            remaining--;
        }
    }

    private IEnumerable<string> CSVHeaders()
    {
        var csvSettings = _settings.CsvSettings;
        yield return csvSettings.HeaderUserId;
        yield return csvSettings.HeaderFirstName;
        yield return csvSettings.HeaderLastName;
        yield return csvSettings.HeaderEmail;
    }

    private IEnumerable<string> CSVIdpHeaders()
    {
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

        var firstLine = true;

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
                            return new[] { alias, identityProviderLink?.UserId, identityProviderLink?.UserName };
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
        await foreach (var (companyUserId, firstName, lastName, email, userEntityId) in _portalRepositories.GetInstance<IUserRepository>()
            .GetOwnCompanyUserQuery(iamUserId)
            .Select(companyUser =>
                new ValueTuple<Guid, string?, string?, string?, string?>(
                    companyUser.Id,
                    companyUser.Firstname,
                    companyUser.Lastname,
                    companyUser.Email,
                    companyUser.UserEntityId))
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

    private async ValueTask<(string UserEntityId, string Alias)> GetUserAliasDataAsync(Guid companyUserId, Guid identityProviderId, IdentityData identity)
    {
        var userAliasData = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, identity.CompanyId).ConfigureAwait(false);
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
            throw new NotFoundException($"identityProvider {identityProviderId} not found in company of user {companyUserId}");
        }
        if (!userAliasData.IsSameCompany)
        {
            throw new ForbiddenException($"user {identity.UserEntityId} does not belong to company of companyUserId {companyUserId}");
        }
        return new ValueTuple<string, string>(
            userAliasData.UserEntityId,
            userAliasData.Alias);
    }

    private sealed record UserProfile(string? FirstName, string? LastName, string? Email);
}
