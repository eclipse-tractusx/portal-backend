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
using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Linq.Expressions;
using PortalBackend.DBAccess.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

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
        _context.Offers.AsNoTracking()
            .Where(o => o.Id == offerId && o.OfferTypeId == offerTypeId)
            .Select(c => new OfferProviderDetailsData(
                c.Name,
                c.Provider,
                c.ContactEmail,
                c.SalesManagerId,
                c.ProviderCompany!.ProviderCompanyDetail!.AutoSetupUrl,
                c.AppInstanceSetup != null && c.AppInstanceSetup!.IsSingleInstance
        )).SingleOrDefaultAsync();

    /// <inheritdoc />
    public Offer CreateOffer(string provider, OfferTypeId offerType, Action<Offer>? setOptionalParameters = null)
    {
        var app = _context.Offers.Add(new Offer(Guid.NewGuid(), provider, DateTimeOffset.UtcNow, offerType)).Entity;
        setOptionalParameters?.Invoke(app);
        return app;
    }

    public void AttachAndModifyOffer(Guid offerId, Action<Offer> setOptionalParameters, Action<Offer>? initializeParemeters = null)
    {
        var entity = new Offer(offerId, null!, default, default);
        initializeParemeters?.Invoke(entity);
        var offer = _context.Attach(entity).Entity;
        setOptionalParameters.Invoke(offer);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<ActiveAppData> GetAllActiveAppsAsync(string? languageShortName) =>
        _context.Offers.AsNoTracking()
            .AsSplitQuery()
            .Where(offer => offer.DateReleased.HasValue && offer.DateReleased <= DateTime.UtcNow && offer.OfferTypeId == OfferTypeId.APP && offer.OfferStatusId == OfferStatusId.ACTIVE)
            .Select(a => new ActiveAppData(
                a.Id,
                a.Name,
                a.ProviderCompany!.Name, // This translates into a 'left join' which does return null for all columns if the foreingn key is null. The '!' just makes the compiler happy
                a.UseCases.Select(uc => uc.Name),
                a.Documents.Where(document => document.DocumentTypeId == DocumentTypeId.APP_LEADIMAGE && document.DocumentStatusId != DocumentStatusId.INACTIVE).Select(document => document.Id).FirstOrDefault(),
                a.LicenseTypeId,
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
            .AsSplitQuery()
            .Where(offer => offer.Id == offerId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new OfferDetailsData(
                offer.Id,
                offer.Name,
                offer.Documents.Where(document => document.DocumentTypeId == DocumentTypeId.APP_LEADIMAGE && document.DocumentStatusId != DocumentStatusId.INACTIVE).Select(document => document.Id).FirstOrDefault(),
                offer.Documents.Where(document => document.DocumentTypeId == DocumentTypeId.APP_IMAGE && document.DocumentStatusId != DocumentStatusId.INACTIVE).Select(document => document.Id),
                offer.MarketingUrl,
                offer.Provider,
                offer.ContactEmail,
                offer.ContactNumber,
                offer.UseCases.Select(uc => new AppUseCaseData(uc.Id, uc.Name)),
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
                offer.Documents
                    .Where(doc => doc.DocumentTypeId != DocumentTypeId.APP_IMAGE && doc.DocumentTypeId != DocumentTypeId.APP_LEADIMAGE) 
                    .Select(d => new DocumentTypeData(d.DocumentTypeId, d.Id, d.DocumentName)),
                offer.OfferAssignedPrivacyPolicies.Select(x => x.PrivacyPolicyId),
                offer.AppInstanceSetup != null && offer.AppInstanceSetup!.IsSingleInstance,
                offer.LicenseTypeId
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public OfferLicense CreateOfferLicenses(string licenseText) =>
        _context.OfferLicenses.Add(new OfferLicense(Guid.NewGuid(), licenseText)).Entity;

    /// <inheritdoc />
    public void AttachAndModifyOfferLicense(Guid offerLicenseId, Action<OfferLicense> setOptionalParameters)
    {
        var offerLicense = _context.OfferLicenses.Attach(new OfferLicense(offerLicenseId, null!)).Entity;
        setOptionalParameters.Invoke(offerLicense);
    }

    /// <inheritdoc />
    public void RemoveOfferAssignedLicense(Guid offerId, Guid offerLicenseId) =>
        _context.OfferAssignedLicenses.Remove(new OfferAssignedLicense(offerId, offerLicenseId));

    /// <inheritdoc />
    public void AddServiceAssignedServiceTypes(IEnumerable<(Guid serviceId, ServiceTypeId serviceTypeId, bool technicalUserNeeded)> serviceAssignedServiceTypes) =>
        _context.ServiceDetails.AddRange(serviceAssignedServiceTypes.Select(s => new ServiceDetail(s.serviceId, s.serviceTypeId, s.technicalUserNeeded)));

    /// <inheritdoc />
    public void RemoveServiceAssignedServiceTypes(IEnumerable<(Guid serviceId, ServiceTypeId serviceTypeId)> serviceAssignedServiceTypes) =>
        _context.ServiceDetails.RemoveRange(serviceAssignedServiceTypes.Select(s => new ServiceDetail(s.serviceId, s.serviceTypeId, default)));

    /// <inheritdoc />
    public OfferAssignedLicense CreateOfferAssignedLicense(Guid appId, Guid appLicenseId) =>
        _context.OfferAssignedLicenses.Add(new OfferAssignedLicense(appId, appLicenseId)).Entity;

    /// <inheritdoc />
    public CompanyUserAssignedAppFavourite CreateAppFavourite(Guid appId, Guid companyUserId) =>
        _context.CompanyUserAssignedAppFavourites.Add(new CompanyUserAssignedAppFavourite(appId, companyUserId)).Entity;
    
    public CompanyUserAssignedAppFavourite DeleteAppFavourite(Guid appId, Guid companyUserId) =>
        _context.CompanyUserAssignedAppFavourites.Remove(new CompanyUserAssignedAppFavourite(appId, companyUserId)).Entity;

    public void DeleteAppFavourites(IEnumerable<(Guid AppId, Guid CompanyUserId)> appFavoriteIds) =>
        _context.CompanyUserAssignedAppFavourites.RemoveRange(appFavoriteIds.Select(ids => new CompanyUserAssignedAppFavourite(ids.AppId, ids.CompanyUserId)));

    ///<inheritdoc/>
    public OfferAssignedDocument CreateOfferAssignedDocument(Guid offerId, Guid documentId) =>
        _context.OfferAssignedDocuments.Add(new OfferAssignedDocument(offerId, documentId)).Entity;

    /// <inheritdoc />
    public void AddAppAssignedUseCases(IEnumerable<(Guid appId, Guid useCaseId)> appUseCases) =>
        _context.AppAssignedUseCases.AddRange(appUseCases.Select(s => new AppAssignedUseCase(s.appId, s.useCaseId)));

    public void CreateDeleteAppAssignedUseCases(Guid appId, IEnumerable<Guid> initialUseCases, IEnumerable<Guid> modifyUseCases) =>
        _context.AddRemoveRange(
            initialUseCases,
            modifyUseCases,
            useCaseId => new AppAssignedUseCase(appId, useCaseId));

    /// <inheritdoc />
    public void AddOfferDescriptions(IEnumerable<(Guid offerId, string languageShortName, string descriptionLong, string descriptionShort)> offerDescriptions) =>
        _context.OfferDescriptions.AddRange(offerDescriptions.Select(s => new OfferDescription(s.offerId, s.languageShortName, s.descriptionLong, s.descriptionShort)));

    /// <inheritdoc />
    public void AddAppLanguages(IEnumerable<(Guid appId, string languageShortName)> appLanguages) =>
        _context.AppLanguages.AddRange(appLanguages.Select(s => new AppLanguage(s.appId, s.languageShortName)));

    /// <inheritdoc />
    public void RemoveAppLanguages(IEnumerable<(Guid appId, string languageShortName)> appLanguageIds) =>
        _context.RemoveRange(appLanguageIds.Select(x => new AppLanguage(x.appId, x.languageShortName)));

    public IAsyncEnumerable<AllOfferData> GetProvidedOffersData(OfferTypeId offerTypeId, string iamUserId) =>
        _context.Offers
            .AsNoTracking()
            .Where(offer =>
                offer.OfferTypeId == offerTypeId &&
                offer.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId))
            .Select(offer => new AllOfferData(
                offer.Id,
                offer.Name,
                offer.Documents.Where(document => document.DocumentTypeId == DocumentTypeId.APP_LEADIMAGE && document.DocumentStatusId != DocumentStatusId.INACTIVE).Select(document => document.Id).FirstOrDefault(),
                offer.Provider,
                offer.OfferStatusId,
                offer.DateLastChanged
            ))
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public Task<(bool IsAppCreated, bool IsProviderUser, string? ContactEmail, string? ContactNumber, string? MarketingUrl, IEnumerable<LocalizedDescription> Descriptions)> GetOfferDetailsForUpdateAsync(Guid appId, string userId, OfferTypeId offerTypeId) =>
        _context.Offers
            .AsNoTracking()
            .Where(a => a.Id == appId && a.OfferTypeId == offerTypeId)
            .Select(a =>
                new ValueTuple<bool,bool,string?,string?,string?,IEnumerable<LocalizedDescription>>(
                    a.OfferStatusId == OfferStatusId.CREATED,
                    a.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == userId),
                    a.ContactEmail,
                    a.ContactNumber,
                    a.MarketingUrl,
                    a.OfferDescriptions.Select(description => new LocalizedDescription(description.LanguageShortName, description.DescriptionLong, description.DescriptionShort))
                ))
            .SingleOrDefaultAsync();
       
    /// <inheritdoc />
    [Obsolete("only referenced by code that is marked as obsolte")]
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
    public Func<int,int,Task<Pagination.Source<ServiceOverviewData>?>> GetActiveServicesPaginationSource(ServiceOverviewSorting? sorting, ServiceTypeId? serviceTypeId) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            _context.Offers
                .AsNoTracking()
                .Where(x => 
                    x.OfferTypeId == OfferTypeId.SERVICE &&
                    x.OfferStatusId == OfferStatusId.ACTIVE &&
                    (serviceTypeId == null || x.ServiceDetails.Any(st => st.ServiceTypeId == serviceTypeId)))
                .GroupBy(s => s.OfferTypeId),
            sorting switch
            {
                ServiceOverviewSorting.ReleaseDateAsc => offers => offers.OrderBy(service => service.DateReleased),
                ServiceOverviewSorting.ReleaseDateDesc => offers => offers.OrderByDescending(service => service.DateReleased),
                ServiceOverviewSorting.ProviderAsc => offers => offers.OrderBy(service => service.Provider),
                ServiceOverviewSorting.ProviderDesc => offers => offers.OrderByDescending(service => service.Provider),
                _ => null
            },
            service =>  new ServiceOverviewData(
                service.Id,
                service.Name!,
                service.Provider,
                service.ContactEmail,
                service.OfferDescriptions.SingleOrDefault(ln => ln.LanguageShortName == DEFAULT_LANGUAGE)!.DescriptionShort,
                service.LicenseTypeId,
                service.OfferLicenses.FirstOrDefault()!.Licensetext,
                service.ServiceDetails.Select(x => x.ServiceTypeId)))
        .SingleOrDefaultAsync();

     /// <inheritdoc />
    public Task<OfferDetailData?> GetOfferDetailByIdUntrackedAsync(Guid serviceId, string languageShortName, string iamUserId, OfferTypeId offerTypeId) => 
        _context.Offers
            .AsNoTracking()
            .Where(x => x.Id == serviceId && x.OfferTypeId == offerTypeId)
            .Select(offer => new OfferDetailData(
                offer.Id,
                offer.Name,
                offer.Provider,
                offer.ContactEmail,
                offer.OfferDescriptions.SingleOrDefault(d => d.LanguageShortName == languageShortName)!.DescriptionLong,
                offer.OfferLicenses.FirstOrDefault()!.Licensetext,
                offer.OfferSubscriptions.Where(os => os.Company!.CompanyUsers.Any(cu => cu.IamUser!.UserEntityId == iamUserId)).Select(x => new OfferSubscriptionStateDetailData(x.Id, x.OfferSubscriptionStatusId))
            ))
            .SingleOrDefaultAsync();
    
     /// <inheritdoc />
     public Task<ServiceDetailData?> GetServiceDetailByIdUntrackedAsync(Guid serviceId, string languageShortName, string iamUserId) => 
         _context.Offers
             .AsNoTracking()
             .Where(x => x.Id == serviceId && x.OfferTypeId == OfferTypeId.SERVICE)
             .Select(offer => new ServiceDetailData(
                 offer.Id,
                 offer.Name,
                 offer.Provider,
                 offer.ContactEmail,
                 offer.OfferDescriptions.SingleOrDefault(d => d.LanguageShortName == languageShortName)!.DescriptionLong,
                 offer.OfferLicenses.FirstOrDefault()!.Licensetext,
                 offer.OfferSubscriptions.Where(os => os.Company!.CompanyUsers.Any(cu => cu.IamUser!.UserEntityId == iamUserId)).Select(x => new OfferSubscriptionStateDetailData(x.Id, x.OfferSubscriptionStatusId)),
                 offer.ServiceDetails.Select(x => x.ServiceTypeId),
                 offer.Documents.Where(doc => doc.DocumentTypeId == DocumentTypeId.ADDITIONAL_DETAILS)
                    .Select(d => new DocumentTypeData(d.DocumentTypeId, d.Id, d.DocumentName)),
                 offer.LicenseTypeId
             ))
             .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Func<int,int,Task<Pagination.Source<InReviewAppData>?>> GetAllInReviewStatusAppsAsync(IEnumerable<OfferStatusId> offerStatusIds, OfferSorting? sorting) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            _context.Offers.AsNoTracking()
                .Where(offer => offer.OfferTypeId == OfferTypeId.APP && offerStatusIds.Contains(offer.OfferStatusId))
                .GroupBy(offer=>offer.OfferTypeId),
            sorting switch
            {
                OfferSorting.DateAsc => (IEnumerable<Offer> offers) => offers.OrderBy(offer => offer.DateCreated),
                OfferSorting.DateDesc => (IEnumerable<Offer> offers) => offers.OrderByDescending(offer => offer.DateCreated),
                OfferSorting.NameAsc => (IEnumerable<Offer> offers) => offers.OrderBy(offer => offer.Name),
                OfferSorting.NameDesc => (IEnumerable<Offer> offers) => offers.OrderByDescending(offer => offer.Name),
                _ => (Expression<Func<IEnumerable<Offer>,IOrderedEnumerable<Offer>>>?)null
            },
            offer => new InReviewAppData(
                offer.Id,
                offer.Name,
                offer.ProviderCompany!.Name,
                offer.OfferStatusId))
            .SingleOrDefaultAsync();
    
    /// <inheritdoc />
    public Task<OfferReleaseData?> GetOfferReleaseDataByIdAsync(Guid offerId, OfferTypeId offerTypeId) =>
        _context.Offers
            .AsNoTracking()
            .Where(o => o.Id == offerId && o.OfferStatusId == OfferStatusId.CREATED && o.OfferTypeId == offerTypeId)
            .Select(o => new OfferReleaseData(
                o.Name,
                o.ProviderCompanyId,
                o.ProviderCompany!.Name,
                o.OfferDescriptions.Any(description => description.DescriptionLong == ""),
                o.OfferDescriptions.Any(description => description.DescriptionShort == ""),
                o.UserRoles.Any(),
                o.Documents.Where(doc => doc.DocumentStatusId != DocumentStatusId.LOCKED )
                    .Select(doc => new DocumentStatusData(doc.Id, doc.DocumentStatusId)),
                o.Documents.Where(doc => doc.DocumentStatusId != DocumentStatusId.LOCKED )
                    .Select(doc => doc.DocumentTypeId)
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(OfferProviderData? OfferProviderData, bool IsProviderCompanyUser)> GetProviderOfferDataWithConsentStatusAsync(Guid offerId, string userId, OfferTypeId offerTypeId) =>
        _context.Offers
            .AsNoTracking()
            .AsSplitQuery() 
            .Where(a => a.Id == offerId && a.OfferTypeId == offerTypeId)
            .Select(offer => new {
                IsProviderCompany = offer.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == userId),
                Offer = offer
            })
            .Select(x => new ValueTuple<OfferProviderData?,bool>(
                x.IsProviderCompany
                    ? new OfferProviderData(
                        x.Offer.Name,
                        x.Offer.Provider,
                        x.Offer.Documents.Where(document => document.DocumentTypeId == DocumentTypeId.APP_LEADIMAGE && document.DocumentStatusId != DocumentStatusId.INACTIVE).Select(document => document.Id).FirstOrDefault(),
                        x.Offer.ProviderCompany!.Name,
                        offerTypeId == OfferTypeId.APP
                            ? x.Offer.UseCases.Select(uc => new AppUseCaseData(uc.Id, uc.Name))
                            : null,
                        x.Offer.OfferDescriptions.Select(description => new LocalizedDescription(description.LanguageShortName, description.DescriptionLong, description.DescriptionShort)),
                        x.Offer.OfferType!.AgreementAssignedOfferTypes
                            .Select(aaot => aaot.Agreement)
                            .Select(agreement => new AgreementAssignedOfferData(
                                agreement!.Id, 
                                agreement.Name,
                                agreement.Consents.SingleOrDefault(consent => consent.ConsentAssignedOffers.Any(cao => cao.OfferId == x.Offer.Id))!.ConsentStatusId)),
                        x.Offer.SupportedLanguages.Select(l => l.ShortName),
                        x.Offer.OfferLicenses
                            .Select(license => license.Licensetext)
                            .FirstOrDefault(),
                        x.Offer.Documents.Where(document => document.DocumentTypeId == DocumentTypeId.APP_IMAGE && document.DocumentStatusId != DocumentStatusId.INACTIVE).Select(document => document.Id),
                        x.Offer.MarketingUrl,
                        x.Offer.ContactEmail,
                        x.Offer.ContactNumber,
                        x.Offer.Documents.Select(d => new DocumentTypeData(d.DocumentTypeId, d.Id, d.DocumentName)),
                        x.Offer.SalesManagerId,
                        x.Offer.OfferAssignedPrivacyPolicies.Select(x => x.PrivacyPolicyId),
                        offerTypeId == OfferTypeId.SERVICE
                            ? x.Offer.ServiceDetails.Select(x => x.ServiceTypeId)
                            : null)
                    : null,
                x.IsProviderCompany))
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
    public Task<(bool OfferExists, bool IsStatusCreated, Guid CompanyUserId)> GetProviderCompanyUserIdForOfferUntrackedAsync(Guid offerId, string userId, OfferStatusId offerStatusId, OfferTypeId offerTypeId) =>
        _context.Offers
            .Where(offer => offer.Id == offerId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new ValueTuple<bool, bool, Guid>(
                true,
                offer.OfferStatusId == offerStatusId,
                offer.ProviderCompany!.CompanyUsers.Where(companyUser => companyUser.IamUser!.UserEntityId == userId).Select(cu => cu.Id).FirstOrDefault()
            ))
            .SingleOrDefaultAsync();

    ///<inheritdoc/>
    public Task<(bool OfferStatus, bool IsProviderCompanyUser, bool IsRoleIdExist)> GetAppUserRoleUntrackedAsync(Guid offerId, string userId, OfferStatusId offerStatusId, Guid roleId) =>
        _context.Offers
            .Where(offer => offer.Id == offerId)
            .Select(offer => new ValueTuple<bool, bool, bool>(
                (offer.OfferStatusId == offerStatusId),
                offer.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == userId),
                offer.UserRoles.Any(userRole => userRole.Id == roleId)
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<AppUpdateData?> GetAppUpdateData(
        Guid appId,
        string iamUserId,
        IEnumerable<string> languageCodes) =>
        _context.Offers
            .AsNoTracking()
            .AsSplitQuery()
            .Where(offer => offer.Id == appId && offer.OfferTypeId == OfferTypeId.APP)
            .Select(x => new AppUpdateData
            (
                x.OfferStatusId,
                x.ProviderCompany!.CompanyUsers.Any(cu => cu.IamUser!.UserEntityId == iamUserId),
                x.OfferDescriptions.Select(description => new LocalizedDescription(description.LanguageShortName, description.DescriptionLong, description.DescriptionShort)),
                x.SupportedLanguages.Select(sl => new ValueTuple<string, bool>(sl.ShortName, languageCodes.Any(lc => lc == sl.ShortName))),
                x.UseCases.Select(uc => uc.Id),
                x.OfferLicenses.Select(ol => new ValueTuple<Guid, string, bool>(ol.Id, ol.Licensetext, ol.Offers.Count > 1)).FirstOrDefault(),
                x.OfferAssignedPrivacyPolicies.Select(x => x.PrivacyPolicyId),
                x.Name,
                x.Provider,
                x.SalesManagerId,
                x.ContactEmail,
                x.ContactNumber,
                x.MarketingUrl
            ))
            .SingleOrDefaultAsync();
    
    /// <inheritdoc />
    public Task<ServiceUpdateData?> GetServiceUpdateData(Guid serviceId, IEnumerable<ServiceTypeId> serviceTypeIds,  string iamUserId) =>
        _context.Offers
            .AsNoTracking()
            .Where(offer => offer.Id == serviceId && offer.OfferTypeId == OfferTypeId.SERVICE)
            .Select(x => new ServiceUpdateData
            (
                x.OfferStatusId,
                x.ProviderCompany!.CompanyUsers.Any(cu => cu.IamUser!.UserEntityId == iamUserId),
                x.ServiceDetails.Select(st => new ValueTuple<ServiceTypeId, bool>(st.ServiceTypeId, serviceTypeIds.Contains(st.ServiceTypeId))),
                x.OfferLicenses.Select(ol => new ValueTuple<Guid, string, bool>(ol.Id, ol.Licensetext, ol.Offers.Count > 1)).FirstOrDefault(),
                x.OfferDescriptions.Select(description => new LocalizedDescription(description.LanguageShortName, description.DescriptionLong, description.DescriptionShort)),
                x.SalesManagerId
            ))
            .SingleOrDefaultAsync();

    ///<inheritdoc/>
    public Task<(bool OfferExists, string? AppName, Guid CompanyUserId, Guid? ProviderCompanyId, IEnumerable<string> ClientClientIds)> GetInsertActiveAppUserRoleDataAsync(Guid offerId, string userId, OfferTypeId offerTypeId) =>
        _context.Offers
            .Where(offer => offer.Id == offerId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new ValueTuple<bool,string?,Guid,Guid?,IEnumerable<string>>(
                true,
                offer.Name,
                offer.ProviderCompany!.CompanyUsers.Where(companyUser => companyUser.IamUser!.UserEntityId == userId).Select(cu => cu.Id).SingleOrDefault(),
                offer.ProviderCompanyId,
                offer.AppInstances.Select(ai => ai.IamClient!.ClientClientId)
            ))
            .SingleOrDefaultAsync();

    ///<inheritdoc/>
    public Task<(bool IsStatusInReview, string? OfferName, Guid? ProviderCompanyId, bool IsSingleInstance, IEnumerable<(Guid InstanceId, string ClientId)> Instances)> GetOfferStatusDataByIdAsync(Guid offerId, OfferTypeId offerTypeId) =>
        _context.Offers
            .Where(offer => offer.Id == offerId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new ValueTuple<bool, string?, Guid?, bool, IEnumerable<(Guid,string)>>(
                offer.OfferStatusId == OfferStatusId.IN_REVIEW,
                offer.Name!,
                offer.ProviderCompanyId,
                offer.AppInstanceSetup != null && offer.AppInstanceSetup.IsSingleInstance,
                offer.AppInstances.Select(x => new ValueTuple<Guid,string>(x.Id, x.IamClient!.ClientClientId))
            ))
            .SingleOrDefaultAsync();
    
    /// <inheritdoc />
    public Task<(string? OfferName, OfferStatusId OfferStatus, Guid? CompanyId)> GetOfferDeclineDataAsync(Guid offerId, string iamUserId, OfferTypeId offerType) =>
        _context.Offers
            .Where(offer => offer.Id == offerId && offer.OfferTypeId == offerType)
            .Select(offer => new ValueTuple<string?, OfferStatusId, Guid?>(
                offer.Name, 
                offer.OfferStatusId,
                offer.ProviderCompanyId))
            .SingleOrDefaultAsync();

    ///<inheritdoc/>
    public Task<(bool IsStatusActive, bool IsUserCompanyProvider)> GetOfferActiveStatusDataByIdAsync(Guid appId, OfferTypeId offerTypeId, string iamUserId) =>
        _context.Offers
            .Where(offer => offer.Id == appId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new ValueTuple<bool, bool>(
                offer.OfferStatusId == OfferStatusId.ACTIVE,
                offer.ProviderCompany!.CompanyUsers.Any(cu => cu.IamUser!.UserEntityId == iamUserId)))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public void AddAppAssignedPrivacyPolicies(IEnumerable<(Guid appId, PrivacyPolicyId privacyPolicy)> privacyPolicies) =>
        _context.OfferAssignedPrivacyPolicies.AddRange(privacyPolicies.Select(s => new OfferAssignedPrivacyPolicy(s.appId, s.privacyPolicy)));
    
    /// <inheritdoc />
    public void CreateDeleteAppAssignedPrivacyPolicies(Guid appId, IEnumerable<PrivacyPolicyId> initialPrivacyPolicy, IEnumerable<PrivacyPolicyId> modifyPrivacyPolicy)=>
        _context.AddRemoveRange(
            initialPrivacyPolicy,
            modifyPrivacyPolicy,
            privacyPolicy => new OfferAssignedPrivacyPolicy(appId, privacyPolicy));
    
    ///<inheritdoc/>
    public Task<InReviewOfferData?> GetInReviewAppDataByIdAsync(Guid id, OfferTypeId offerTypeId) =>
        _context.Offers.AsNoTracking()
            .AsSplitQuery()
            .Where(offer => offer.Id == id && offer.OfferTypeId == offerTypeId && offer.OfferStatusId == OfferStatusId.IN_REVIEW)
            .Select(offer =>  
                new InReviewOfferData(
                    offer.Id,
                    offer.Name,
                    offer.Documents.Where(document => document.DocumentTypeId == DocumentTypeId.APP_LEADIMAGE).Select(document => document.Id).FirstOrDefault(),
                    offer.Documents.Where(document => document.DocumentTypeId == DocumentTypeId.APP_IMAGE).Select(document => document.Id),
                    offer.Provider,
                    offer.UseCases.Select(uc => uc.Name),
                    offer.OfferDescriptions.Select(od => new LocalizedDescription(od.LanguageShortName, od.DescriptionLong, od.DescriptionShort)),
                    offer.Documents.Where(d => d.DocumentTypeId != DocumentTypeId.APP_IMAGE && d.DocumentTypeId != DocumentTypeId.APP_LEADIMAGE)
                        .Select(d => new DocumentTypeData(d.DocumentTypeId, d.Id, d.DocumentName)),
                    offer.UserRoles.Select(ur => ur.UserRoleText),
                    offer.SupportedLanguages.Select(lang => lang.ShortName),
                    offer.MarketingUrl,
                    offer.ContactEmail,
                    offer.ContactNumber,
                    offer.OfferLicenses.Select(license => license.Licensetext).FirstOrDefault(),
                    offer.Tags.Select(t => t.Name),
                    offer.OfferAssignedPrivacyPolicies.Select(p=>p.PrivacyPolicyId),
                    offer.LicenseTypeId))
            .SingleOrDefaultAsync();
    
    ///<inheritdoc/>
    public Task<(bool IsStatusActive, bool IsProviderCompanyUser, IEnumerable<LocalizedDescription>? OfferDescriptionDatas)> GetActiveOfferDescriptionDataByIdAsync(Guid appId, OfferTypeId offerTypeId, string iamUserId) =>
        _context.Offers
            .Where(offer => offer.Id == appId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new {
                Offer = offer,
                IsProviderCompanyUser = offer.ProviderCompany!.CompanyUsers.Any(cu => cu.IamUser!.UserEntityId == iamUserId)
            })
            .Select(x => new ValueTuple<bool,bool,IEnumerable<LocalizedDescription>?>(
                x.Offer.OfferStatusId == OfferStatusId.ACTIVE,
                x.IsProviderCompanyUser,
                x.IsProviderCompanyUser ? x.Offer.OfferDescriptions.Select(od => new LocalizedDescription(od.LanguageShortName, od.DescriptionLong, od.DescriptionShort)) : null))
            .SingleOrDefaultAsync();
    
    ///<inheritdoc/>
    public void CreateUpdateDeleteOfferDescriptions(Guid offerId, IEnumerable<LocalizedDescription> initialItems, IEnumerable<(string LanguageCode, string LongDescription, string ShortDescription)> modifiedItems) =>
        _context.AddAttachRemoveRange(
            initialItems,
            modifiedItems,
            initial => initial.LanguageCode,
            modify =>  modify.LanguageCode,
            languageCode => new OfferDescription(offerId, languageCode, null!, null!),
            (initial, modified) => (initial.LongDescription == modified.LongDescription && initial.ShortDescription == modified.ShortDescription),
            (entity, initial) =>
                {
                    entity.DescriptionLong = initial.LongDescription;
                    entity.DescriptionShort = initial.ShortDescription;
                },
            (entity, modified) =>
                {
                    entity.DescriptionLong = modified.LongDescription;
                    entity.DescriptionShort = modified.ShortDescription;
                });

    /// <inheritdoc />
    public void RemoveOfferAssignedDocument(Guid offerId, Guid documentId) => 
        _context.OfferAssignedDocuments.Remove(new OfferAssignedDocument(offerId, documentId));
    
    ///<inheritdoc/>
    public Task<(bool IsValidApp,bool IsOfferType,bool IsOfferStatus,bool IsProviderCompanyUser,AppDeleteData? DeleteData)> GetAppDeleteDataAsync(Guid offerId, OfferTypeId offerTypeId, string userId, OfferStatusId offerStatusId) =>
        _context.Offers
            .AsNoTracking()
            .AsSplitQuery()
            .Where(offer => offer.Id == offerId)
            .Select(offer => new {
                Offer = offer,
                IsOfferTypeId = offer.OfferTypeId == offerTypeId,
                IsOfferStatusId = offer.OfferStatusId == offerStatusId,
                IsProviderCompanyUser = offer.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == userId),
            })
            .Select(x => new ValueTuple<bool,bool,bool,bool,AppDeleteData?>(
                true,
                x.IsOfferTypeId,
                x.IsOfferStatusId,
                x.IsProviderCompanyUser,
                x.IsOfferTypeId && x.IsOfferStatusId && x.IsProviderCompanyUser
                    ? new AppDeleteData(
                        x.Offer.OfferLicenses.Select(offerlicense =>offerlicense.Id),
                        x.Offer.UseCases.Select(uc => uc.Id),
                        x.Offer.OfferAssignedPrivacyPolicies.Select(pp=>pp.PrivacyPolicyId),
                        x.Offer.Documents.Select(doc => new ValueTuple<Guid,DocumentStatusId>(doc.Id, doc.DocumentStatusId)),
                        x.Offer.SupportedLanguages.Select(sl => sl.ShortName),
                        x.Offer.Tags.Select(offerTag=>offerTag.Name),
                        x.Offer.OfferDescriptions.Select(description => description.LanguageShortName))
                    : null
            ))
            .SingleOrDefaultAsync();
    
    ///<inheritdoc/>
    public void RemoveOfferAssignedLicenses(IEnumerable<(Guid OfferId, Guid LicenseId)> offerLicenseIds) =>
        _context.OfferAssignedLicenses.RemoveRange(offerLicenseIds.Select(offerLicenseId => new OfferAssignedLicense(offerLicenseId.OfferId, offerLicenseId.LicenseId)));
    
    ///<inheritdoc/>
    public void RemoveOfferAssignedUseCases(IEnumerable<(Guid OfferId, Guid UseCaseId)> offerUseCaseIds) =>
        _context.AppAssignedUseCases.RemoveRange(offerUseCaseIds.Select(offerUseCaseId => new AppAssignedUseCase(offerUseCaseId.OfferId, offerUseCaseId.UseCaseId)));
    
    ///<inheritdoc/>
    public void RemoveOfferAssignedPrivacyPolicies(IEnumerable<(Guid OfferId, PrivacyPolicyId PrivacyPolicyId)> offerPrivacyPolicyIds) =>
        _context.OfferAssignedPrivacyPolicies.RemoveRange(offerPrivacyPolicyIds.Select(offerPrivacyPolicyId => new OfferAssignedPrivacyPolicy(offerPrivacyPolicyId.OfferId, offerPrivacyPolicyId.PrivacyPolicyId)));
    
    ///<inheritdoc/>
    public void RemoveOfferAssignedDocuments(IEnumerable<(Guid OfferId, Guid DocumentId)> offerDocumentIds) =>
        _context.OfferAssignedDocuments.RemoveRange(offerDocumentIds.Select(offerDocumentId => new OfferAssignedDocument(offerDocumentId.OfferId, offerDocumentId.DocumentId)));
    
    ///<inheritdoc/>
    public void RemoveOfferTags(IEnumerable<(Guid OfferId, string TagName)> offerTagNames) =>
        _context.OfferTags.RemoveRange(offerTagNames.Select(offerTagName => new OfferTag(offerTagName.OfferId, offerTagName.TagName)));
    
    ///<inheritdoc/>
    public void RemoveOfferDescriptions(IEnumerable<(Guid OfferId, string LanguageShortName)> offerLanguageShortNames) =>
        _context.OfferDescriptions.RemoveRange(offerLanguageShortNames.Select(offerLanguageShortName => new OfferDescription(offerLanguageShortName.OfferId, offerLanguageShortName.LanguageShortName, null!, null!)));
    
    ///<inheritdoc/>
    public void RemoveOffer(Guid offerId) => 
        _context.Offers.Remove(new Offer(offerId, null!, default, default));   

    ///<inheritdoc/>
    public Task<(bool IsStatusActive, Guid CompanyUserId, IEnumerable<DocumentStatusData> documentStatusDatas)> GetOfferAssignedAppLeadImageDocumentsByIdAsync(Guid offerId, string iamUserId, OfferTypeId offerTypeId) =>
        _context.Offers
            .AsNoTracking()
            .Where(offer => offer.Id == offerId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new ValueTuple<bool, Guid, IEnumerable<DocumentStatusData>>(
                offer.OfferStatusId == OfferStatusId.ACTIVE,
                offer.ProviderCompany!.CompanyUsers.Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)
                    .Select(cu => cu.Id).FirstOrDefault(),
                offer.Documents.Where(doc => doc.DocumentTypeId == DocumentTypeId.APP_LEADIMAGE)
                    .Select(doc => new DocumentStatusData(doc.Id, doc.DocumentStatusId))))
            .SingleOrDefaultAsync();

    ///<inheritdoc/>
    public Task<ServiceDetailsData?> GetServiceDetailsByIdAsync(Guid serviceId) =>
        _context.Offers.AsNoTracking()
            .Where(service => service.Id == serviceId && service.OfferTypeId == OfferTypeId.SERVICE)
            .Select(offer => new ServiceDetailsData(
                offer.Id,
                offer.Name,
                offer.ServiceDetails.Select(x => x.ServiceType!.Label),
                offer.Provider,
                offer.OfferDescriptions.Select(description => new LocalizedDescription(description.LanguageShortName, description.DescriptionLong, description.DescriptionShort)),
                offer.Documents.Select(d => new DocumentTypeData(d.DocumentTypeId, d.Id, d.DocumentName)),
                offer.MarketingUrl,
                offer.ContactEmail,
                offer.ContactNumber,
                offer.OfferStatusId,
                offer.LicenseTypeId
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Func<int,int,Task<Pagination.Source<AllOfferStatusData>?>> GetCompanyProvidedServiceStatusDataAsync(IEnumerable<OfferStatusId> offerStatusIds, OfferTypeId offerTypeId, string userId, OfferSorting? sorting, string? offerName) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            _context.Offers.AsNoTracking()
                .Where(offer => offer.OfferTypeId == offerTypeId && 
                    offer.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == userId) &&
                    offerStatusIds.Contains(offer.OfferStatusId) && (offerName == null || EF.Functions.ILike(offer.Name!, $"%{offerName!.EscapeForILike()}%")))
                .GroupBy(offer=>offer.OfferTypeId),
            sorting switch
            {
                OfferSorting.DateAsc => (IEnumerable<Offer> offers) => offers.OrderBy(offer => offer.DateCreated),
                OfferSorting.DateDesc => (IEnumerable<Offer> offers) => offers.OrderByDescending(offer => offer.DateCreated),
                _ => (Expression<Func<IEnumerable<Offer>,IOrderedEnumerable<Offer>>>?)null
            },
            offer => new AllOfferStatusData(
                offer.Id,
                offer.Name,
                offer.ProviderCompany!.Name,
                offer.OfferStatusId
                ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Func<int,int,Task<Pagination.Source<InReviewServiceData>?>> GetAllInReviewStatusServiceAsync(IEnumerable<OfferStatusId> offerStatusIds, OfferTypeId offerTypeId, OfferSorting? sorting, string? offerName, string? languageShortName) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            _context.Offers.AsNoTracking()
                .Where(offer => offer.OfferTypeId == offerTypeId && offerStatusIds.Contains(offer.OfferStatusId) && (offerName == null || EF.Functions.ILike(offer.Name!, $"%{offerName!.EscapeForILike()}%")))
                .GroupBy(offer=>offer.OfferTypeId),
            sorting switch
            {
                OfferSorting.DateAsc => (IEnumerable<Offer> offers) => offers.OrderBy(offer => offer.DateCreated),
                OfferSorting.DateDesc => (IEnumerable<Offer> offers) => offers.OrderByDescending(offer => offer.DateCreated),
                OfferSorting.NameAsc => (IEnumerable<Offer> offers) => offers.OrderBy(offer => offer.Name),
                OfferSorting.NameDesc => (IEnumerable<Offer> offers) => offers.OrderByDescending(offer => offer.Name),
                _ => (Expression<Func<IEnumerable<Offer>,IOrderedEnumerable<Offer>>>?)null
            },
            offer => new InReviewServiceData(
                offer.Id,
                offer.Name,
                offer.OfferStatusId,
                offer.ProviderCompany!.Name,
                offer.OfferDescriptions.SingleOrDefault(d => d.LanguageShortName == languageShortName)!.DescriptionShort
                           ?? offer.OfferDescriptions.SingleOrDefault(d => d.LanguageShortName == Constants.DefaultLanguage)!.DescriptionShort
                        ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(OfferStatusId OfferStatus, bool IsUserOfProvidingCompany, AppInstanceSetupTransferData? SetupTransferData, IEnumerable<(Guid AppInstanceId, Guid ClientId, string ClientClientId)> AppInstanceData)> GetOfferWithSetupDataById(Guid offerId, string iamUserId, OfferTypeId offerTypeId) =>
        _context.Offers
            .AsNoTracking()
            .Where(x => x.OfferTypeId == offerTypeId && x.Id == offerId)
            .Select(o => new ValueTuple<OfferStatusId, bool, AppInstanceSetupTransferData?, IEnumerable<(Guid, Guid, string)>>(
                o.OfferStatusId,
                o.ProviderCompany!.CompanyUsers.Any(cu => cu.IamUser!.UserEntityId == iamUserId),
                o.AppInstanceSetup == null ? 
                    null : 
                    new AppInstanceSetupTransferData(o.AppInstanceSetup!.AppId, o.AppInstanceSetup!.IsSingleInstance, o.AppInstanceSetup!.InstanceUrl),
                o.AppInstances.Select(x => new ValueTuple<Guid, Guid, string>(x.Id, x.IamClientId, x.IamClient!.ClientClientId))
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public AppInstanceSetup CreateAppInstanceSetup(Guid appId, Action<AppInstanceSetup>? setOptionalParameter)
    {
        var appInstanceSetup = _context.AppInstanceSetups.Add(new AppInstanceSetup(Guid.NewGuid(), appId)).Entity;
        setOptionalParameter?.Invoke(appInstanceSetup);
        return appInstanceSetup;
    }

    /// <inheritdoc />
    public Task<SingleInstanceOfferData?> GetSingleInstanceOfferData(Guid offerId, OfferTypeId offerTypeId) =>
        _context.Offers.Where(o => o.Id == offerId && o.OfferTypeId == offerTypeId)
            .Select(o => new SingleInstanceOfferData(
                o.ProviderCompany!.Id,
                o.Name,
                o.ProviderCompany.BusinessPartnerNumber,
                o.AppInstanceSetup != null && o.AppInstanceSetup.IsSingleInstance,
                o.AppInstances.Select(x => new ValueTuple<Guid,string>(x.Id, x.IamClient!.ClientClientId))
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public void AttachAndModifyAppInstanceSetup(Guid appInstanceSetupId, Guid offerId, Action<AppInstanceSetup> setOptionalParameters, Action<AppInstanceSetup>? initializeParameter = null)
    {
        var entity = new AppInstanceSetup(appInstanceSetupId, offerId);
        initializeParameter?.Invoke(entity);
        var appInstanceSetup = _context.Attach(entity).Entity;
        setOptionalParameters.Invoke(appInstanceSetup);
    }

    /// <inheritdoc />
    public Task<(bool IsSingleInstance, bool TechnicalUserNeeded, string? OfferName)> GetServiceAccountProfileData(Guid offerId) =>
        _context.Offers.Where(x => x.Id == offerId)
            .Select(o => new ValueTuple<bool, bool, string?>(
                o.AppInstanceSetup != null && o.AppInstanceSetup.IsSingleInstance,
                o.ServiceDetails.Any(x => x.TechnicalUserNeeded),
                o.Name
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(bool IsSingleInstance, bool TechnicalUserNeeded, string? OfferName)> GetServiceAccountProfileDataForSubscription(Guid subscriptionId) =>
        _context.OfferSubscriptions.Where(x => x.Id == subscriptionId)
            .Select(o => new ValueTuple<bool, bool, string?>(
                o.Offer!.AppInstanceSetup != null && o.Offer.AppInstanceSetup.IsSingleInstance,
                o.Offer.ServiceDetails.Any(x => x.TechnicalUserNeeded),
                o.Offer.Name
            ))
            .SingleOrDefaultAsync();
}
