using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Library.Enums;
using CatenaX.NetworkServices.Provisioning.DBAccess;
using Microsoft.Extensions.Options;
using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.Provisioning.Library.ViewModels;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class ServiceAccountBusinessLogic : IServiceAccountBusinessLogic
{
    private readonly IProvisioningManager _provisioningManager;
    private readonly IProvisioningDBAccess _provisioningDBAccess;
    private readonly IPortalBackendDBAccess _portalDBAccess;
    private readonly UserSettings _settings;

    public ServiceAccountBusinessLogic(
        IProvisioningManager provisioningManager,
        IProvisioningDBAccess provisioningDBAccess,
        IPortalBackendDBAccess portalDBAccess,
        IOptions<UserSettings> settings)
    {
        _provisioningManager = provisioningManager;
        _provisioningDBAccess = provisioningDBAccess;
        _portalDBAccess = portalDBAccess;
        _settings = settings.Value;
    }

    public async Task<ServiceAccountDetails> CreateOwnCompanyServiceAccountAsync(ServiceAccountCreationInfo serviceAccountCreationInfos, string iamAdminId)
    {
        if (String.IsNullOrWhiteSpace(serviceAccountCreationInfos.Name))
        {
            throw new ArgumentException("name must not be empty","name");
        }

        var iamServiceAccountUserEntityId = Guid.NewGuid().ToString(); //TODO create serviceaccount on keycloak and retrieve serviceAccounts userEntityId
        var clientId = "dummy-client";

        var companyId = await _portalDBAccess.GetCompanyIdForIamUserUntrackedAsync(iamAdminId).ConfigureAwait(false);

        var serviceAccount = _portalDBAccess.CreateCompanyServiceAccount(
            companyId,
            CompanyServiceAccountStatusId.ACTIVE,
            serviceAccountCreationInfos.Name,
            serviceAccountCreationInfos.Description);

        var IamServiceAccount = _portalDBAccess.CreateIamServiceAccount(
            iamServiceAccountUserEntityId,
            clientId,
            serviceAccount.Id);

        await _portalDBAccess.SaveAsync().ConfigureAwait(false);
        return new ServiceAccountDetails(
            serviceAccount.Id,
            clientId,
            serviceAccountCreationInfos.Name,
            serviceAccountCreationInfos.Description,
            serviceAccountCreationInfos.IamClientAuthMethod)
        {
            Secret = "asdhgöaölsdgh" //TODO get secret from keycloak
        };
    }

    public async Task<int> DeleteOwnCompanyServiceAccountAsync(Guid serviceAccountId, string iamAdminId)
    {
        var serviceAccount = await _portalDBAccess.GetOwnCompanyServiceAccountWithIamServiceAccountAsync(serviceAccountId, iamAdminId).ConfigureAwait(false);
        if (serviceAccount == null)
        {
            throw new NotFoundException($"serviceAccount {serviceAccountId} not found in company of user {iamAdminId}");
        }
        if (serviceAccount.CompanyServiceAccountStatusId == CompanyServiceAccountStatusId.INACTIVE)
        {
            throw new ArgumentException($"serviceAccount {serviceAccountId} is already INACTIVE");
        }
        serviceAccount.CompanyServiceAccountStatusId = CompanyServiceAccountStatusId.INACTIVE;
        if (serviceAccount.IamServiceAccount != null)
        {
            _portalDBAccess.RemoveIamServiceAccount(serviceAccount.IamServiceAccount);
        }
        return await _portalDBAccess.SaveAsync().ConfigureAwait(false);
    }

    public async Task<ServiceAccountDetails> GetOwnCompanyServiceAccountDetailsAsync(Guid serviceAccountId, string iamAdminId)
    {
        var result = await _portalDBAccess.GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(serviceAccountId, iamAdminId);

        if (result == null)
        {
            throw new NotFoundException($"serviceAccount {serviceAccountId} not found in company of {iamAdminId}");
        }
        return new ServiceAccountDetails(result.ServiceAccountId, result.ClientId, result.Name, result.Description, IamClientAuthMethod.SECRET) //get add clientAuthMethod, and secret from keycloak
        {
            Secret = "asdhgöaölsdgh"
        };
    }

    public async Task<ServiceAccountDetails> UpdateOwnCompanyServiceAccountDetailsAsync(Guid serviceAccountId, ServiceAccountEditableDetails serviceAccountEditableDetails, string iamAdminId)
    {
        if (serviceAccountId != serviceAccountEditableDetails.ServiceAccountId)
        {
            throw new ArgumentException($"serviceAccountId {serviceAccountId} from path does not match the one in body {serviceAccountEditableDetails.ServiceAccountId}","serviceAccountId");
        }
        var result = await _portalDBAccess.GetOwnCompanyServiceAccountWithIamClientIdAsync(serviceAccountId, iamAdminId).ConfigureAwait(false);
        if (result == null)
        {
            throw new NotFoundException($"serviceAccount {serviceAccountId} not found in company of user {iamAdminId}");
        }
        var serviceAccount = result.CompanyServiceAccount;
        if (serviceAccount.CompanyServiceAccountStatusId == CompanyServiceAccountStatusId.INACTIVE)
        {
            throw new ArgumentException($"serviceAccount {serviceAccountId} is already INACTIVE");
        }
        serviceAccount.Name = serviceAccountEditableDetails.Name;
        serviceAccount.Description = serviceAccountEditableDetails.Description;

        await _portalDBAccess.SaveAsync().ConfigureAwait(false);

        return new ServiceAccountDetails(
            serviceAccount.Id,
            result.ClientClientId,
            serviceAccount.Name,
            serviceAccount.Description,
            serviceAccountEditableDetails.IamClientAuthMethod) //TODO update iamClientAuthMethod in Keycloak and retrieve secret
        {
            Secret = "asdhgöaölsdgh"
        };
    }

    public async Task<Pagination.Response<ServiceAccountData>> GetOwnCompanyServiceAccountsDataAsync(int page, int size, string iamAdminId)
    {
        var result = await Pagination.CreateResponseAsync<ServiceAccountData>(
            page,
            size,
            15,
            (int skip, int take) => _portalDBAccess.GetOwnCompanyServiceAccountDetailsUntracked(skip, take, iamAdminId)).ConfigureAwait(false);

        if (result == null)
        {
            throw new ArgumentException($"user {iamAdminId} is not associated with any company");
        }
        return result;
    }
}
