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

/// Implementation of <see cref="IOfferRepository"/> accessing database with EF Core.
public class OfferRepository : IOfferRepository
{
    private const string DEFAULT_LANGUAGE = "en";
    private readonly PortalDbContext _context;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalDbContext">PortalDb context.</param>
    public OfferRepository(PortalDbContext portalDbContext)
    {
        this._context = portalDbContext;
    }

    /// <inheritdoc />
    public Task<bool> CheckAppExistsById(Guid appId) => 
        _context.Offers.AnyAsync(x => x.Id == appId && x.OfferTypeId == OfferTypeId.APP);

    ///<inheritdoc/>
    public Task<AppProviderDetailsData?> GetAppProviderDetailsAsync(Guid appId) =>
        _context.Offers.AsNoTracking().Where(a => a.Id == appId).Select(c => new AppProviderDetailsData(
            c.Name,
            c.Provider,
            c.ContactEmail,
            c.SalesManagerId
        )).SingleOrDefaultAsync();

    /// <inheritdoc/>
    public Task<string?> GetAppAssignedClientIdUntrackedAsync(Guid appId, Guid companyId) =>
        _context.OfferSubscriptions.AsNoTracking()
            .Where(appClient => appClient.Id == appId && appClient.CompanyId == companyId)
            .Select(x => x.AppSubscriptionDetail!.AppInstance!.IamClient!.ClientClientId)
            .SingleOrDefaultAsync();
    
    /// <inheritdoc />
    public Offer CreateOffer(string provider, OfferTypeId offerType, Action<Offer>? setOptionalParameters = null)
    {
        var app = _context.Offers.Add(new Offer(Guid.NewGuid(), provider, DateTimeOffset.UtcNow, offerType)).Entity;
        setOptionalParameters?.Invoke(app);
        return app;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<AppData> GetAllActiveAppsAsync(string? languageShortName) =>
        _context.Offers.AsNoTracking()
            .Where(app => app.DateReleased.HasValue && app.DateReleased <= DateTime.UtcNow && app.OfferTypeId == OfferTypeId.APP)
            .Select(a => new {
                a.Id,
                a.Name,
                VendorCompanyName = a.ProviderCompany!.Name, // This translates into a 'left join' which does return null for all columns if the foreingn key is null. The '!' just makes the compiler happy
                UseCaseNames = a.UseCases.Select(uc => uc.Name),
                a.ThumbnailUrl,
                ShortDescription =
                    _context.Languages.Any(l => l.ShortName == languageShortName)
                        ? a.OfferDescriptions.SingleOrDefault(d => d.LanguageShortName == languageShortName)!.DescriptionShort
                            ?? a.OfferDescriptions.SingleOrDefault(d => d.LanguageShortName == Constants.DefaultLanguage)!.DescriptionShort
                        : null,
                LicenseText = a.OfferLicenses
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
       var app = await _context.Offers.AsNoTracking()
            .Where(a => a.Id == appId)
            .Select(a => new
            {
                a.Id,
                Title = a.Name,
                LeadPictureUri = a.ThumbnailUrl,
                DetailPictureUris = a.OfferDetailImages.Select(adi => adi.ImageUrl),
                ProviderUri = a.MarketingUrl,
                a.Provider,
                a.ContactEmail,
                a.ContactNumber,
                UseCases = a.UseCases.Select(u => u.Name),
                LongDescription =
                    _context.Languages.Any(l => l.ShortName == languageShortName)
                    ? a.OfferDescriptions.SingleOrDefault(d => d.LanguageShortName == languageShortName)!.DescriptionLong
                        ?? a.OfferDescriptions.SingleOrDefault(d => d.LanguageShortName == Constants.DefaultLanguage)!.DescriptionLong
                    : null,
                Price = a.OfferLicenses
                    .Select(license => license.Licensetext)
                    .FirstOrDefault(),
                Tags = a.Tags.Select(t => t.Name),
                IsPurchased = a.Companies.Where(c => c.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId))
                    .SelectMany(company => company.OfferSubscriptions.Where(x => x.OfferId == appId))
                    .Select(x => x.OfferSubscriptionStatusId)
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
    public OfferLicense CreateOfferLicenses(string licenseText) =>
        _context.OfferLicenses.Add(new OfferLicense(Guid.NewGuid(), licenseText)).Entity;

    /// <inheritdoc />
    public OfferAssignedLicense CreateOfferAssignedLicense(Guid appId, Guid appLicenseId) =>
        _context.OfferAssignedLicenses.Add(new OfferAssignedLicense(appId, appLicenseId)).Entity;

    /// <inheritdoc />
    public CompanyUserAssignedAppFavourite CreateAppFavourite(Guid appId, Guid companyUserId) =>
        _context.CompanyUserAssignedAppFavourites.Add(new CompanyUserAssignedAppFavourite(appId, companyUserId)).Entity;

    /// <inheritdoc />
    public void AddAppAssignedUseCases(IEnumerable<(Guid appId, Guid useCaseId)> appUseCases) =>
        _context.AppAssignedUseCases.AddRange(appUseCases.Select(s => new AppAssignedUseCase(s.appId, s.useCaseId)));

    /// <inheritdoc />
    public void AddOfferDescriptions(IEnumerable<(Guid appId, string languageShortName, string descriptionLong, string descriptionShort)> appDescriptions) =>
        _context.OfferDescriptions.AddRange(appDescriptions.Select(s => new OfferDescription(s.appId, s.languageShortName, s.descriptionLong, s.descriptionShort)));

    /// <inheritdoc />
    public void AddAppLanguages(IEnumerable<(Guid appId, string languageShortName)> appLanguages) =>
        _context.AppLanguages.AddRange(appLanguages.Select(s => new AppLanguage(s.appId, s.languageShortName)));
    
    ///<inheritdoc />
    public void AddAppDetailImages(IEnumerable<(Guid appId, string imageUrl)> appImages)=>
        _context.OfferDetailImages.AddRange(appImages.Select(s=> new OfferDetailImage(Guid.NewGuid(), s.appId, s.imageUrl)));
    

    /// <inheritdoc />
    public IAsyncEnumerable<AllAppData> GetProvidedAppsData(string iamUserId) =>
        _context.Offers
            .AsNoTracking()
            .Where(app=>app.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId))
            .Select(app => new AllAppData(
                app.Id,
                app.Name,
                app.ThumbnailUrl,
                app.Provider,
                app.OfferStatusId.ToString(),
                app.DateLastChanged
            ))
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public Task<(bool IsAppCreated, bool IsProviderUser, IEnumerable<(string LanguageShortName ,string DescriptionLong,string DescriptionShort)> LanguageShortNames, IEnumerable<(Guid Id, string Url)> ImageUrls)> GetAppDetailsForUpdateAsync(Guid appId, string userId) =>
        _context.Offers
            .AsNoTracking()
            .AsSplitQuery()
            .Where(a => a.Id == appId)
            .Select(a =>
                new ValueTuple<bool, bool, IEnumerable<(string,string,string)>, IEnumerable<(Guid,string)>>(
                    a.OfferStatusId == OfferStatusId.CREATED,
                    a.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == userId),
                    a.OfferDescriptions.Select(description => new ValueTuple<string,string, string>(description.LanguageShortName, description.DescriptionLong, description.DescriptionShort)),
                    a.OfferDetailImages.Select(image => new ValueTuple<Guid,string>(image.Id, image.ImageUrl))
                ))
            .SingleOrDefaultAsync();
       
    /// <inheritdoc />
    public IAsyncEnumerable<ClientRoles> GetClientRolesAsync(Guid appId, string? languageShortName = null) =>
        _context.Offers
            .Where(app => app.Id == appId)
            .SelectMany(app => app.UserRoles)
            .Select(roles => new ClientRoles(
                roles.Id,
                roles.UserRoleText,
                languageShortName == null
                    ? roles.UserRoleDescriptions.SingleOrDefault(desc => desc.LanguageShortName == DEFAULT_LANGUAGE)!.Description
                    : roles.UserRoleDescriptions.SingleOrDefault(desc => desc.LanguageShortName == languageShortName)!.Description
            )).AsAsyncEnumerable();
    
     /// <inheritdoc />
     public Task<bool> CheckServiceExistsById(Guid serviceId) => 
         _context.Offers.AnyAsync(x => x.Id == serviceId && x.OfferTypeId == OfferTypeId.SERVICE);

     /// <inheritdoc />
    public IQueryable<(Guid id, string? name, string provider, string? thumbnailUrl, string? contactEmail, string? price)> GetActiveServices() =>
        _context.Offers
            .AsNoTracking()
            .Where(x => x.OfferTypeId == OfferTypeId.SERVICE && x.OfferStatusId == OfferStatusId.ACTIVE)
            .Select(app => new ValueTuple<Guid, string?, string, string?, string?, string?>(
                app.Id,
                app.Name,
                app.Provider,
                app.ThumbnailUrl,
                app.ContactEmail,
                app.OfferLicenses.FirstOrDefault()!.Licensetext
            ));

     /// <inheritdoc />
    public Task<(Guid Id, string? Title, string Provider, string? LeadPictureUri, string? ContactEmail, string? Description, string? Price)> GetServiceDetailByIdUntrackedAsync(Guid serviceId, string languageShortName) => 
        _context.Offers
            .AsNoTracking()
            .Where(x => x.Id == serviceId)
            .Select(app => new ValueTuple<Guid,string?,string,string?,string?,string?,string?>(
                app.Id,
                app.Name,
                app.Provider,
                app.ThumbnailUrl,
                app.ContactEmail,
                app.OfferDescriptions.SingleOrDefault(d => d.LanguageShortName == languageShortName)!.DescriptionLong,
                app.OfferLicenses.FirstOrDefault()!.Licensetext
            ))
            .SingleOrDefaultAsync();

    public IQueryable<Offer> GetAllInReviewStatusAppsAsync() =>
        _context.Offers.Where(offer => offer.OfferTypeId == OfferTypeId.APP && offer.OfferStatusId == OfferStatusId.IN_REVIEW);
}
