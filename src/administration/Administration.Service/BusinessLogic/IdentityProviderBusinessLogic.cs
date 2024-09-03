/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class IdentityProviderBusinessLogic(
    IPortalRepositories portalRepositories,
    IProvisioningManager provisioningManager,
    IIdentityService identityService,
    IErrorMessageService errorMessageService,
    IMailingProcessCreation mailingProcessCreation,
    IOptions<IdentityProviderSettings> options,
    ILogger<IdentityProviderBusinessLogic> logger)
    : IIdentityProviderBusinessLogic
{
    private readonly IIdentityData _identityData = identityService.IdentityData;
    private readonly IdentityProviderSettings _settings = options.Value;

    private static readonly Regex DisplayNameValidationExpression = new(@"^[a-zA-Z0-9\!\?\@\&\#\'\x22\(\)_\-\=\/\*\.\,\;\: ]+$", RegexOptions.None, TimeSpan.FromSeconds(1));

    public async IAsyncEnumerable<IdentityProviderDetails> GetOwnCompanyIdentityProvidersAsync(string? displayName, string? alias)
    {
        var companyId = _identityData.CompanyId;
        await foreach (var identityProviderData in portalRepositories.GetInstance<IIdentityProviderRepository>().GetCompanyIdentityProviderCategoryDataUntracked(companyId, alias).ConfigureAwait(false))
        {
            var details = identityProviderData.CategoryId switch
            {
                IdentityProviderCategoryId.KEYCLOAK_OIDC => await GetIdentityProviderDetailsOidc(identityProviderData.IdentityProviderId, identityProviderData.Alias, identityProviderData.CategoryId, identityProviderData.TypeId, identityProviderData.MetadataUrl).ConfigureAwait(false),
                IdentityProviderCategoryId.KEYCLOAK_SAML => await GetIdentityProviderDetailsSaml(identityProviderData.IdentityProviderId, identityProviderData.Alias, identityProviderData.TypeId),
                _ => throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_UNEXPECT_VAL_FOR_CATEGORY_ID, new ErrorParameter[] { new("categoryId", identityProviderData.CategoryId.ToString()) })
            };

            if (displayName == null || (details.DisplayName != null && details.DisplayName.Contains(displayName)))
                yield return details;
        }
    }

    public ValueTask<IdentityProviderDetails> CreateOwnCompanyIdentityProviderAsync(IamIdentityProviderProtocol protocol, IdentityProviderTypeId typeId, string? displayName)
    {
        var identityProviderCategory = protocol switch
        {
            IamIdentityProviderProtocol.SAML => IdentityProviderCategoryId.KEYCLOAK_SAML,
            IamIdentityProviderProtocol.OIDC => IdentityProviderCategoryId.KEYCLOAK_OIDC,
            _ => throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_UNEXPECT_VAL_OF_PROTOCOL, new ErrorParameter[] { new(nameof(protocol), protocol.ToString()) })
        };
        var requiredCompanyRoles = typeId switch
        {
            IdentityProviderTypeId.OWN => Enumerable.Empty<CompanyRoleId>(),
            IdentityProviderTypeId.MANAGED => new[] { CompanyRoleId.OPERATOR, CompanyRoleId.ONBOARDING_SERVICE_PROVIDER },
            _ => throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_PROVIDER_TYPE_CREATION_NOT_SUPPORTED, new ErrorParameter[] { new(nameof(typeId), typeId.ToString()) })
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
            throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_DISPLAY_NAME_CHAR_BET_TWO_TO_THIRTY);
        }
        if (!DisplayNameValidationExpression.IsMatch(displayName))
        {
            throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_ALLO_CHAR_AS_PER_REG_EX);
        }
    }

    private async ValueTask<IdentityProviderDetails> CreateOwnCompanyIdentityProviderInternalAsync(IdentityProviderCategoryId identityProviderCategory, IamIdentityProviderProtocol protocol, IdentityProviderTypeId typeId, string? displayName, IEnumerable<CompanyRoleId> requiredCompanyRoles)
    {
        var companyId = _identityData.CompanyId;
        var identityProviderRepository = portalRepositories.GetInstance<IIdentityProviderRepository>();
        var result = await portalRepositories.GetInstance<ICompanyRepository>().CheckCompanyAndCompanyRolesAsync(companyId, requiredCompanyRoles).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!result.IsValidCompany)
        {
            throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_COMPANY_NOT_EXIST, new ErrorParameter[] { new(nameof(companyId), companyId.ToString()) });
        }

        if (!result.IsAllowed)
        {
            throw ForbiddenException.Create(AdministrationIdentityProviderErrors.IDENTITY_FORBIDDEN_NOT_ALLOW_CREATE_PROVIDER_TYPE, new ErrorParameter[] { new(nameof(typeId), typeId.ToString()) });
        }

        var alias = await provisioningManager.CreateOwnIdpAsync(displayName ?? result.CompanyName, result.CompanyName, protocol).ConfigureAwait(ConfigureAwaitOptions.None);
        var identityProviderId = identityProviderRepository.CreateIdentityProvider(identityProviderCategory, typeId, companyId, null).Id;
        if (typeId == IdentityProviderTypeId.OWN)
        {
            identityProviderRepository.CreateCompanyIdentityProvider(companyId, identityProviderId);
        }
        identityProviderRepository.CreateIamIdentityProvider(identityProviderId, alias);
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);

        return protocol switch
        {
            IamIdentityProviderProtocol.OIDC => await GetIdentityProviderDetailsOidc(identityProviderId, alias, IdentityProviderCategoryId.KEYCLOAK_OIDC, typeId, null).ConfigureAwait(false),
            IamIdentityProviderProtocol.SAML => await GetIdentityProviderDetailsSaml(identityProviderId, alias, typeId).ConfigureAwait(false),
            _ => throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_UNEXPECT_VAL_OF_PROTOCOL, new ErrorParameter[] { new(nameof(protocol), protocol.ToString()) })
        };
    }

    public async ValueTask<IdentityProviderDetails> GetOwnCompanyIdentityProviderAsync(Guid identityProviderId)
    {
        var (alias, category, typeId, metadataUrl) = await ValidateGetOwnCompanyIdentityProviderArguments(identityProviderId).ConfigureAwait(false);

        return category switch
        {
            IdentityProviderCategoryId.KEYCLOAK_OIDC => await GetIdentityProviderDetailsOidc(identityProviderId, alias, category, typeId, metadataUrl).ConfigureAwait(false),
            IdentityProviderCategoryId.KEYCLOAK_SAML => await GetIdentityProviderDetailsSaml(identityProviderId, alias, typeId).ConfigureAwait(false),
            _ => throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_UNEXPECT_VAL_FOR_CATEGORY_OF_PROVIDER, new ErrorParameter[] { new(nameof(category), category.ToString()), new(nameof(identityProviderId), identityProviderId.ToString()) })
        };
    }

    private async ValueTask<(string Alias, IdentityProviderCategoryId Category, IdentityProviderTypeId TypeId, string? MetadataUrl)> ValidateGetOwnCompanyIdentityProviderArguments(Guid identityProviderId)
    {
        var companyId = _identityData.CompanyId;
        var (alias, category, isOwnOrOwnerCompany, typeId, metadataUrl) = await portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderAliasUntrackedAsync(identityProviderId, companyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!isOwnOrOwnerCompany)
        {
            throw ConflictException.Create(AdministrationIdentityProviderErrors.IDENTITY_CONFLICT_PROVIDER_NOT_ASSOCIATE_WITH_COMPANY, new ErrorParameter[] { new(nameof(identityProviderId), identityProviderId.ToString()), new(nameof(companyId), companyId.ToString()) });
        }

        if (alias == null)
        {
            throw NotFoundException.Create(AdministrationIdentityProviderErrors.IDENTITY_NOT_PROVIDER_NOT_EXIST, new ErrorParameter[] { new(nameof(identityProviderId), identityProviderId.ToString()) });
        }

        if (category == IdentityProviderCategoryId.KEYCLOAK_SAML && typeId is IdentityProviderTypeId.SHARED)
        {
            throw ConflictException.Create(AdministrationIdentityProviderErrors.IDENTITY_CONFLICT_SHARED_IDP_NOT_USE_SAML);
        }

        return (alias, category, typeId, metadataUrl);
    }

    public async ValueTask<IdentityProviderDetails> SetOwnCompanyIdentityProviderStatusAsync(Guid identityProviderId, bool enabled)
    {
        var (category, alias, typeId, companyUsersLinked, ownerCompanyName, metadataUrl) = await ValidateSetOwnCompanyIdentityProviderStatusArguments(identityProviderId, enabled).ConfigureAwait(false);

        switch (category)
        {
            case IdentityProviderCategoryId.KEYCLOAK_OIDC when typeId is IdentityProviderTypeId.SHARED:
                await provisioningManager.SetSharedIdentityProviderStatusAsync(alias, enabled).ConfigureAwait(false);
                return await GetIdentityProviderDetailsOidc(identityProviderId, alias, category, typeId, null).ConfigureAwait(false);
            case IdentityProviderCategoryId.KEYCLOAK_OIDC:
                await provisioningManager.SetCentralIdentityProviderStatusAsync(alias, enabled).ConfigureAwait(false);
                if (typeId == IdentityProviderTypeId.MANAGED && !enabled && companyUsersLinked)
                {
                    await SendIdpMail(identityProviderId, alias, ownerCompanyName, _settings.DeactivateIdpRoles).ConfigureAwait(ConfigureAwaitOptions.None);
                }
                return await GetIdentityProviderDetailsOidc(identityProviderId, alias, category, typeId, metadataUrl).ConfigureAwait(false);
            case IdentityProviderCategoryId.KEYCLOAK_SAML:
                await provisioningManager.SetCentralIdentityProviderStatusAsync(alias, enabled).ConfigureAwait(false);
                if (typeId == IdentityProviderTypeId.MANAGED && !enabled && companyUsersLinked)
                {
                    await SendIdpMail(identityProviderId, alias, ownerCompanyName, _settings.DeactivateIdpRoles).ConfigureAwait(ConfigureAwaitOptions.None);
                }
                return await GetIdentityProviderDetailsSaml(identityProviderId, alias, typeId).ConfigureAwait(false);
            default:
                throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_UNEXPECT_VAL_FOR_CATEGORY_OF_PROVIDER, new ErrorParameter[] { new(nameof(category), category.ToString()), new(nameof(identityProviderId), identityProviderId.ToString()) });
        }
    }

    private Task SendIdpMail(Guid identityProviderId, string? alias, string ownerCompanyName, IEnumerable<UserRoleConfig> idpRoles) =>
        mailingProcessCreation.RoleBaseSendMailForIdp(
            idpRoles,
            new[] { ("idpAlias", alias ?? identityProviderId.ToString()), ("ownerCompanyName", ownerCompanyName) },
            ("username", "User"),
            new[] { "DeactivateManagedIdp" },
            identityProviderId);

    private async ValueTask<(IdentityProviderCategoryId Category, string Alias, IdentityProviderTypeId TypeId, bool CompanyUsersLinked, string OwnerCompanyName, string? MetadataUrl)> ValidateSetOwnCompanyIdentityProviderStatusArguments(Guid identityProviderId, bool enabled)
    {
        var companyId = _identityData.CompanyId;
        var result = await portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderStatusUpdateData(identityProviderId, companyId, !enabled).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw NotFoundException.Create(AdministrationIdentityProviderErrors.IDENTITY_NOT_PROVIDER_NOT_EXIST, new ErrorParameter[] { new(nameof(identityProviderId), identityProviderId.ToString()) });
        }
        var (isOwner, (alias, identityProviderCategory, identityProviderTypeId, metadataUrl), companyIdAliase, companyUsersLinked, ownerCompanyName) = result;
        if (!isOwner)
        {
            throw ForbiddenException.Create(AdministrationIdentityProviderErrors.IDENTITY_FORBIDDEN_COMP_NOT_OWNER_PROVIDER, new ErrorParameter[] { new(nameof(companyId), companyId.ToString()), new(nameof(identityProviderId), identityProviderId.ToString()) });
        }
        if (alias == null)
        {
            throw ConflictException.Create(AdministrationIdentityProviderErrors.IDENTITY_CONFLICT_PROVIDER_NOT_HAVE_IAMIDENTITY_PROVIDER_ALIAS, new ErrorParameter[] { new(nameof(identityProviderId), identityProviderId.ToString()) });
        }
        if (identityProviderTypeId != IdentityProviderTypeId.MANAGED &&
            !enabled &&
            !await ValidateOtherActiveIdentityProvider(
                alias,
                companyIdAliase ?? throw UnexpectedConditionException.Create(AdministrationIdentityProviderErrors.IDENTITY_UNEXPECT_COMPANYID_ALIAS_NOT_NULL)).ConfigureAwait(false))
        {
            throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_NOT_DISABLE_PROVIDER, new ErrorParameter[] { new(nameof(identityProviderId), identityProviderId.ToString()) });
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
                throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_UNEXPECT_VAL_FOR_CATEGORY_OF_PROVIDER, new ErrorParameter[] { new(nameof(category), category.ToString()), new(nameof(identityProviderId), identityProviderId.ToString()) });
        }
    }

    private async ValueTask<(IdentityProviderCategoryId Category, string Alias, IdentityProviderTypeId TypeId, string? MetadataUrl)> ValidateUpdateOwnCompanyIdentityProviderArguments(Guid identityProviderId, IdentityProviderEditableDetails details)
    {
        var companyId = _identityData.CompanyId;
        ValidateDisplayName(details.DisplayName);

        var result = await portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderUpdateData(identityProviderId, companyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw NotFoundException.Create(AdministrationIdentityProviderErrors.IDENTITY_NOT_PROVIDER_NOT_EXIST, new ErrorParameter[] { new(nameof(identityProviderId), identityProviderId.ToString()) });
        }
        var (isOwner, alias, identityProviderCategory, identityProviderTypeId, metadataUrl) = result;
        if (!isOwner)
        {
            throw ForbiddenException.Create(AdministrationIdentityProviderErrors.IDENTITY_FORBIDDEN_USER_NOT_ALLOW_CHANGE_PROVIDER, new ErrorParameter[] { new(nameof(identityProviderId), identityProviderId.ToString()) });
        }
        if (alias == null)
        {
            throw ConflictException.Create(AdministrationIdentityProviderErrors.IDENTITY_CONFLICT_PROVIDER_NOT_HAVE_IAMIDENTITY_PROVIDER_ALIAS, new ErrorParameter[] { new(nameof(identityProviderId), identityProviderId.ToString()) });
        }
        return (identityProviderCategory, alias, identityProviderTypeId, metadataUrl);
    }

    private async ValueTask UpdateIdentityProviderOidc(string alias, string? metadataUrl, IdentityProviderEditableDetails details, CancellationToken cancellationToken)
    {
        if (details.Oidc == null)
        {
            throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_OIDC_NOT_NULL);
        }
        if (details.Saml != null)
        {
            throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_SAML_NOT_NULL);
        }
        await provisioningManager.UpdateCentralIdentityProviderDataOIDCAsync(
            new IdentityProviderEditableConfigOidc(
                alias,
                details.DisplayName,
                details.Oidc.MetadataUrl,
                details.Oidc.ClientAuthMethod,
                details.Oidc.ClientId,
                details.Oidc.Secret,
                details.Oidc.SignatureAlgorithm), cancellationToken)
            .ConfigureAwait(false);
        portalRepositories.GetInstance<IIdentityProviderRepository>()
            .AttachAndModifyIamIdentityProvider(
                alias,
                iamIdentityProvider => iamIdentityProvider.MetadataUrl = metadataUrl,
                iamIdentityProvider => iamIdentityProvider.MetadataUrl = details.Oidc.MetadataUrl);
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private async ValueTask UpdateIdentityProviderSaml(string alias, IdentityProviderEditableDetails details)
    {
        if (details.Saml == null)
        {
            throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_SAML_NOT_NULL);
        }
        if (details.Oidc != null)
        {
            throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_OIDC_NOT_NULL);
        }
        await provisioningManager.UpdateCentralIdentityProviderDataSAMLAsync(
            new IdentityProviderEditableConfigSaml(
                alias,
                details.DisplayName,
                details.Saml.ServiceProviderEntityId,
                details.Saml.SingleSignOnServiceUrl))
            .ConfigureAwait(false);
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private async ValueTask UpdateIdentityProviderShared(string alias, IdentityProviderEditableDetails details)
    {
        if (details.Oidc != null)
        {
            throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_OIDC_NOT_NULL);
        }
        if (details.Saml != null)
        {
            throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_SAML_NOT_NULL);
        }
        await provisioningManager.UpdateSharedIdentityProviderAsync(alias, details.DisplayName).ConfigureAwait(false);
    }

    private async ValueTask<bool> ValidateOtherActiveIdentityProvider(string? alias, IEnumerable<(Guid CompanyId, IEnumerable<string> Aliase)> companyIdAliase)
    {
        var aliasStatus = (await Task.WhenAll(companyIdAliase.SelectMany(x => x.Aliase).Where(x => x != alias).Distinct().Select(async alias => (Alias: alias, Enabled: await provisioningManager.IsCentralIdentityProviderEnabled(alias).ConfigureAwait(false)))).ConfigureAwait(ConfigureAwaitOptions.None)).ToDictionary(x => x.Alias, x => x.Enabled);
        return companyIdAliase.All(x =>
            x.Aliase.Where(a => a != alias).Any(a => aliasStatus[a]));
    }

    public async ValueTask DeleteCompanyIdentityProviderAsync(Guid identityProviderId)
    {
        var identityProviderRepository = portalRepositories.GetInstance<IIdentityProviderRepository>();
        var (alias, typeId, ownerCompanyName) = await ValidateDeleteOwnCompanyIdentityProviderArguments(identityProviderId, identityProviderRepository).ConfigureAwait(false);

        if (alias != null)
        {
            identityProviderRepository.DeleteIamIdentityProvider(alias);
            if (typeId == IdentityProviderTypeId.SHARED)
            {
                await provisioningManager.DeleteSharedIdpRealmAsync(alias).ConfigureAwait(false);
            }
            await provisioningManager.DeleteCentralIdentityProviderAsync(alias).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        if (typeId == IdentityProviderTypeId.MANAGED)
        {
            await DeleteManagedIdpLinks(identityProviderId, alias, ownerCompanyName, identityProviderRepository).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        identityProviderRepository.DeleteIdentityProvider(identityProviderId);
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private async Task DeleteManagedIdpLinks(Guid identityProviderId, string? alias, string ownerCompanyName, IIdentityProviderRepository identityProviderRepository)
    {
        var roleIds = await mailingProcessCreation.GetRoleData(_settings.DeleteIdpRoles).ConfigureAwait(ConfigureAwaitOptions.None);
        var idpLinkedData = identityProviderRepository.GetManagedIdpLinkedData(identityProviderId, roleIds.Distinct());

        async IAsyncEnumerable<(string Email, IReadOnlyDictionary<string, string> Parameters)> DeleteLinksReturningMaildata()
        {
            var companyRepository = portalRepositories.GetInstance<ICompanyRepository>();
            var userRepository = portalRepositories.GetInstance<IUserRepository>();
            var userRolesRepository = portalRepositories.GetInstance<IUserRolesRepository>();

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

                foreach (var userData in data.Identities.Where(i => i is { IsInUserRoles: true, Userdata.UserMail: not null }).Select(i => i.Userdata))
                {
                    var userName = string.Join(" ", new[] { userData.FirstName, userData.LastName }.Where(item => !string.IsNullOrWhiteSpace(item)));
                    var mailParameters = ImmutableDictionary.CreateRange(new[]
                    {
                        KeyValuePair.Create("idpAlias", alias ?? identityProviderId.ToString()),
                        KeyValuePair.Create("ownerCompanyName", ownerCompanyName),
                        KeyValuePair.Create("username", string.IsNullOrWhiteSpace(userName) ? "User" : userName)
                    });
                    yield return (userData.UserMail!, mailParameters);
                }
            }
        }

        await foreach (var mailData in DeleteLinksReturningMaildata().ConfigureAwait(false))
        {
            mailingProcessCreation.CreateMailProcess(mailData.Email, "DeleteManagedIdp", mailData.Parameters);
        }
    }

    private async Task DeleteKeycloakUsers(IEnumerable<Guid> identityIds)
    {
        foreach (var identityId in identityIds)
        {
            string? userId;
            if ((userId = await provisioningManager.GetUserByUserName(identityId.ToString()).ConfigureAwait(ConfigureAwaitOptions.None)) != null)
            {
                await provisioningManager.DeleteCentralRealmUserAsync(userId).ConfigureAwait(ConfigureAwaitOptions.None);
            }
        }
    }

    private async ValueTask<(string? Alias, IdentityProviderTypeId TypeId, string OwnerCompanyName)> ValidateDeleteOwnCompanyIdentityProviderArguments(Guid identityProviderId, IIdentityProviderRepository identityProviderRepository)
    {
        var companyId = _identityData.CompanyId;
        var result = await identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataForDelete(identityProviderId, companyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw NotFoundException.Create(AdministrationIdentityProviderErrors.IDENTITY_NOT_PROVIDER_NOT_EXIST, new ErrorParameter[] { new(nameof(identityProviderId), identityProviderId.ToString()) });
        }

        var (isOwner, alias, typeId, aliase, ownerCompanyName) = result;
        if (!isOwner)
        {
            throw ForbiddenException.Create(AdministrationIdentityProviderErrors.IDENTITY_FORBIDDEN_COMP_NOT_OWNER_PROVIDER, new ErrorParameter[] { new(nameof(companyId), companyId.ToString()), new(nameof(identityProviderId), identityProviderId.ToString()) });
        }

        if (alias == null || typeId == IdentityProviderTypeId.MANAGED)
        {
            return (alias, typeId, ownerCompanyName);
        }

        if (await provisioningManager.IsCentralIdentityProviderEnabled(alias).ConfigureAwait(false))
        {
            throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_CANNOT_DEL_ENABLE_PROVIDERID, new ErrorParameter[] { new(nameof(identityProviderId), identityProviderId.ToString()) });
        }

        if (!await ValidateOtherActiveIdentityProvider(
                alias,
                aliase).ConfigureAwait(false))
        {
            throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_NOT_DELETE_PROVIDER, new ErrorParameter[] { new(nameof(identityProviderId), identityProviderId.ToString()) });
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
                identityProviderDataOidc = await provisioningManager.GetCentralIdentityProviderDataOIDCAsync(alias)
                    .ConfigureAwait(false);
                aliasExisting = true;
            }
            catch (KeycloakEntityNotFoundException ex)
            {
                logger.LogInformation("Can't receive oidc data for {Alias} with following exception {Exception}", alias, ex.Message);
                aliasExisting = false;
            }

            if (aliasExisting)
            {
                identityProviderMapper = await provisioningManager.GetIdentityProviderMappers(alias).ToListAsync().ConfigureAwait(false);
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
            Oidc = identityProviderDataOidc == null
                ? null
                : new IdentityProviderDetailsOidc(
                    metadataUrl,
                    identityProviderDataOidc.AuthorizationUrl,
                    identityProviderDataOidc.TokenUrl,
                    identityProviderDataOidc.LogoutUrl,
                    identityProviderDataOidc.ClientId,
                    !string.IsNullOrEmpty(identityProviderDataOidc.ClientSecret),
                    identityProviderDataOidc.ClientAuthMethod,
                    identityProviderDataOidc.SignatureAlgorithm)
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
                identityProviderDataSaml = await provisioningManager
                    .GetCentralIdentityProviderDataSAMLAsync(alias).ConfigureAwait(false);
                aliasExisting = true;
            }
            catch (KeycloakEntityNotFoundException ex)
            {
                logger.LogInformation("Can't receive saml data for {Alias} with following exception {Exception}", alias, ex.Message);
                aliasExisting = false;
            }

            if (aliasExisting)
            {
                identityProviderMapper = await provisioningManager.GetIdentityProviderMappers(alias).ToListAsync()
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
            Saml = identityProviderDataSaml == null
                ? null
                : new IdentityProviderDetailsSaml(
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
            await provisioningManager.DeleteProviderUserLinkToCentralUserAsync(iamUserId, alias);
        }
        catch (KeycloakEntityNotFoundException)
        {
            // for create-and-update semantics this is expected and not an error
        }

        await provisioningManager.AddProviderUserLinkToCentralUserAsync(
            iamUserId,
            new IdentityProviderLink(
                alias,
                userLinkData.userId,
                userLinkData.userName))
            .ConfigureAwait(ConfigureAwaitOptions.None);

        return new UserIdentityProviderLinkData(
            identityProviderId,
            userLinkData.userId,
            userLinkData.userName);
    }

    public async ValueTask<UserIdentityProviderLinkData> GetOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, Guid identityProviderId)
    {
        var companyId = _identityData.CompanyId;
        var (iamUserId, alias) = await GetUserAliasDataAsync(companyUserId, identityProviderId, companyId).ConfigureAwait(false);

        var result = await provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(iamUserId).FirstOrDefaultAsync(identityProviderLink => identityProviderLink.Alias == alias).ConfigureAwait(false);
        if (result == default)
        {
            throw NotFoundException.Create(AdministrationIdentityProviderErrors.IDENTITY_NOT_COMP_USERID_NO_KEYLOCK_LINK_FOUND, new ErrorParameter[] { new(nameof(identityProviderId), identityProviderId.ToString()), new(nameof(companyUserId), companyUserId.ToString()) });
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
            await provisioningManager.DeleteProviderUserLinkToCentralUserAsync(iamUserId, alias).ConfigureAwait(ConfigureAwaitOptions.None);
        }
        catch (KeycloakEntityNotFoundException)
        {
            throw NotFoundException.Create(AdministrationIdentityProviderErrors.IDENTITY_NOT_COMP_USERID_NO_KEYLOCK_LINK_FOUND, new ErrorParameter[] { new(nameof(identityProviderId), identityProviderId.ToString()), new(nameof(companyUserId), companyUserId.ToString()) });
        }
    }

    public async ValueTask<IdentityProviderDetailsWithConnectedCompanies> GetOwnIdentityProviderWithConnectedCompanies(Guid identityProviderId)
    {
        var companyId = _identityData.CompanyId;

        var (alias, category, isOwnerCompany, typeId, metadataUrl, connectedCompanies) = await portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnIdentityProviderWithConnectedCompanies(identityProviderId, companyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!isOwnerCompany)
        {
            throw ConflictException.Create(AdministrationIdentityProviderErrors.IDENTITY_CONFLICT_PROVIDER_NOT_ASSOCIATE_WITH_COMPANY, new ErrorParameter[] { new(nameof(identityProviderId), identityProviderId.ToString()), new(nameof(companyId), companyId.ToString()) });
        }

        if (alias == null)
        {
            throw NotFoundException.Create(AdministrationIdentityProviderErrors.IDENTITY_NOT_PROVIDER_NOT_EXIST, new ErrorParameter[] { new(nameof(identityProviderId), identityProviderId.ToString()) });
        }

        if (category == IdentityProviderCategoryId.KEYCLOAK_SAML && typeId is IdentityProviderTypeId.SHARED)
        {
            throw ConflictException.Create(AdministrationIdentityProviderErrors.IDENTITY_CONFLICT_SHARED_IDP_NOT_USE_SAML);
        }

        var details = category switch
        {
            IdentityProviderCategoryId.KEYCLOAK_OIDC => await GetIdentityProviderDetailsOidc(identityProviderId, alias, category, typeId, metadataUrl).ConfigureAwait(false),
            IdentityProviderCategoryId.KEYCLOAK_SAML => await GetIdentityProviderDetailsSaml(identityProviderId, alias, typeId).ConfigureAwait(false),
            _ => throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_UNEXPECT_VAL_FOR_CATEGORY_OF_PROVIDER, new ErrorParameter[] { new(nameof(category), category.ToString()), new(nameof(identityProviderId), identityProviderId.ToString()) })
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
            throw UnsupportedMediaTypeException.Create(AdministrationIdentityProviderErrors.IDENTITY_UNSUPPORTEDMEDIA_CONTENT_TYPE_ALLOWED, new ErrorParameter[] { new("contentType", _settings.CsvSettings.ContentType) });
        }

        return UploadOwnCompanyUsersIdentityProviderLinkDataInternalAsync(document, cancellationToken);
    }

    private async ValueTask<IdentityProviderUpdateStats> UploadOwnCompanyUsersIdentityProviderLinkDataInternalAsync(IFormFile document, CancellationToken cancellationToken)
    {
        var userRepository = portalRepositories.GetInstance<IUserRepository>();
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
            DetailException detailException when detailException.HasDetails => new UserUpdateError(line, detailException.GetErrorMessage(errorMessageService), detailException.GetErrorDetails(errorMessageService)),
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
        await foreach (var userProfileLinkData in userProfileLinkDatas.WithCancellation(cancellationToken))
        {
            if (userProfileLinkData == default)
            {
                throw UnexpectedConditionException.Create(AdministrationIdentityProviderErrors.IDENTITY_UNEXPECT_USER_PROFILE_LINK_DATA_NEVER_DEFAULT);
            }
            var (companyUserId, profile, identityProviderLinks) = userProfileLinkData;
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
        var identityProviderCategoryData = await portalRepositories.GetInstance<IIdentityProviderRepository>()
                .GetCompanyIdentityProviderCategoryDataUntracked(companyId, null)
                .Where(data => data.Alias != null)
                .Select(data => (data.IdentityProviderId, data.TypeId, Alias: data.Alias!))
                .ToListAsync().ConfigureAwait(false);
        var sharedIdpAlias = identityProviderCategoryData.Where(data => data.TypeId == IdentityProviderTypeId.SHARED).Select(data => (data.IdentityProviderId, data.Alias)).SingleOrDefault();
        var validAliase = identityProviderCategoryData.Select(data => (data.IdentityProviderId, data.Alias)).ToList();
        return (sharedIdpAlias, validAliase);
    }

    private async ValueTask<(string IamUserId, UserProfile ExistingProfile, IAsyncEnumerable<IdentityProviderLink> ExistingLinks)> GetExistingUserAndLinkDataAsync(IUserRepository userRepository, Guid companyUserId, Guid companyId)
    {
        var userEntityData = await userRepository.GetUserEntityDataAsync(companyUserId, companyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (userEntityData == default)
        {
            throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_UNEXPECT_CSV_COMPAY_USERID, new ErrorParameter[] { new("headerUserId", _settings.CsvSettings.HeaderUserId.ToString()), new(nameof(companyUserId), companyUserId.ToString()) });
        }
        var (existingFirstName, existingLastName, existingEmail) = userEntityData;

        var iamUserId = await provisioningManager.GetUserByUserName(companyUserId.ToString()).ConfigureAwait(ConfigureAwaitOptions.None) ?? throw new ConflictException($"user {companyUserId} does not exist in keycloak");

        return (
            iamUserId,
            new UserProfile(existingFirstName, existingLastName, existingEmail),
            provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(iamUserId)
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
            throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_UNEXPECT_SHARED_IDENTITY_PROVIDER_LINK, new ErrorParameter[] { new(nameof(alias), alias), new(nameof(companyUserId), companyUserId.ToString()), new(nameof(userId), userId), new(nameof(userName), userName) });
        }

        if (existingLink != null)
        {
            await provisioningManager.DeleteProviderUserLinkToCentralUserAsync(iamUserId, alias).ConfigureAwait(ConfigureAwaitOptions.None);
        }
        await provisioningManager.AddProviderUserLinkToCentralUserAsync(iamUserId, identityProviderLink).ConfigureAwait(ConfigureAwaitOptions.None);
        await InsertUpdateCompanyUserAssignedIdentityProvider(companyUserId, existingIdps.Single(x => x.Alias == alias).IdentityProviderId, identityProviderLink).ConfigureAwait(ConfigureAwaitOptions.None);
        return true;
    }

    private async Task InsertUpdateCompanyUserAssignedIdentityProvider(Guid companyUserId, Guid identityProviderId, IdentityProviderLink providerLink)
    {
        var userRepository = portalRepositories.GetInstance<IUserRepository>();
        var data = await userRepository.GetCompanyUserAssignedIdentityProvider(companyUserId, identityProviderId).ConfigureAwait(ConfigureAwaitOptions.None);
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

        await provisioningManager.UpdateCentralUserAsync(iamUserId, firstName, lastName, email).ConfigureAwait(ConfigureAwaitOptions.None);

        if (sharedIdp != default)
        {
            var sharedIdpLink = existingLinks.FirstOrDefault(link => link.Alias == sharedIdp.Alias);
            if (sharedIdpLink != default)
            {
                await provisioningManager.UpdateSharedRealmUserAsync(sharedIdp.Alias, sharedIdpLink.UserId, firstName, lastName, email).ConfigureAwait(ConfigureAwaitOptions.None);
            }
        }

        userRepository.AttachAndModifyCompanyUser(companyUserId, null, companyUser =>
            {
                companyUser.Firstname = profile.FirstName;
                companyUser.Lastname = profile.LastName;
                companyUser.Email = profile.Email;
            });
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private int ParseCSVFirstLineReturningNumIdps(string firstLine)
    {
        var headers = firstLine.Split(_settings.CsvSettings.Separator).GetEnumerator();
        foreach (var csvHeader in CSVHeaders())
        {
            if (!headers.MoveNext())
            {
                throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_INVALID_FORMAT_CSVHEADER, new ErrorParameter[] { new(nameof(csvHeader), csvHeader) });
            }
            if ((string)headers.Current != csvHeader)
            {
                throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_INVALID_FORMAT_CSVHEADER_WITH_CURR, new ErrorParameter[] { new(nameof(csvHeader), csvHeader), new("current", headers.Current.ToString() ?? "") });
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
                    throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_INVALID_FORMAT_CSVHEADER, new ErrorParameter[] { new(nameof(csvHeader), csvHeader) });
                }
                if ((string)headers.Current != csvHeader)
                {
                    throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_INVALID_FORMAT_CSVHEADER_WITH_CURR, new ErrorParameter[] { new(nameof(csvHeader), csvHeader), new("current", headers.Current.ToString() ?? "") });
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
            throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_VAL_HEADER_USERID, new ErrorParameter[] { new("headerUserId", _settings.CsvSettings.HeaderUserId) });
        }
        if (!Guid.TryParse(items.Current, out var companyUserId))
        {
            throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_VAL_HEADER_USERID_WITH_CURRENT_ITEMS, new ErrorParameter[] { new("headerUserId", _settings.CsvSettings.HeaderUserId), new("current", items.Current) });
        }
        if (!items.MoveNext())
        {
            throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_VAL_FOR_HEADER_FIRSTNAME, new ErrorParameter[] { new("headerFirstName", _settings.CsvSettings.HeaderFirstName) });
        }
        var firstName = items.Current;
        if (!items.MoveNext())
        {
            throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_VAL_FOR_HEADER_LASTNAME, new ErrorParameter[] { new("headerLastName", _settings.CsvSettings.HeaderLastName) });
        }
        var lastName = items.Current;
        if (!items.MoveNext())
        {
            throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_VAL_FOR_HEADER_EMAIL, new ErrorParameter[] { new("headerEmail", _settings.CsvSettings.HeaderEmail) });
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
                throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_VAL_HEADER_PROVIDERALIAS, new ErrorParameter[] { new("headerProviderAlias", _settings.CsvSettings.HeaderProviderAlias) });
            }
            var identityProviderAlias = items.Current;
            if (!existingAliase.Contains(identityProviderAlias))
            {
                throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_VAL_HEADER_PROVIDERALIAS_WITH_IDENTITY_ALIAS, new ErrorParameter[] { new("headerProviderAlias", _settings.CsvSettings.HeaderProviderAlias), new(nameof(identityProviderAlias), identityProviderAlias) });
            }
            if (!items.MoveNext())
            {
                throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_VAL_HEADER_PROVIDER_USERID, new ErrorParameter[] { new("headerProviderUserId", _settings.CsvSettings.HeaderProviderUserId) });
            }
            var identityProviderUserId = items.Current;
            if (!items.MoveNext())
            {
                throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_VAL_HEADER_PROVIDER_USERNAME, new ErrorParameter[] { new("headerProviderUserName", _settings.CsvSettings.HeaderProviderUserName) });
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
            throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_ATLEAST_ONE_PROVIDERID_SPECIFIED);
        }
        var identityProviderData = await portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderAliasDataUntracked(companyId, identityProviderIds).ToListAsync().ConfigureAwait(false);

        identityProviderIds.Except(identityProviderData.Select(data => data.IdentityProviderId)).IfAny(invalidIds =>
        {
            throw ControllerArgumentException.Create(AdministrationIdentityProviderErrors.IDENTITY_ARGUMENT_INVALID_IDENTITY_PROVIDER_IDS, new ErrorParameter[] { new(nameof(invalidIds), string.Join(", ", invalidIds)), new(nameof(companyId), companyId.ToString()) });
        });

        return identityProviderData;
    }

    private async IAsyncEnumerable<(Guid CompanyUserId, UserProfile UserProfile, IAsyncEnumerable<IdentityProviderLink> LinkDatas)> GetOwnCompanyIdentityProviderLinkDataInternalAsync(Guid companyId)
    {
        await foreach (var (companyUserId, firstName, lastName, email) in portalRepositories.GetInstance<IUserRepository>()
            .GetOwnCompanyUserQuery(companyId)
            .Select(companyUser =>
                new ValueTuple<Guid, string?, string?, string?>(
                    companyUser.Id,
                    companyUser.Firstname,
                    companyUser.Lastname,
                    companyUser.Email))
            .ToAsyncEnumerable().ConfigureAwait(false))
        {
            var iamUserId = await provisioningManager.GetUserByUserName(companyUserId.ToString()).ConfigureAwait(ConfigureAwaitOptions.None);
            if (iamUserId != null)
            {
                yield return (
                    companyUserId,
                    new UserProfile(firstName, lastName, email),
                    provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(iamUserId)
                );
            }
        }
    }

    private async ValueTask<(string IamUserId, string Alias)> GetUserAliasDataAsync(Guid companyUserId, Guid identityProviderId, Guid companyId)
    {
        var (isValidUser, alias, isSameCompany) = await portalRepositories.GetInstance<IIdentityProviderRepository>().GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, companyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!isValidUser)
        {
            throw NotFoundException.Create(AdministrationIdentityProviderErrors.IDENTITY_NOT_COMPANY_USERID_NOT_EXIST, new ErrorParameter[] { new(nameof(companyUserId), companyUserId.ToString()) });
        }
        if (alias == null)
        {
            throw NotFoundException.Create(AdministrationIdentityProviderErrors.IDENTITY_NOT_FOUND_COMPANY_OF_COMPANY_USER_ID, new ErrorParameter[] { new(nameof(identityProviderId), identityProviderId.ToString()), new(nameof(companyUserId), companyUserId.ToString()) });
        }
        if (!isSameCompany)
        {
            throw ConflictException.Create(AdministrationIdentityProviderErrors.IDENTITY_CONFLICT_PROVIDER_NOT_ASSOCIATE_WITH_COMPANY, new ErrorParameter[] { new(nameof(identityProviderId), identityProviderId.ToString()), new(nameof(companyId), companyId.ToString()) });
        }
        var iamUserId = await provisioningManager.GetUserByUserName(companyUserId.ToString()).ConfigureAwait(ConfigureAwaitOptions.None);
        if (iamUserId == null)
        {
            throw UnexpectedConditionException.Create(AdministrationIdentityProviderErrors.IDENTITY_UNEXPECT_COMPANY_USERID_NOT_LINKED_KEYCLOAK, new ErrorParameter[] { new(nameof(companyUserId), companyUserId.ToString()) });
        }
        return (iamUserId, alias);
    }

    private sealed record UserProfile(string? FirstName, string? LastName, string? Email);
}
