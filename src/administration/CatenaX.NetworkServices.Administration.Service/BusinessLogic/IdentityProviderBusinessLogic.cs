using CatenaX.NetworkServices.Administration.Service.Models;
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
                        identityProviderData.CategoryId,
                        identityProviderDataOIDC.DisplayName,
                        identityProviderDataOIDC.RedirectUrl,
                        identityProviderDataOIDC.Enabled)
                    {
                        ClientId = identityProviderDataOIDC.ClientId,
                        AuthorizationUrl = identityProviderDataOIDC.AuthorizationUrl,
                        ClientAuthMethod = identityProviderDataOIDC.ClientAuthMethod
                    };
                    break;
                case IdentityProviderCategoryId.KEYCLOAK_SAML:
                    var identityProviderDataSAML = await _provisioningManager.GetCentralIdentityProviderDataSAMLAsync(identityProviderData.Alias).ConfigureAwait(false);
                    yield return new IdentityProviderDetails(
                        identityProviderData.Id,
                        identityProviderData.CategoryId,
                        identityProviderDataSAML.DisplayName,
                        identityProviderDataSAML.RedirectUrl,
                        identityProviderDataSAML.Enabled)
                    {
                        ServiceProviderEntityId = identityProviderDataSAML.EntityId,
                        SingleSignOnServiceUrl = identityProviderDataSAML.SingleSignOnServiceUrl
                    };
                    break;
            }
        }
    }

    public async Task<IdentityProviderDetails> CreateOwnCompanyIdentityProvider(IamIdentityProviderProtocol protocol, string iamUserId)
    {
        IdentityProviderCategoryId identityProviderCatogory = default!;
        switch (protocol)
        {
            case IamIdentityProviderProtocol.SAML:
                identityProviderCatogory = IdentityProviderCategoryId.KEYCLOAK_SAML;
                break;
            case IamIdentityProviderProtocol.OIDC:
                identityProviderCatogory = IdentityProviderCategoryId.KEYCLOAK_OIDC;
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
        var identityProvider = identityProviderRepository.CreateIdentityProvider(identityProviderCatogory);
        identityProvider.CompanyIdentityProviders.Add(identityProviderRepository.CreateCompanyIdentityProvider(companyId, identityProvider.Id));
        var iamIdentityProvider = identityProviderRepository.CreateIamIdentityProvider(identityProvider, alias);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);

        switch (protocol)
        {
            case IamIdentityProviderProtocol.SAML:
                var identityProviderDataSAML = await _provisioningManager.GetCentralIdentityProviderDataSAMLAsync(alias).ConfigureAwait(false);
                return new IdentityProviderDetails(
                    identityProvider.Id,
                    identityProviderCatogory,
                    companyName,
                    identityProviderDataSAML.RedirectUrl,
                    identityProviderDataSAML.Enabled)
                    {
                        ServiceProviderEntityId = identityProviderDataSAML.EntityId,
                        SingleSignOnServiceUrl = identityProviderDataSAML.SingleSignOnServiceUrl
                    };
            case IamIdentityProviderProtocol.OIDC:
                var identityProviderDataOIDC = await _provisioningManager.GetCentralIdentityProviderDataOIDCAsync(alias).ConfigureAwait(false);
                return new IdentityProviderDetails(
                    identityProvider.Id,
                    identityProviderCatogory,
                    companyName,
                    identityProviderDataOIDC.RedirectUrl,
                    identityProviderDataOIDC.Enabled)
                    {
                        ClientId = identityProviderDataOIDC.ClientId,
                        AuthorizationUrl = identityProviderDataOIDC.AuthorizationUrl,
                        ClientAuthMethod = identityProviderDataOIDC.ClientAuthMethod
                    };
            default:
                throw new ArgumentException($"unexcepted value of protocol: {protocol.ToString()}", nameof(protocol));
        }
    }
}
