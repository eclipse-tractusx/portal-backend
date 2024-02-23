/********************************************************************************
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.Service;
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
    private readonly IIdentityData _identityData;
    private readonly IErrorMessageService _errorMessageService;
    private readonly IRoleBaseMailService _roleBaseMailService;
    private readonly IMailingService _mailingService;
    private readonly IdentityProviderSettings _settings;
    private readonly ILogger<IdentityProviderBusinessLogic> _logger;

    private static readonly Regex DisplayNameValidationExpression = new(@"^[a-zA-Z0-9\!\?\@\&\#\'\x22\(\)_\-\=\/\*\.\,\;\: ]+$", RegexOptions.None, TimeSpan.FromSeconds(1));

    public IdentityProviderBusinessLogic(
        IPortalRepositories portalRepositories,
        IProvisioningManager provisioningManager,
        IIdentityService identityService,
        IErrorMessageService errorMessageService,
        IRoleBaseMailService roleBaseMailService,
        IMailingService mailingService,
        IOptions<IdentityProviderSettings> options,
        ILogger<IdentityProviderBusinessLogic> logger)
    {
        _portalRepositories = portalRepositories;
        _provisioningManager = provisioningManager;
        _identityData = identityService.IdentityData;
        _errorMessageService = errorMessageService;
        _roleBaseMailService = roleBaseMailService;
        _mailingService = mailingService;
        _settings = options.Value;
        _logger = logger;
    }

    public async IAsyncEnumerable<IdentityProviderDetails> GetOwnCompanyIdentityProvidersAsync()
    {
        var companyId = _identityData.CompanyId;
        await foreach (var identityProviderData in _portalRepositories.GetInstance<IIdentityProviderRepository>().GetCompanyIdentityProviderCategoryDataUntracked(companyId).ConfigureAwait(false))
        {
            yield return identityProviderData.CategoryId switch
            {
                IdentityProviderCategoryId.KEYCLOAK_OIDC => await GetIdentityProviderDetailsOidc(identityProviderData.IdentityProviderId, identityProviderData.Alias, identityProviderData.CategoryId, identityProviderData.TypeId, identityProviderData.MetadataUrl).ConfigureAwait(false),
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
        var requiredCompanyRoles = typeId switch
        {
            IdentityProviderTypeId.OWN => Enumerable.Empty<CompanyRoleId>(),
            IdentityProviderTypeId.MANAGED => new[] { CompanyRoleId.OPERATOR, CompanyRoleId.ONBOARDING_SERVICE_PROVIDER },
            _ => throw new ControllerArgumentException($"creation of identityProviderType {typeId} is not supported")
        };
        if (displayName != null)
        {
            ValidateDisplayName(displayName);
        }

        return CreateOwnCompanyIdentityProviderInternalAsync(identityProviderCategory, protocol, typeId, displayName, requiredCompanyRoles);
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

    private async ValueTask<IdentityProviderDetails> CreateOwnCompanyIdentityProviderInternalAsync(IdentityProviderCategoryId identityProviderCategory, IamIdentityProviderProtocol protocol, IdentityProviderTypeId typeId, string? displayName, IEnumerable<CompanyRoleId> requiredCompanyRoles)
    {
        var companyId = _identityData.CompanyId;
        var identityProviderRepository = _portalRepositories.GetInstance<IIdentityProviderRepository>();
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
            IamIdentityProviderProtocol.OIDC => await GetIdentityProviderDetailsOidc(identityProviderId, alias, IdentityProviderCategoryId.KEYCLOAK_OIDC, typeId, null).ConfigureAwait(false),
            IamIdentityProviderProtocol.SAML => await GetIdentityProviderDetailsSaml(identityProviderId, alias, typeId).ConfigureAwait(false),
            _ => throw new UnexpectedConditionException($"unexpected value of protocol: '{protocol.ToString()}'")
        };
    }

    public async ValueTask<IdentityProviderDetails> GetOwnCompanyIdentityProviderAsync(Guid identityProviderId)
    {
        var (alias, category, typeId, metadataUrl) = await ValidateGetOwnCompanyIdentityProviderArguments(identityProviderId).ConfigureAwait(false);

        return category switch
        {
            IdentityProviderCategoryId.KEYCLOAK_OIDC => await GetIdentityProviderDetailsOidc(identityProviderId, alias, category, typeId, metadataUrl).ConfigureAwait(false),
            IdentityProviderCategoryId.KEYCLOAK_SAML => await GetIdentityProviderDetailsSaml(identityProviderId, alias, typeId).ConfigureAwait(false),
            _ => throw new ControllerArgumentException($"unexpected value for category '{category}' of identityProvider '{identityProviderId}'")
        };
    }

    private async ValueTask<(string Alias, IdentityProviderCategoryId Category, IdentityProviderTypeId TypeId, string? MetadataUrl)> ValidateGetOwnCompanyIdentityProviderArguments(Guid identityProviderId)
    {
        var companyId = _identityData.CompanyId;
        var (alias, category, isOwnOrOwnerCompany, typeId, metadataUrl) = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderAliasUntrackedAsync(identityProviderId, companyId).ConfigureAwait(false);
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

        return (alias, category, typeId, metadataUrl);
    }

    public async ValueTask<IdentityProviderDetails> SetOwnCompanyIdentityProviderStatusAsync(Guid identityProviderId, bool enabled)
    {
        var (category, alias, typeId, companyUsersLinked, ownerCompanyName, metadataUrl) = await ValidateSetOwnCompanyIdentityProviderStatusArguments(identityProviderId, enabled).ConfigureAwait(false);

        switch (category)
        {
            case IdentityProviderCategoryId.KEYCLOAK_OIDC when typeId is IdentityProviderTypeId.SHARED:
                await _provisioningManager.SetSharedIdentityProviderStatusAsync(alias, enabled).ConfigureAwait(false);
                return await GetIdentityProviderDetailsOidc(identityProviderId, alias, category, typeId, null).ConfigureAwait(false);
            case IdentityProviderCategoryId.KEYCLOAK_OIDC:
                await _provisioningManager.SetCentralIdentityProviderStatusAsync(alias, enabled).ConfigureAwait(false);
                if (typeId == IdentityProviderTypeId.MANAGED && !enabled && companyUsersLinked)
                {
                    await SendIdpMail(identityProviderId, alias, ownerCompanyName, _settings.DeactivateIdpRoles).ConfigureAwait(false);
                }
                return await GetIdentityProviderDetailsOidc(identityProviderId, alias, category, typeId, metadataUrl).ConfigureAwait(false);
            case IdentityProviderCategoryId.KEYCLOAK_SAML:
                await _provisioningManager.SetCentralIdentityProviderStatusAsync(alias, enabled).ConfigureAwait(false);
                if (typeId == IdentityProviderTypeId.MANAGED && !enabled && companyUsersLinked)
                {
                    await SendIdpMail(identityProviderId, alias, ownerCompanyName, _settings.DeactivateIdpRoles).ConfigureAwait(false);
                }
                return await GetIdentityProviderDetailsSaml(identityProviderId, alias, typeId).ConfigureAwait(false);
            default:
                throw new ControllerArgumentException($"unexpected value for category '{category}' of identityProvider '{identityProviderId}'");
        }
    }

    private Task SendIdpMail(Guid identityProviderId, string? alias, string ownerCompanyName, IEnumerable<UserRoleConfig> idpRoles) =>
        _roleBaseMailService.RoleBaseSendMailForIdp(
            idpRoles,
            new[] { ("idpAlias", alias ?? identityProviderId.ToString()), ("ownerCompanyName", ownerCompanyName) },
            ("username", "User"),
            new[] { "DeactivateManagedIdp" },
            identityProviderId);

    private async ValueTask<(IdentityProviderCategoryId Category, string Alias, IdentityProviderTypeId TypeId, bool CompanyUsersLinked, string OwnerCompanyName, string? MetadataUrl)> ValidateSetOwnCompanyIdentityProviderStatusArguments(Guid identityProviderId, bool enabled)
    {
        var companyId = _identityData.CompanyId;
        var result = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderStatusUpdateData(identityProviderId, companyId, !enabled).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"identityProvider {identityProviderId} does not exist");
        }
        var (isOwner, (alias, identityProviderCategory, identityProviderTypeId, metadataUrl), companyIdAliase, companyUsersLinked, ownerCompanyName) = result;
        if (!isOwner)
        {
            throw new ForbiddenException($"company {companyId} is not the owner of identityProvider {identityProviderId}");
        }
        if (alias == null)
        {
            throw new ConflictException($"identityprovider {identityProviderId} does not have an iamIdentityProvider.alias");
        }
        if (identityProviderTypeId != IdentityProviderTypeId.MANAGED &&
            !enabled &&
            !await ValidateOtherActiveIdentityProvider(
                alias,
                companyIdAliase ?? throw new UnexpectedConditionException("CompanyIdAliase should never be null here")).ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"cannot disable indentityProvider {identityProviderId} as no other active identityProvider exists for this company");
        }
        return (identityProviderCategory, alias, identityProviderTypeId, companyUsersLinked, ownerCompanyName, metadataUrl);
    }

    public async ValueTask<IdentityProviderDetails> UpdateOwnCompanyIdentityProviderAsync(Guid identityProviderId, IdentityProviderEditableDetails details, CancellationToken cancellationToken)
    {
        var (category, alias, typeId, metadataUrl) = await ValidateUpdateOwnCompanyIdentityProviderArguments(identityProviderId, details).ConfigureAwait(false);

        switch (category)
        {
            case IdentityProviderCategoryId.KEYCLOAK_OIDC when typeId is IdentityProviderTypeId.SHARED:
                await UpdateIdentityProviderShared(alias, details).ConfigureAwait(false);
                return await GetIdentityProviderDetailsOidc(identityProviderId, alias, category, typeId, null).ConfigureAwait(false);
            case IdentityProviderCategoryId.KEYCLOAK_OIDC:
                await UpdateIdentityProviderOidc(alias, metadataUrl, details, cancellationToken).ConfigureAwait(false);
                return await GetIdentityProviderDetailsOidc(identityProviderId, alias, category, typeId, details.Oidc?.MetadataUrl).ConfigureAwait(false);
            case IdentityProviderCategoryId.KEYCLOAK_SAML:
                await UpdateIdentityProviderSaml(alias, details).ConfigureAwait(false);
                return await GetIdentityProviderDetailsSaml(identityProviderId, alias, typeId).ConfigureAwait(false);
            default:
                throw new ControllerArgumentException($"unexpected value for category '{category.ToString()}' of identityProvider '{identityProviderId}'");
        }
    }

    private async ValueTask<(IdentityProviderCategoryId Category, string Alias, IdentityProviderTypeId TypeId, string? MetadataUrl)> ValidateUpdateOwnCompanyIdentityProviderArguments(Guid identityProviderId, IdentityProviderEditableDetails details)
    {
        var companyId = _identityData.CompanyId;
        ValidateDisplayName(details.DisplayName);

        var result = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderUpdateData(identityProviderId, companyId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"identityProvider {identityProviderId} does not exist");
        }
        var (isOwner, alias, identityProviderCategory, identityProviderTypeId, metadataUrl) = result;
        if (!isOwner)
        {
            throw new ForbiddenException($"User not allowed to run the change for identity provider {identityProviderId}");
        }
        if (alias == null)
        {
            throw new ConflictException($"identityprovider {identityProviderId} does not have an iamIdentityProvider.alias");
        }
        return (identityProviderCategory, alias, identityProviderTypeId, metadataUrl);
    }

    private async ValueTask UpdateIdentityProviderOidc(string alias, string? metadataUrl, IdentityProviderEditableDetails details, CancellationToken cancellationToken)
    {
        if (details.Oidc == null)
        {
            throw new ControllerArgumentException("property 'oidc' must not be null", nameof(details.Oidc));
        }
        if (details.Saml != null)
        {
            throw new ControllerArgumentException("property 'saml' must be null", nameof(details.Saml));
        }
        await _provisioningManager.UpdateCentralIdentityProviderDataOIDCAsync(
            new IdentityProviderEditableConfigOidc(
                alias,
                details.DisplayName,
                details.Oidc.MetadataUrl,
                details.Oidc.ClientAuthMethod,
                details.Oidc.ClientId,
                details.Oidc.Secret,
                details.Oidc.SignatureAlgorithm), cancellationToken)
            .ConfigureAwait(false);
        _portalRepositories.GetInstance<IIdentityProviderRepository>()
            .AttachAndModifyIamIdentityProvider(
                alias,
                iamIdentityProvider => iamIdentityProvider.MetadataUrl = metadataUrl,
                iamIdentityProvider => iamIdentityProvider.MetadataUrl = details.Oidc.MetadataUrl);
    }

    private async ValueTask UpdateIdentityProviderSaml(string alias, IdentityProviderEditableDetails details)
    {
        if (details.Saml == null)
        {
            throw new ControllerArgumentException("property 'saml' must not be null", nameof(details.Saml));
        }
        if (details.Oidc != null)
        {
            throw new ControllerArgumentException("property 'oidc' must be null", nameof(details.Oidc));
        }
        await _provisioningManager.UpdateCentralIdentityProviderDataSAMLAsync(
            new IdentityProviderEditableConfigSaml(
                alias,
                details.DisplayName,
                details.Saml.ServiceProviderEntityId,
                details.Saml.SingleSignOnServiceUrl))
            .ConfigureAwait(false);
    }

    private async ValueTask UpdateIdentityProviderShared(string alias, IdentityProviderEditableDetails details)
    {
        if (details.Oidc != null)
        {
            throw new ControllerArgumentException("property 'oidc' must be null", nameof(details.Oidc));
        }
        if (details.Saml != null)
        {
            throw new ControllerArgumentException("property 'saml' must be null", nameof(details.Saml));
        }
        await _provisioningManager.UpdateSharedIdentityProviderAsync(alias, details.DisplayName).ConfigureAwait(false);
    }

    private async ValueTask<bool> ValidateOtherActiveIdentityProvider(string? alias, IEnumerable<(Guid CompanyId, IEnumerable<string> Aliase)> companyIdAliase)
    {
        var aliasStatus = (await Task.WhenAll(companyIdAliase.SelectMany(x => x.Aliase).Where(x => x != alias).Distinct().Select(async alias => (Alias: alias, Enabled: await _provisioningManager.IsCentralIdentityProviderEnabled(alias).ConfigureAwait(false)))).ConfigureAwait(false)).ToDictionary(x => x.Alias, x => x.Enabled);
        return companyIdAliase.All(x =>
            x.Aliase.Where(a => a != alias).Any(a => aliasStatus[a]));
    }

    public async ValueTask DeleteCompanyIdentityProviderAsync(Guid identityProviderId)
    {
        var identityProviderRepository = _portalRepositories.GetInstance<IIdentityProviderRepository>();
        var (alias, typeId, ownerCompanyName) = await ValidateDeleteOwnCompanyIdentityProviderArguments(identityProviderId, identityProviderRepository).ConfigureAwait(false);

        if (alias != null)
        {
            identityProviderRepository.DeleteIamIdentityProvider(alias);
            if (typeId == IdentityProviderTypeId.SHARED)
            {
                await _provisioningManager.DeleteSharedIdpRealmAsync(alias).ConfigureAwait(false);
            }
            await _provisioningManager.DeleteCentralIdentityProviderAsync(alias).ConfigureAwait(false);
        }

        if (typeId == IdentityProviderTypeId.MANAGED)
        {
            await DeleteManagedIdpLinks(identityProviderId, alias, ownerCompanyName, identityProviderRepository).ConfigureAwait(false);
        }
        else
        {
            await DeleteOwnCompanyIdpLinks(identityProviderId, identityProviderRepository).ConfigureAwait(false);
        }

        identityProviderRepository.DeleteIdentityProvider(identityProviderId);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private async Task DeleteOwnCompanyIdpLinks(Guid identityProviderId, IIdentityProviderRepository identityProviderRepository)
    {
        var companyId = _identityData.CompanyId;
        var companyUserIds = await identityProviderRepository.GetIdpLinkedCompanyUserIds(identityProviderId, companyId).ToListAsync();

        identityProviderRepository.DeleteCompanyIdentityProvider(companyId, identityProviderId);
        _portalRepositories.GetInstance<IUserRepository>().RemoveCompanyUserAssignedIdentityProviders(companyUserIds.Select(id => (id, identityProviderId)));
    }

    private async Task DeleteManagedIdpLinks(Guid identityProviderId, string? alias, string ownerCompanyName, IIdentityProviderRepository identityProviderRepository)
    {
        var roleIds = await _roleBaseMailService.GetRoleData(_settings.DeleteIdpRoles).ConfigureAwait(false);
        var idpLinkedData = identityProviderRepository.GetManagedIdpLinkedData(identityProviderId, roleIds.Distinct());

        async IAsyncEnumerable<(string Email, IDictionary<string, string> Parameters)> DeleteLinksReturningMaildata()
        {
            var companyRepository = _portalRepositories.GetInstance<ICompanyRepository>();
            var userRepository = _portalRepositories.GetInstance<IUserRepository>();
            var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();

            await foreach (var data in idpLinkedData.ConfigureAwait(false))
            {
                if (!data.HasMoreIdentityProviders)
                {
                    companyRepository.AttachAndModifyCompany(data.CompanyId,
                        c => { c.CompanyStatusId = data.CompanyStatusId; },
                        c => { c.CompanyStatusId = CompanyStatusId.INACTIVE; });
                    userRepository.AttachAndModifyIdentities(data.Identities.Select(x => new ValueTuple<Guid, Action<Identity>?, Action<Identity>>(x.IdentityId, null, identity => { identity.UserStatusId = UserStatusId.INACTIVE; })));
                    userRolesRepository.DeleteCompanyUserAssignedRoles(data.Identities.SelectMany(i => i.UserRoleIds.Select(ur => (i.IdentityId, ur))));
                    await DeleteKeycloakUsers(data.Identities.Select(i => i.IdentityId));
                }
                identityProviderRepository.DeleteCompanyIdentityProvider(data.CompanyId, identityProviderId);
                userRepository.RemoveCompanyUserAssignedIdentityProviders(data.Identities.Where(x => x.IsLinkedCompanyUser).Select(x => (x.IdentityId, identityProviderId)));

                foreach (var userData in data.Identities.Where(i => i is { IsInUserRoles: true, Userdata.UserMail: not null }).Select(i => i.Userdata))
                {
                    var userName = string.Join(" ", new[] { userData.FirstName, userData.LastName }.Where(item => !string.IsNullOrWhiteSpace(item)));
                    var mailParameters = new Dictionary<string, string>
                    {
                        {"idpAlias", alias ?? identityProviderId.ToString()},
                        {"ownerCompanyName", ownerCompanyName},
                        { "username", string.IsNullOrWhiteSpace(userName) ? "User" : userName }
                    };
                    yield return (userData.UserMail!, mailParameters);
                }
            }
        }

        var mailTemplates = Enumerable.Repeat("DeleteManagedIdp", 1);

        foreach (var mailData in await DeleteLinksReturningMaildata().ToListAsync().ConfigureAwait(false))
        {
            await _mailingService.SendMails(mailData.Email, mailData.Parameters, mailTemplates).ConfigureAwait(false);
        }
    }

    private async Task DeleteKeycloakUsers(IEnumerable<Guid> identityIds)
    {
        foreach (var identityId in identityIds)
        {
            string? userId;
            if ((userId = await _provisioningManager.GetUserByUserName(identityId.ToString()).ConfigureAwait(false)) != null)
            {
                await _provisioningManager.DeleteCentralRealmUserAsync(userId).ConfigureAwait(false);
            }
        }
    }

    private async ValueTask<(string? Alias, IdentityProviderTypeId TypeId, string OwnerCompanyName)> ValidateDeleteOwnCompanyIdentityProviderArguments(Guid identityProviderId, IIdentityProviderRepository identityProviderRepository)
    {
        var companyId = _identityData.CompanyId;
        var result = await identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataForDelete(identityProviderId, companyId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"identityProvider {identityProviderId} does not exist");
        }

        var (isOwner, alias, typeId, aliase, ownerCompanyName) = result;
        if (!isOwner)
        {
            throw new ForbiddenException($"company {companyId} is not the owner of identityProvider {identityProviderId}");
        }

        if (alias == null || typeId == IdentityProviderTypeId.MANAGED)
        {
            return (alias, typeId, ownerCompanyName);
        }

        if (await _provisioningManager.IsCentralIdentityProviderEnabled(alias).ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"cannot delete identityProvider {identityProviderId} as it is enabled");
        }

        if (!await ValidateOtherActiveIdentityProvider(
                alias,
                aliase).ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"cannot delete indentityProvider {identityProviderId} as no other active identityProvider exists for this company");
        }

        return (alias, typeId, ownerCompanyName);
    }

    private async ValueTask<IdentityProviderDetails> GetIdentityProviderDetailsOidc(Guid identityProviderId, string? alias, IdentityProviderCategoryId categoryId, IdentityProviderTypeId typeId, string? metadataUrl)
    {
        IdentityProviderConfigOidc? identityProviderDataOidc = null;
        IEnumerable<IdentityProviderMapperModel>? identityProviderMapper = null;

        if (!string.IsNullOrWhiteSpace(alias))
        {
            bool aliasExisting;
            try
            {
                identityProviderDataOidc = await _provisioningManager.GetCentralIdentityProviderDataOIDCAsync(alias)
                    .ConfigureAwait(false);
                aliasExisting = true;
            }
            catch (KeycloakEntityNotFoundException ex)
            {
                _logger.LogInformation("Can't receive oidc data for {Alias} with following exception {Exception}", alias, ex.Message);
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
            Oidc = identityProviderDataOidc == null ?
                null :
                new IdentityProviderDetailsOidc(
                    metadataUrl,
                    identityProviderDataOidc.AuthorizationUrl,
                    identityProviderDataOidc.TokenUrl,
                    identityProviderDataOidc.LogoutUrl,
                    identityProviderDataOidc.ClientId,
                    !string.IsNullOrEmpty(identityProviderDataOidc.ClientSecret),
                    identityProviderDataOidc.ClientAuthMethod)
                {
                    SignatureAlgorithm = identityProviderDataOidc.SignatureAlgorithm
                }
        };
    }

    private async ValueTask<IdentityProviderDetails> GetIdentityProviderDetailsSaml(Guid identityProviderId, string? alias, IdentityProviderTypeId typeId)
    {
        IdentityProviderConfigSaml? identityProviderDataSaml = null;
        IEnumerable<IdentityProviderMapperModel>? identityProviderMapper = null;
        if (!string.IsNullOrWhiteSpace(alias))
        {
            bool aliasExisting;
            try
            {
                identityProviderDataSaml = await _provisioningManager
                    .GetCentralIdentityProviderDataSAMLAsync(alias).ConfigureAwait(false);
                aliasExisting = true;
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
            Saml = identityProviderDataSaml == null ?
                null :
                new IdentityProviderDetailsSaml(
                    identityProviderDataSaml.EntityId,
                    identityProviderDataSaml.SingleSignOnServiceUrl)
        };
    }

    public async ValueTask<UserIdentityProviderLinkData> CreateOrUpdateOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, Guid identityProviderId, UserLinkData userLinkData)
    {
        var companyId = _identityData.CompanyId;
        var (iamUserId, alias) = await GetUserAliasDataAsync(companyUserId, identityProviderId, companyId).ConfigureAwait(false);

        try
        {
            await _provisioningManager.DeleteProviderUserLinkToCentralUserAsync(iamUserId, alias);
        }
        catch (KeycloakEntityNotFoundException)
        {
            // for create-and-update semantics this is expected and not an error
        }

        await _provisioningManager.AddProviderUserLinkToCentralUserAsync(
            iamUserId,
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

    public async ValueTask<UserIdentityProviderLinkData> GetOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, Guid identityProviderId)
    {
        var companyId = _identityData.CompanyId;
        var (iamUserId, alias) = await GetUserAliasDataAsync(companyUserId, identityProviderId, companyId).ConfigureAwait(false);

        var result = await _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(iamUserId).FirstOrDefaultAsync(identityProviderLink => identityProviderLink.Alias == alias).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"identityProviderLink for identityProvider {identityProviderId} not found in keycloak for user {companyUserId}");
        }

        return new UserIdentityProviderLinkData(
            identityProviderId,
            result.UserId,
            result.UserName);
    }

    public async ValueTask DeleteOwnCompanyUserIdentityProviderDataAsync(Guid companyUserId, Guid identityProviderId)
    {
        var companyId = _identityData.CompanyId;
        var (iamUserId, alias) = await GetUserAliasDataAsync(companyUserId, identityProviderId, companyId).ConfigureAwait(false);
        try
        {
            await _provisioningManager.DeleteProviderUserLinkToCentralUserAsync(iamUserId, alias).ConfigureAwait(false);
        }
        catch (KeycloakEntityNotFoundException e)
        {
            throw new NotFoundException($"identityProviderLink for identityProvider {identityProviderId} not found in keycloak for user {companyUserId}", e);
        }
    }

    public async ValueTask<IdentityProviderDetailsWithConnectedCompanies> GetOwnIdentityProviderWithConnectedCompanies(Guid identityProviderId)
    {
        var companyId = _identityData.CompanyId;

        var (alias, category, isOwnerCompany, typeId, metadataUrl, connectedCompanies) = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnIdentityProviderWithConnectedCompanies(identityProviderId, companyId).ConfigureAwait(false);
        if (!isOwnerCompany)
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

        var details = category switch
        {
            IdentityProviderCategoryId.KEYCLOAK_OIDC => await GetIdentityProviderDetailsOidc(identityProviderId, alias, category, typeId, metadataUrl).ConfigureAwait(false),
            IdentityProviderCategoryId.KEYCLOAK_SAML => await GetIdentityProviderDetailsSaml(identityProviderId, alias, typeId).ConfigureAwait(false),
            _ => throw new UnexpectedConditionException($"unexpected value for category '{category}' of identityProvider '{identityProviderId}'")
        };

        return new(details.IdentityProviderId, details.Alias, details.IdentityProviderCategoryId, details.IdentityProviderTypeId, details.DisplayName, details.RedirectUrl, details.Enabled, connectedCompanies);
    }

    public async IAsyncEnumerable<UserIdentityProviderData> GetOwnCompanyUsersIdentityProviderDataAsync(IEnumerable<Guid> identityProviderIds, bool unlinkedUsersOnly)
    {
        var companyId = _identityData.CompanyId;
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

    public (Stream FileStream, string ContentType, string FileName, Encoding Encoding) GetOwnCompanyUsersIdentityProviderLinkDataStream(IEnumerable<Guid> identityProviderIds, bool unlinkedUsersOnly)
    {
        var companyId = _identityData.CompanyId;
        var csvSettings = _settings.CsvSettings;
        return (new AsyncEnumerableStringStream(GetOwnCompanyUsersIdentityProviderDataLines(identityProviderIds, unlinkedUsersOnly, companyId), csvSettings.Encoding), csvSettings.ContentType, csvSettings.FileName, csvSettings.Encoding);
    }

    public ValueTask<IdentityProviderUpdateStats> UploadOwnCompanyUsersIdentityProviderLinkDataAsync(IFormFile document, CancellationToken cancellationToken)
    {
        if (!document.ContentType.Equals(_settings.CsvSettings.ContentType, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnsupportedMediaTypeException($"Only contentType {_settings.CsvSettings.ContentType} files are allowed.");
        }

        return UploadOwnCompanyUsersIdentityProviderLinkDataInternalAsync(document, cancellationToken);
    }

    private async ValueTask<IdentityProviderUpdateStats> UploadOwnCompanyUsersIdentityProviderLinkDataInternalAsync(IFormFile document, CancellationToken cancellationToken)
    {
        var userRepository = _portalRepositories.GetInstance<IUserRepository>();
        var companyId = _identityData.CompanyId;
        var (sharedIdp, existingAliase) = await GetCompanyAliasDataAsync(companyId).ConfigureAwait(false);

        using var stream = document.OpenReadStream();

        int numIdps = default;

        var (numProcessed, numLines, errors) = await CsvParser.ProcessCsvAsync(
            stream,
            line =>
            {
                numIdps = ParseCSVFirstLineReturningNumIdps(line);
            },
            line => ParseCSVLine(line, numIdps, existingAliase.Select(x => x.Alias)),
            lines => ProcessOwnCompanyUsersIdentityProviderLinkDataInternalAsync(lines, userRepository, companyId, sharedIdp, existingAliase, cancellationToken),
            cancellationToken
        ).ConfigureAwait(false);

        var numErrors = errors.Count();
        var numUnchanged = numLines - numProcessed - numErrors;

        return new IdentityProviderUpdateStats(
            numProcessed,
            numUnchanged,
            numErrors,
            numLines,
            errors.Select(x => CreateUserUpdateError(x.Line, x.Error)));
    }

    private UserUpdateError CreateUserUpdateError(int line, Exception error) =>
        error switch
        {
            DetailException detailException when detailException.HasDetails => new UserUpdateError(line, detailException.GetErrorMessage(_errorMessageService), detailException.GetErrorDetails(_errorMessageService)),
            _ => new UserUpdateError(line, error.Message, Enumerable.Empty<ErrorDetails>())
        };

    private async IAsyncEnumerable<(bool, Exception?)> ProcessOwnCompanyUsersIdentityProviderLinkDataInternalAsync(
        IAsyncEnumerable<(Guid CompanyUserId, UserProfile UserProfile, IEnumerable<IdentityProviderLink> IdentityProviderLinks)> userProfileLinkDatas,
        IUserRepository userRepository,
        Guid companyId,
        (Guid IdentityProviderId, string Alias) sharedIdp,
        IEnumerable<(Guid IdentityProviderId, string Alias)> existingIdps,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var (companyUserId, profile, identityProviderLinks) in userProfileLinkDatas)
        {
            Exception? error = null;
            var success = false;
            try
            {
                var (iamUserId, existingProfile, links) = await GetExistingUserAndLinkDataAsync(userRepository, companyUserId, companyId).ConfigureAwait(false);
                var existingLinks = await links.ToListAsync(cancellationToken).ConfigureAwait(false);
                var updated = false;

                cancellationToken.ThrowIfCancellationRequested();

                foreach (var identityProviderLink in identityProviderLinks)
                {
                    updated |= await UpdateIdentityProviderLinksAsync(iamUserId, companyUserId, identityProviderLink, existingLinks, sharedIdp, existingIdps).ConfigureAwait(false);
                }

                if (existingProfile != profile)
                {
                    await UpdateUserProfileAsync(userRepository, iamUserId, companyUserId, profile, existingLinks, sharedIdp).ConfigureAwait(false);
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

    private async ValueTask<((Guid IdentityProviderId, string Alias) SharedIdp, IEnumerable<(Guid IdentityProviderId, string Alias)> ValidAliase)> GetCompanyAliasDataAsync(Guid companyId)
    {
        var identityProviderCategoryData = await _portalRepositories.GetInstance<IIdentityProviderRepository>()
                .GetCompanyIdentityProviderCategoryDataUntracked(companyId)
                .Where(data => data.Alias != null)
                .Select(data => (data.IdentityProviderId, data.TypeId, Alias: data.Alias!))
                .ToListAsync().ConfigureAwait(false);
        var sharedIdpAlias = identityProviderCategoryData.Where(data => data.TypeId == IdentityProviderTypeId.SHARED).Select(data => (data.IdentityProviderId, data.Alias)).SingleOrDefault();
        var validAliase = identityProviderCategoryData.Select(data => (data.IdentityProviderId, data.Alias)).ToList();
        return (sharedIdpAlias, validAliase);
    }

    private async ValueTask<(string IamUserId, UserProfile ExistingProfile, IAsyncEnumerable<IdentityProviderLink> ExistingLinks)> GetExistingUserAndLinkDataAsync(IUserRepository userRepository, Guid companyUserId, Guid companyId)
    {
        var userEntityData = await userRepository.GetUserEntityDataAsync(companyUserId, companyId).ConfigureAwait(false);
        if (userEntityData == default)
        {
            throw new ControllerArgumentException($"unexpected value of {_settings.CsvSettings.HeaderUserId}: '{companyUserId}'");
        }
        var (existingFirstName, existingLastName, existingEmail) = userEntityData;

        var iamUserId = await _provisioningManager.GetUserByUserName(companyUserId.ToString()).ConfigureAwait(false) ?? throw new ConflictException($"user {companyUserId} does not exist in keycloak");

        return (
            iamUserId,
            new UserProfile(existingFirstName, existingLastName, existingEmail),
            _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(iamUserId)
        );
    }

    private async ValueTask<bool> UpdateIdentityProviderLinksAsync(
        string iamUserId,
        Guid companyUserId,
        IdentityProviderLink identityProviderLink,
        IEnumerable<IdentityProviderLink> existingLinks,
        (Guid IdentityProviderId, string Alias) sharedIdp,
        IEnumerable<(Guid IdentityProviderId, string Alias)> existingIdps)
    {
        var (alias, userId, userName) = identityProviderLink;

        var existingLink = existingLinks.SingleOrDefault(link => link.Alias == alias);

        if ((existingLink == null && string.IsNullOrWhiteSpace(userName) && string.IsNullOrWhiteSpace(userId)) ||
            (existingLink != null && existingLink.UserName == userName && existingLink.UserId == userId))
        {
            return false;
        }

        if (alias == sharedIdp.Alias)
        {
            throw new ControllerArgumentException($"unexpected update of shared identityProviderLink, alias '{alias}', companyUser '{companyUserId}', providerUserId: '{userId}', providerUserName: '{userName}'");
        }

        if (existingLink != null)
        {
            await _provisioningManager.DeleteProviderUserLinkToCentralUserAsync(iamUserId, alias).ConfigureAwait(false);
        }
        await _provisioningManager.AddProviderUserLinkToCentralUserAsync(iamUserId, identityProviderLink).ConfigureAwait(false);
        await InsertUpdateCompanyUserAssignedIdentityProvider(companyUserId, existingIdps.Single(x => x.Alias == alias).IdentityProviderId, identityProviderLink).ConfigureAwait(false);
        return true;
    }

    private async Task InsertUpdateCompanyUserAssignedIdentityProvider(Guid companyUserId, Guid identityProviderId, IdentityProviderLink providerLink)
    {
        var userRepository = _portalRepositories.GetInstance<IUserRepository>();
        var data = await userRepository.GetCompanyUserAssignedIdentityProvider(companyUserId, identityProviderId).ConfigureAwait(false);
        if (data == default)
        {
            userRepository.AddCompanyUserAssignedIdentityProvider(companyUserId, identityProviderId, providerLink.UserId, providerLink.UserName);
        }
        else
        {
            userRepository.AttachAndModifyUserAssignedIdentityProvider(companyUserId, identityProviderId,
                uaip =>
                {
                    uaip.ProviderId = data.ProviderId;
                    uaip.UserName = data.Username;
                },
                uaip =>
                {
                    uaip.ProviderId = providerLink.UserId;
                    uaip.UserName = providerLink.UserName;
                });
        }
    }

    private async ValueTask UpdateUserProfileAsync(IUserRepository userRepository, string iamUserId, Guid companyUserId, UserProfile profile, IEnumerable<IdentityProviderLink> existingLinks, (Guid IdentityProviderId, string Alias) sharedIdp)
    {
        var (firstName, lastName, email) = (profile.FirstName ?? "", profile.LastName ?? "", profile.Email ?? "");

        await _provisioningManager.UpdateCentralUserAsync(iamUserId, firstName, lastName, email).ConfigureAwait(false);

        if (sharedIdp != default)
        {
            var sharedIdpLink = existingLinks.FirstOrDefault(link => link.Alias == sharedIdp.Alias);
            if (sharedIdpLink != default)
            {
                await _provisioningManager.UpdateSharedRealmUserAsync(sharedIdp.Alias, sharedIdpLink.UserId, firstName, lastName, email).ConfigureAwait(false);
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

        identityProviderIds.Except(identityProviderData.Select(data => data.IdentityProviderId)).IfAny(invalidIds =>
        {
            throw new ControllerArgumentException($"invalid identityProviders: [{string.Join(", ", invalidIds)}] for company {companyId}", nameof(identityProviderIds));
        });

        return identityProviderData;
    }

    private async IAsyncEnumerable<(Guid CompanyUserId, UserProfile UserProfile, IAsyncEnumerable<IdentityProviderLink> LinkDatas)> GetOwnCompanyIdentityProviderLinkDataInternalAsync(Guid companyId)
    {
        await foreach (var (companyUserId, firstName, lastName, email) in _portalRepositories.GetInstance<IUserRepository>()
            .GetOwnCompanyUserQuery(companyId)
            .Select(companyUser =>
                new ValueTuple<Guid, string?, string?, string?>(
                    companyUser.Id,
                    companyUser.Firstname,
                    companyUser.Lastname,
                    companyUser.Email))
            .ToAsyncEnumerable().ConfigureAwait(false))
        {
            var iamUserId = await _provisioningManager.GetUserByUserName(companyUserId.ToString()).ConfigureAwait(false);
            if (iamUserId != null)
            {
                yield return (
                    companyUserId,
                    new UserProfile(firstName, lastName, email),
                    _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(iamUserId)
                );
            }
        }
    }

    private async ValueTask<(string IamUserId, string Alias)> GetUserAliasDataAsync(Guid companyUserId, Guid identityProviderId, Guid companyId)
    {
        var (isValidUser, alias, isSameCompany) = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, companyId).ConfigureAwait(false);
        if (!isValidUser)
        {
            throw new NotFoundException($"companyUserId {companyUserId} does not exist");
        }
        if (alias == null)
        {
            throw new NotFoundException($"identityProvider {identityProviderId} not found in company of user {companyUserId}");
        }
        if (!isSameCompany)
        {
            throw new ForbiddenException($"identityProvider {identityProviderId} is not associated with company {companyId}");
        }
        var iamUserId = await _provisioningManager.GetUserByUserName(companyUserId.ToString()).ConfigureAwait(false);
        if (iamUserId == null)
        {
            throw new UnexpectedConditionException($"companyUserId {companyUserId} is not linked to keycloak");
        }
        return (iamUserId, alias);
    }

    private sealed record UserProfile(string? FirstName, string? LastName, string? Email);
}
