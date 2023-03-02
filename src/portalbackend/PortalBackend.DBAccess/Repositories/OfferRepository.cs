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
        _context.Offers.AsNoTracking().Where(o => o.Id == offerId && o.OfferTypeId == offerTypeId).Select(c => new OfferProviderDetailsData(
            c.Name,
            c.Provider,
            c.ContactEmail,
            c.SalesManagerId,
            c.ProviderCompany!.ProviderCompanyDetail!.AutoSetupUrl
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
    public IAsyncEnumerable<(Guid Id, string? Name, string VendorCompanyName, IEnumerable<string> UseCaseNames, Guid LeadPictureId, string? ShortDescription, string? LicenseText)> GetAllActiveAppsAsync(string? languageShortName) =>
        _context.Offers.AsNoTracking()
            .Where(offer => offer.DateReleased.HasValue && offer.DateReleased <= DateTime.UtcNow && offer.OfferTypeId == OfferTypeId.APP && offer.OfferStatusId == OfferStatusId.ACTIVE)
            .Select(a => new ValueTuple<Guid,string?,string,IEnumerable<string>,Guid,string?,string?>(
                a.Id,
                a.Name,
                a.ProviderCompany!.Name, // This translates into a 'left join' which does return null for all columns if the foreingn key is null. The '!' just makes the compiler happy
                a.UseCases.Select(uc => uc.Name),
                a.Documents.Where(document => document.DocumentTypeId == DocumentTypeId.APP_LEADIMAGE && document.DocumentStatusId != DocumentStatusId.INACTIVE).Select(document => document.Id).FirstOrDefault(),
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
                offer.Documents
                    .Where(doc => doc.DocumentTypeId != DocumentTypeId.APP_IMAGE && doc.DocumentTypeId != DocumentTypeId.APP_LEADIMAGE) 
                    .Select(d => new DocumentTypeData(d.DocumentTypeId, d.Id, d.DocumentName)),
                offer.OfferAssignedPrivacyPolicies.Select(x => x.PrivacyPolicyId)
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
    public Task<(bool IsAppCreated, bool IsProviderUser, string? ContactEmail, string? ContactNumber, string? MarketingUrl, IEnumerable<OfferDescriptionData> Descriptions)> GetOfferDetailsForUpdateAsync(Guid appId, string userId, OfferTypeId offerTypeId) =>
        _context.Offers
            .AsNoTracking()
            .Where(a => a.Id == appId && a.OfferTypeId == offerTypeId)
            .Select(a =>
                new ValueTuple<bool,bool,string?,string?,string?,IEnumerable<OfferDescriptionData>>(
                    a.OfferStatusId == OfferStatusId.CREATED,
                    a.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == userId),
                    a.ContactEmail,
                    a.ContactNumber,
                    a.MarketingUrl,
                    a.OfferDescriptions.Select(description => new OfferDescriptionData(description.LanguageShortName, description.DescriptionLong, description.DescriptionShort))
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
                    .Select(d => new DocumentTypeData(d.DocumentTypeId, d.Id, d.DocumentName))
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
                o.Documents.Where(doc => doc.DocumentStatusId != DocumentStatusId.LOCKED)
                    .Select(doc => new DocumentStatusData(doc.Id, doc.DocumentStatusId))
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(OfferProviderData OfferProviderData, bool IsProviderCompanyUser)> GetProviderOfferDataWithConsentStatusAsync(Guid offerId, string userId, OfferTypeId offerTypeId) =>
        _context.Offers
            .AsNoTracking()
            .AsSplitQuery() 
            .Where(a => a.Id == offerId && a.OfferTypeId == offerTypeId)
            .Select(offer => new ValueTuple<OfferProviderData,bool>(
                new OfferProviderData(
                    offer.Name,
                    offer.Provider,
                    offer.Documents.Where(document => document.DocumentTypeId == DocumentTypeId.APP_LEADIMAGE && document.DocumentStatusId != DocumentStatusId.INACTIVE).Select(document => document.Id).FirstOrDefault(),
                    offer.ProviderCompany!.Name,
                    offer.UseCases.Select(uc => uc.Name),
                    offer.OfferDescriptions.Select(description => new OfferDescriptionData(description.LanguageShortName, description.DescriptionLong, description.DescriptionShort)),
                    offer.OfferType!.AgreementAssignedOfferTypes
                    .Select(aaot => aaot.Agreement)
                    .Select(agreement => new AgreementAssignedOfferData(
                        agreement!.Id, 
                        agreement.Name,
                        agreement.Consents.SingleOrDefault(consent => consent.ConsentAssignedOffers.Any(cao => cao.OfferId == offer.Id))!.ConsentStatusId)),
                    offer.SupportedLanguages.Select(l => l.ShortName),
                    offer.OfferLicenses
                        .Select(license => license.Licensetext)
                        .FirstOrDefault(),
                    offer.Documents.Where(document => document.DocumentTypeId == DocumentTypeId.APP_IMAGE && document.DocumentStatusId != DocumentStatusId.INACTIVE).Select(document => document.Id),
                    offer.MarketingUrl,
                    offer.ContactEmail,
                    offer.ContactNumber,
                    offer.Documents.Select(d => new DocumentTypeData(d.DocumentTypeId, d.Id, d.DocumentName)),
                    offer.SalesManagerId,
                    offer.OfferAssignedPrivacyPolicies.Select(x => x.PrivacyPolicyId)),
                offer.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == userId)
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
                x.OfferDescriptions.Select(description => new OfferDescriptionData(description.LanguageShortName, description.DescriptionLong, description.DescriptionShort)),
                x.SupportedLanguages.Select(sl => new ValueTuple<string, bool>(sl.ShortName, languageCodes.Any(lc => lc == sl.ShortName))),
                x.UseCases.Select(uc => uc.Id),
                x.OfferLicenses.Select(ol => new ValueTuple<Guid, string, bool>(ol.Id, ol.Licensetext, ol.Offers.Count > 1)).FirstOrDefault(),
                x.SalesManagerId,
                x.OfferAssignedPrivacyPolicies.Select(x => x.PrivacyPolicyId)
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
                x.OfferDescriptions.Select(description => new OfferDescriptionData(description.LanguageShortName, description.DescriptionLong, description.DescriptionShort)),
                x.SalesManagerId
            ))
            .SingleOrDefaultAsync();

    ///<inheritdoc/>
    public Task<(bool OfferExists, string? AppName, Guid CompanyUserId, Guid? ProviderCompanyId)> GetOfferNameProviderCompanyUserAsync(Guid offerId, string userId, OfferTypeId offerTypeId) =>
        _context.Offers
            .Where(offer => offer.Id == offerId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new ValueTuple<bool,string?,Guid, Guid?>(
                true,
                offer.Name,
                offer.ProviderCompany!.CompanyUsers.SingleOrDefault(companyUser => companyUser.IamUser!.UserEntityId == userId)!.Id,
                offer.ProviderCompanyId
            ))
            .SingleOrDefaultAsync();

    ///<inheritdoc/>
    public Task<(bool IsStatusInReview, string? OfferName, Guid? ProviderCompanyId)> GetOfferStatusDataByIdAsync(Guid appId, OfferTypeId offerTypeId) =>
        _context.Offers
            .Where(offer => offer.Id == appId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new ValueTuple<bool, string?, Guid?>(
                offer.OfferStatusId == OfferStatusId.IN_REVIEW,
                offer.Name!,
                offer.ProviderCompanyId
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
                    offer.OfferDescriptions.Select(od => new OfferDescriptionData(od.LanguageShortName, od.DescriptionLong, od.DescriptionShort)),
                    offer.Documents.Where(d => d.DocumentTypeId != DocumentTypeId.APP_IMAGE && d.DocumentTypeId != DocumentTypeId.APP_LEADIMAGE)
                        .Select(d => new DocumentTypeData(d.DocumentTypeId, d.Id, d.DocumentName)),
                    offer.UserRoles.Select(ur => ur.UserRoleText),
                    offer.SupportedLanguages.Select(lang => lang.ShortName),
                    offer.MarketingUrl,
                    offer.ContactEmail,
                    offer.ContactNumber,
                    offer.OfferLicenses.Select(license => license.Licensetext).FirstOrDefault(),
                    offer.Tags.Select(t => t.Name)))
            .SingleOrDefaultAsync();
    
    ///<inheritdoc/>
    public Task<(bool IsStatusActive, bool IsProviderCompanyUser, IEnumerable<OfferDescriptionData>? OfferDescriptionDatas)> GetActiveOfferDescriptionDataByIdAsync(Guid appId, OfferTypeId offerTypeId, string iamUserId) =>
        _context.Offers
            .Where(offer => offer.Id == appId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new {
                Offer = offer,
                IsProviderCompanyUser = offer.ProviderCompany!.CompanyUsers.Any(cu => cu.IamUser!.UserEntityId == iamUserId)
            })
            .Select(x => new ValueTuple<bool,bool,IEnumerable<OfferDescriptionData>?>(
                x.Offer.OfferStatusId == OfferStatusId.ACTIVE,
                x.IsProviderCompanyUser,
                x.IsProviderCompanyUser ? x.Offer.OfferDescriptions.Select(od => new OfferDescriptionData(od.LanguageShortName, od.DescriptionLong, od.DescriptionShort)) : null))
            .SingleOrDefaultAsync();
    
    ///<inheritdoc/>
    public void CreateUpdateDeleteOfferDescriptions(Guid offerId, IEnumerable<OfferDescriptionData> initialItems, IEnumerable<(string LanguageCode, string LongDescription, string ShortDescription)> modifiedItems) =>
        _context.AddAttachRemoveRange(
            initialItems,
            modifiedItems,
            initial => initial.languageCode,
            modify =>  modify.LanguageCode,
            languageCode => new OfferDescription(offerId, languageCode, null!, null!),
            (initial, modified) => (initial.longDescription == modified.LongDescription && initial.shortDescription == modified.ShortDescription),
            (entity, initial) =>
                {
                    entity.DescriptionLong = initial.longDescription;
                    entity.DescriptionShort = initial.shortDescription;
                },
            (entity, modified) =>
                {
                    entity.DescriptionLong = modified.LongDescription;
                    entity.DescriptionShort = modified.ShortDescription;
                });

    /// <inheritdoc />
    public void RemoveOfferAssignedDocument(Guid offerId, Guid documentId) => 
        _context.OfferAssignedDocuments.Remove(new OfferAssignedDocument(offerId, documentId));
}
