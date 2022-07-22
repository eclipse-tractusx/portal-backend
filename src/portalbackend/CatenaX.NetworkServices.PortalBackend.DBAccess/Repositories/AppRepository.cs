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
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// Implementation of <see cref="IAppRepository"/> accessing database with EF Core.
public class AppRepository : IAppRepository
{
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
    public async Task<(string appName, string providerName, string providerContactEmail)> GetAppProviderDetailsAsync(Guid appId)
    {
        var appDetails = await _context.Apps.AsNoTracking().Where(a => a.Id == appId).Select(c => new
        {
            c.Name,
            c.Provider,
            c.ContactEmail
        }).SingleAsync();

        if(new []{ appDetails.Name, appDetails.Provider, appDetails.ContactEmail }.Any(d => d is null))
        {
            var nullProperties = new List<string>();
            if (appDetails.Name is null)
            {
                nullProperties.Add($"{nameof(App)}.{nameof(appDetails.Name)}");
            }
            if (appDetails.Provider is null)
            {
                nullProperties.Add($"{nameof(App)}.{nameof(appDetails.Provider)}");
            }
            if(appDetails.ContactEmail is null)
            {
                nullProperties.Add($"{nameof(App)}.{nameof(appDetails.ContactEmail)}");
            }
            throw new Exception($"The following fields of app '{appId}' have not been configured properly: {string.Join(", ", nullProperties)}");
        }

        return (appName: appDetails.Name!, providerName: appDetails.Provider!, providerContactEmail: appDetails.ContactEmail!);
    }

    /// <inheritdoc/>
    public Task<string?> GetAppAssignedClientIdUntrackedAsync(Guid appId) =>
        _context.AppAssignedClients.AsNoTracking()
            .Where(appClient => appClient.AppId == appId)
            .Select(appClient => appClient.IamClient!.ClientClientId)
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public App CreateApp(Guid id, string provider) =>
        _context.Apps.Add(new App(id, provider, DateTimeOffset.UtcNow)).Entity;

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
    public async Task<AppDetailsData> GetAppDetailsByIdAsync(Guid appId, string? iamUserId, string? languageShortName)
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
                IsPurchased = iamUserId == null ?
                    (bool?)null :
                    a.Companies.Any(c => c.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)),
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
        _context.AppDescriptions.AddRange(appDescriptions.Select(s => new AppDescription(s.appId, s.languageShortName, s.descriptionLong, s.descriptionLong)));

    /// <inheritdoc />
    public void AddAppLanguages(IEnumerable<(Guid appId, string languageShortName)> appLanguages) =>
        _context.AppLanguages.AddRange(appLanguages.Select(s => new AppLanguage(s.appId, s.languageShortName)));
}
