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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;

public class OfferSetupService : IOfferSetupService
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IServiceAccountCreation _serviceAccountCreation;
    private readonly INotificationService _notificationService;
    private readonly IOfferSubscriptionProcessService _offerSubscriptionProcessService;
    private readonly IMailingService _mailingService;
    private readonly ITechnicalUserProfileService _technicalUserProfileService;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="provisioningManager">Access to the provisioning manager</param>
    /// <param name="serviceAccountCreation">Access to the service account creation</param>
    /// <param name="notificationService">Creates notifications for the user</param>
    /// <param name="offerSubscriptionProcessService">Access to offer subscription process service</param>
    /// <param name="mailingService">Mailing service to send mails to the user</param>
    /// <param name="technicalUserProfileService">Access to the technical user profile service</param>
    public OfferSetupService(
        IPortalRepositories portalRepositories,
        IProvisioningManager provisioningManager,
        IServiceAccountCreation serviceAccountCreation,
        INotificationService notificationService,
        IOfferSubscriptionProcessService offerSubscriptionProcessService,
        IMailingService mailingService,
        ITechnicalUserProfileService technicalUserProfileService)
    {
        _portalRepositories = portalRepositories;
        _provisioningManager = provisioningManager;
        _serviceAccountCreation = serviceAccountCreation;
        _notificationService = notificationService;
        _offerSubscriptionProcessService = offerSubscriptionProcessService;
        _mailingService = mailingService;
        _technicalUserProfileService = technicalUserProfileService;
    }

    public async Task<OfferAutoSetupResponseData> AutoSetupOfferAsync(OfferAutoSetupData data, IDictionary<string, IEnumerable<string>> itAdminRoles, string iamUserId, OfferTypeId offerTypeId, string basePortalAddress, IDictionary<string, IEnumerable<string>> serviceManagerRoles)
    {
        var offerSubscriptionsRepository = _portalRepositories.GetInstance<IOfferSubscriptionsRepository>();
        var offerDetails = await GetAndValidateOfferDetails(data.RequestId, iamUserId, offerTypeId, offerSubscriptionsRepository).ConfigureAwait(false);

        offerSubscriptionsRepository.AttachAndModifyOfferSubscription(data.RequestId, subscription =>
        {
            subscription.OfferSubscriptionStatusId = OfferSubscriptionStatusId.ACTIVE;
            subscription.LastEditorId = offerDetails.CompanyUserId;
        });

        if (offerDetails.InstanceData.IsSingleInstance)
        {
            _portalRepositories.GetInstance<IAppSubscriptionDetailRepository>()
                .CreateAppSubscriptionDetail(data.RequestId, appSubscriptionDetail =>
                {
                    appSubscriptionDetail.AppInstanceId = offerDetails.AppInstanceIds.Single();
                    appSubscriptionDetail.AppSubscriptionUrl = offerDetails.InstanceData.InstanceUrl;
                });
            await CreateNotifications(itAdminRoles, offerTypeId, offerDetails).ConfigureAwait(false);
            await SetNotificationsToDone(serviceManagerRoles, offerTypeId, offerDetails.OfferId, offerDetails.SalesManagerId).ConfigureAwait(false);
            await _portalRepositories.SaveAsync().ConfigureAwait(false);
            return new OfferAutoSetupResponseData(null, null);
        }

        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
        ClientInfoData? clientInfoData = null;
        if (offerTypeId == OfferTypeId.APP)
        {
            var (clientId, iamClientId) = await CreateClient(data.OfferUrl, offerDetails.OfferId, true, userRolesRepository).ConfigureAwait(false);
            clientInfoData = new ClientInfoData(clientId, data.OfferUrl);
            CreateAppInstance(data.RequestId, data.OfferUrl, offerDetails.OfferId, iamClientId);
        }

        var technicalUserClientId = clientInfoData?.ClientId ?? $"{offerDetails.OfferName}-{offerDetails.CompanyName}";
        var createTechnicalUserData = new CreateTechnicalUserData(offerDetails.CompanyId, offerDetails.OfferName, offerDetails.Bpn, technicalUserClientId, offerTypeId == OfferTypeId.APP);
        var technicalUserInfoData = await CreateTechnicalUserForSubscription(data.RequestId, createTechnicalUserData).ConfigureAwait(false);

        await CreateNotifications(itAdminRoles, offerTypeId, offerDetails).ConfigureAwait(false);
        await SetNotificationsToDone(serviceManagerRoles, offerTypeId, offerDetails.OfferId, offerDetails.SalesManagerId).ConfigureAwait(false);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(offerDetails.RequesterEmail))
        {
            return new OfferAutoSetupResponseData(technicalUserInfoData, clientInfoData);
        }

        await SendMail(basePortalAddress, $"{offerDetails.RequesterFirstname} {offerDetails.RequesterLastname}", offerDetails.RequesterEmail, offerDetails.OfferName, offerDetails.OfferTypeId).ConfigureAwait(false);
        return new OfferAutoSetupResponseData(
            technicalUserInfoData,
            clientInfoData);
    }

    private async Task<TechnicalUserInfoData?> CreateTechnicalUserForSubscription(Guid subscriptionId, CreateTechnicalUserData data)
    {
        var technicalUserInfoCreations = await _technicalUserProfileService.GetTechnicalUserProfilesForOfferSubscription(subscriptionId).ConfigureAwait(false);

        ServiceAccountCreationInfo? serviceAccountCreationInfo;
        try
        {
            serviceAccountCreationInfo = technicalUserInfoCreations.SingleOrDefault();
        }
        catch (InvalidOperationException)
        {
            throw new UnexpectedConditionException($"There should only be one or none technical user profile configured for {subscriptionId}");
        }

        if (serviceAccountCreationInfo == null)
        {
            return null;
        }

        var (technicalClientId, serviceAccountData, serviceAccountId, userRoleData) = await _serviceAccountCreation
            .CreateServiceAccountAsync(
                serviceAccountCreationInfo,
                data.CompanyId,
                data.Bpn == null ? Enumerable.Empty<string>() : Enumerable.Repeat(data.Bpn, 1),
                CompanyServiceAccountTypeId.MANAGED,
                data.EnhanceTechnicalUserName,
                sa => { sa.OfferSubscriptionId = subscriptionId; })
            .ConfigureAwait(false);

        return new TechnicalUserInfoData(serviceAccountId, userRoleData.Select(x => x.UserRoleText), serviceAccountData.AuthData.Secret, technicalClientId);
    }

    /// <inheritdoc />
    public async Task SetupSingleInstance(Guid offerId, string instanceUrl)
    {
        if (await _portalRepositories.GetInstance<IAppInstanceRepository>()
               .CheckInstanceExistsForOffer(offerId)
               .ConfigureAwait(false))
        {
            throw new ConflictException($"The app instance for offer {offerId} already exist");
        }

        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
        var (_, iamClientId) = await CreateClient(instanceUrl, offerId, false, userRolesRepository);
        _portalRepositories.GetInstance<IAppInstanceRepository>().CreateAppInstance(offerId, iamClientId);
    }

    /// <inheritdoc />
    public async Task DeleteSingleInstance(Guid appInstanceId, Guid clientId, string clientClientId)
    {
        var appInstanceRepository = _portalRepositories.GetInstance<IAppInstanceRepository>();
        if (await appInstanceRepository.CheckInstanceHasAssignedSubscriptions(appInstanceId))
        {
            throw new ConflictException($"The app instance {appInstanceId} is associated with exiting subscriptions");
        }
        await _provisioningManager.DeleteCentralClientAsync(clientClientId)
            .ConfigureAwait(false);

        _portalRepositories.GetInstance<IClientRepository>().RemoveClient(clientId);
        var serviceAccountIds = await appInstanceRepository.GetAssignedServiceAccounts(appInstanceId).ToListAsync().ConfigureAwait(false);
        if (serviceAccountIds.Any())
        {
            appInstanceRepository.RemoveAppInstanceAssignedServiceAccounts(appInstanceId, serviceAccountIds);
        }
        appInstanceRepository.RemoveAppInstance(appInstanceId);
    }

    public async Task<IEnumerable<string?>> ActivateSingleInstanceAppAsync(Guid offerId)
    {
        var data = await _portalRepositories.GetInstance<IOfferRepository>().GetSingleInstanceOfferData(offerId, OfferTypeId.APP).ConfigureAwait(false);
        if (data == null)
        {
            throw new ConflictException($"App {offerId} does not exist.");
        }
        if (!data.IsSingleInstance)
        {
            throw new ConflictException($"offer {offerId} is not set up as single instance app");
        }

        Guid instanceId;
        string internalClientId;
        try
        {
            (instanceId, internalClientId) = data.Instances.Single();
        }
        catch (InvalidOperationException)
        {
            throw new UnexpectedConditionException($"There should always be exactly one instance defined for a single instance offer {offerId}");
        }

        if (string.IsNullOrEmpty(internalClientId))
        {
            throw new ConflictException($"clientId must not be empty for single instance offer {offerId}");
        }
        await _provisioningManager.EnableClient(internalClientId).ConfigureAwait(false);

        var technicalUserData = await CreateTechnicalUsersForOffer(offerId, OfferTypeId.APP, new CreateTechnicalUserData(data.CompanyId, data.OfferName, data.Bpn, internalClientId, true)).ToListAsync()
            .ConfigureAwait(false);

        _portalRepositories.GetInstance<IAppInstanceRepository>().CreateAppInstanceAssignedServiceAccounts(technicalUserData.Select(x => new ValueTuple<Guid, Guid>(instanceId, x.TechnicalUserId)));

        return technicalUserData.Select(x => x.TechnicalClientId);
    }

    private async IAsyncEnumerable<TechnicalUserInfoData> CreateTechnicalUsersForOffer(
        Guid offerId,
        OfferTypeId offerTypeId,
        CreateTechnicalUserData data)
    {
        var creationData = await _technicalUserProfileService.GetTechnicalUserProfilesForOffer(offerId, offerTypeId).ConfigureAwait(false);
        foreach (var creationInfo in creationData)
        {
            var (technicalClientId, serviceAccountData, serviceAccountId, userRoleData) = await _serviceAccountCreation
                .CreateServiceAccountAsync(
                    creationInfo,
                    data.CompanyId,
                    data.Bpn == null ? Enumerable.Empty<string>() : new[] { data.Bpn },
                    CompanyServiceAccountTypeId.MANAGED,
                    data.EnhanceTechnicalUserName)
                .ConfigureAwait(false);
            yield return new TechnicalUserInfoData(serviceAccountId, userRoleData.Select(x => x.UserRoleText), serviceAccountData.AuthData.Secret, technicalClientId);
        }
    }

    /// <inheritdoc />
    public Task UpdateSingleInstance(string clientClientId, string instanceUrl) =>
        _provisioningManager.UpdateClient(clientClientId, instanceUrl, instanceUrl.AppendToPathEncoded("*"));

    private static async Task<OfferSubscriptionTransferData> GetAndValidateOfferDetails(Guid requestId, string iamUserId, OfferTypeId offerTypeId, IOfferSubscriptionsRepository offerSubscriptionsRepository)
    {
        var offerDetails = await offerSubscriptionsRepository
            .GetOfferDetailsAndCheckUser(requestId, iamUserId, offerTypeId)
            .ConfigureAwait(false);
        if (offerDetails == null)
        {
            throw new NotFoundException($"OfferSubscription {requestId} does not exist");
        }

        if (offerDetails.Status is not OfferSubscriptionStatusId.PENDING)
        {
            throw new ConflictException("Status of the offer subscription must be pending");
        }

        if (offerDetails.CompanyUserId == Guid.Empty && offerDetails.TechnicalUserId == Guid.Empty)
        {
            throw new ForbiddenException("Only the providing company can setup the service");
        }

        if (offerDetails.InstanceData.IsSingleInstance && offerDetails.AppInstanceIds.Count() != 1)
        {
            throw new ConflictException("There must only be one app instance for single instance apps");
        }

        return offerDetails;
    }

    private async Task<(string clientId, Guid iamClientId)> CreateClient(string offerUrl, Guid offerId, bool enabled, IUserRolesRepository userRolesRepository)
    {
        var userRoles = await userRolesRepository.GetUserRolesForOfferIdAsync(offerId).ToListAsync().ConfigureAwait(false);
        offerUrl.EnsureValidHttpUrl(() => nameof(offerUrl));
        var redirectUrl = offerUrl.AppendToPathEncoded("*");

        var clientId = await _provisioningManager.SetupClientAsync(redirectUrl, offerUrl, userRoles, enabled)
            .ConfigureAwait(false);
        var iamClient = _portalRepositories.GetInstance<IClientRepository>().CreateClient(clientId);
        return (clientId, iamClient.Id);
    }

    private void CreateAppInstance(
        Guid offerSubscriptionId,
        string offerUrl,
        Guid offerId,
        Guid iamClientId)
    {
        var appInstance = _portalRepositories.GetInstance<IAppInstanceRepository>()
            .CreateAppInstance(offerId, iamClientId);
        _portalRepositories.GetInstance<IAppSubscriptionDetailRepository>()
            .CreateAppSubscriptionDetail(offerSubscriptionId, appSubscriptionDetail =>
            {
                appSubscriptionDetail.AppInstanceId = appInstance.Id;
                appSubscriptionDetail.AppSubscriptionUrl = offerUrl;
            });
    }

    private async Task CreateNotifications(
        IDictionary<string, IEnumerable<string>> itAdminRoles,
        OfferTypeId offerTypeId,
        OfferSubscriptionTransferData offerDetails)
    {
        var appSubscriptionActivation = offerTypeId == OfferTypeId.APP
            ? NotificationTypeId.APP_SUBSCRIPTION_ACTIVATION
            : NotificationTypeId.SERVICE_ACTIVATION;
        var notificationContent = JsonSerializer.Serialize(new
        {
            offerDetails.OfferId,
            offerDetails.CompanyName,
            offerDetails.OfferName
        });
        var notifications = new List<(string?, NotificationTypeId)>
        {
            (notificationContent, appSubscriptionActivation)
        };

        if (!offerDetails.InstanceData.IsSingleInstance)
        {
            notifications.Add((JsonSerializer.Serialize(new { offerDetails.OfferId, offerDetails.OfferName }), NotificationTypeId.TECHNICAL_USER_CREATION));
        }
        Guid? creatorId = offerDetails.CompanyUserId != Guid.Empty ? offerDetails.CompanyUserId : null;
        var userIdsOfNotifications = await _notificationService.CreateNotifications(
            itAdminRoles,
            creatorId,
            notifications,
            offerDetails.CompanyId).ToListAsync().ConfigureAwait(false);

        if (!userIdsOfNotifications.Contains(offerDetails.RequesterId))
        {
            _portalRepositories.GetInstance<INotificationRepository>().CreateNotification(offerDetails.RequesterId, appSubscriptionActivation, false, notification =>
            {
                notification.Content = notificationContent;
                notification.CreatorUserId = creatorId;
            });
        }
    }

    private async Task SetNotificationsToDone(
        IDictionary<string, IEnumerable<string>> serviceManagerRoles,
        OfferTypeId offerTypeId,
        Guid offerId,
        Guid? salesManagerId)
    {
        var notificationType = offerTypeId == OfferTypeId.APP
            ? NotificationTypeId.APP_SUBSCRIPTION_REQUEST
            : NotificationTypeId.SERVICE_REQUEST;
        await _notificationService.SetNotificationsForOfferToDone(
            serviceManagerRoles,
            Enumerable.Repeat(notificationType, 1),
            offerId,
            salesManagerId == null ? Enumerable.Empty<Guid>() : Enumerable.Repeat(salesManagerId.Value, 1))
            .ConfigureAwait(false);
    }

    private async Task SendMail(string basePortalAddress, string userName, string requesterEmail, string? offerName, OfferTypeId offerType)
    {
        var mailParams = new Dictionary<string, string>
        {
            {"offerCustomerName", !string.IsNullOrWhiteSpace(userName) ? userName : "User"},
            {"offerName", offerName ?? "unnamed Offer"},
            {"url", basePortalAddress},
        };
        await _mailingService
            .SendMails(requesterEmail, mailParams, new[] { $"{offerType.ToString().ToLower()}-subscription-activation" })
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task StartAutoSetupAsync(OfferAutoSetupData data, string iamUserId, OfferTypeId offerTypeId)
    {
        var offerSubscriptionRepository = _portalRepositories.GetInstance<IOfferSubscriptionsRepository>();
        var details = await GetAndValidateOfferDetails(data.RequestId, iamUserId, offerTypeId, offerSubscriptionRepository).ConfigureAwait(false);
        if (details.InstanceData.IsSingleInstance)
        {
            throw new ConflictException("This step is not eligible to run for single instance apps");
        }

        var context = await _offerSubscriptionProcessService.VerifySubscriptionAndProcessSteps(data.RequestId,
            ProcessStepTypeId.START_AUTOSETUP, null, true).ConfigureAwait(false);

        offerSubscriptionRepository.CreateOfferSubscriptionProcessData(data.RequestId, data.OfferUrl);

        var nextProcessStepTypeIds = new[]
        {
            offerTypeId == OfferTypeId.APP ?
                ProcessStepTypeId.OFFERSUBSCRIPTION_CLIENT_CREATION :
                ProcessStepTypeId.OFFERSUBSCRIPTION_TECHNICALUSER_CREATION
        };

        _offerSubscriptionProcessService.FinalizeProcessSteps(context, nextProcessStepTypeIds);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateSingleInstanceSubscriptionDetail(Guid offerSubscriptionId)
    {
        var offerSubscriptionRepository = _portalRepositories.GetInstance<IOfferSubscriptionsRepository>();
        var offerDetails = await offerSubscriptionRepository.GetSubscriptionActivationDataByIdAsync(offerSubscriptionId).ConfigureAwait(false);
        if (offerDetails == null)
        {
            throw new NotFoundException($"Offer Subscription {offerSubscriptionId} does not exist");
        }

        switch (offerDetails.InstanceData.IsSingleInstance)
        {
            case false:
                throw new ConflictException("The process step is only executable for single instance apps");
            case true when offerDetails.AppInstanceIds.Count() != 1:
                throw new ConflictException("There must only be one app instance for single instance apps");
            default:
                _portalRepositories.GetInstance<IAppSubscriptionDetailRepository>()
                    .CreateAppSubscriptionDetail(offerSubscriptionId, appSubscriptionDetail =>
                    {
                        appSubscriptionDetail.AppInstanceId = offerDetails.AppInstanceIds.Single();
                        appSubscriptionDetail.AppSubscriptionUrl = offerDetails.InstanceData.InstanceUrl;
                    });

                return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
                    new[]
                    {
                        ProcessStepTypeId.ACTIVATE_SUBSCRIPTION
                    },
                    ProcessStepStatusId.DONE,
                    true,
                    null);
        }
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateClient(Guid offerSubscriptionId)
    {
        var clientCreationData = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>().GetClientCreationData(offerSubscriptionId).ConfigureAwait(false);
        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
        if (clientCreationData == null)
        {
            throw new NotFoundException($"offer subscription {offerSubscriptionId} does not exist");
        }

        if (string.IsNullOrWhiteSpace(clientCreationData.OfferUrl))
        {
            throw new ConflictException("OfferUrl should be set");
        }

        if (clientCreationData.OfferType != OfferTypeId.APP)
        {
            throw new ConflictException($"Offers without type {OfferTypeId.APP} are not eligible to run");
        }

        var (_, iamClientId) = await CreateClient(clientCreationData.OfferUrl, clientCreationData.OfferId, true, userRolesRepository);
        CreateAppInstance(offerSubscriptionId, clientCreationData.OfferUrl, clientCreationData.OfferId, iamClientId);

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            new[]
            {
                clientCreationData.IsTechnicalUserNeeded ?
                    ProcessStepTypeId.OFFERSUBSCRIPTION_TECHNICALUSER_CREATION :
                    ProcessStepTypeId.ACTIVATE_APPLICATION
            },
            ProcessStepStatusId.DONE,
            true,
            null);
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateTechnicalUser(Guid offerSubscriptionId, IDictionary<string, IEnumerable<string>> itAdminRoles)
    {
        var data = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetTechnicalUserCreationData(offerSubscriptionId)
            .ConfigureAwait(false);
        if (data == null)
        {
            throw new NotFoundException($"Offer subscription {offerSubscriptionId} does not exist");
        }

        if (!data.IsTechnicalUserNeeded)
        {
            throw new ConflictException("Technical user is not needed");
        }

        var technicalUserClientId = data.ClientId ?? $"{data.OfferName}-{data.CompanyName}";
        var createTechnicalUserData = new CreateTechnicalUserData(data.CompanyId, data.OfferName, data.Bpn, technicalUserClientId, true);
        var technicalUserInfoData = await CreateTechnicalUserForSubscription(offerSubscriptionId, createTechnicalUserData).ConfigureAwait(false);
        var technicalClientId = technicalUserInfoData?.TechnicalClientId;

        var content = JsonSerializer.Serialize(new
        {
            technicalClientId,
        });

        await _notificationService.CreateNotifications(
            itAdminRoles,
            null,
            new[]
            {
                (content, NotificationTypeId.TECHNICAL_USER_CREATION)
            }!,
            data.CompanyId).AwaitAll().ConfigureAwait(false);

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            new[]
            {
                ProcessStepTypeId.ACTIVATE_SUBSCRIPTION
            },
            ProcessStepStatusId.DONE,
            true,
            null);
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> ActivateSubscription(Guid offerSubscriptionId, IDictionary<string, IEnumerable<string>> itAdminRoles, IDictionary<string, IEnumerable<string>> serviceManagerRoles, string basePortalAddress)
    {
        var offerSubscriptionRepository = _portalRepositories.GetInstance<IOfferSubscriptionsRepository>();
        var offerDetails = await offerSubscriptionRepository.GetSubscriptionActivationDataByIdAsync(offerSubscriptionId).ConfigureAwait(false);
        if (offerDetails == null)
        {
            throw new NotFoundException($"Offer Subscription {offerSubscriptionId} does not exist");
        }

        switch (offerDetails.InstanceData.IsSingleInstance)
        {
            case true when offerDetails.AppInstanceIds.Count() != 1:
                throw new ConflictException("There must only be one app instance for single instance apps");
            case true:
                await SetNotificationsToDone(serviceManagerRoles, offerDetails.OfferTypeId, offerDetails.OfferId, offerDetails.SalesManagerId).ConfigureAwait(false);
                break;
        }

        offerSubscriptionRepository.AttachAndModifyOfferSubscription(offerSubscriptionId, subscription =>
        {
            subscription.OfferSubscriptionStatusId = OfferSubscriptionStatusId.ACTIVE;
        });

        if (offerDetails.HasOfferSubscriptionProcessData)
        {
            offerSubscriptionRepository.RemoveOfferSubscriptionProcessData(offerSubscriptionId);
        }

        var notificationContent = JsonSerializer.Serialize(new
        {
            offerDetails.OfferId,
            offerDetails.CompanyName,
            offerDetails.OfferName,
            OfferSubscriptionId = offerSubscriptionId
        });
        var notificationTypeId = offerDetails.OfferTypeId == OfferTypeId.APP
            ? NotificationTypeId.APP_SUBSCRIPTION_ACTIVATION
            : NotificationTypeId.SERVICE_ACTIVATION;
        var userIdsOfNotifications = await _notificationService
                .CreateNotificationsWithExistenceCheck(
                    itAdminRoles,
                    null,
                    new List<(string?, NotificationTypeId)>
                    {
                        (notificationContent, notificationTypeId)
                    },
                    offerDetails.CompanyId,
                    nameof(offerSubscriptionId),
                    offerSubscriptionId.ToString())
                .ToListAsync().ConfigureAwait(false);

        var notificationRepository = _portalRepositories.GetInstance<INotificationRepository>();
        if (!userIdsOfNotifications.Contains(offerDetails.RequesterId) &&
            !await notificationRepository
                .CheckNotificationExistsForParam(offerDetails.RequesterId, notificationTypeId, nameof(offerSubscriptionId), offerSubscriptionId.ToString())
                .ConfigureAwait(false))
        {
            notificationRepository.CreateNotification(offerDetails.RequesterId,
                notificationTypeId, false, notification => { notification.Content = notificationContent; });
        }

        if (string.IsNullOrWhiteSpace(offerDetails.RequesterEmail))
        {
            return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
                offerDetails.InstanceData.IsSingleInstance ? null : new[] { ProcessStepTypeId.TRIGGER_PROVIDER_CALLBACK },
                ProcessStepStatusId.DONE,
                true,
                null);
        }

        try
        {
            await SendMail(basePortalAddress, $"{offerDetails.RequesterFirstname} {offerDetails.RequesterLastname}", offerDetails.RequesterEmail, offerDetails.OfferName, offerDetails.OfferTypeId).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message, true);
        }

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            offerDetails.InstanceData.IsSingleInstance ? null : new[] { ProcessStepTypeId.TRIGGER_PROVIDER_CALLBACK },
            ProcessStepStatusId.DONE,
            true,
            null);
    }

    internal record CreateTechnicalUserData(Guid CompanyId, string? OfferName, string? Bpn, string TechnicalUserName, bool EnhanceTechnicalUserName);
}
