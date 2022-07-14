﻿/********************************************************************************
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
using CatenaX.NetworkServices.App.Service.ViewModels;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.App.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IAppsBusinessLogic"/>.
/// </summary>
public class AppsBusinessLogic : IAppsBusinessLogic
{
    private const string ERROR_STRING = "ERROR";
    private const string DEFAULT_LANGUAGE = "en";
    private readonly PortalDbContext _context;
    private readonly ICompanyAssignedAppsRepository _companyAssignedAppsRepository;
    private readonly IAppRepository _appRepository;
    private readonly IMailingService _mailingService;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="context">Database context dependency.</param>
    /// <param name="companyAssignedAppsRepository">Repository to access the CompanyAssignedApps on the persistence layer.</param>
    /// <param name="appRepository">Repository to access the apps on the persistence layer.</param>
    /// <param name="mailingService">Mail service.</param>
    public AppsBusinessLogic(PortalDbContext context, ICompanyAssignedAppsRepository companyAssignedAppsRepository, IAppRepository appRepository, IMailingService mailingService)
    {
        this._context = context;
        this._companyAssignedAppsRepository = companyAssignedAppsRepository;
        this._appRepository = appRepository;
        this._mailingService = mailingService;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<AppViewModel> GetAllActiveAppsAsync(string? languageShortName = null)
    {
        await foreach(var app in _context.Apps.AsNoTracking()
            .Where(app => app.DateReleased.HasValue && app.DateReleased <= DateTime.UtcNow)
            .Select(a => new {
                a.Id,
                Name = (string?)a.Name,
                VendorCompanyName = a.ProviderCompany.Name, // This translates into a 'left join' which does return null for all columns if the foreingn key is null. The '!' just makes the compiler happy
                UseCaseNames = a.UseCases.Select(uc => uc.Name),
                ThumbnailUrl = (string?)a.ThumbnailUrl,
                ShortDescription =
                    this._context.Languages.SingleOrDefault(l => l.ShortName == languageShortName) == null
                    ? null 
                    : a.AppDescriptions.SingleOrDefault(d => d.LanguageShortName == languageShortName)!.DescriptionShort
                      ?? a.AppDescriptions.SingleOrDefault(d => d.LanguageShortName == DEFAULT_LANGUAGE)!.DescriptionShort,
                LicenseText = a.AppLicenses
                    .Select(license => license.Licensetext)
                    .FirstOrDefault()
            }).AsAsyncEnumerable())
            {
                yield return new AppViewModel(
                    app.Name ?? ERROR_STRING,
                    app.ShortDescription ?? ERROR_STRING,
                    app.VendorCompanyName ?? ERROR_STRING,
                    app.LicenseText ?? ERROR_STRING,
                    app.ThumbnailUrl ?? ERROR_STRING) 
                {
                    Id = app.Id,
                    UseCases = app.UseCaseNames.Select(name => name).ToList()
                };
            }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<BusinessAppViewModel> GetAllUserUserBusinessAppsAsync(string userId)
    {
        await foreach (var app in _context.IamUsers.AsNoTracking().Where(u => u.UserEntityId == userId)
            .SelectMany(u => u.CompanyUser!.Company!.BoughtApps)
            .Intersect(
                _context.IamUsers.AsNoTracking().Where(u => u.UserEntityId == userId)
                .SelectMany(u => u.CompanyUser!.UserRoles.SelectMany(r => r.IamClient!.Apps))
            )
            .Select( a => new
            {
                a.Id,
                a.Name,
                a.AppUrl,
                a.ThumbnailUrl,
                a.Provider
            }).AsAsyncEnumerable())
        {
            yield return new BusinessAppViewModel(
                app.Name ?? ERROR_STRING, 
                app.AppUrl ?? ERROR_STRING, 
                app.ThumbnailUrl ?? ERROR_STRING, 
                app.Provider
            )
            {
                Id = app.Id
            };
        }
    }

    /// <inheritdoc/>
    public async Task<AppDetailsViewModel> GetAppDetailsByIdAsync(Guid appId, string userId, string? languageShortName = null)
    {
        var companyId = await GetCompanyIdByIamUserIdAsync(userId).ConfigureAwait(false);

        if (!await this._companyAssignedAppsRepository.ExistsByAppAndCompanyIdAsync(appId, companyId).ConfigureAwait(false))
        {
            throw new ArgumentException("Company Assigned App does not exists");
        }

        var app = await this._context.Apps.AsNoTracking()
            .Where(a => a.Id == appId)
            .Select(a => new
            {
                a.Id,
                Title = a.Name,
                LeadPictureUri = a.ThumbnailUrl,
                DetailPictureUris = a.AppDetailImages.Select(adi => adi.ImageUrl),
                ProviderUri = a.MarketingUrl,
                a.Provider,
                a.ContactEmail,
                a.ContactNumber,
                UseCases = a.UseCases.Select(u => u.Name),
                LongDescription = 
                    this._context.Languages.SingleOrDefault(l => l.ShortName == languageShortName) == null
                    ? null 
                    : a.AppDescriptions.SingleOrDefault(d => d.LanguageShortName == languageShortName)!.DescriptionLong
                      ?? a.AppDescriptions.SingleOrDefault(d => d.LanguageShortName == DEFAULT_LANGUAGE)!.DescriptionLong,
                Price = a.AppLicenses
                    .Select(license => license.Licensetext)
                    .FirstOrDefault(),
                Tags = a.Tags.Select(t => t.Name),
                IsSubscribed = this._context.CompanyAssignedApps.First(x => x.CompanyId == companyId).AppSubscriptionStatusId,
                Languages = a.SupportedLanguages.Select(l => l.ShortName)
            })
            .SingleAsync().ConfigureAwait(false);

        return new AppDetailsViewModel(
            app.Title ?? ERROR_STRING,
            app.LeadPictureUri ?? ERROR_STRING,
            app.ProviderUri ?? ERROR_STRING,
            app.Provider,
            app.LongDescription ?? ERROR_STRING,
            app.Price ?? ERROR_STRING
            )
        {
            Id = app.Id,
            IsSubscribed = app.IsSubscribed,
            Tags = app.Tags,
            UseCases = app.UseCases,
            DetailPictureUris = app.DetailPictureUris,
            ContactEmail = app.ContactEmail,
            ContactNumber = app.ContactNumber,
            Languages = app.Languages
        };
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<Guid> GetAllFavouriteAppsForUserAsync(string userId)
    {
        return this._context.IamUsers.AsNoTracking()
            .Where(u => u.UserEntityId == userId) // Id is unique, so single user
            .SelectMany(u => u.CompanyUser!.Apps.Select(a => a.Id))
            .ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public async Task RemoveFavouriteAppForUserAsync(Guid appId, string userId)
    {
        try
        {
            var companyUserId = await GetCompanyUserIdbyIamUserIdAsync(userId).ConfigureAwait(false);
            var rowToRemove = new CompanyUserAssignedAppFavourite(appId, companyUserId);
            this._context.CompanyUserAssignedAppFavourites.Attach(rowToRemove);
            this._context.CompanyUserAssignedAppFavourites.Remove(rowToRemove);
            await this._context.SaveChangesAsync().ConfigureAwait(false);
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
            this._context.CompanyUserAssignedAppFavourites.Add(
                new CompanyUserAssignedAppFavourite(appId, companyUserId)
            );
            await this._context.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException($"Parameters are invalid or app is already favourited.");
        }

    }

    /// <inheritdoc/>
    public async Task<IAsyncEnumerable<AppSubscriptionStatusViewModel>> GetCompanySubscribedAppSubscriptionStatusesForUserAsync(string iamUserId)
    {
        var companyId = await GetCompanyIdByIamUserIdAsync(iamUserId);
        return _context.CompanyAssignedApps.AsNoTracking()
            .Where(s => s.CompanyId == companyId)
            .Select(s => new AppSubscriptionStatusViewModel { AppId = s.AppId, AppSubscriptionStatus = s.AppSubscriptionStatusId})
            .ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public async Task<IAsyncEnumerable<AppCompanySubscriptionStatusViewModel>> GetCompanyProvidedAppSubscriptionStatusesForUserAsync(string iamUserId)
    {
        var companyId = await GetCompanyIdByIamUserIdAsync(iamUserId);
        return _context.CompanyAssignedApps.AsNoTracking()
            .Where(s => s.App!.ProviderCompanyId == companyId)
            .GroupBy(s => s.AppId)
            .Select(g => new AppCompanySubscriptionStatusViewModel
            {
                AppId = g.Key,
                CompanySubscriptionStatuses = g.Select(s => new CompanySubscriptionStatusViewModel 
                { 
                    CompanyId = s.CompanyId,
                    AppSubscriptionStatus = s.AppSubscriptionStatusId
                }).ToList()
            })
            .ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public async Task AddCompanyAppSubscriptionAsync(Guid appId, string userId)
    {
        try
        {
            var companyId = await GetCompanyIdByIamUserIdAsync(userId).ConfigureAwait(false);

            this._context.CompanyAssignedApps.Add(new CompanyAssignedApp(appId, companyId, AppSubscriptionStatusId.PENDING));

            await this._context.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException("Parameters are invalid or app is already subscribed to.");
        }

        var appDetails = await _appRepository.GetAppProviderDetailsAsync(appId).ConfigureAwait(false);

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
        var isExistingApp = await this._context.Apps.AnyAsync(a => a.Id == appId).ConfigureAwait(false);
        if(!isExistingApp)
        {
            throw new NotFoundException($"App {appId} does not exist.");
        }

        var companyId = await this.GetCompanyIdByIamUserIdAsync(userId).ConfigureAwait(false);

        var isMemberOfCompanyProvidingApp = await this._context.Companies.AsNoTracking()
            .Where(c => c.Id == companyId)
            .SelectMany(c => c.ProvidedApps.Select(a => a.Id)).ContainsAsync(appId).ConfigureAwait(false);
        if(!isMemberOfCompanyProvidingApp)
        {
            throw new ArgumentException("Missing permission: The user's company does not provide the requested app so they cannot activate it.");
        }

        var subscription = await this._context.CompanyAssignedApps.FindAsync(companyId, appId).ConfigureAwait(false);
        if (subscription is null || subscription.AppSubscriptionStatusId != PortalBackend.PortalEntities.Enums.AppSubscriptionStatusId.PENDING)
        {
            throw new ArgumentException("No pending subscription for provided parameters existing.");
        }
        subscription.AppSubscriptionStatusId = PortalBackend.PortalEntities.Enums.AppSubscriptionStatusId.ACTIVE;
        await this._context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UnsubscribeCompanyAppSubscriptionAsync(Guid appId, string userId)
    {
        var companyId = await GetCompanyIdByIamUserIdAsync(userId).ConfigureAwait(false);
        var appExists = await this._appRepository.CheckAppExistsById(appId).ConfigureAwait(false);
        if (!appExists)
        {
            throw new NotFoundException($"App '{appId}' does not exist.");
        }

        await this._companyAssignedAppsRepository.UpdateSubscriptionStatusAsync(companyId, appId, AppSubscriptionStatusId.INACTIVE).ConfigureAwait(false);
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
        this._context.Apps.Add(appEntity);

        var appLicenseEntity = new AppLicense(Guid.NewGuid(), appInputModel.Price);
        this._context.AppLicenses.Add(appLicenseEntity);

        this._context.AppAssignedLicenses.Add(new AppAssignedLicense(appEntity.Id, appLicenseEntity.Id));
        this._context.AppAssignedUseCases.AddRange(appInputModel.UseCaseIds.Select(uc => new AppAssignedUseCase(appEntity.Id, uc)));
        this._context.AppDescriptions.AddRange(appInputModel.Descriptions.Select(d => new AppDescription(appEntity.Id, d.LanguageCode, d.LongDescription, d.ShortDescription)));
        this._context.AppLanguages.AddRange(appInputModel.SupportedLanguageCodes.Select(c => new AppLanguage(appEntity.Id, c)));
        await this._context.SaveChangesAsync().ConfigureAwait(false);

        return appEntity.Id;
    }

    private Task<Guid> GetCompanyUserIdbyIamUserIdAsync(string userId) => 
        this._context.CompanyUsers.AsNoTracking()
            .Where(cu => cu.IamUser!.UserEntityId == userId)
            .Select(cu => cu.Id)
            .SingleAsync();

    private Task<Guid> GetCompanyIdByIamUserIdAsync(string userId) => 
        this._context.CompanyUsers.AsNoTracking()
            .Where(cu => cu.IamUser!.UserEntityId == userId)
            .Select(cu => cu.CompanyId)
            .SingleAsync();
}
