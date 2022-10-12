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

using Org.CatenaX.Ng.Portal.Backend.Framework.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;

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
    public Task<OfferProviderDetailsData?> GetOfferProviderDetailsAsync(Guid offerId, OfferTypeId offerTypeId) =>
        _context.Offers.AsNoTracking().Where(o => o.Id == offerId && o.OfferTypeId == offerTypeId).Select(c => new OfferProviderDetailsData(
            c.Name,
            c.Provider,
            c.ContactEmail,
            c.SalesManagerId,
            c.ProviderCompany!.ServiceProviderCompanyDetail!.AutoSetupUrl
        )).SingleOrDefaultAsync();

    /// <inheritdoc/>
    public Task<string?> GetAppAssignedClientIdUntrackedAsync(Guid appId, Guid companyId) =>
        _context.OfferSubscriptions.AsNoTracking()
            .Where(subscription => subscription.OfferId == appId && subscription.CompanyId == companyId)
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
    public IAsyncEnumerable<(Guid Id, string? Name, string VendorCompanyName, IEnumerable<string> UseCaseNames, string? ThumbnailUrl, string? ShortDescription, string? LicenseText)> GetAllActiveAppsAsync(string? languageShortName) =>
        _context.Offers.AsNoTracking()
            .Where(offer => offer.DateReleased.HasValue && offer.DateReleased <= DateTime.UtcNow && offer.OfferTypeId == OfferTypeId.APP && offer.OfferStatusId == OfferStatusId.ACTIVE)
            .Select(a => new ValueTuple<Guid,string?,string,IEnumerable<string>,string?,string?,string?>(
                a.Id,
                a.Name,
                a.ProviderCompany!.Name, // This translates into a 'left join' which does return null for all columns if the foreingn key is null. The '!' just makes the compiler happy
                a.UseCases.Select(uc => uc.Name),
                a.ThumbnailUrl,
                _context.Languages.Any(l => l.ShortName == languageShortName)
                        ? a.OfferDescriptions.SingleOrDefault(d => d.LanguageShortName == languageShortName)!.DescriptionShort
                            ?? a.OfferDescriptions.SingleOrDefault(d => d.LanguageShortName == Constants.DefaultLanguage)!.DescriptionShort
                        : null,
                a.OfferLicenses
                    .Select(license => license.Licensetext)
                    .FirstOrDefault()))
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public Task<OfferDetailsData?> GetOfferDetailsByIdAsync(Guid offerId, string iamUserId, string? languageShortName, string defaultLanguageShortName, OfferTypeId offerTypeId) =>
        _context.Offers.AsNoTracking()
            .Where(offer => offer.Id == offerId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new OfferDetailsData(
                offer.Id,
                offer.Name,
                offer.ThumbnailUrl,
                offer.OfferDetailImages.Select(adi => adi.ImageUrl),
                offer.MarketingUrl,
                offer.Provider,
                offer.ContactEmail,
                offer.ContactNumber,
                offer.UseCases.Select(u => u.Name),
                _context.Languages.Any(l => l.ShortName == languageShortName)
                    ? offer.OfferDescriptions.SingleOrDefault(d => d.LanguageShortName == languageShortName)!.DescriptionLong
                        ?? offer.OfferDescriptions.SingleOrDefault(d => d.LanguageShortName == defaultLanguageShortName)!.DescriptionLong
                    : null,
                offer.OfferLicenses
                    .Select(license => license.Licensetext)
                    .FirstOrDefault(),
                offer.Tags.Select(t => t.Name),
                offer.Companies.Where(c => c.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId))
                    .SelectMany(company => company.OfferSubscriptions.Where(x => x.OfferId == offerId))
                    .Select(x => x.OfferSubscriptionStatusId)
                    .FirstOrDefault(),
                offer.SupportedLanguages.Select(l => l.ShortName),
                offer.Documents.Select(d => new DocumentTypeData(d.DocumentTypeId, d.Id, d.DocumentName))
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public OfferLicense CreateOfferLicenses(string licenseText) =>
        _context.OfferLicenses.Add(new OfferLicense(Guid.NewGuid(), licenseText)).Entity;

    /// <inheritdoc />
    public OfferAssignedLicense CreateOfferAssignedLicense(Guid appId, Guid appLicenseId) =>
        _context.OfferAssignedLicenses.Add(new OfferAssignedLicense(appId, appLicenseId)).Entity;

    /// <inheritdoc />
    public CompanyUserAssignedAppFavourite CreateAppFavourite(Guid appId, Guid companyUserId) =>
        _context.CompanyUserAssignedAppFavourites.Add(new CompanyUserAssignedAppFavourite(appId, companyUserId)).Entity;

    ///<inheritdoc/>
    public OfferAssignedDocument CreateOfferAssignedDocument(Guid offerId, Guid documentId) =>
        _context.OfferAssignedDocuments.Add(new OfferAssignedDocument(offerId, documentId)).Entity;

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
    public Task<(bool IsAppCreated, bool IsProviderUser, string? ContactEmail, string? ContactNumber, string? MarketingUrl, IEnumerable<(string LanguageShortName ,string DescriptionLong,string DescriptionShort)> Descriptions, IEnumerable<(Guid Id, string Url)> ImageUrls)> GetAppDetailsForUpdateAsync(Guid appId, string userId) =>
        _context.Offers
            .AsNoTracking()
            .AsSplitQuery()
            .Where(a => a.Id == appId)
            .Select(a =>
                new ValueTuple<bool,bool,string?,string?,string?,IEnumerable<(string,string,string)>, IEnumerable<(Guid,string)>>(
                    a.OfferStatusId == OfferStatusId.CREATED,
                    a.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == userId),
                    a.ContactEmail,
                    a.ContactNumber,
                    a.MarketingUrl,
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
    public Task<OfferDetailData?> GetOfferDetailByIdUntrackedAsync(Guid serviceId, string languageShortName, string iamUserId, OfferTypeId offerTypeId) => 
        _context.Offers
            .AsNoTracking()
            .Where(x => x.Id == serviceId && x.OfferTypeId == offerTypeId)
            .Select(offer => new OfferDetailData(
                offer.Id,
                offer.Name,
                offer.Provider,
                offer.ThumbnailUrl,
                offer.ContactEmail,
                offer.OfferDescriptions.SingleOrDefault(d => d.LanguageShortName == languageShortName)!.DescriptionLong,
                offer.OfferLicenses.FirstOrDefault()!.Licensetext,
                offer.OfferSubscriptions.Where(os => os.Company!.CompanyUsers.Any(cu => cu.IamUser!.UserEntityId == iamUserId)).Select(x => new OfferSubscriptionStateDetailData(x.Id, x.OfferSubscriptionStatusId))
            ))
            .SingleOrDefaultAsync();
    
    /// <inheritdoc />
    public IQueryable<Offer> GetAllInReviewStatusAppsAsync() =>
        _context.Offers.Where(offer => offer.OfferTypeId == OfferTypeId.APP && offer.OfferStatusId == OfferStatusId.IN_REVIEW);
    
    /// <inheritdoc />
    public Task<OfferReleaseData?> GetOfferReleaseDataByIdAsync(Guid offerId) =>
        _context.Offers
            .AsNoTracking()
            .Where(a => a.Id == offerId && a.OfferStatusId == OfferStatusId.CREATED)
            .Select(c => new OfferReleaseData(
                c.Name,
                c.ThumbnailUrl,
                c.SalesManagerId,
                c.ProviderCompanyId,
                c.ProviderCompany!.Name,
                c.OfferDescriptions.Any(description => (description.DescriptionLong == "")),
                c.OfferDescriptions.Any(description => (description.DescriptionShort == ""))
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(OfferProviderData OfferProviderData, bool IsProviderCompanyUser)> GetProviderOfferDataWithConsentStatusAsync(Guid offerId, string userId, OfferTypeId offerTypeId) =>
        _context.Offers
            .AsNoTracking()
            .AsSplitQuery() 
            .Where(a => a.Id == offerId && a.OfferTypeId == offerTypeId)
            .Select(a => new ValueTuple<OfferProviderData,bool>(
                new OfferProviderData(
                    a.Name,
                    a.Provider,
                    a.ThumbnailUrl,
                    a.ProviderCompany!.Name,
                    a.UseCases.Select(uc => uc.Name),
                    a.OfferDescriptions.Select(description => new OfferDescriptionData(description.LanguageShortName, description.DescriptionLong, description.DescriptionShort)),
                    a.ConsentAssignedOffers.Select(consentAssignedOffer => new OfferAgreement(
                        consentAssignedOffer.Consent!.AgreementId,
                        consentAssignedOffer.Consent.Agreement!.Name,
                        consentAssignedOffer.Consent.ConsentStatusId)),
                    a.SupportedLanguages.Select(l => l.ShortName),
                    a.OfferLicenses
                        .Select(license => license.Licensetext)
                        .FirstOrDefault(),
                    a.OfferDetailImages.Select(image => image.ImageUrl),
                    a.MarketingUrl,
                    a.ContactEmail,
                    a.ContactNumber,
                    a.Documents.Select(d => new DocumentTypeData(d.DocumentTypeId, d.Id, d.DocumentName))),
                a.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == userId)
                ))
            .SingleOrDefaultAsync();

    ///<inheritdoc/>
    public Task<(bool OfferExists, bool IsProviderCompanyUser)> IsProviderCompanyUserAsync(Guid offerId, string userId, OfferTypeId offerTypeId) =>
        _context.Offers
            .Where(offer => offer.Id == offerId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new ValueTuple<bool,bool>(
                true,
                offer.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == userId)
            ))
            .SingleOrDefaultAsync();

    ///<inheritdoc/>
    public Task<(bool OfferExists, Guid CompanyUserId)> GetProviderCompanyUserIdForOfferUntrackedAsync(Guid offerId, string userId, OfferStatusId offerStatusId, OfferTypeId offerTypeId) =>
        _context.Offers
            .Where(offer => offer.Id == offerId && offer.OfferStatusId == offerStatusId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new ValueTuple<bool,Guid>(
                true,
                offer.ProviderCompany!.CompanyUsers.First(companyUser => companyUser.IamUser!.UserEntityId == userId).Id
            ))
            .SingleOrDefaultAsync();
}
