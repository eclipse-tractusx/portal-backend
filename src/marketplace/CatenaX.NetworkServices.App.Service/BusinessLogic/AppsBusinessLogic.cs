/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using CatenaX.NetworkServices.App.Service.InputModels;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.App.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IAppsBusinessLogic"/>.
/// </summary>
public class AppsBusinessLogic : IAppsBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IMailingService _mailingService;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="mailingService">Mail service.</param>
    public AppsBusinessLogic(IPortalRepositories portalRepositories, IMailingService mailingService)
    {
        _portalRepositories = portalRepositories;
        _mailingService = mailingService;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<AppData> GetAllActiveAppsAsync(string? languageShortName = null) =>
        _portalRepositories.GetInstance<IAppRepository>().GetAllActiveAppsAsync(languageShortName);

    /// <inheritdoc/>
    public IAsyncEnumerable<BusinessAppData> GetAllUserUserBusinessAppsAsync(string userId) =>
        _portalRepositories.GetInstance<IUserRepository>().GetAllBusinessAppDataForUserIdAsync(userId);

    /// <inheritdoc/>
    public async Task<AppDetailsData> GetAppDetailsByIdAsync(Guid appId, string? userId = null, string? languageShortName = null)
    {
        var companyId = userId == null ?
            (Guid?)null :
            await _portalRepositories.GetInstance<IUserRepository>().GetCompanyIdForIamUserUntrackedAsync(userId).ConfigureAwait(false);
        return await _portalRepositories.GetInstance<IAppRepository>()
            .GetDetailsByIdAsync(appId, companyId, languageShortName)
            .ConfigureAwait(false);
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
            _portalRepositories.GetInstance<IAppRepository>().CreateAppFavourite(appId, companyUserId);
            await _portalRepositories.SaveAsync().ConfigureAwait(false);
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException($"Parameters are invalid or app is already favourited.");
        }
    }

    /// <inheritdoc/>
    public async Task<IAsyncEnumerable<(Guid AppId, AppSubscriptionStatusId AppSubscriptionStatus)>> GetCompanySubscribedAppSubscriptionStatusesForUserAsync(string iamUserId)
    {
        var companyId = await _portalRepositories.GetInstance<IUserRepository>().GetCompanyIdForIamUserUntrackedAsync(iamUserId).ConfigureAwait(false);
        return _portalRepositories
            .GetInstance<ICompanyAssignedAppsRepository>()
            .GetCompanySubscribedAppSubscriptionStatusesForCompanyUntrackedAsync(companyId);
    }

    /// <inheritdoc/>
    public async Task<IAsyncEnumerable<AppCompanySubscriptionStatusData>> GetCompanyProvidedAppSubscriptionStatusesForUserAsync(string iamUserId)
    {
        var companyId = await _portalRepositories.GetInstance<IUserRepository>().GetCompanyIdForIamUserUntrackedAsync(iamUserId).ConfigureAwait(false);
        return _portalRepositories.GetInstance<ICompanyAssignedAppsRepository>()
            .GetCompanyProvidedAppSubscriptionStatusesForUserAsync(companyId);
    }

    /// <inheritdoc/>
    public async Task AddCompanyAppSubscriptionAsync(Guid appId, string userId)
    {
        try
        {
            var companyId = await _portalRepositories.GetInstance<IUserRepository>().GetCompanyIdForIamUserUntrackedAsync(userId).ConfigureAwait(false);
            var companyAssignedApp = _portalRepositories.GetInstance<ICompanyAssignedAppsRepository>().CreateCompanyAssignedApp(appId, companyId);
            companyAssignedApp.AppSubscriptionStatusId = AppSubscriptionStatusId.PENDING;

            await _portalRepositories.SaveAsync().ConfigureAwait(false);
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException("Parameters are invalid or app is already subscribed to.");
        }

        var appDetails = await _portalRepositories.GetInstance<IAppRepository>().GetAppProviderDetailsAsync(appId).ConfigureAwait(false);
        var mailParams = new Dictionary<string, string>
            {
                { "appProviderName", appDetails.providerName},
                { "appName", appDetails.appName }
            };
        await _mailingService.SendMails(appDetails.providerContactEmail, mailParams, new List<string> { "subscription-request" }).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task ActivateCompanyAppSubscriptionAsync(Guid appId, Guid subscribingCompanyId, string userId)
    {

        var isExistingApp = await _portalRepositories.GetInstance<IAppRepository>().CheckAppExistsById(appId).ConfigureAwait(false);
        if(!isExistingApp)
        {
            throw new NotFoundException($"App {appId} does not exist.");
        }

        var companyId = await _portalRepositories.GetInstance<IUserRepository>().GetCompanyIdForIamUserUntrackedAsync(userId).ConfigureAwait(false);

        var isMemberOfCompanyProvidingApp = await _portalRepositories
            .GetInstance<ICompanyRepository>()
            .CheckIsMemberOfCompanyProvidingAppUntrackedAsync(companyId, appId)
            .ConfigureAwait(false);

        if(!isMemberOfCompanyProvidingApp)
        {
            throw new ArgumentException("Missing permission: The user's company does not provide the requested app so they cannot activate it.");
        }

        var subscription = await _portalRepositories.GetInstance<ICompanyAssignedAppsRepository>().FindAsync(companyId, appId).ConfigureAwait(false);
        if (subscription is null || subscription.AppSubscriptionStatusId != PortalBackend.PortalEntities.Enums.AppSubscriptionStatusId.PENDING)
        {
            throw new ArgumentException("No pending subscription for provided parameters existing.");
        }
        subscription.AppSubscriptionStatusId = PortalBackend.PortalEntities.Enums.AppSubscriptionStatusId.ACTIVE;
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UnsubscribeCompanyAppSubscriptionAsync(Guid appId, string userId)
    {
        var companyId = await _portalRepositories.GetInstance<IUserRepository>().GetCompanyIdForIamUserUntrackedAsync(userId).ConfigureAwait(false);
        if (!await _portalRepositories.GetInstance<IAppRepository>().CheckAppExistsById(appId).ConfigureAwait(false))
        {
            throw new NotFoundException($"App '{appId}' does not exist.");
        }

        var subscription = await _portalRepositories.GetInstance<ICompanyAssignedAppsRepository>().FindAsync(companyId, appId).ConfigureAwait(false);
        if (subscription is null)
        {
            throw new ArgumentException($"There is no active subscription for company '{companyId}' and app '{appId}'");
        }
        subscription.AppSubscriptionStatusId = AppSubscriptionStatusId.INACTIVE;
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Guid> CreateAppAsync(AppInputModel appInputModel)
    {
        // Add app to db
        var appRepository = _portalRepositories.GetInstance<IAppRepository>();

        var appEntity = appRepository.CreateApp(Guid.NewGuid(), appInputModel.Provider);

        appEntity.Name = appInputModel.Title;
        appEntity.MarketingUrl = appInputModel.ProviderUri;
        appEntity.AppUrl = appInputModel.AppUri;
        appEntity.ThumbnailUrl = appInputModel.LeadPictureUri;
        appEntity.ContactEmail = appInputModel.ContactEmail;
        appEntity.ContactNumber = appInputModel.ContactNumber;
        appEntity.ProviderCompanyId = appInputModel.ProviderCompanyId;
        appEntity.AppStatusId = AppStatusId.CREATED;

        var appLicense = appRepository.CreateAppLicenses(appInputModel.Price);
        appRepository.CreateAppAssignedLicense(appEntity.Id, appLicense.Id);
        appRepository.AddUseCases(appInputModel.UseCaseIds.Select(uc => new AppAssignedUseCase(appEntity.Id, uc)));
        appRepository.AddAppDescriptions(appInputModel.Descriptions.Select(d => new AppDescription(appEntity.Id, d.LanguageCode, d.LongDescription, d.ShortDescription)));
        appRepository.AddAppLanguages(appInputModel.SupportedLanguageCodes.Select(c => new AppLanguage(appEntity.Id, c)));

        await _portalRepositories.SaveAsync().ConfigureAwait(false);

        return appEntity.Id;
    }
}
