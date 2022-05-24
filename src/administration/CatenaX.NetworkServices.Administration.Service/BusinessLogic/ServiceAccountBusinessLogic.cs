using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.Provisioning.Library.ViewModels;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class ServiceAccountBusinessLogic : IServiceAccountBusinessLogic
{
    private readonly IProvisioningManager _provisioningManager;
    private readonly IPortalBackendDBAccess _portalDBAccess;

    public ServiceAccountBusinessLogic(
        IProvisioningManager provisioningManager,
        IPortalBackendDBAccess portalDBAccess)
    {
        _provisioningManager = provisioningManager;
        _portalDBAccess = portalDBAccess;
    }

    public async Task<ServiceAccountDetails> CreateOwnCompanyServiceAccountAsync(ServiceAccountCreationInfo serviceAccountCreationInfos, string iamAdminId)
    {
        if (String.IsNullOrWhiteSpace(serviceAccountCreationInfos.Name))
        {
            throw new ArgumentException("name must not be empty","name");
        }

        var companyId = await _portalDBAccess.GetCompanyIdForIamUserUntrackedAsync(iamAdminId).ConfigureAwait(false);
        if (companyId == null)
        {
            throw new ArgumentException($"user {iamAdminId} is not associated with any company","iamAdminId");
        }

        var clientId = await _provisioningManager.GetNextServiceAccountClientIdAsync().ConfigureAwait(false);
        var serviceAccountData = await _provisioningManager.SetupCentralServiceAccountClientAsync(
            clientId,
            new ClientConfigData(
                serviceAccountCreationInfos.Name,
                serviceAccountCreationInfos.Description,
                serviceAccountCreationInfos.IamClientAuthMethod)).ConfigureAwait(false);

        var serviceAccount = _portalDBAccess.CreateCompanyServiceAccount(
            companyId,
            CompanyServiceAccountStatusId.ACTIVE,
            serviceAccountCreationInfos.Name,
            serviceAccountCreationInfos.Description);

        _portalDBAccess.CreateIamServiceAccount(
            serviceAccountData.InternalClientId,
            clientId,
            serviceAccountData.UserEntityId,
            serviceAccount.Id);

        await _portalDBAccess.SaveAsync().ConfigureAwait(false);
        return new ServiceAccountDetails(
            serviceAccount.Id,
            clientId,
            serviceAccountCreationInfos.Name,
            serviceAccountCreationInfos.Description,
            serviceAccountCreationInfos.IamClientAuthMethod)
        {
            Secret = serviceAccountData.AuthData.Secret
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
            await _provisioningManager.DeleteCentralClientAsync(serviceAccount.IamServiceAccount.ClientId).ConfigureAwait(false);
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
        var authData = await _provisioningManager.GetCentralClientAuthDataAsync(result.ClientId).ConfigureAwait(false);
        return new ServiceAccountDetails(
            result.ServiceAccountId,
            result.ClientClientId,
            result.Name,
            result.Description,
            authData.IamClientAuthMethod)
            {
                Secret = authData.Secret
            };
    }

    public async Task<ServiceAccountDetails> ResetOwnCompanyServiceAccountSecretAsync(Guid serviceAccountId, string iamAdminId)
    {
        var result = await _portalDBAccess.GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(serviceAccountId, iamAdminId);

        if (result == null)
        {
            throw new NotFoundException($"serviceAccount {serviceAccountId} not found in company of {iamAdminId}");
        }
        var authData = await _provisioningManager.ResetCentralClientAuthDataAsync(result.ClientId).ConfigureAwait(false);
        return new ServiceAccountDetails(
            result.ServiceAccountId,
            result.ClientClientId,
            result.Name,
            result.Description,
            authData.IamClientAuthMethod)
            {
                Secret = authData.Secret
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

        await _provisioningManager.UpdateCentralClientAsync(
            result.ClientId,
            new ClientConfigData(
                serviceAccountEditableDetails.Name,
                serviceAccountEditableDetails.Description,
                serviceAccountEditableDetails.IamClientAuthMethod)).ConfigureAwait(false);
        
        var authData = await _provisioningManager.GetCentralClientAuthDataAsync(result.ClientId).ConfigureAwait(false);

        serviceAccount.Name = serviceAccountEditableDetails.Name;
        serviceAccount.Description = serviceAccountEditableDetails.Description;

        await _portalDBAccess.SaveAsync().ConfigureAwait(false);

        return new ServiceAccountDetails(
            serviceAccount.Id,
            result.ClientClientId,
            serviceAccount.Name,
            serviceAccount.Description,
            authData.IamClientAuthMethod)
        {
            Secret = authData.Secret
        };
    }

    public async Task<Pagination.Response<CompanyServiceAccountData>> GetOwnCompanyServiceAccountsDataAsync(int page, int size, string iamAdminId)
    {
        var result = await Pagination.CreateResponseAsync<CompanyServiceAccountData>(
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
