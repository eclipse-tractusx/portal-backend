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
    private readonly IMailingService mailingService;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="mailingService">Mail service.</param>
    public AppsBusinessLogic(IPortalRepositories portalRepositories, IMailingService mailingService)
    {
        _portalRepositories = portalRepositories;
        this.mailingService = mailingService;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<AppData> GetAllActiveAppsAsync(string? languageShortName = null) =>
        this._portalRepositories.GetInstance<IAppRepository>().GetAllActiveAppsAsync(languageShortName);

    /// <inheritdoc/>
    public IAsyncEnumerable<BusinessAppData> GetAllUserUserBusinessAppsAsync(string userId) =>
        this._portalRepositories.GetInstance<IUserRepository>().GetAllBusinessAppDataForUserIdAsync(userId);

    /// <inheritdoc/>
    public async Task<AppDetailsData> GetAppDetailsByIdAsync(Guid appId, string? userId = null, string? languageShortName = null)
    {
        var companyId = userId == null ?
            (Guid?)null :
            await GetCompanyIdByIamUserIdAsync(userId).ConfigureAwait(false);
        return await this._portalRepositories.GetInstance<IAppRepository>()
            .GetDetailsByIdAsync(appId, companyId, languageShortName)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<Guid> GetAllFavouriteAppsForUserAsync(string userId) =>
        this._portalRepositories
            .GetInstance<IUserRepository>()
            .GetAllFavouriteAppsForUserUntrackedAsync(userId);

    /// <inheritdoc/>
    public async Task RemoveFavouriteAppForUserAsync(Guid appId, string userId)
    {
        try
        {
            var companyUserId = await GetCompanyUserIdbyIamUserIdAsync(userId).ConfigureAwait(false);
            this._portalRepositories.GetInstance<ICompanyUserAssignedAppFavouritesRepository>().RemoveFavouriteAppForUser(appId, companyUserId);
            await this._portalRepositories.SaveAsync().ConfigureAwait(false);
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
            var companyUserId = await GetCompanyUserIdbyIamUserIdAsync(userId).ConfigureAwait(false);
            this._portalRepositories.GetInstance<ICompanyUserAssignedAppFavouritesRepository>().AddAppFavourite(new CompanyUserAssignedAppFavourite(appId, companyUserId));
            await this._portalRepositories.SaveAsync().ConfigureAwait(false);
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException($"Parameters are invalid or app is already favourited.");
        }
    }

    /// <inheritdoc/>
    public async Task<IAsyncEnumerable<(Guid AppId, AppSubscriptionStatusId AppSubscriptionStatus)>> GetCompanySubscribedAppSubscriptionStatusesForUserAsync(string iamUserId)
    {
        var companyId = await GetCompanyIdByIamUserIdAsync(iamUserId);
        return this._portalRepositories
            .GetInstance<ICompanyAssignedAppsRepository>()
            .GetCompanySubscribedAppSubscriptionStatusesForCompanyUntrackedAsync(companyId);
    }

    /// <inheritdoc/>
    public async Task<IAsyncEnumerable<AppCompanySubscriptionStatusData>> GetCompanyProvidedAppSubscriptionStatusesForUserAsync(string iamUserId)
    {
        var companyId = await GetCompanyIdByIamUserIdAsync(iamUserId);
        return this._portalRepositories.GetInstance<ICompanyAssignedAppsRepository>()
            .GetCompanyProvidedAppSubscriptionStatusesForUserAsync(companyId);
    }

    /// <inheritdoc/>
    public async Task AddCompanyAppSubscriptionAsync(Guid appId, string userId)
    {
        try
        {
            var companyId = await GetCompanyIdByIamUserIdAsync(userId).ConfigureAwait(false);
            this._portalRepositories.GetInstance<ICompanyAssignedAppsRepository>().AddCompanyAssignedApp(new CompanyAssignedApp(appId, companyId) { AppSubscriptionStatusId = AppSubscriptionStatusId.PENDING});

            await this._portalRepositories.SaveAsync().ConfigureAwait(false);
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException("Parameters are invalid or app is already subscribed to.");
        }

        var appDetails = await this._portalRepositories.GetInstance<IAppRepository>().GetAppProviderDetailsAsync(appId).ConfigureAwait(false);

        var mailParams = new Dictionary<string, string>
            {
                { "appProviderName", appDetails.providerName},
                { "appName", appDetails.appName }
            };
        await mailingService.SendMails(appDetails.providerContactEmail, mailParams, new List<string> { "subscription-request" }).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task ActivateCompanyAppSubscriptionAsync(Guid appId, Guid subscribingCompanyId, string userId)
    {

        var isExistingApp = await this._portalRepositories.GetInstance<IAppRepository>().CheckAppExistsById(appId).ConfigureAwait(false);
        if(!isExistingApp)
        {
            throw new NotFoundException($"App {appId} does not exist.");
        }

        var companyId = await this.GetCompanyIdByIamUserIdAsync(userId).ConfigureAwait(false);

        var isMemberOfCompanyProvidingApp = await this._portalRepositories
            .GetInstance<ICompanyRepository>()
            .CheckIsMemberOfCompanyProvidingAppUntrackedAsync(companyId, appId)
            .ConfigureAwait(false);

        if(!isMemberOfCompanyProvidingApp)
        {
            throw new ArgumentException("Missing permission: The user's company does not provide the requested app so they cannot activate it.");
        }

        var subscription = await this._portalRepositories.GetInstance<ICompanyAssignedAppsRepository>().FindAsync(companyId, appId).ConfigureAwait(false);
        if (subscription is null || subscription.AppSubscriptionStatusId != PortalBackend.PortalEntities.Enums.AppSubscriptionStatusId.PENDING)
        {
            throw new ArgumentException("No pending subscription for provided parameters existing.");
        }
        subscription.AppSubscriptionStatusId = PortalBackend.PortalEntities.Enums.AppSubscriptionStatusId.ACTIVE;
        await this._portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UnsubscribeCompanyAppSubscriptionAsync(Guid appId, string userId)
    {
        var companyId = await GetCompanyIdByIamUserIdAsync(userId).ConfigureAwait(false);
        var appExists = await this._portalRepositories.GetInstance<IAppRepository>().CheckAppExistsById(appId).ConfigureAwait(false);
        if (!appExists)
        {
            throw new NotFoundException($"App '{appId}' does not exist.");
        }

        await this._portalRepositories.GetInstance<ICompanyAssignedAppsRepository>().UpdateSubscriptionStatusAsync(companyId, appId, AppSubscriptionStatusId.INACTIVE).ConfigureAwait(false);
        await this._portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Guid> CreateAppAsync(AppInputModel appInputModel)
    {
        // Add app to db
        var appEntity = new PortalBackend.PortalEntities.Entities.App(Guid.NewGuid(), appInputModel.Provider, DateTimeOffset.UtcNow)
        {
            Name = appInputModel.Title,
            MarketingUrl = appInputModel.ProviderUri,
            AppUrl = appInputModel.AppUri,
            ThumbnailUrl = appInputModel.LeadPictureUri,
            ContactEmail = appInputModel.ContactEmail,
            ContactNumber = appInputModel.ContactNumber,
            ProviderCompanyId = appInputModel.ProviderCompanyId,
            AppStatusId = PortalBackend.PortalEntities.Enums.AppStatusId.CREATED
        };
        this._portalRepositories.GetInstance<IAppRepository>().AddApp(appEntity);

        var appLicenseEntity = new AppLicense(Guid.NewGuid(), appInputModel.Price);
        this._portalRepositories.GetInstance<IAppLicensesRepository>()
            .AddAppLicenses(appLicenseEntity);
        this._portalRepositories.GetInstance<IAppAssignedLicensesRepository>()
            .AddAppAssignedLicense(new AppAssignedLicense(appEntity.Id, appLicenseEntity.Id));
        this._portalRepositories.GetInstance<IAppAssignedUseCasesRepository>()
            .AddUseCases(appInputModel.UseCaseIds.Select(uc => new AppAssignedUseCase(appEntity.Id, uc)));
        this._portalRepositories.GetInstance<IAppDescriptionsRepository>()
            .AddAppDescriptions(appInputModel.Descriptions.Select(d => new AppDescription(appEntity.Id, d.LanguageCode, d.LongDescription, d.ShortDescription)));
        this._portalRepositories.GetInstance<IAppLanguagesRepository>()
            .AddAppLanguages(appInputModel.SupportedLanguageCodes.Select(c => new AppLanguage(appEntity.Id, c)));

        await this._portalRepositories.SaveAsync().ConfigureAwait(false);

        return appEntity.Id;
    }

    private Task<Guid> GetCompanyUserIdbyIamUserIdAsync(string userId) =>
        this._portalRepositories.GetInstance<IUserRepository>().GetCompanyUserIdForIamUserUntrackedAsync(userId);

    private Task<Guid> GetCompanyIdByIamUserIdAsync(string userId) => 
        this._portalRepositories.GetInstance<IUserRepository>().GetCompanyIdForIamUserUntrackedAsync(userId);
}
