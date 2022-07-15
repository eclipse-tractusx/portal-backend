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
            case IamIdentityProviderProtocol.SAML:
                var identityProviderDataSAML = await _provisioningManager.GetCentralIdentityProviderDataSAMLAsync(alias).ConfigureAwait(false);
                return new IdentityProviderDetails(
                    identityProvider.Id,
                    alias,
                    identityProviderCategory,
                    companyName,
                    identityProviderDataSAML.RedirectUrl,
                    identityProviderDataSAML.Enabled)
                    {
                        saml = new IdentityProviderDetailsSAML(
                            identityProviderDataSAML.EntityId,
                            identityProviderDataSAML.SingleSignOnServiceUrl)
                    };
            case IamIdentityProviderProtocol.OIDC:
                var identityProviderDataOIDC = await _provisioningManager.GetCentralIdentityProviderDataOIDCAsync(alias).ConfigureAwait(false);
                return new IdentityProviderDetails(
                    identityProvider.Id,
                    alias,
                    identityProviderCategory,
                    companyName,
                    identityProviderDataOIDC.RedirectUrl,
                    identityProviderDataOIDC.Enabled)
                    {
                        oidc = new IdentityProviderDetailsOIDC(
                            identityProviderDataOIDC.ClientId,
                            identityProviderDataOIDC.AuthorizationUrl,
                            identityProviderDataOIDC.ClientAuthMethod)
                    };
            default:
                throw new ArgumentException($"unexcepted value of protocol: {protocol.ToString()}", nameof(protocol));
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
                return new IdentityProviderDetails(
                    identityProviderId,
                    alias,
                    category,
                    details.displayName,
                    identityProviderDataOIDC.RedirectUrl,
                    identityProviderDataOIDC.Enabled)
                    {
                        oidc = new IdentityProviderDetailsOIDC(
                            identityProviderDataOIDC.AuthorizationUrl,
                            identityProviderDataOIDC.ClientId,
                            identityProviderDataOIDC.ClientAuthMethod)
                    };
            case IdentityProviderCategoryId.KEYCLOAK_SAML:
                if(details.saml == null)
                {
                    throw new ArgumentNullException("property 'saml' must not be null", nameof(details));
                }
                await _provisioningManager.UpdateCentralIdentityProviderDataSAMLAsync(alias, details.displayName, details.enabled, details.saml.serviceProviderEntityId, details.saml.singleSignOnServiceUrl).ConfigureAwait(false);
                var identityProviderDataSAML = await _provisioningManager.GetCentralIdentityProviderDataSAMLAsync(alias).ConfigureAwait(false);
                return new IdentityProviderDetails(
                    identityProviderId,
                    alias,
                    category,
                    details.displayName,
                    identityProviderDataSAML.RedirectUrl,
                    identityProviderDataSAML.Enabled)
                    {
                        saml = new IdentityProviderDetailsSAML(
                            identityProviderDataSAML.EntityId,
                            identityProviderDataSAML.SingleSignOnServiceUrl)
                    };
            default:
                throw new ArgumentException($"identityProvider {identityProviderId} category cannot be updated");
        }
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
                        linkData.alias,
                        linkData.userId,
                        linkData.userName))
                );
            }
        }
    }
}
