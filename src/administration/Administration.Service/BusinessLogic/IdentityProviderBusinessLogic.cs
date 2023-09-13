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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
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
    private readonly IIdentityService _identityService;
    private readonly ILogger<IdentityProviderBusinessLogic> _logger;
    private readonly IdentityProviderSettings _settings;

    private static readonly Regex DisplayNameValidationExpression = new(@"^[a-zA-Z0-9\!\?\@\&\#\'\x22\(\)_\-\=\/\*\.\,\;\: ]+$", RegexOptions.None, TimeSpan.FromSeconds(1));

    public IdentityProviderBusinessLogic(IPortalRepositories portalRepositories, IProvisioningManager provisioningManager, IIdentityService identityService, IOptions<IdentityProviderSettings> options, ILogger<IdentityProviderBusinessLogic> logger)
    {
        _portalRepositories = portalRepositories;
        _provisioningManager = provisioningManager;
        _identityService = identityService;
        _settings = options.Value;
        _logger = logger;
    }

    public async IAsyncEnumerable<IdentityProviderDetails> GetOwnCompanyIdentityProvidersAsync()
    {
        var companyId = _identityService.IdentityData.CompanyId;
        await foreach (var identityProviderData in _portalRepositories.GetInstance<IIdentityProviderRepository>().GetCompanyIdentityProviderCategoryDataUntracked(companyId).ConfigureAwait(false))
        {
            yield return identityProviderData.CategoryId switch
            {
                IdentityProviderCategoryId.KEYCLOAK_OIDC => await GetIdentityProviderDetailsOidc(identityProviderData.IdentityProviderId, identityProviderData.Alias, identityProviderData.CategoryId, identityProviderData.TypeId).ConfigureAwait(false),
                IdentityProviderCategoryId.KEYCLOAK_SAML => await GetIdentityProviderDetailsSaml(identityProviderData.IdentityProviderId, identityProviderData.Alias, identityProviderData.TypeId),
                _ => throw new ControllerArgumentException($"unexpected value for category '{identityProviderData.CategoryId}'")
            };
        }
    }

    public ValueTask<IdentityProviderDetails> CreateOwnCompanyIdentityProviderAsync(IamIdentityProviderProtocol protocol, IdentityProviderTypeId typeId, string? displayName)
    {
        var identityProviderCategory = protocol switch
        {
            IamIdentityProviderProtocol.SAML => IdentityProviderCategoryId.KEYCLOAK_SAML,
            IamIdentityProviderProtocol.OIDC => IdentityProviderCategoryId.KEYCLOAK_OIDC,
            _ => throw new ControllerArgumentException($"unexcepted value of protocol: '{protocol}'", nameof(protocol))
        };
        if (displayName != null)
        {
            ValidateDisplayName(displayName);
        }

        return CreateOwnCompanyIdentityProviderInternalAsync(identityProviderCategory, protocol, typeId, displayName);
    }

    private static void ValidateDisplayName(string displayName)
    {
        if (displayName.Length is < 2 or > 30)
        {
            throw new ControllerArgumentException("displayName length must be 2-30 characters");
        }
        if (!DisplayNameValidationExpression.IsMatch(displayName))
        {
            throw new ControllerArgumentException("allowed characters in displayName: 'a-zA-Z0-9!?@&#'\"()_-=/*.,;: '");
        }
    }

    private async ValueTask<IdentityProviderDetails> CreateOwnCompanyIdentityProviderInternalAsync(IdentityProviderCategoryId identityProviderCategory, IamIdentityProviderProtocol protocol, IdentityProviderTypeId typeId, string? displayName)
    {
        var companyId = _identityService.IdentityData.CompanyId;
        var identityProviderRepository = _portalRepositories.GetInstance<IIdentityProviderRepository>();
        var requiredCompanyRoles = typeId switch
        {
            IdentityProviderTypeId.OWN => Enumerable.Empty<CompanyRoleId>(),
            IdentityProviderTypeId.MANAGED => new[] { CompanyRoleId.OPERATOR, CompanyRoleId.ONBOARDING_SERVICE_PROVIDER },
            _ => throw new ControllerArgumentException($"creation of identityProviderType {typeId} is not supported")
        };
        var result = await _portalRepositories.GetInstance<ICompanyRepository>().CheckCompanyAndCompanyRolesAsync(companyId, requiredCompanyRoles).ConfigureAwait(false);
        if (!result.IsValidCompany)
        {
            throw new ControllerArgumentException($"company {companyId} does not exist", nameof(companyId));
        }

        if (!result.IsAllowed)
        {
            throw new ForbiddenException($"Not allowed to create an identityProvider of type {typeId}");
        }

        var alias = await _provisioningManager.CreateOwnIdpAsync(displayName ?? result.CompanyName, result.CompanyName, protocol).ConfigureAwait(false);
        var identityProviderId = identityProviderRepository.CreateIdentityProvider(identityProviderCategory, typeId, companyId, null).Id;
        if (typeId == IdentityProviderTypeId.OWN)
        {
            identityProviderRepository.CreateCompanyIdentityProvider(companyId, identityProviderId);
        }
        identityProviderRepository.CreateIamIdentityProvider(identityProviderId, alias);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);

        return protocol switch
        {
            IamIdentityProviderProtocol.OIDC => await GetIdentityProviderDetailsOidc(identityProviderId, alias, IdentityProviderCategoryId.KEYCLOAK_OIDC, typeId).ConfigureAwait(false),
            IamIdentityProviderProtocol.SAML => await GetIdentityProviderDetailsSaml(identityProviderId, alias, typeId).ConfigureAwait(false),
            _ => throw new UnexpectedConditionException($"unexpected value of protocol: '{protocol.ToString()}'")
        };
    }

    public async ValueTask<IdentityProviderDetails> GetOwnCompanyIdentityProviderAsync(Guid identityProviderId)
    {
        var companyId = _identityService.IdentityData.CompanyId;
        var (alias, category, typeId) = await ValidateGetOwnCompanyIdentityProviderArguments(identityProviderId, companyId).ConfigureAwait(false);

        return category switch
        {
            IdentityProviderCategoryId.KEYCLOAK_OIDC => await GetIdentityProviderDetailsOidc(identityProviderId, alias, category, typeId).ConfigureAwait(false),
            IdentityProviderCategoryId.KEYCLOAK_SAML => await GetIdentityProviderDetailsSaml(identityProviderId, alias, typeId).ConfigureAwait(false),
            _ => throw new ControllerArgumentException($"unexpected value for category '{category}' of identityProvider '{identityProviderId}'")
        };
    }

    private async ValueTask<(string Alias, IdentityProviderCategoryId Category, IdentityProviderTypeId TypeId)> ValidateGetOwnCompanyIdentityProviderArguments(Guid identityProviderId, Guid companyId)
    {
        var (alias, category, isOwnOrOwnerCompany, typeId) = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderAliasUntrackedAsync(identityProviderId, companyId).ConfigureAwait(false);
        if (!isOwnOrOwnerCompany)
        {
            throw new ConflictException($"identityProvider {identityProviderId} is not associated with company {companyId}");
        }

        if (alias == null)
        {
            throw new NotFoundException($"identityProvider {identityProviderId} does not exist");
        }

        if (category == IdentityProviderCategoryId.KEYCLOAK_SAML && typeId is IdentityProviderTypeId.SHARED)
        {
            throw new ConflictException("Shared Idps must not use SAML");
        }

        return new ValueTuple<string, IdentityProviderCategoryId, IdentityProviderTypeId>(alias, category, typeId);
    }

    public async ValueTask<IdentityProviderDetails> SetOwnCompanyIdentityProviderStatusAsync(Guid identityProviderId, bool enabled)
    {
        var companyId = _identityService.IdentityData.CompanyId;
        var (category, alias, typeId) = await ValidateSetOwnCompanyIdentityProviderStatusArguments(identityProviderId, enabled, companyId).ConfigureAwait(false);

        switch (category)
        {
            case IdentityProviderCategoryId.KEYCLOAK_OIDC when typeId is IdentityProviderTypeId.SHARED:
                await _provisioningManager.SetSharedIdentityProviderStatusAsync(alias, enabled).ConfigureAwait(false);
                return await GetIdentityProviderDetailsOidc(identityProviderId, alias, category, typeId).ConfigureAwait(false);
            case IdentityProviderCategoryId.KEYCLOAK_OIDC:
                await _provisioningManager.SetCentralIdentityProviderStatusAsync(alias, enabled).ConfigureAwait(false);
                return await GetIdentityProviderDetailsOidc(identityProviderId, alias, category, typeId).ConfigureAwait(false);
            case IdentityProviderCategoryId.KEYCLOAK_SAML:
                await _provisioningManager.SetCentralIdentityProviderStatusAsync(alias, enabled).ConfigureAwait(false);
                return await GetIdentityProviderDetailsSaml(identityProviderId, alias, typeId).ConfigureAwait(false);
            default:
                throw new ControllerArgumentException($"unexpected value for category '{category}' of identityProvider '{identityProviderId}'");
        }
    }

    private async ValueTask<(IdentityProviderCategoryId Category, string Alias, IdentityProviderTypeId TypeId)> ValidateSetOwnCompanyIdentityProviderStatusArguments(Guid identityProviderId, bool enabled, Guid companyId)
    {
        var result = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(identityProviderId, companyId, true).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"identityProvider {identityProviderId} does not exist");
        }
        var (isOwner, alias, identityProviderCategory, identityProviderTypeId, companyIdAliase) = result;
        if (!isOwner)
        {
            throw new ForbiddenException($"company {companyId} is not the owner of identityProvider {identityProviderId}");
        }
        if (alias == null)
        {
            throw new ConflictException($"identityprovider {identityProviderId} does not have an iamIdentityProvider.alias");
        }
        if (!enabled &&
            !await ValidateOtherActiveIdentityProvider(
                alias,
                companyIdAliase ?? throw new UnexpectedConditionException("CompanyIdAliase should never be null here")).ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"cannot disable indentityProvider {identityProviderId} as no other active identityProvider exists for this company");
        }
        return new ValueTuple<IdentityProviderCategoryId, string, IdentityProviderTypeId>(identityProviderCategory, alias, identityProviderTypeId);
    }

    public async ValueTask<IdentityProviderDetails> UpdateOwnCompanyIdentityProviderAsync(Guid identityProviderId, IdentityProviderEditableDetails details)
    {
        var (category, alias, typeId) = await ValidateUpdateOwnCompanyIdentityProviderArguments(identityProviderId, details).ConfigureAwait(false);

        switch (category)
        {
            case IdentityProviderCategoryId.KEYCLOAK_OIDC when typeId is IdentityProviderTypeId.SHARED:
                await UpdateIdentityProviderShared(alias, details).ConfigureAwait(false);
                return await GetIdentityProviderDetailsOidc(identityProviderId, alias, category, typeId).ConfigureAwait(false);
            case IdentityProviderCategoryId.KEYCLOAK_OIDC:
                await UpdateIdentityProviderOidc(alias, details).ConfigureAwait(false);
                return await GetIdentityProviderDetailsOidc(identityProviderId, alias, category, typeId).ConfigureAwait(false);
            case IdentityProviderCategoryId.KEYCLOAK_SAML:
                await UpdateIdentityProviderSaml(alias, details).ConfigureAwait(false);
                return await GetIdentityProviderDetailsSaml(identityProviderId, alias, typeId).ConfigureAwait(false);
            default:
                throw new ControllerArgumentException($"unexpected value for category '{category.ToString()}' of identityProvider '{identityProviderId}'");
        }
    }

    private async ValueTask<(IdentityProviderCategoryId Category, string Alias, IdentityProviderTypeId TypeId)> ValidateUpdateOwnCompanyIdentityProviderArguments(Guid identityProviderId, IdentityProviderEditableDetails details)
    {
        var companyId = _identityService.IdentityData.CompanyId;
        ValidateDisplayName(details.displayName);

        var result = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(identityProviderId, companyId, false).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"identityProvider {identityProviderId} does not exist");
        }
        var (isOwner, alias, identityProviderCategory, identityProviderTypeId, _) = result;
        if (!isOwner)
        {
            throw new ForbiddenException($"User not allowed to run the change for identity provider {identityProviderId}");
        }
        if (alias == null)
        {
            throw new ConflictException($"identityprovider {identityProviderId} does not have an iamIdentityProvider.alias");
        }
        return new ValueTuple<IdentityProviderCategoryId, string, IdentityProviderTypeId>(identityProviderCategory, alias, identityProviderTypeId);
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

    private async ValueTask<bool> ValidateOtherActiveIdentityProvider(string? alias, IEnumerable<(Guid CompanyId, IEnumerable<string> Aliase)> companyIdAliase)
    {
        var aliasStatus = (await Task.WhenAll(companyIdAliase.SelectMany(x => x.Aliase).Where(x => x != alias).Distinct().Select(async alias => (Alias: alias, Enabled: await _provisioningManager.IsCentralIdentityProviderEnabled(alias).ConfigureAwait(false)))).ConfigureAwait(false)).ToDictionary(x => x.Alias, x => x.Enabled);
        return companyIdAliase.All(x =>
            x.Aliase.Where(a => a != alias).Any(a => aliasStatus[a]));
    }

    public async ValueTask DeleteCompanyIdentityProviderAsync(Guid identityProviderId)
    {
        var companyId = _identityService.IdentityData.CompanyId;
        var (alias, typeId) = await ValidateDeleteOwnCompanyIdentityProviderArguments(identityProviderId, companyId).ConfigureAwait(false);

        _portalRepositories.Remove(new CompanyIdentityProvider(companyId, identityProviderId));

        if (alias != null)
        {
            _portalRepositories.Remove(new IamIdentityProvider(alias, Guid.Empty));
            if (typeId == IdentityProviderTypeId.SHARED)
            {
                await _provisioningManager.DeleteSharedIdpRealmAsync(alias).ConfigureAwait(false);
            }
            await _provisioningManager.DeleteCentralIdentityProviderAsync(alias).ConfigureAwait(false);
        }
        _portalRepositories.Remove(_portalRepositories.Attach(new IdentityProvider(identityProviderId, default, default, default, default)));

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private async ValueTask<(string? Alias, IdentityProviderTypeId TypeId)> ValidateDeleteOwnCompanyIdentityProviderArguments(Guid identityProviderId, Guid companyId)
    {
        var result = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(identityProviderId, companyId, true).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"identityProvider {identityProviderId} does not exist");
        }
        var (isOwner, alias, _, typeId, aliase) = result;
        if (!isOwner)
        {
            throw new ForbiddenException($"company {companyId} is not the owner of identityProvider {identityProviderId}");
        }

        if (typeId == IdentityProviderTypeId.MANAGED)
        {
            throw new ConflictException($"IdentityProviders of type {typeId} can not be deleted");
        }

        if (alias != null)
        {
            if (await _provisioningManager.IsCentralIdentityProviderEnabled(alias).ConfigureAwait(false))
            {
                throw new ControllerArgumentException($"cannot delete identityProvider {identityProviderId} as it is enabled");
            }

            if (!await ValidateOtherActiveIdentityProvider(
                alias,
                aliase ?? throw new UnexpectedConditionException("CompanyIdAliase should never be null here")).ConfigureAwait(false))
            {
                throw new ControllerArgumentException($"cannot delete indentityProvider {identityProviderId} as no other active identityProvider exists for this company");
            }
        }

        return (alias, typeId);
    }

    private async ValueTask<IdentityProviderDetails> GetIdentityProviderDetailsOidc(Guid identityProviderId, string? alias, IdentityProviderCategoryId categoryId, IdentityProviderTypeId typeId)
    {
        IdentityProviderConfigOidc? identityProviderDataOidc = null;
        List<IdentityProviderMapperModel>? identityProviderMapper = null;

        if (!string.IsNullOrWhiteSpace(alias))
        {
            var aliasExisting = true;
            try
            {
                identityProviderDataOidc = await _provisioningManager.GetCentralIdentityProviderDataOIDCAsync(alias)
                    .ConfigureAwait(false);
            }
            catch (KeycloakEntityNotFoundException ex)
            {
                _logger.LogInformation("Can't receive saml data for {Alias} with following exception {Exception}", alias, ex.Message);
                aliasExisting = false;
            }

            if (aliasExisting)
            {
                identityProviderMapper = await _provisioningManager.GetIdentityProviderMappers(alias).ToListAsync().ConfigureAwait(false);
            }
        }

        return new IdentityProviderDetails(
            identityProviderId,
            alias,
            categoryId,
            typeId,
            identityProviderDataOidc?.DisplayName,
            identityProviderDataOidc?.RedirectUrl,
            identityProviderDataOidc?.Enabled,
            identityProviderMapper)
        {
            oidc = identityProviderDataOidc == null ?
                null :
                new IdentityProviderDetailsOidc(
                    identityProviderDataOidc.AuthorizationUrl,
                    identityProviderDataOidc.ClientId,
                    identityProviderDataOidc.ClientAuthMethod)
                {
                    signatureAlgorithm = identityProviderDataOidc.SignatureAlgorithm
                }
        };
    }

    private async ValueTask<IdentityProviderDetails> GetIdentityProviderDetailsSaml(Guid identityProviderId, string? alias, IdentityProviderTypeId typeId)
    {

        IdentityProviderConfigSaml? identityProviderDataSaml = null;
        List<IdentityProviderMapperModel>? identityProviderMapper = null;
        if (!string.IsNullOrWhiteSpace(alias))
        {
            var aliasExisting = true;
            try
            {
                identityProviderDataSaml = await _provisioningManager
                    .GetCentralIdentityProviderDataSAMLAsync(alias).ConfigureAwait(false);
            }
            catch (KeycloakEntityNotFoundException ex)
            {
                _logger.LogInformation("Can't receive saml data for {Alias} with following exception {Exception}", alias, ex.Message);
                aliasExisting = false;
            }

            if (aliasExisting)
            {
                identityProviderMapper = await _provisioningManager.GetIdentityProviderMappers(alias).ToListAsync()
                    .ConfigureAwait(false);
            }
        }

        return new IdentityProviderDetails(
            identityProviderId,
            alias,
            IdentityProviderCategoryId.KEYCLOAK_SAML,
            typeId,
            identityProviderDataSaml?.DisplayName,
            identityProviderDataSaml?.RedirectUrl,
            identityProviderDataSaml?.Enabled,
            identityProviderMapper)
        {
            saml = identityProviderDataSaml == null ?
                null :
                new IdentityProviderDetailsSaml(
                    identityProviderDataSaml.EntityId,
                    identityProviderDataSaml.SingleSignOnServiceUrl)
        };
    }

    public async ValueTask<UserIdentityProviderLinkData> CreateOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, UserIdentityProviderLinkData identityProviderLinkData, Guid companyId)
    {
        var (userEntityId, alias) = await GetUserAliasDataAsync(companyUserId, identityProviderLinkData.identityProviderId, companyId).ConfigureAwait(false);

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

    public async ValueTask<UserIdentityProviderLinkData> CreateOrUpdateOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, Guid identityProviderId, UserLinkData userLinkData, Guid companyId)
    {
        var (userEntityId, alias) = await GetUserAliasDataAsync(companyUserId, identityProviderId, companyId).ConfigureAwait(false);

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

    public async ValueTask<UserIdentityProviderLinkData> GetOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, Guid identityProviderId, Guid companyId)
    {
        var (userEntityId, alias) = await GetUserAliasDataAsync(companyUserId, identityProviderId, companyId).ConfigureAwait(false);

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

    public async ValueTask DeleteOwnCompanyUserIdentityProviderDataAsync(Guid companyUserId, Guid identityProviderId, Guid companyId)
    {
        var (userEntityId, alias) = await GetUserAliasDataAsync(companyUserId, identityProviderId, companyId).ConfigureAwait(false);
        try
        {
            await _provisioningManager.DeleteProviderUserLinkToCentralUserAsync(userEntityId, alias).ConfigureAwait(false);
        }
        catch (KeycloakEntityNotFoundException e)
        {
            throw new NotFoundException($"identityProviderLink for identityProvider {identityProviderId} not found in keycloak for user {companyUserId}", e);
        }
    }

    public async IAsyncEnumerable<UserIdentityProviderData> GetOwnCompanyUsersIdentityProviderDataAsync(IEnumerable<Guid> identityProviderIds, Guid companyId, bool unlinkedUsersOnly)
    {
        var identityProviderAliasDatas = await GetOwnCompanyUsersIdentityProviderAliasDataInternalAsync(identityProviderIds, companyId).ConfigureAwait(false);
        var idPerAlias = identityProviderAliasDatas.ToDictionary(item => item.Alias, item => item.IdentityProviderId);
        var aliase = identityProviderAliasDatas.Select(item => item.Alias).ToList();

        await foreach (var (companyUserId, userProfile, links) in GetOwnCompanyIdentityProviderLinkDataInternalAsync(companyId).ConfigureAwait(false))
        {
            var identityProviderLinks = await links.ToListAsync().ConfigureAwait(false);
            if (!unlinkedUsersOnly || aliase.Except(identityProviderLinks.Select(link => link.Alias)).Any())
            {
                yield return new UserIdentityProviderData(
                    companyUserId,
                    userProfile.FirstName,
                    userProfile.LastName,
                    userProfile.Email,
                    identityProviderLinks
                        .IntersectBy(aliase, link => link.Alias)
                        .Select(linkData => new UserIdentityProviderLinkData(
                            idPerAlias[linkData.Alias],
                            linkData.UserId,
                            linkData.UserName))
                );
            }
        }
    }

    public (Stream FileStream, string ContentType, string FileName, Encoding Encoding) GetOwnCompanyUsersIdentityProviderLinkDataStream(IEnumerable<Guid> identityProviderIds, Guid companyId, bool unlinkedUsersOnly)
    {
        var csvSettings = _settings.CsvSettings;
        return (new AsyncEnumerableStringStream(GetOwnCompanyUsersIdentityProviderDataLines(identityProviderIds, unlinkedUsersOnly, companyId), csvSettings.Encoding), csvSettings.ContentType, csvSettings.FileName, csvSettings.Encoding);
    }

    public ValueTask<IdentityProviderUpdateStats> UploadOwnCompanyUsersIdentityProviderLinkDataAsync(IFormFile document, Guid companyId, CancellationToken cancellationToken)
    {
        if (!document.ContentType.Equals(_settings.CsvSettings.ContentType, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnsupportedMediaTypeException($"Only contentType {_settings.CsvSettings.ContentType} files are allowed.");
        }
        return UploadOwnCompanyUsersIdentityProviderLinkDataInternalAsync(document, companyId, cancellationToken);
    }

    private async ValueTask<IdentityProviderUpdateStats> UploadOwnCompanyUsersIdentityProviderLinkDataInternalAsync(IFormFile document, Guid companyId, CancellationToken cancellationToken)
    {
        var userRepository = _portalRepositories.GetInstance<IUserRepository>();
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
            lines => ProcessOwnCompanyUsersIdentityProviderLinkDataInternalAsync(lines, userRepository, companyId, sharedIdpAlias, cancellationToken),
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
                    await UpdateUserProfileAsync(userRepository, userEntityId, companyUserId, profile, existingLinks, sharedIdpAlias).ConfigureAwait(false);
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
        var identityProviderCategoryData = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetCompanyIdentityProviderCategoryDataUntracked(companyId).ToListAsync().ConfigureAwait(false);
        var sharedIdpAlias = identityProviderCategoryData.Where(data => data.TypeId == IdentityProviderTypeId.SHARED).Select(data => data.Alias).SingleOrDefault();
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

    private async ValueTask UpdateUserProfileAsync(IUserRepository userRepository, string userEntityId, Guid companyUserId, UserProfile profile, IEnumerable<IdentityProviderLink> existingLinks, string? sharedIdpAlias)
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

    private async IAsyncEnumerable<string> GetOwnCompanyUsersIdentityProviderDataLines(IEnumerable<Guid> identityProviderIds, bool unlinkedUsersOnly, Guid companyId)
    {
        var idpAliasDatas = await GetOwnCompanyUsersIdentityProviderAliasDataInternalAsync(identityProviderIds, companyId).ConfigureAwait(false);
        var aliase = idpAliasDatas.Select(data => data.Alias).ToList();
        var csvSettings = _settings.CsvSettings;

        var firstLine = true;

        await foreach (var (companyUserId, userProfile, identityProviderLinksAsync) in GetOwnCompanyIdentityProviderLinkDataInternalAsync(companyId).ConfigureAwait(false))
        {
            if (firstLine)
            {
                firstLine = false;
                yield return string.Join(csvSettings.Separator, CSVHeaders().Concat(idpAliasDatas.SelectMany(data => CSVIdpHeaders())));
            }
            var identityProviderLinks = await identityProviderLinksAsync.ToListAsync().ConfigureAwait(false);
            if (!unlinkedUsersOnly || aliase.Except(identityProviderLinks.Select(link => link.Alias)).Any())
            {
                yield return string.Join(
                    csvSettings.Separator,
                    companyUserId,
                    userProfile.FirstName,
                    userProfile.LastName,
                    userProfile.Email,
                    string.Join(csvSettings.Separator, aliase.SelectMany(alias =>
                        {
                            var identityProviderLink = identityProviderLinks.Find(linkData => linkData.Alias == alias);
                            return new[] { alias, identityProviderLink?.UserId, identityProviderLink?.UserName };
                        })));
            }
        }
    }

    private async ValueTask<IEnumerable<(Guid IdentityProviderId, string Alias)>> GetOwnCompanyUsersIdentityProviderAliasDataInternalAsync(IEnumerable<Guid> identityProviderIds, Guid companyId)
    {
        if (!identityProviderIds.Any())
        {
            throw new ControllerArgumentException("at least one identityProviderId must be specified", nameof(identityProviderIds));
        }
        var identityProviderData = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderAliasDataUntracked(companyId, identityProviderIds).ToListAsync().ConfigureAwait(false);

        var invalidIds = identityProviderIds.Except(identityProviderData.Select(data => data.IdentityProviderId));
        if (invalidIds.Any())
        {
            throw new ControllerArgumentException($"invalid identityProviders: [{String.Join(", ", invalidIds)}] for company {companyId}", nameof(identityProviderIds));
        }

        return identityProviderData;
    }

    private async IAsyncEnumerable<(Guid CompanyUserId, UserProfile UserProfile, IAsyncEnumerable<IdentityProviderLink> LinkDatas)> GetOwnCompanyIdentityProviderLinkDataInternalAsync(Guid companyId)
    {
        await foreach (var (companyUserId, firstName, lastName, email, userEntityId) in _portalRepositories.GetInstance<IUserRepository>()
            .GetOwnCompanyUserQuery(companyId)
            .Select(companyUser =>
                new ValueTuple<Guid, string?, string?, string?, string?>(
                    companyUser.Id,
                    companyUser.Firstname,
                    companyUser.Lastname,
                    companyUser.Email,
                    companyUser.Identity!.UserEntityId))
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

    private async ValueTask<(string UserEntityId, string Alias)> GetUserAliasDataAsync(Guid companyUserId, Guid identityProviderId, Guid companyId)
    {
        var userAliasData = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, companyId).ConfigureAwait(false);
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
            throw new ForbiddenException($"identityProvider {identityProviderId} is not associated with company {companyId}");
        }
        return new ValueTuple<string, string>(
            userAliasData.UserEntityId,
            userAliasData.Alias);
    }

    private sealed record UserProfile(string? FirstName, string? LastName, string? Email);
}
