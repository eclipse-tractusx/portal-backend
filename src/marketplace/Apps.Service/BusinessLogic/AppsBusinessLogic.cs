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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Security.Cryptography;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IAppsBusinessLogic"/>.
/// </summary>
public class AppsBusinessLogic : IAppsBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferSubscriptionService _offerSubscriptionService;
    private readonly AppsSettings _settings;
    private readonly IOfferService _offerService;
    private readonly IOfferSetupService _offerSetupService;
    private readonly IMailingService _mailingService;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="offerSubscriptionService">OfferSubscription Service.</param>
    /// <param name="offerService">Offer service</param>
    /// <param name="offerSetupService">Offer Setup Service</param>
    /// <param name="settings">Settings</param>
    /// <param name="mailingService">Mailing service</param>
    public AppsBusinessLogic(
        IPortalRepositories portalRepositories,
        IOfferSubscriptionService offerSubscriptionService,
        IOfferService offerService,
        IOfferSetupService offerSetupService,
        IOptions<AppsSettings> settings,
        IMailingService mailingService)
    {
        _portalRepositories = portalRepositories;
        _offerSubscriptionService = offerSubscriptionService;
        _offerService = offerService;
        _offerSetupService = offerSetupService;
        _mailingService = mailingService;
        _settings = settings.Value;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<AppData> GetAllActiveAppsAsync(string? languageShortName) =>
        _portalRepositories.GetInstance<IOfferRepository>().GetAllActiveAppsAsync(languageShortName, Constants.DefaultLanguage)
            .Select(app => new AppData(
                    app.Id,
                    app.Name ?? Constants.ErrorString,
                    app.ShortDescription ?? Constants.ErrorString,
                    app.VendorCompanyName,
                    app.LicenseType,
                    app.LicenseText ?? Constants.ErrorString,
                    app.LeadPictureId,
                    app.UseCaseNames));

    /// <inheritdoc/>
    public IAsyncEnumerable<BusinessAppData> GetAllUserUserBusinessAppsAsync(string userId) =>
        _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetAllBusinessAppDataForUserIdAsync(userId)
            .Select(x =>
                new BusinessAppData(
                    x.OfferId,
                    x.SubscriptionId,
                    x.OfferName ?? Constants.ErrorString,
                    x.SubscriptionUrl,
                    x.LeadPictureId,
                    x.Provider));

    /// <inheritdoc/>
    public async Task<AppDetailResponse> GetAppDetailsByIdAsync(Guid appId, string iamUserId, string? languageShortName = null)
    {
        var result = await _portalRepositories.GetInstance<IOfferRepository>()
            .GetOfferDetailsByIdAsync(appId, iamUserId, languageShortName, Constants.DefaultLanguage, OfferTypeId.APP).ConfigureAwait(false);
        if (result == null)
        {
            throw new NotFoundException($"appId {appId} does not exist");
        }

        return new AppDetailResponse(
            result.Id,
            result.Title ?? Constants.ErrorString,
            result.LeadPictureId,
            result.Images,
            result.ProviderUri ?? Constants.ErrorString,
            result.Provider,
            result.ContactEmail,
            result.ContactNumber,
            result.UseCases,
            result.LongDescription ?? Constants.ErrorString,
            result.LicenseTypeId,
            result.Price ?? Constants.ErrorString,
            result.Tags,
            result.IsSubscribed == default ? null : result.IsSubscribed,
            result.Languages,
            result.Documents.GroupBy(d => d.DocumentTypeId).ToDictionary(g => g.Key, g => g.Select(d => new DocumentData(d.DocumentId, d.DocumentName))),
            result.PrivacyPolicies,
            result.IsSingleInstance,
            result.TechnicalUserProfile.ToDictionary(g => g.TechnicalUserProfileId, g => g.UserRoles)
        );
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<Guid> GetAllFavouriteAppsForUserAsync(string userId) =>
        _portalRepositories
            .GetInstance<IUserRepository>()
            .GetAllFavouriteAppsForUserUntrackedAsync(userId);

    /// <inheritdoc/>
    public async Task RemoveFavouriteAppForUserAsync(Guid appId, string userId)
    {
        var companyUserId = await _portalRepositories.GetInstance<IUserRepository>().GetCompanyUserIdForIamUserUntrackedAsync(userId).ConfigureAwait(false);
        _portalRepositories.Remove(new CompanyUserAssignedAppFavourite(appId, companyUserId));
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task AddFavouriteAppForUserAsync(Guid appId, string userId)
    {
        var companyUserId = await _portalRepositories.GetInstance<IUserRepository>().GetCompanyUserIdForIamUserUntrackedAsync(userId).ConfigureAwait(false);
        _portalRepositories.GetInstance<IOfferRepository>().CreateAppFavourite(appId, companyUserId);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<Pagination.Response<OfferSubscriptionStatusDetailData>> GetCompanySubscribedAppSubscriptionStatusesForUserAsync(int page, int size, string iamUserId) =>
        _offerService.GetCompanySubscribedOfferSubscriptionStatusesForUserAsync(page, size, iamUserId, OfferTypeId.APP, DocumentTypeId.APP_LEADIMAGE);

    /// <inheritdoc/>
    public async Task<Pagination.Response<OfferCompanySubscriptionStatusResponse>> GetCompanyProvidedAppSubscriptionStatusesForUserAsync(int page, int size, string iamUserId, SubscriptionStatusSorting? sorting, OfferSubscriptionStatusId? statusId, Guid? offerId)
    {
        async Task<Pagination.Source<OfferCompanySubscriptionStatusResponse>?> GetCompanyProvidedAppSubscriptionStatusData(int skip, int take)
        {
            var offerCompanySubscriptionResponse = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
                .GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(iamUserId, OfferTypeId.APP, sorting, statusId ?? OfferSubscriptionStatusId.ACTIVE, offerId)(skip, take).ConfigureAwait(false);

            return offerCompanySubscriptionResponse == null
                ? null
                : new Pagination.Source<OfferCompanySubscriptionStatusResponse>(
                    offerCompanySubscriptionResponse.Count,
                    offerCompanySubscriptionResponse.Data.Select(item =>
                        new OfferCompanySubscriptionStatusResponse(
                            item.OfferId,
                            item.ServiceName,
                            item.CompanySubscriptionStatuses,
                            item.Image == Guid.Empty ? null : item.Image)));
        }
        return await Pagination.CreateResponseAsync(page, size, _settings.ApplicationsMaxPageSize, GetCompanyProvidedAppSubscriptionStatusData).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task<Guid> AddOwnCompanyAppSubscriptionAsync(Guid appId, IEnumerable<OfferAgreementConsentData> offerAgreementConsentData, string iamUserId, string accessToken) =>
        _offerSubscriptionService.AddOfferSubscriptionAsync(appId, offerAgreementConsentData, iamUserId, OfferTypeId.APP, _settings.BasePortalAddress);

    /// <inheritdoc/>
    [Obsolete("This Method is not used anymore")]
    public async Task ActivateOwnCompanyProvidedAppSubscriptionAsync(Guid appId, Guid subscribingCompanyId, string iamUserId)
    {
        var offerSubscriptionRepository = _portalRepositories.GetInstance<IOfferSubscriptionsRepository>();
        var assignedAppData = await offerSubscriptionRepository.GetCompanyAssignedAppDataForProvidingCompanyUserAsync(appId, subscribingCompanyId, iamUserId).ConfigureAwait(false);
        if (assignedAppData == default)
        {
            throw new NotFoundException($"App {appId} does not exist.");
        }

        var (subscriptionId, subscriptionStatusId, requesterId, appName, companyUserId, requesterData) = assignedAppData;
        if (companyUserId == Guid.Empty)
        {
            throw new ControllerArgumentException("Missing permission: The user's company does not provide the requested app so they cannot activate it.");
        }

        if (subscriptionId == Guid.Empty)
        {
            throw new ControllerArgumentException($"subscription for app {appId}, company {subscribingCompanyId} has not been created yet");
        }

        if (subscriptionStatusId != OfferSubscriptionStatusId.PENDING)
        {
            throw new ControllerArgumentException($"subscription for app {appId}, company {subscribingCompanyId} is not in status PENDING");
        }

        if (appName is null)
        {
            throw new ConflictException("App Name is not yet set.");
        }

        offerSubscriptionRepository.AttachAndModifyOfferSubscription(subscriptionId, subscription => subscription.OfferSubscriptionStatusId = OfferSubscriptionStatusId.ACTIVE);

        _portalRepositories.GetInstance<INotificationRepository>().CreateNotification(requesterId,
            NotificationTypeId.APP_SUBSCRIPTION_ACTIVATION, false,
            notification =>
            {
                notification.CreatorUserId = companyUserId;
                notification.Content = JsonSerializer.Serialize(new
                {
                    AppId = appId,
                    AppName = appName
                });
            });

        var userName = string.Join(" ", requesterData.Firstname, requesterData.Lastname);

        if (!string.IsNullOrWhiteSpace(requesterData.Email))
        {
            var mailParams = new Dictionary<string, string>
            {
                { "offerCustomerName", !string.IsNullOrWhiteSpace(userName) ? userName : "App Owner" },
                { "offerName", appName },
                { "url", _settings.BasePortalAddress },
            };
            await _mailingService.SendMails(requesterData.Email, mailParams, new List<string> { "subscription-activation" }).ConfigureAwait(false);
        }
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UnsubscribeOwnCompanyAppSubscriptionAsync(Guid appId, string iamUserId)
    {
        var assignedAppData = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>().GetCompanyAssignedAppDataForCompanyUserAsync(appId, iamUserId).ConfigureAwait(false);

        if (assignedAppData == default)
        {
            throw new NotFoundException($"App {appId} does not exist.");
        }

        var (subscription, _) = assignedAppData;

        if (subscription == null)
        {
            throw new ArgumentException($"There is no active subscription for user '{iamUserId}' and app '{appId}'");
        }
        subscription.OfferSubscriptionStatusId = OfferSubscriptionStatusId.INACTIVE;
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<AllOfferData> GetCompanyProvidedAppsDataForUserAsync(string userId) =>
        _portalRepositories.GetInstance<IOfferRepository>().GetProvidedOffersData(OfferTypeId.APP, userId);

    /// <inheritdoc />
    public Task<OfferAutoSetupResponseData> AutoSetupAppAsync(OfferAutoSetupData data, string iamUserId) =>
        _offerSetupService.AutoSetupOfferAsync(data, _settings.ITAdminRoles, iamUserId, OfferTypeId.APP, _settings.UserManagementAddress, _settings.ServiceManagerRoles);

    /// <inheritdoc />
    public Task StartAutoSetupAsync(OfferAutoSetupData data, string iamUserId) =>
        _offerSetupService.StartAutoSetupAsync(data, iamUserId, OfferTypeId.APP);

    /// <inheritdoc />
    public IAsyncEnumerable<AgreementData> GetAppAgreement(Guid appId) =>
        _offerService.GetOfferAgreementsAsync(appId, OfferTypeId.APP);

    /// <inheritdoc />
    public Task<(byte[] Content, string ContentType, string FileName)> GetAppDocumentContentAsync(Guid appId, Guid documentId, CancellationToken cancellationToken) =>
        _offerService.GetOfferDocumentContentAsync(appId, documentId, _settings.AppImageDocumentTypeIds, OfferTypeId.APP, cancellationToken);

    /// <inheritdoc />
    public Task<ProviderSubscriptionDetailData> GetSubscriptionDetailForProvider(Guid appId, Guid subscriptionId, string iamUserId) =>
        _offerService.GetSubscriptionDetailsForProviderAsync(appId, subscriptionId, iamUserId, OfferTypeId.APP, _settings.CompanyAdminRoles);

    /// <inheritdoc />
    public Task<SubscriberSubscriptionDetailData> GetSubscriptionDetailForSubscriber(Guid appId, Guid subscriptionId, string iamUserId) =>
        _offerService.GetSubscriptionDetailsForSubscriberAsync(appId, subscriptionId, iamUserId, OfferTypeId.APP, _settings.SalesManagerRoles);
}
