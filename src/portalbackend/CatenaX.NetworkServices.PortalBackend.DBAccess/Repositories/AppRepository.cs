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

using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// Implementation of <see cref="IAppRepository"/> accessing database with EF Core.
public class AppRepository : IAppRepository
{
    private const string DEFAULT_LANGUAGE = "en";
    private readonly PortalDbContext _context;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalDbContext">PortalDb context.</param>
    public AppRepository(PortalDbContext portalDbContext)
    {
        this._context = portalDbContext;
    }

    /// <inheritdoc />
    public Task<bool> CheckAppExistsById(Guid appId) => 
        _context.Apps.AnyAsync(x => x.Id == appId);

    ///<inheritdoc/>
    public Task<AppProviderDetailsData?> GetAppProviderDetailsAsync(Guid appId) =>
        _context.Apps.AsNoTracking().Where(a => a.Id == appId).Select(c => new AppProviderDetailsData(
            c.Name,
            c.Provider,
            c.ContactEmail,
            c.SalesManagerId
        )).SingleOrDefaultAsync();

    /// <inheritdoc/>
    public Task<string?> GetAppAssignedClientIdUntrackedAsync(Guid appId, Guid companyId) =>
        _context.CompanyAssignedApps.AsNoTracking()
            .Where(appClient => appClient.Id == appId && appClient.CompanyId == companyId)
            .Select(x => x.AppInstance!.IamClient!.ClientClientId)
            .SingleOrDefaultAsync();
    
    /// <inheritdoc />
    public App CreateApp(string provider, Action<App>? setOptionalParameters = null)
    {
        var app = _context.Apps.Add(new App(Guid.NewGuid(), provider, DateTimeOffset.UtcNow)).Entity;
        setOptionalParameters?.Invoke(app);
        return app;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<AppData> GetAllActiveAppsAsync(string? languageShortName) =>
        _context.Apps.AsNoTracking()
            .Where(app => app.DateReleased.HasValue && app.DateReleased <= DateTime.UtcNow)
            .Select(a => new {
                a.Id,
                a.Name,
                VendorCompanyName = a.ProviderCompany!.Name, // This translates into a 'left join' which does return null for all columns if the foreingn key is null. The '!' just makes the compiler happy
                UseCaseNames = a.UseCases.Select(uc => uc.Name),
                a.ThumbnailUrl,
                ShortDescription =
                    _context.Languages.SingleOrDefault(l => l.ShortName == languageShortName) == null 
                        ? null 
                        : a.AppDescriptions.SingleOrDefault(d => d.LanguageShortName == languageShortName)!.DescriptionShort
                          ?? a.AppDescriptions.SingleOrDefault(d => d.LanguageShortName == Constants.DefaultLanguage)!.DescriptionShort,
                LicenseText = a.AppLicenses
                    .Select(license => license.Licensetext)
                    .FirstOrDefault()
            }).AsAsyncEnumerable().Select(app => new AppData(
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

    /// <inheritdoc />
    public async Task<AppDetailsData> GetAppDetailsByIdAsync(Guid appId, string iamUserId, string? languageShortName)
    {
       var app = await _context.Apps.AsNoTracking()
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
                    _context.Languages.SingleOrDefault(l => l.ShortName == languageShortName) == null
                    ? null
                    : a.AppDescriptions.SingleOrDefault(d => d.LanguageShortName == languageShortName)!.DescriptionLong
                      ?? a.AppDescriptions.SingleOrDefault(d => d.LanguageShortName == Constants.DefaultLanguage)!.DescriptionLong,
                Price = a.AppLicenses
                    .Select(license => license.Licensetext)
                    .FirstOrDefault(),
                Tags = a.Tags.Select(t => t.Name),
                IsPurchased = a.Companies.Where(c => c.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId))
                    .SelectMany(company => company.CompanyAssignedApps.Where(x => x.AppId == appId))
                    .Select(x => x.AppSubscriptionStatusId)
                    .FirstOrDefault(),
                Languages = a.SupportedLanguages.Select(l => l.ShortName)
            })
            .SingleAsync().ConfigureAwait(false);

        return new AppDetailsData(
            app.Title ?? Constants.ErrorString,
            app.LeadPictureUri ?? Constants.ErrorString,
            app.ProviderUri ?? Constants.ErrorString,
            app.Provider,
            app.LongDescription ?? Constants.ErrorString,
            app.Price ?? Constants.ErrorString
            )
        {
            Id = app.Id,
            IsSubscribed = app.IsPurchased,
            Tags = app.Tags,
            UseCases = app.UseCases,
            DetailPictureUris = app.DetailPictureUris,
            ContactEmail = app.ContactEmail,
            ContactNumber = app.ContactNumber,
            Languages = app.Languages
        };
    }

    /// <inheritdoc />
    public AppLicense CreateAppLicenses(string licenseText) =>
        _context.AppLicenses.Add(new AppLicense(Guid.NewGuid(), licenseText)).Entity;

    /// <inheritdoc />
    public AppAssignedLicense CreateAppAssignedLicense(Guid appId, Guid appLicenseId) =>
        _context.AppAssignedLicenses.Add(new AppAssignedLicense(appId, appLicenseId)).Entity;

    /// <inheritdoc />
    public CompanyUserAssignedAppFavourite CreateAppFavourite(Guid appId, Guid companyUserId) =>
        _context.CompanyUserAssignedAppFavourites.Add(new CompanyUserAssignedAppFavourite(appId, companyUserId)).Entity;

    /// <inheritdoc />
    public void AddAppAssignedUseCases(IEnumerable<(Guid appId, Guid useCaseId)> appUseCases) =>
        _context.AppAssignedUseCases.AddRange(appUseCases.Select(s => new AppAssignedUseCase(s.appId, s.useCaseId)));

    /// <inheritdoc />
    public void AddAppDescriptions(IEnumerable<(Guid appId, string languageShortName, string descriptionLong, string descriptionShort)> appDescriptions) =>
        _context.AppDescriptions.AddRange(appDescriptions.Select(s => new AppDescription(s.appId, s.languageShortName, s.descriptionLong, s.descriptionShort)));

    /// <inheritdoc />
    public void AddAppLanguages(IEnumerable<(Guid appId, string languageShortName)> appLanguages) =>
        _context.AppLanguages.AddRange(appLanguages.Select(s => new AppLanguage(s.appId, s.languageShortName)));

    /// <inheritdoc />
    public IAsyncEnumerable<AllAppData> GetProvidedAppsData(string iamUserId) =>
        _context.Apps
            .AsNoTracking()
            .Where(app=>app.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId))
            .Select(app => new AllAppData(
                app.Id,
                app.Name,
                app.ThumbnailUrl,
                app.Provider,
                app.AppStatusId.ToString(),
                app.DateLastChanged
            ))
            .AsAsyncEnumerable();

     /// <inheritdoc />
    public  Task<(IEnumerable<AppDescription> descriptions, IEnumerable<AppDetailImage> images)> GetAppByIdAsync(Guid appId, string userId)
    =>
        _context.Apps
             .Where(a => a.Id == appId && a.AppStatusId == AppStatusId.CREATED
             && a.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == userId))
             .Select(a => new ValueTuple<IEnumerable<AppDescription>, IEnumerable<AppDetailImage>>(
                a.AppDescriptions.Select(d => new AppDescription(appId,d.LanguageShortName,d.DescriptionLong,d.DescriptionShort)),
                       a.AppDetailImages.Select(adi => new AppDetailImage(appId,adi.ImageUrl))
                       ))
             .SingleOrDefaultAsync();
       
    
}
