/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Notification.Library;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IAppsBusinessLogic"/>.
/// </summary>
public class AppsBusinessLogic : IAppsBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IMailingService _mailingService;
    private readonly INotificationService _notificationService;
    private readonly AppsSettings _settings;
    private readonly IOfferService _offerService;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="mailingService">Mail service.</param>
    /// <param name="notificationService">Notification service.</param>
    /// <param name="offerService">Offer service</param>
    /// <param name="settings">Settings</param>
    public AppsBusinessLogic(IPortalRepositories portalRepositories, IMailingService mailingService, INotificationService notificationService, IOfferService offerService, IOptions<AppsSettings> settings)
    {
        _portalRepositories = portalRepositories;
        _mailingService = mailingService;
        _notificationService = notificationService;
        _offerService = offerService;
        _settings = settings.Value;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<AppData> GetAllActiveAppsAsync(string? languageShortName = null) =>
        _portalRepositories.GetInstance<IOfferRepository>().GetAllActiveAppsAsync(languageShortName)
            .Select(app => new AppData(
                    app.Name ?? Constants.ErrorString,
                    app.ShortDescription ?? Constants.ErrorString,
                    app.VendorCompanyName ?? Constants.ErrorString,
                    app.LicenseText ?? Constants.ErrorString,
                    app.ThumbnailUrl ?? Constants.ErrorString
                    )
                {
                    Id = app.Id,
                    UseCases = app.UseCaseNames.Select(name => name).ToList()
                });

    /// <inheritdoc/>
    public IAsyncEnumerable<BusinessAppData> GetAllUserUserBusinessAppsAsync(string userId) =>
        _portalRepositories.GetInstance<IUserRepository>().GetAllBusinessAppDataForUserIdAsync(userId);

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
            result.LeadPictureUri ?? Constants.ErrorString,
            result.DetailPictureUris,
            result.ProviderUri ?? Constants.ErrorString,
            result.Provider,
            result.ContactEmail,
            result.ContactNumber,
            result.UseCases,
            result.LongDescription ?? Constants.ErrorString,
            result.Price ?? Constants.ErrorString,
            result.Tags,
            result.IsSubscribed == default ? null : result.IsSubscribed,
            result.Languages,
            result.Documents.GroupBy(d => d.documentTypeId).ToDictionary(g => g.Key, g => g.Select(d => new DocumentData(d.documentId, d.documentName)))
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
        try
        {
            var companyUserId = await _portalRepositories.GetInstance<IUserRepository>().GetCompanyUserIdForIamUserUntrackedAsync(userId).ConfigureAwait(false);
            _portalRepositories.Remove(new CompanyUserAssignedAppFavourite(appId, companyUserId));
            await _portalRepositories.SaveAsync().ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ArgumentException($"Parameters are invalid or favourite does not exist.");
        }
    }

    /// <inheritdoc/>
    public async Task AddFavouriteAppForUserAsync(Guid appId, string userId)
    {
        try
        {
            var companyUserId = await _portalRepositories.GetInstance<IUserRepository>().GetCompanyUserIdForIamUserUntrackedAsync(userId).ConfigureAwait(false);
            _portalRepositories.GetInstance<IOfferRepository>().CreateAppFavourite(appId, companyUserId);
            await _portalRepositories.SaveAsync().ConfigureAwait(false);
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException($"Parameters are invalid or app is already favourited.");
        }
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<AppWithSubscriptionStatus> GetCompanySubscribedAppSubscriptionStatusesForUserAsync(string iamUserId) =>
        _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetOwnCompanySubscribedAppSubscriptionStatusesUntrackedAsync(iamUserId);

    /// <inheritdoc/>
    public IAsyncEnumerable<AppCompanySubscriptionStatusData> GetCompanyProvidedAppSubscriptionStatusesForUserAsync(string iamUserId) =>
        _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetOwnCompanyProvidedAppSubscriptionStatusesUntrackedAsync(iamUserId);

    /// <inheritdoc/>
    public async Task AddOwnCompanyAppSubscriptionAsync(Guid appId, string iamUserId)
    {
        var appDetails = await _portalRepositories.GetInstance<IOfferRepository>().GetOfferProviderDetailsAsync(appId, OfferTypeId.APP).ConfigureAwait(false);
        if (appDetails == null)
        {
            throw new NotFoundException($"App {appId} does not exist");
        }
        
        var (requesterId, requesterEmail) = await _portalRepositories.GetInstance<IUserRepository>()
            .GetCompanyUserIdAndEmailForIamUserUntrackedAsync(iamUserId).ConfigureAwait(false);
        var companyName = await GetOrCreateCompanyAppSubscriptionData(appId, iamUserId, requesterId);

        if(appDetails.AppName is null || appDetails.ProviderContactEmail is null)
        {
            var nullProperties = new List<string>();
            if (appDetails.AppName is null)
            {
                nullProperties.Add($"{nameof(Offer)}.{nameof(appDetails.AppName)}");
            }
            if(appDetails.ProviderContactEmail is null)
            {
                nullProperties.Add($"{nameof(Offer)}.{nameof(appDetails.ProviderContactEmail)}");
            }
            throw new UnexpectedConditionException($"The following fields of app '{appId}' have not been configured properly: {string.Join(", ", nullProperties)}");
        }

        if (appDetails.SalesManagerId.HasValue)
        {
            var notificationContent = new
            {
                appDetails.AppName,
                RequestorCompanyName = companyName,
                UserEmail = requesterEmail,
            };
            _portalRepositories.GetInstance<INotificationRepository>().CreateNotification(appDetails.SalesManagerId.Value, NotificationTypeId.APP_SUBSCRIPTION_REQUEST, false,
                notification =>
                {
                    notification.CreatorUserId = requesterId;
                    notification.Content = JsonSerializer.Serialize(notificationContent);
                });
        }
        
        await _portalRepositories.SaveAsync().ConfigureAwait(false);

        var mailParams = new Dictionary<string, string>
            {
                { "appProviderName", appDetails.ProviderName},
                { "appName", appDetails.AppName },
                { "url", _settings.BasePortalAddress },
            };
        await _mailingService.SendMails(appDetails.ProviderContactEmail, mailParams, new List<string> { "subscription-request" }).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task ActivateOwnCompanyProvidedAppSubscriptionAsync(Guid appId, Guid subscribingCompanyId, string iamUserId)
    {
        var assignedAppData = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>().GetCompanyAssignedAppDataForProvidingCompanyUserAsync(appId, subscribingCompanyId, iamUserId).ConfigureAwait(false);
        if(assignedAppData == default)
        {
            throw new NotFoundException($"App {appId} does not exist.");
        }

        var (subscription, isMemberOfCompanyProvidingApp, appName, companyUserId) = assignedAppData;
        if(!isMemberOfCompanyProvidingApp)
        {
            throw new ArgumentException("Missing permission: The user's company does not provide the requested app so they cannot activate it.");
        }

        if (subscription is not { OfferSubscriptionStatusId: OfferSubscriptionStatusId.PENDING })
        {
            throw new ArgumentException("No pending subscription for provided parameters existing.");
        }
        subscription.OfferSubscriptionStatusId = OfferSubscriptionStatusId.ACTIVE;

        _portalRepositories.GetInstance<INotificationRepository>().CreateNotification(subscription.RequesterId,
            NotificationTypeId.APP_SUBSCRIPTION_ACTIVATION, false,
            notification =>
            {
                notification.CreatorUserId = companyUserId;
                notification.Content = JsonSerializer.Serialize(new
                {
                    AppName = appName
                });
            });
        
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UnsubscribeOwnCompanyAppSubscriptionAsync(Guid appId, string iamUserId)
    {
        var assignedAppData = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>().GetCompanyAssignedAppDataForCompanyUserAsync(appId, iamUserId).ConfigureAwait(false);

        if(assignedAppData == default)
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
    public async Task<Guid> CreateAppAsync(AppInputModel appInputModel)
    {
        // Add app to db
        var appRepository = _portalRepositories.GetInstance<IOfferRepository>();

        var appId = appRepository.CreateOffer(appInputModel.Provider, OfferTypeId.APP, app =>
        {
            app.Name = appInputModel.Title;
            app.MarketingUrl = appInputModel.ProviderUri;
            app.ThumbnailUrl = appInputModel.LeadPictureUri;
            app.ContactEmail = appInputModel.ContactEmail;
            app.ContactNumber = appInputModel.ContactNumber;
            app.ProviderCompanyId = appInputModel.ProviderCompanyId;
            app.OfferStatusId = OfferStatusId.CREATED;
            app.SalesManagerId = appInputModel.SalesManagerId;
        }).Id;

        var licenseId = appRepository.CreateOfferLicenses(appInputModel.Price).Id;
        appRepository.CreateOfferAssignedLicense(appId, licenseId);
        appRepository.AddAppAssignedUseCases(appInputModel.UseCaseIds.Select(uc =>
            new ValueTuple<Guid, Guid>(appId, uc)));
        appRepository.AddOfferDescriptions(appInputModel.Descriptions.Select(d =>
            new ValueTuple<Guid, string, string, string>(appId, d.LanguageCode, d.LongDescription, d.ShortDescription)));
        appRepository.AddAppLanguages(appInputModel.SupportedLanguageCodes.Select(c =>
            new ValueTuple<Guid, string>(appId, c)));

        await _portalRepositories.SaveAsync().ConfigureAwait(false);

        return appId;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<AllAppData> GetCompanyProvidedAppsDataForUserAsync(string userId)=>
        _portalRepositories.GetInstance<IOfferRepository>().GetProvidedAppsData(userId);
    
    /// <inheritdoc/>
    public  Task<Guid> AddAppAsync(AppRequestModel appRequestModel)
    {
        if(appRequestModel.ProviderCompanyId == Guid.Empty)
        {
            throw new ArgumentException($"Company Id  does not exist"); 
        }

        var languageCodes = appRequestModel.SupportedLanguageCodes.Where(item => !String.IsNullOrWhiteSpace(item)).Distinct();
        if (!languageCodes.Any())
        {
            throw new ArgumentException($"Language Code does not exist"); 
        }
        
        var useCaseIds = appRequestModel.UseCaseIds.Where(item => !string.IsNullOrWhiteSpace(item)).Distinct().ToList();
        if (!useCaseIds.Any())
        {
            throw new ArgumentException($"Use Case does not exist");
        }
        
        if (useCaseIds.Any(item => !Guid.TryParse(item, out _)))
        {
            throw new ArgumentException($"Use Case does not exist");
        }
        return  this.CreateAppAsync(appRequestModel);
    }
    
    private async Task<Guid> CreateAppAsync(AppRequestModel appRequestModel)
    {   
        // Add app to db
        var appRepository = _portalRepositories.GetInstance<IOfferRepository>();
        var appId = appRepository.CreateOffer(appRequestModel.Provider, OfferTypeId.APP, app =>
        {
            app.Name = appRequestModel.Title;
            app.ThumbnailUrl = appRequestModel.LeadPictureUri;
            app.ProviderCompanyId = appRequestModel.ProviderCompanyId;
            app.OfferStatusId = OfferStatusId.CREATED;
        }).Id;

        appRepository.AddOfferDescriptions(appRequestModel.Descriptions.Select(d =>
              (appId, d.LanguageCode, d.LongDescription, d.ShortDescription)));
        appRepository.AddAppLanguages(appRequestModel.SupportedLanguageCodes.Select(c =>
              (appId, c)));
        appRepository.AddAppAssignedUseCases(appRequestModel.UseCaseIds.Select(uc =>
              (appId, Guid.Parse(uc))));
        var licenseId = appRepository.CreateOfferLicenses(appRequestModel.Price).Id;
        appRepository.CreateOfferAssignedLicense(appId, licenseId);

        try
        {
            await _portalRepositories.SaveAsync().ConfigureAwait(false);
            return appId;
        }
        catch(Exception exception)when (exception?.InnerException?.Message.Contains("violates foreign key constraint") ?? false)
        {
            throw new NotFoundException($"language code or UseCaseId does not exist");
        }
    }
    
    private async Task<string> GetOrCreateCompanyAppSubscriptionData(Guid appId, string iamUserId, Guid requesterId)
    {
        var companyAssignedAppRepository = _portalRepositories.GetInstance<IOfferSubscriptionsRepository>();
        var companyAppSubscriptionData = await companyAssignedAppRepository
            .GetCompanyIdWithAssignedOfferForCompanyUserAsync(appId, iamUserId, OfferTypeId.APP).ConfigureAwait(false);
        if (companyAppSubscriptionData == default)
        {
            throw new ControllerArgumentException($"user {iamUserId} is not assigned with a company");
        }

        var (companyId, offerSubscription, companyName, companyUserId) = companyAppSubscriptionData;
        if (offerSubscription == null)
        {
            companyAssignedAppRepository.CreateOfferSubscription(appId, companyId, OfferSubscriptionStatusId.PENDING,
                requesterId, companyUserId);
        }
        else
        {
            if (offerSubscription.OfferSubscriptionStatusId is OfferSubscriptionStatusId.ACTIVE
                or OfferSubscriptionStatusId.PENDING)
            {
                throw new ConflictException($"company {companyId} is already subscribed to {appId}");
            }

            offerSubscription.OfferSubscriptionStatusId = OfferSubscriptionStatusId.PENDING;
        }

        return companyName;
    }
    
    /// <inheritdoc/>
    public async Task SubmitAppReleaseRequestAsync(Guid appId, string iamUserId)
    {
        var appDetails = await _portalRepositories.GetInstance<IOfferRepository>().GetOfferReleaseDataByIdAsync(appId).ConfigureAwait(false);
        if (appDetails == null)
        {
            throw new NotFoundException($"App {appId} does not exist");
        }
        
        var requesterId = await _portalRepositories.GetInstance<IUserRepository>()
            .GetCompanyUserIdForIamUserUntrackedAsync(iamUserId).ConfigureAwait(false);

        if(appDetails.name is null || appDetails.thumbnailUrl is null 
            || appDetails.salesManagerId is null || appDetails.providerCompanyId is null
            || appDetails.descriptionLongIsNullOrEmpty || appDetails.descriptionShortIsNullOrEmpty)
        {
            var nullProperties = new List<string>();
            if (appDetails.name is null)
            {
                nullProperties.Add($"{nameof(Offer)}.{nameof(appDetails.name)}");
            }
            if(appDetails.thumbnailUrl is null)
            {
                nullProperties.Add($"{nameof(Offer)}.{nameof(appDetails.thumbnailUrl)}");
            }
            if(appDetails.salesManagerId is null)
            {
                nullProperties.Add($"{nameof(Offer)}.{nameof(appDetails.salesManagerId)}");
            }
            if(appDetails.providerCompanyId is null)
            {
                nullProperties.Add($"{nameof(Offer)}.{nameof(appDetails.providerCompanyId)}");
            }
            if(appDetails.descriptionLongIsNullOrEmpty)
            {
                nullProperties.Add($"{nameof(Offer)}.{nameof(appDetails.descriptionLongIsNullOrEmpty)}");
            }
            if(appDetails.descriptionShortIsNullOrEmpty)
            {
                nullProperties.Add($"{nameof(Offer)}.{nameof(appDetails.descriptionShortIsNullOrEmpty)}");
            }
            throw new ConflictException($"Missing  : {string.Join(", ", nullProperties)}");
        }
        _portalRepositories.Attach(new Offer(appId), app =>
        {
            app.OfferStatusId = OfferStatusId.IN_REVIEW;
            app.DateLastChanged = DateTimeOffset.UtcNow;
        });

        var notificationContent = new
        {
            appId,
            RequestorCompanyName = appDetails.companyName
        };
        
        var serializeNotificationContent = JsonSerializer.Serialize(notificationContent);
        var content = _settings.NotificationTypeIds.Select(typeId => new ValueTuple<string?, NotificationTypeId>(serializeNotificationContent, typeId));
        await _notificationService.CreateNotifications(_settings.CompanyAdminRoles, requesterId, content).ConfigureAwait(false);
    }

     /// <inheritdoc/>
    public Task<Pagination.Response<InReviewAppData>> GetAllInReviewStatusAppsAsync(int page = 0, int size = 15)
    {
        var apps = _portalRepositories.GetInstance<IOfferRepository>().GetAllInReviewStatusAppsAsync();

        return Pagination.CreateResponseAsync(
            page,
            size,
            15,
            (int skip, int take) => new Pagination.AsyncSource<InReviewAppData>(
                apps.CountAsync(),
                apps.OrderBy(app => app.Id)
                    .Skip(skip)
                    .Take(take)
                    .Select(app => new InReviewAppData(
                        app.Id,
                        app.Name,
                        app.ProviderCompany!.Name,
                        app.ThumbnailUrl))
                    .AsAsyncEnumerable()));
    }
     
    /// <inheritdoc />
    public Task<OfferAutoSetupResponseData> AutoSetupAppAsync(OfferAutoSetupData data, string iamUserId) =>
        _offerService.AutoSetupServiceAsync(data, _settings.ServiceAccountRoles, _settings.CompanyAdminRoles, iamUserId, OfferTypeId.APP);
}
