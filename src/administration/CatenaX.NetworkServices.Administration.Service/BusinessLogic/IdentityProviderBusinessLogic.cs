using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Library.Enums;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class IdentityProviderBusinessLogic : IIdentityProviderBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProvisioningManager _provisioningManager;

    public IdentityProviderBusinessLogic(IPortalRepositories portalRepositories, IProvisioningManager provisioningManager)
    {
        _portalRepositories = portalRepositories;
        _provisioningManager = provisioningManager;
    }

    public async IAsyncEnumerable<IdentityProviderDetails> GetOwnCompanyIdentityProviders(string iamUserId)
    {
        await foreach ( var identityProviderData in _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderDataUntracked(iamUserId).ConfigureAwait(false))
        {
            switch(identityProviderData.CategoryId)
            {
                case IdentityProviderCategoryId.KEYCLOAK_SHARED:
                case IdentityProviderCategoryId.KEYCLOAK_OIDC:
                    var identityProviderDataOIDC = await _provisioningManager.GetCentralIdentityProviderDataOIDCAsync(identityProviderData.Alias).ConfigureAwait(false);
                    yield return new IdentityProviderDetails(
                        identityProviderData.Id,
                        identityProviderData.Alias,
                        identityProviderData.CategoryId,
                        identityProviderDataOIDC.DisplayName,
                        identityProviderDataOIDC.RedirectUrl,
                        identityProviderDataOIDC.Enabled)
                        {
                            oidc = new IdentityProviderDetailsOIDC(
                                identityProviderDataOIDC.AuthorizationUrl,
                                identityProviderDataOIDC.ClientId,
                                identityProviderDataOIDC.ClientAuthMethod)
                        };
                    break;
                case IdentityProviderCategoryId.KEYCLOAK_SAML:
                    var identityProviderDataSAML = await _provisioningManager.GetCentralIdentityProviderDataSAMLAsync(identityProviderData.Alias).ConfigureAwait(false);
                    yield return new IdentityProviderDetails(
                        identityProviderData.Id,
                        identityProviderData.Alias,
                        identityProviderData.CategoryId,
                        identityProviderDataSAML.DisplayName,
                        identityProviderDataSAML.RedirectUrl,
                        identityProviderDataSAML.Enabled)
                        {
                            saml = new IdentityProviderDetailsSAML(
                                identityProviderDataSAML.EntityId,
                                identityProviderDataSAML.SingleSignOnServiceUrl)
                        };
                    break;
            }
        }
    }

    public async Task<IdentityProviderDetails> CreateOwnCompanyIdentityProvider(IamIdentityProviderProtocol protocol, string iamUserId)
    {
        IdentityProviderCategoryId identityProviderCategory = default!;
        switch (protocol)
        {
            case IamIdentityProviderProtocol.SAML:
                identityProviderCategory = IdentityProviderCategoryId.KEYCLOAK_SAML;
                break;
            case IamIdentityProviderProtocol.OIDC:
                identityProviderCategory = IdentityProviderCategoryId.KEYCLOAK_OIDC;
                break;
            default:
                throw new ArgumentException($"unexcepted value of protocol: {protocol.ToString()}", nameof(protocol));
        }

        var identityProviderRepository = _portalRepositories.GetInstance<IIdentityProviderRepository>();

        var (companyName, companyId) = await _portalRepositories.GetInstance<ICompanyRepository>().GetCompanyNameIdUntrackedAsync(iamUserId).ConfigureAwait(false);
        if (companyName == null || companyId == default)
        {
            throw new Exception($"user {iamUserId} is not associated with a company");
        }
        var alias = await _provisioningManager.CreateOwnIdpAsync(companyName, protocol).ConfigureAwait(false);
        var identityProvider = identityProviderRepository.CreateIdentityProvider(identityProviderCategory);
        identityProvider.CompanyIdentityProviders.Add(identityProviderRepository.CreateCompanyIdentityProvider(companyId, identityProvider.Id));
        var iamIdentityProvider = identityProviderRepository.CreateIamIdentityProvider(identityProvider, alias);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);

        switch (protocol)
        {
            case IamIdentityProviderProtocol.OIDC:
                return await GetCentralIdentityProviderDetailsOIDCAsync(identityProvider.Id, alias).ConfigureAwait(false);
            case IamIdentityProviderProtocol.SAML:
                return await GetCentralIdentityProviderDetailsSAMLAsync(identityProvider.Id, alias).ConfigureAwait(false);
            default:
                throw new ArgumentException($"unexcepted value of protocol: {protocol.ToString()}", nameof(protocol));
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
                return await GetCentralIdentityProviderDetailsOIDCAsync(identityProviderId, alias).ConfigureAwait(false);
            case IdentityProviderCategoryId.KEYCLOAK_SAML:
                return await GetCentralIdentityProviderDetailsSAMLAsync(identityProviderId, alias).ConfigureAwait(false);
            default:
                throw new ArgumentException($"identityProvider {identityProviderId} category cannot be updated");
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
                    throw new ArgumentNullException("property 'oidc' must not be null", nameof(details));
                }
                await _provisioningManager.UpdateCentralIdentityProviderDataOIDCAsync(alias, details.displayName, details.enabled, details.oidc.authorizationUrl, details.oidc.clientAuthMethod, details.oidc.secret).ConfigureAwait(false);
                var identityProviderDataOIDC = await _provisioningManager.GetCentralIdentityProviderDataOIDCAsync(alias).ConfigureAwait(false);
                return await GetCentralIdentityProviderDetailsOIDCAsync(identityProviderId, alias).ConfigureAwait(false);
            case IdentityProviderCategoryId.KEYCLOAK_SAML:
                if(details.saml == null)
                {
                    throw new ArgumentNullException("property 'saml' must not be null", nameof(details));
                }
                await _provisioningManager.UpdateCentralIdentityProviderDataSAMLAsync(alias, details.displayName, details.enabled, details.saml.serviceProviderEntityId, details.saml.singleSignOnServiceUrl).ConfigureAwait(false);
                var identityProviderDataSAML = await _provisioningManager.GetCentralIdentityProviderDataSAMLAsync(alias).ConfigureAwait(false);
                return await GetCentralIdentityProviderDetailsSAMLAsync(identityProviderId, alias).ConfigureAwait(false);
            default:
                throw new ArgumentException($"identityProvider {identityProviderId} category cannot be updated");
        }
    }

    public async Task DeleteOwnCompanyIdentityProvider(Guid identityProviderId, string iamUserId)
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
        await _provisioningManager.DeleteCentralIdentityProviderAsync(alias).ConfigureAwait(false);
    }

    private async Task<IdentityProviderDetails> GetCentralIdentityProviderDetailsOIDCAsync(Guid identityProviderId, string alias)
    {
        var identityProviderDataOIDC = await _provisioningManager.GetCentralIdentityProviderDataOIDCAsync(alias).ConfigureAwait(false);
        return new IdentityProviderDetails(
            identityProviderId,
            alias,
            IdentityProviderCategoryId.KEYCLOAK_OIDC,
            identityProviderDataOIDC.DisplayName,
            identityProviderDataOIDC.RedirectUrl,
            identityProviderDataOIDC.Enabled)
            {
                oidc = new IdentityProviderDetailsOIDC(
                    identityProviderDataOIDC.AuthorizationUrl,
                    identityProviderDataOIDC.ClientId,
                    identityProviderDataOIDC.ClientAuthMethod)
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
                saml = new IdentityProviderDetailsSAML(
                    identityProviderDataSAML.EntityId,
                    identityProviderDataSAML.SingleSignOnServiceUrl)
            };
    }


    public async Task<UserIdentityProviderData> CreateOwnCompanyUserIdentityProviderDataAsync(Guid companyUserId, string alias, UserLinkData userLinkData, string iamUserId)
    {
        var userAliasData = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, alias, iamUserId).ConfigureAwait(false);
        if (userAliasData == default)
        {
            throw new NotFoundException($"companyUserId {companyUserId} does not exist");
        }
        if (userAliasData.UserEntityId == null)
        {
            throw new Exception($"companyUserId {companyUserId} is not linked to keycloak");
        }
        if (!userAliasData.IdentityProviders.Any(identityProviderData => identityProviderData.Alias == alias))
        {
            throw new NotFoundException($"identityProvider alias {alias} not found in company of user {companyUserId}" );
        }
        if (!userAliasData.IsSameCompany)
        {
            throw new ForbiddenException($"user {iamUserId} does not belong to company of companyUserId {companyUserId}");
        }
        await _provisioningManager.AddProviderUserLinkToCentralUserAsync(userAliasData.UserEntityId, alias, userLinkData.userId, userLinkData.userName).ConfigureAwait(false);
        var linkDatas = await _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(userAliasData.IdentityProviders.Select(identityProviderData => identityProviderData.Alias), userAliasData.UserEntityId).ConfigureAwait(false);
        return new UserIdentityProviderData(
                companyUserId,
                userAliasData.FirstName,
                userAliasData.LastName,
                userAliasData.Email,
                linkDatas.Select(linkData => new UserIdentityProviderLinkData(
                    linkData.Alias,
                    linkData.UserId,
                    linkData.UserName))
            );
    }

    public async Task<UserIdentityProviderData> DeleteOwnCompanyUserIdentityProviderDataAsync(Guid companyUserId, string alias, string iamUserId)
    {
        var userAliasData = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, alias, iamUserId).ConfigureAwait(false);
        if (userAliasData == default)
        {
            throw new NotFoundException($"companyUserId {companyUserId} does not exist");
        }
        if (userAliasData.UserEntityId == null)
        {
            throw new Exception($"companyUserId {companyUserId} is not linked to keycloak");
        }
        if (!userAliasData.IdentityProviders.Any(identityProviderData => identityProviderData.Alias == alias))
        {
            throw new NotFoundException($"identityProvider alias {alias} not found in company of user {companyUserId}" );
        }
        if (!userAliasData.IsSameCompany)
        {
            throw new ForbiddenException($"user {iamUserId} does not belong to company of companyUserId {companyUserId}");
        }
        await _provisioningManager.DeleteProviderUserLinkToCentralUserAsync(userAliasData.UserEntityId, alias);
        var linkDatas = await _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(userAliasData.IdentityProviders.Select(identityProviderData => identityProviderData.Alias), userAliasData.UserEntityId).ConfigureAwait(false);
        return new UserIdentityProviderData(
                companyUserId,
                userAliasData.FirstName,
                userAliasData.LastName,
                userAliasData.Email,
                linkDatas.Select(linkData => new UserIdentityProviderLinkData(
                    linkData.Alias,
                    linkData.UserId,
                    linkData.UserName))
            );
    }

    public async IAsyncEnumerable<UserIdentityProviderData> GetOwnCompanyUserIdentityProviderDataAsync(IEnumerable<string> aliase, string iamUserId)
    {
        var identityProviderData = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderDataUntracked(iamUserId).ToListAsync().ConfigureAwait(false);

        var invalidAliase = aliase.Except(identityProviderData.Select(identityProvider => identityProvider.Alias));
        if (invalidAliase.Count() > 0)
        {
            throw new ArgumentException($"invalid identityProvider aliase: [{String.Join(", ", invalidAliase)}]",nameof(aliase));
        }

        await foreach(var (companyUserId, firstName, lastName, email, userEntityId) in _portalRepositories.GetInstance<IUserRepository>()
            .GetOwnCompanyUserQuery(iamUserId)
            .Select(companyUser =>
                ((Guid, string?, string?, string?, string?)) new (
                    companyUser.Id,
                    companyUser.Firstname,
                    companyUser.Lastname,
                    companyUser.Email,
                    companyUser.IamUser!.UserEntityId))
            .ToAsyncEnumerable())
        {
            if (userEntityId != null)
            {
                var linkDatas = await _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(aliase, userEntityId).ConfigureAwait(false);
                yield return new UserIdentityProviderData(
                    companyUserId,
                    firstName,
                    lastName,
                    email,
                    linkDatas.Select(linkData => new UserIdentityProviderLinkData(
                        linkData.Alias,
                        linkData.UserId,
                        linkData.UserName))
                );
            }
        }
    }
}
