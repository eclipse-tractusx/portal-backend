/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Implementation of <see cref="IOfferRepository"/> accessing database with EF Core.
/// </summary>
/// <param name="dbContext"></param>
public class OfferRepository(PortalDbContext dbContext) : IOfferRepository
{
    ///<inheritdoc/>
    public Task<OfferProviderDetailsData?> GetOfferProviderDetailsAsync(Guid offerId, OfferTypeId offerTypeId) =>
        dbContext.Offers.AsNoTracking()
            .Where(o => o.Id == offerId && o.OfferTypeId == offerTypeId)
            .Select(c => new OfferProviderDetailsData(
                c.Name,
                c.ProviderCompany!.Name,
                c.ContactEmail,
                c.SalesManagerId,
                c.ProviderCompany!.ProviderCompanyDetail!.AutoSetupUrl,
                c.AppInstanceSetup != null && c.AppInstanceSetup!.IsSingleInstance,
                c.ProviderCompanyId
        )).SingleOrDefaultAsync();

    /// <inheritdoc />
    public Offer CreateOffer(OfferTypeId offerType, Guid providerCompanyId, Action<Offer>? setOptionalParameters = null)
    {
        var app = dbContext.Offers.Add(new Offer(Guid.NewGuid(), providerCompanyId, DateTimeOffset.UtcNow, offerType)).Entity;
        setOptionalParameters?.Invoke(app);
        return app;
    }

    public void AttachAndModifyOffer(Guid offerId, Action<Offer> setOptionalParameters, Action<Offer>? initializeParemeters = null)
    {
        var entity = new Offer(offerId, Guid.Empty, default, default);
        initializeParemeters?.Invoke(entity);
        var offer = dbContext.Attach(entity).Entity;
        setOptionalParameters.Invoke(offer);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<ActiveAppData> GetAllActiveAppsAsync(string? languageShortName, string defaultLanguageShortName) =>
        dbContext.Offers.AsNoTracking()
            .AsSplitQuery()
            .Where(offer => offer.DateReleased.HasValue && offer.DateReleased <= DateTime.UtcNow && offer.OfferTypeId == OfferTypeId.APP && offer.OfferStatusId == OfferStatusId.ACTIVE)
            .OrderByDescending(x => x.DateReleased)
            .Select(a => new ActiveAppData(
                a.Id,
                a.Name,
                a.ProviderCompany!.Name, // This translates into a 'left join' which does return null for all columns if the foreingn key is null. The '!' just makes the compiler happy
                a.UseCases.Select(uc => uc.Name),
                a.Documents.Where(document => document.DocumentTypeId == DocumentTypeId.APP_LEADIMAGE && document.DocumentStatusId != DocumentStatusId.INACTIVE).Select(document => document.Id).FirstOrDefault(),
                a.LicenseTypeId,
                dbContext.Languages.Any(l => l.ShortName == languageShortName)
                        ? a.OfferDescriptions.SingleOrDefault(d => d.LanguageShortName == languageShortName)!.DescriptionShort
                            : a.OfferDescriptions.SingleOrDefault(d => d.LanguageShortName == defaultLanguageShortName)!.DescriptionShort,
                a.OfferLicenses
                    .Select(license => license.Licensetext)
                    .FirstOrDefault()))
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public Task<OfferDetailsData?> GetOfferDetailsByIdAsync(Guid offerId, Guid userCompanyId, string? languageShortName, string defaultLanguageShortName, OfferTypeId offerTypeId) =>
        dbContext.Offers.AsNoTracking()
            .AsSplitQuery()
            .Where(offer => offer.Id == offerId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new OfferDetailsData(
                offer.Id,
                offer.Name,
                offer.Documents.Where(document => document.DocumentTypeId == DocumentTypeId.APP_LEADIMAGE && document.DocumentStatusId != DocumentStatusId.INACTIVE).Select(document => document.Id).FirstOrDefault(),
                offer.Documents.Where(document => document.DocumentTypeId == DocumentTypeId.APP_IMAGE && document.DocumentStatusId != DocumentStatusId.INACTIVE).Select(document => document.Id),
                offer.MarketingUrl,
                offer.ProviderCompany!.Name,
                offer.ContactEmail,
                offer.ContactNumber,
                offer.UseCases.Select(uc => new AppUseCaseData(uc.Id, uc.Name)),
                dbContext.Languages.Any(l => l.ShortName == languageShortName)
                    ? offer.OfferDescriptions.SingleOrDefault(d => d.LanguageShortName == languageShortName)!.DescriptionLong
                        ?? offer.OfferDescriptions.SingleOrDefault(d => d.LanguageShortName == defaultLanguageShortName)!.DescriptionLong
                    : null,
                offer.OfferLicenses
                    .Select(license => license.Licensetext)
                    .FirstOrDefault(),
                offer.Tags.Select(t => t.Name),
                offer.Companies.Where(c => c.Id == userCompanyId)
                    .SelectMany(company => company.OfferSubscriptions.Where(x => x.OfferId == offerId))
                    .OrderByDescending(x => x.DateCreated)
                    .Select(x => x.OfferSubscriptionStatusId)
                    .FirstOrDefault(),
                offer.SupportedLanguages.Select(l => l.ShortName),
                offer.Documents
                    .Where(doc => doc.DocumentTypeId != DocumentTypeId.APP_IMAGE && doc.DocumentTypeId != DocumentTypeId.APP_LEADIMAGE)
                    .Select(d => new DocumentTypeData(d.DocumentTypeId, d.Id, d.DocumentName)),
                offer.OfferAssignedPrivacyPolicies.Select(x => x.PrivacyPolicyId),
                offer.AppInstanceSetup != null && offer.AppInstanceSetup!.IsSingleInstance,
                offer.LicenseTypeId,
                offer.TechnicalUserProfiles.Where(tup => tup.TechnicalUserProfileAssignedUserRoles.Any()).Select(tup => new TechnicalUserRoleData(
                    tup.Id,
                    tup.TechnicalUserProfileAssignedUserRoles.Select(ur => ur.UserRole!.UserRoleText)))
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public OfferLicense CreateOfferLicenses(string licenseText) =>
        dbContext.OfferLicenses.Add(new OfferLicense(Guid.NewGuid(), licenseText)).Entity;

    /// <inheritdoc />
    public void AttachAndModifyOfferLicense(Guid offerLicenseId, Action<OfferLicense> setOptionalParameters)
    {
        var offerLicense = dbContext.OfferLicenses.Attach(new OfferLicense(offerLicenseId, null!)).Entity;
        setOptionalParameters.Invoke(offerLicense);
    }

    /// <inheritdoc />
    public void RemoveOfferAssignedLicense(Guid offerId, Guid offerLicenseId) =>
        dbContext.OfferAssignedLicenses.Remove(new OfferAssignedLicense(offerId, offerLicenseId));

    /// <inheritdoc />
    public void AddServiceAssignedServiceTypes(IEnumerable<(Guid serviceId, ServiceTypeId serviceTypeId)> serviceAssignedServiceTypes) =>
        dbContext.ServiceDetails.AddRange(serviceAssignedServiceTypes.Select(s => new ServiceDetail(s.serviceId, s.serviceTypeId)));

    /// <inheritdoc />
    public void RemoveServiceAssignedServiceTypes(IEnumerable<(Guid serviceId, ServiceTypeId serviceTypeId)> serviceAssignedServiceTypes) =>
        dbContext.ServiceDetails.RemoveRange(serviceAssignedServiceTypes.Select(s => new ServiceDetail(s.serviceId, s.serviceTypeId)));

    /// <inheritdoc />
    public OfferAssignedLicense CreateOfferAssignedLicense(Guid appId, Guid appLicenseId) =>
        dbContext.OfferAssignedLicenses.Add(new OfferAssignedLicense(appId, appLicenseId)).Entity;

    /// <inheritdoc />
    public CompanyUserAssignedAppFavourite CreateAppFavourite(Guid appId, Guid companyUserId) =>
        dbContext.CompanyUserAssignedAppFavourites.Add(new CompanyUserAssignedAppFavourite(appId, companyUserId)).Entity;

    public CompanyUserAssignedAppFavourite DeleteAppFavourite(Guid appId, Guid companyUserId) =>
        dbContext.CompanyUserAssignedAppFavourites.Remove(new CompanyUserAssignedAppFavourite(appId, companyUserId)).Entity;

    public void DeleteAppFavourites(IEnumerable<(Guid AppId, Guid CompanyUserId)> appFavoriteIds) =>
        dbContext.CompanyUserAssignedAppFavourites.RemoveRange(appFavoriteIds.Select(ids => new CompanyUserAssignedAppFavourite(ids.AppId, ids.CompanyUserId)));

    ///<inheritdoc/>
    public OfferAssignedDocument CreateOfferAssignedDocument(Guid offerId, Guid documentId) =>
        dbContext.OfferAssignedDocuments.Add(new OfferAssignedDocument(offerId, documentId)).Entity;

    /// <inheritdoc />
    public void AddAppAssignedUseCases(IEnumerable<(Guid appId, Guid useCaseId)> appUseCases) =>
        dbContext.AppAssignedUseCases.AddRange(appUseCases.Select(s => new AppAssignedUseCase(s.appId, s.useCaseId)));

    public void CreateDeleteAppAssignedUseCases(Guid appId, IEnumerable<Guid> initialUseCases, IEnumerable<Guid> modifyUseCases) =>
        dbContext.AddRemoveRange(
            initialUseCases,
            modifyUseCases,
            useCaseId => new AppAssignedUseCase(appId, useCaseId));

    /// <inheritdoc />
    public void AddOfferDescriptions(IEnumerable<(Guid offerId, string languageShortName, string descriptionLong, string descriptionShort)> offerDescriptions) =>
        dbContext.OfferDescriptions.AddRange(offerDescriptions.Select(s => new OfferDescription(s.offerId, s.languageShortName, s.descriptionLong, s.descriptionShort)));

    /// <inheritdoc />
    public void AddAppLanguages(IEnumerable<(Guid appId, string languageShortName)> appLanguages) =>
        dbContext.AppLanguages.AddRange(appLanguages.Select(s => new AppLanguage(s.appId, s.languageShortName)));

    /// <inheritdoc />
    public void RemoveAppLanguages(IEnumerable<(Guid appId, string languageShortName)> appLanguageIds) =>
        dbContext.RemoveRange(appLanguageIds.Select(x => new AppLanguage(x.appId, x.languageShortName)));

    public Func<int, int, Task<Pagination.Source<AllOfferData>?>> GetProvidedOffersData(IEnumerable<OfferStatusId> offerStatusIds, OfferTypeId offerTypeId, Guid userCompanyId, OfferSorting sorting, string? offerName) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            dbContext.Offers.AsNoTracking()
                .Where(offer =>
                    offer.OfferTypeId == offerTypeId &&
                    offer.ProviderCompanyId == userCompanyId &&
                    offerStatusIds.Contains(offer.OfferStatusId) &&
                    (offerName == null || EF.Functions.ILike(offer.Name!, $"%{offerName.EscapeForILike()}%")))
                .GroupBy(offer => offer.OfferTypeId),
            sorting switch
            {
                OfferSorting.DateAsc => (IEnumerable<Offer> offers) => offers.OrderBy(offer => offer.DateCreated),
                OfferSorting.DateDesc => (IEnumerable<Offer> offers) => offers.OrderByDescending(offer => offer.DateCreated),
                OfferSorting.NameAsc => (IEnumerable<Offer> offers) => offers.OrderBy(offer => offer.Name),
                OfferSorting.NameDesc => (IEnumerable<Offer> offers) => offers.OrderByDescending(offer => offer.Name),
                _ => throw new ArgumentOutOfRangeException(nameof(sorting), sorting, null)
            },
            offer => new AllOfferData(
                offer.Id,
                offer.Name,
                offer.Documents.Where(document => document.DocumentTypeId == DocumentTypeId.APP_LEADIMAGE && document.DocumentStatusId != DocumentStatusId.INACTIVE).Select(document => document.Id).FirstOrDefault(),
                offer.ProviderCompany!.Name,
                offer.OfferStatusId,
                offer.DateLastChanged
                ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Func<int, int, Task<Pagination.Source<ServiceOverviewData>?>> GetActiveServicesPaginationSource(ServiceOverviewSorting? sorting, ServiceTypeId? serviceTypeId, string languageShortName) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            dbContext.Offers
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
                ServiceOverviewSorting.ProviderAsc => offers => offers.OrderBy(service => service.ProviderCompany!.Name),
                ServiceOverviewSorting.ProviderDesc => offers => offers.OrderByDescending(service => service.ProviderCompany!.Name),
                _ => null
            },
            service => new ServiceOverviewData(
                service.Id,
                service.Name,
                service.ProviderCompany!.Name,
                service.Documents.Where(document => document.DocumentTypeId == DocumentTypeId.SERVICE_LEADIMAGE && document.DocumentStatusId != DocumentStatusId.INACTIVE).Select(document => document.Id).FirstOrDefault(),
                service.ContactEmail,
                service.OfferDescriptions.SingleOrDefault(ln => ln.LanguageShortName == languageShortName)!.DescriptionShort,
                service.LicenseTypeId,
                service.OfferLicenses.FirstOrDefault()!.Licensetext,
                service.ServiceDetails.Select(x => x.ServiceTypeId)))
        .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<OfferDetailData?> GetOfferDetailByIdUntrackedAsync(Guid serviceId, string languageShortName, Guid userCompanyId, OfferTypeId offerTypeId) =>
        dbContext.Offers
            .AsNoTracking()
            .Where(x => x.Id == serviceId && x.OfferTypeId == offerTypeId)
            .Select(offer => new OfferDetailData(
                offer.Id,
                offer.Name,
                offer.ProviderCompany!.Name,
                offer.ContactEmail,
                offer.OfferDescriptions.SingleOrDefault(d => d.LanguageShortName == languageShortName)!.DescriptionLong,
                offer.OfferLicenses.FirstOrDefault()!.Licensetext,
                offer.OfferSubscriptions.Where(os => os.CompanyId == userCompanyId).Select(x => new OfferSubscriptionStateDetailData(x.Id, x.OfferSubscriptionStatusId))
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<ServiceDetailData?> GetServiceDetailByIdUntrackedAsync(Guid serviceId, string languageShortName, Guid userCompanyId) =>
        dbContext.Offers
            .AsNoTracking()
            .AsSplitQuery()
            .Where(x => x.Id == serviceId && x.OfferTypeId == OfferTypeId.SERVICE)
            .Select(offer => new ServiceDetailData(
                offer.Id,
                offer.Name,
                offer.ProviderCompany!.Name,
                offer.Documents.Where(document => document.DocumentTypeId == DocumentTypeId.SERVICE_LEADIMAGE && document.DocumentStatusId != DocumentStatusId.INACTIVE).Select(document => document.Id).FirstOrDefault(),
                offer.ContactEmail,
                offer.OfferDescriptions.SingleOrDefault(d => d.LanguageShortName == languageShortName)!.DescriptionLong,
                offer.OfferLicenses.FirstOrDefault()!.Licensetext,
                offer.MarketingUrl,
                offer.OfferSubscriptions
                    .Where(os => os.CompanyId == userCompanyId)
                    .Select(x => new OfferSubscriptionStateDetailData(x.Id, x.OfferSubscriptionStatusId)),
                offer.ServiceDetails.Select(x => x.ServiceTypeId),
                offer.Documents
                    .Where(doc => doc.DocumentTypeId == DocumentTypeId.ADDITIONAL_DETAILS)
                    .Select(d => new DocumentTypeData(d.DocumentTypeId, d.Id, d.DocumentName)),
                offer.LicenseTypeId,
                offer.TechnicalUserProfiles.Where(x => x.TechnicalUserProfileAssignedUserRoles.Any()).Select(tup => new TechnicalUserRoleData(
                    tup.Id,
                    tup.TechnicalUserProfileAssignedUserRoles.Select(ur => ur.UserRole!.UserRoleText)))
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Func<int, int, Task<Pagination.Source<InReviewAppData>?>> GetAllInReviewStatusAppsAsync(IEnumerable<OfferStatusId> offerStatusIds, OfferSorting? sorting) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            dbContext.Offers.AsNoTracking()
                .Where(offer => offer.OfferTypeId == OfferTypeId.APP && offerStatusIds.Contains(offer.OfferStatusId))
                .GroupBy(offer => offer.OfferTypeId),
            sorting switch
            {
                OfferSorting.DateAsc => (IEnumerable<Offer> offers) => offers.OrderBy(offer => offer.DateCreated),
                OfferSorting.DateDesc => (IEnumerable<Offer> offers) => offers.OrderByDescending(offer => offer.DateCreated),
                OfferSorting.NameAsc => (IEnumerable<Offer> offers) => offers.OrderBy(offer => offer.Name),
                OfferSorting.NameDesc => (IEnumerable<Offer> offers) => offers.OrderByDescending(offer => offer.Name),
                _ => null
            },
            offer => new InReviewAppData(
                offer.Id,
                offer.Name,
                offer.ProviderCompany!.Name,
                offer.OfferStatusId))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<OfferReleaseData?> GetOfferReleaseDataByIdAsync(Guid offerId, OfferTypeId offerTypeId) =>
        dbContext.Offers
            .AsNoTracking()
            .Where(o => o.Id == offerId && o.OfferStatusId == OfferStatusId.CREATED && o.OfferTypeId == offerTypeId)
            .Select(o => new OfferReleaseData(
                o.Name,
                o.ProviderCompany!.Name,
                o.OfferDescriptions.Any(description => description.DescriptionLong == ""),
                o.OfferDescriptions.Any(description => description.DescriptionShort == ""),
                o.UserRoles.Any(),
                o.OfferAssignedPrivacyPolicies.Any(),
                o.Documents.Where(doc => doc.DocumentStatusId == DocumentStatusId.PENDING || doc.DocumentStatusId == DocumentStatusId.LOCKED)
                    .Select(doc => new ValueTuple<Guid, DocumentStatusId, DocumentTypeId>(doc.Id, doc.DocumentStatusId, doc.DocumentTypeId))
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(OfferProviderData? OfferProviderData, bool IsProviderCompanyUser)> GetProviderOfferDataWithConsentStatusAsync(Guid offerId, Guid userCompanyId, OfferTypeId offerTypeId, DocumentTypeId documentTypeId, string languageShortName) =>
        dbContext.Offers
            .AsNoTracking()
            .AsSplitQuery()
            .Where(a => a.Id == offerId && a.OfferTypeId == offerTypeId)
            .Select(offer => new
            {
                IsProviderCompany = offer.ProviderCompanyId == userCompanyId,
                Offer = offer
            })
            .Select(x => new ValueTuple<OfferProviderData?, bool>(
                x.IsProviderCompany
                    ? new OfferProviderData(
                        x.Offer.Name,
                        x.Offer.ProviderCompany!.Name,
                        x.Offer.Documents.Where(document => document.DocumentTypeId == documentTypeId && document.DocumentStatusId != DocumentStatusId.INACTIVE).Select(document => document.Id).FirstOrDefault(),
                        x.Offer.ProviderCompany!.Name,
                        offerTypeId == OfferTypeId.APP
                            ? x.Offer.UseCases.Select(uc => new AppUseCaseData(uc.Id, uc.Name))
                            : null,
                        x.Offer.OfferDescriptions.Select(description => new LocalizedDescription(description.LanguageShortName, description.DescriptionLong, description.DescriptionShort)),
                        x.Offer.OfferType!.AgreementAssignedOfferTypes
                            .Select(aaot => aaot.Agreement)
                            .Select(agreement => new AgreementAssignedOfferData(
                                agreement!.Id,
                                agreement.AgreementDescriptions.SingleOrDefault(x => x.LanguageShortName == languageShortName)!.Description,
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
                            : null,
                        x.Offer.TechnicalUserProfiles.Select(tup => new TechnicalUserRoleData(
                            tup.Id,
                            tup.TechnicalUserProfileAssignedUserRoles.Select(ur => ur.UserRole!.UserRoleText))))
                    : null,
                x.IsProviderCompany))
            .SingleOrDefaultAsync();

    ///<inheritdoc/>
    public Task<(bool OfferExists, bool IsProviderCompanyUser)> IsProviderCompanyUserAsync(Guid offerId, Guid companyId, OfferTypeId offerTypeId) =>
        dbContext.Offers
            .Where(offer => offer.Id == offerId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new ValueTuple<bool, bool>(
                true,
                offer.ProviderCompanyId == companyId
            ))
            .SingleOrDefaultAsync();

    ///<inheritdoc/>
    public Task<(bool OfferExists, bool IsStatusMatching, bool IsUserOfProvider)> GetProviderCompanyUserIdForOfferUntrackedAsync(Guid offerId, Guid companyId, OfferStatusId offerStatusId, OfferTypeId offerTypeId) =>
        dbContext.Offers
            .Where(offer => offer.Id == offerId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new ValueTuple<bool, bool, bool>(
                true,
                offer.OfferStatusId == offerStatusId,
                offer.ProviderCompanyId == companyId
            ))
            .SingleOrDefaultAsync();

    ///<inheritdoc/>
    public Task<(bool OfferStatus, bool IsProviderCompanyUser, bool IsRoleIdExist)> GetAppUserRoleUntrackedAsync(Guid offerId, Guid userCompanyId, OfferStatusId offerStatusId, Guid roleId) =>
        dbContext.Offers
            .Where(offer => offer.Id == offerId)
            .Select(offer => new ValueTuple<bool, bool, bool>(
                (offer.OfferStatusId == offerStatusId),
                offer.ProviderCompanyId == userCompanyId,
                offer.UserRoles.Any(userRole => userRole.Id == roleId)
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<AppUpdateData?> GetAppUpdateData(
        Guid appId,
        Guid userCompanyId,
        IEnumerable<string> languageCodes) =>
        dbContext.Offers
            .AsNoTracking()
            .AsSplitQuery()
            .Where(offer => offer.Id == appId && offer.OfferTypeId == OfferTypeId.APP)
            .Select(x => new AppUpdateData
            (
                x.OfferStatusId,
                x.ProviderCompanyId == userCompanyId,
                x.OfferDescriptions.Select(description => new LocalizedDescription(description.LanguageShortName, description.DescriptionLong, description.DescriptionShort)),
                x.SupportedLanguages.Select(sl => new ValueTuple<string, bool>(sl.ShortName, languageCodes.Any(lc => lc == sl.ShortName))),
                x.UseCases.Select(uc => uc.Id),
                x.OfferLicenses.Select(ol => new ValueTuple<Guid, string, bool>(ol.Id, ol.Licensetext, ol.Offers.Count > 1)).FirstOrDefault(),
                x.OfferAssignedPrivacyPolicies.Select(oapp => oapp.PrivacyPolicyId),
                x.Name,
                x.ProviderCompany!.Name,
                x.SalesManagerId,
                x.ContactEmail,
                x.ContactNumber,
                x.MarketingUrl
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<ServiceUpdateData?> GetServiceUpdateData(Guid serviceId, IEnumerable<ServiceTypeId> serviceTypeIds, Guid userCompanyId) =>
        dbContext.Offers
            .AsNoTracking()
            .Where(offer => offer.Id == serviceId && offer.OfferTypeId == OfferTypeId.SERVICE)
            .Select(x => new ServiceUpdateData
            (
                x.OfferStatusId,
                x.ProviderCompanyId == userCompanyId,
                x.ServiceDetails.Select(st => new ValueTuple<ServiceTypeId, bool>(st.ServiceTypeId, serviceTypeIds.Contains(st.ServiceTypeId))),
                x.OfferLicenses.Select(ol => new ValueTuple<Guid, string, bool>(ol.Id, ol.Licensetext, ol.Offers.Count > 1)).FirstOrDefault(),
                x.OfferDescriptions.Select(description => new LocalizedDescription(description.LanguageShortName, description.DescriptionLong, description.DescriptionShort)),
                x.SalesManagerId
            ))
            .SingleOrDefaultAsync();

    ///<inheritdoc/>
    public Task<(bool OfferExists, string? AppName, Guid? ProviderCompanyId, IEnumerable<string> ClientClientIds)> GetInsertActiveAppUserRoleDataAsync(Guid offerId, OfferTypeId offerTypeId) =>
        dbContext.Offers
            .Where(offer => offer.Id == offerId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new ValueTuple<bool, string?, Guid?, IEnumerable<string>>(
                true,
                offer.Name,
                offer.ProviderCompanyId,
                offer.AppInstances.Select(ai => ai.IamClient!.ClientClientId)
            ))
            .SingleOrDefaultAsync();

    ///<inheritdoc/>
    public Task<(bool IsStatusInReview, string? OfferName, Guid? ProviderCompanyId, bool IsSingleInstance, IEnumerable<(Guid InstanceId, string ClientId)> Instances)> GetOfferStatusDataByIdAsync(Guid offerId, OfferTypeId offerTypeId) =>
        dbContext.Offers
            .Where(offer => offer.Id == offerId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new ValueTuple<bool, string?, Guid?, bool, IEnumerable<(Guid, string)>>(
                offer.OfferStatusId == OfferStatusId.IN_REVIEW,
                offer.Name!,
                offer.ProviderCompanyId,
                offer.AppInstanceSetup != null && offer.AppInstanceSetup.IsSingleInstance,
                offer.AppInstances.Select(x => new ValueTuple<Guid, string>(x.Id, x.IamClient!.ClientClientId))
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(string? OfferName, OfferStatusId OfferStatus, Guid? CompanyId, IEnumerable<DocumentStatusData> ActiveDocumentStatusDatas)> GetOfferDeclineDataAsync(Guid offerId, OfferTypeId offerType) =>
        dbContext.Offers
            .Where(offer => offer.Id == offerId && offer.OfferTypeId == offerType)
            .Select(offer => new ValueTuple<string?, OfferStatusId, Guid?, IEnumerable<DocumentStatusData>>(
                offer.Name,
                offer.OfferStatusId,
                offer.ProviderCompanyId,
                offer.Documents
                    .Where(document => document.DocumentStatusId != DocumentStatusId.INACTIVE)
                    .Select(documents => new DocumentStatusData(
                        documents.Id,
                        documents.DocumentStatusId))))
            .SingleOrDefaultAsync();

    ///<inheritdoc/>
    public Task<(bool IsStatusActive, bool IsUserCompanyProvider)> GetOfferActiveStatusDataByIdAsync(Guid offerId, OfferTypeId offerTypeId, Guid userCompanyId) =>
        dbContext.Offers
            .Where(offer => offer.Id == offerId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new ValueTuple<bool, bool>(
                offer.OfferStatusId == OfferStatusId.ACTIVE,
                offer.ProviderCompanyId == userCompanyId))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public void AddAppAssignedPrivacyPolicies(IEnumerable<(Guid appId, PrivacyPolicyId privacyPolicy)> privacyPolicies) =>
        dbContext.OfferAssignedPrivacyPolicies.AddRange(privacyPolicies.Select(s => new OfferAssignedPrivacyPolicy(s.appId, s.privacyPolicy)));

    /// <inheritdoc />
    public void CreateDeleteAppAssignedPrivacyPolicies(Guid appId, IEnumerable<PrivacyPolicyId> initialPrivacyPolicy, IEnumerable<PrivacyPolicyId> modifyPrivacyPolicy) =>
        dbContext.AddRemoveRange(
            initialPrivacyPolicy,
            modifyPrivacyPolicy,
            privacyPolicy => new OfferAssignedPrivacyPolicy(appId, privacyPolicy));

    ///<inheritdoc/>
    public Task<InReviewOfferData?> GetInReviewAppDataByIdAsync(Guid id, OfferTypeId offerTypeId) =>
        dbContext.Offers.AsNoTracking()
            .AsSplitQuery()
            .Where(offer => offer.Id == id && offer.OfferTypeId == offerTypeId && (offer.OfferStatusId == OfferStatusId.IN_REVIEW || offer.OfferStatusId == OfferStatusId.ACTIVE))
            .Select(offer =>
                new InReviewOfferData(
                    offer.Id,
                    offer.Name,
                    offer.Documents.Where(document => document.DocumentTypeId == DocumentTypeId.APP_LEADIMAGE).Select(document => document.Id).FirstOrDefault(),
                    offer.Documents.Where(document => document.DocumentTypeId == DocumentTypeId.APP_IMAGE).Select(document => document.Id),
                    offer.ProviderCompany!.Name,
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
                    offer.OfferAssignedPrivacyPolicies.Select(p => p.PrivacyPolicyId),
                    offer.LicenseTypeId,
                    offer.OfferStatusId,
                    offer.TechnicalUserProfiles.Select(tup => new TechnicalUserRoleData(
                        tup.Id,
                        tup.TechnicalUserProfileAssignedUserRoles.Select(ur => ur.UserRole!.UserRoleText)))))
            .SingleOrDefaultAsync();

    ///<inheritdoc/>
    public Task<(bool IsStatusActive, bool IsProviderCompanyUser, IEnumerable<LocalizedDescription>? OfferDescriptionDatas)> GetActiveOfferDescriptionDataByIdAsync(Guid appId, OfferTypeId offerTypeId, Guid userCompanyId) =>
        dbContext.Offers
            .Where(offer => offer.Id == appId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new
            {
                Offer = offer,
                IsProviderCompanyUser = offer.ProviderCompanyId == userCompanyId
            })
            .Select(x => new ValueTuple<bool, bool, IEnumerable<LocalizedDescription>?>(
                x.Offer.OfferStatusId == OfferStatusId.ACTIVE,
                x.IsProviderCompanyUser,
                x.IsProviderCompanyUser ? x.Offer.OfferDescriptions.Select(od => new LocalizedDescription(od.LanguageShortName, od.DescriptionLong, od.DescriptionShort)) : null))
            .SingleOrDefaultAsync();

    ///<inheritdoc/>
    public void CreateUpdateDeleteOfferDescriptions(Guid offerId, IEnumerable<LocalizedDescription> initialItems, IEnumerable<(string LanguageCode, string LongDescription, string ShortDescription)> modifiedItems) =>
        dbContext.AddAttachRemoveRange(
            initialItems,
            modifiedItems,
            initial => initial.LanguageCode,
            modify => modify.LanguageCode,
            languageCode => new OfferDescription(offerId, languageCode, null!, null!),
            (initial, modified) => initial.LongDescription == modified.LongDescription && initial.ShortDescription == modified.ShortDescription,
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
        dbContext.OfferAssignedDocuments.Remove(new OfferAssignedDocument(offerId, documentId));

    ///<inheritdoc/>
    public Task<(bool IsValidApp, bool IsOfferType, bool IsOfferStatus, bool IsProviderCompanyUser, AppDeleteData? DeleteData)> GetAppDeleteDataAsync(Guid offerId, OfferTypeId offerTypeId, Guid userCompanyId, OfferStatusId offerStatusId) =>
        dbContext.Offers
            .AsNoTracking()
            .AsSplitQuery()
            .Where(offer => offer.Id == offerId)
            .Select(offer => new
            {
                Offer = offer,
                IsOfferTypeId = offer.OfferTypeId == offerTypeId,
                IsOfferStatusId = offer.OfferStatusId == offerStatusId,
                IsProviderCompanyUser = offer.ProviderCompanyId == userCompanyId,
            })
            .Select(x => new ValueTuple<bool, bool, bool, bool, AppDeleteData?>(
                true,
                x.IsOfferTypeId,
                x.IsOfferStatusId,
                x.IsProviderCompanyUser,
                x.IsOfferTypeId && x.IsOfferStatusId && x.IsProviderCompanyUser
                    ? new AppDeleteData(
                        x.Offer.OfferLicenses.Select(offerlicense => offerlicense.Id),
                        x.Offer.UseCases.Select(uc => uc.Id),
                        x.Offer.OfferAssignedPrivacyPolicies.Select(pp => pp.PrivacyPolicyId),
                        x.Offer.Documents.Select(doc => new ValueTuple<Guid, DocumentStatusId>(doc.Id, doc.DocumentStatusId)),
                        x.Offer.SupportedLanguages.Select(sl => sl.ShortName),
                        x.Offer.Tags.Select(offerTag => offerTag.Name),
                        x.Offer.OfferDescriptions.Select(description => description.LanguageShortName))
                    : null
            ))
            .SingleOrDefaultAsync();

    ///<inheritdoc/>
    public void RemoveOfferAssignedLicenses(IEnumerable<(Guid OfferId, Guid LicenseId)> offerLicenseIds) =>
        dbContext.OfferAssignedLicenses.RemoveRange(offerLicenseIds.Select(offerLicenseId => new OfferAssignedLicense(offerLicenseId.OfferId, offerLicenseId.LicenseId)));

    ///<inheritdoc/>
    public void RemoveOfferAssignedUseCases(IEnumerable<(Guid OfferId, Guid UseCaseId)> offerUseCaseIds) =>
        dbContext.AppAssignedUseCases.RemoveRange(offerUseCaseIds.Select(offerUseCaseId => new AppAssignedUseCase(offerUseCaseId.OfferId, offerUseCaseId.UseCaseId)));

    ///<inheritdoc/>
    public void RemoveOfferAssignedPrivacyPolicies(IEnumerable<(Guid OfferId, PrivacyPolicyId PrivacyPolicyId)> offerPrivacyPolicyIds) =>
        dbContext.OfferAssignedPrivacyPolicies.RemoveRange(offerPrivacyPolicyIds.Select(offerPrivacyPolicyId => new OfferAssignedPrivacyPolicy(offerPrivacyPolicyId.OfferId, offerPrivacyPolicyId.PrivacyPolicyId)));

    ///<inheritdoc/>
    public void RemoveOfferAssignedDocuments(IEnumerable<(Guid OfferId, Guid DocumentId)> offerDocumentIds) =>
        dbContext.OfferAssignedDocuments.RemoveRange(offerDocumentIds.Select(offerDocumentId => new OfferAssignedDocument(offerDocumentId.OfferId, offerDocumentId.DocumentId)));

    ///<inheritdoc/>
    public void RemoveOfferTags(IEnumerable<(Guid OfferId, string TagName)> offerTagNames) =>
        dbContext.OfferTags.RemoveRange(offerTagNames.Select(offerTagName => new OfferTag(offerTagName.OfferId, offerTagName.TagName)));

    ///<inheritdoc/>
    public void RemoveOfferDescriptions(IEnumerable<(Guid OfferId, string LanguageShortName)> offerLanguageShortNames) =>
        dbContext.OfferDescriptions.RemoveRange(offerLanguageShortNames.Select(offerLanguageShortName => new OfferDescription(offerLanguageShortName.OfferId, offerLanguageShortName.LanguageShortName, null!, null!)));

    ///<inheritdoc/>
    public void RemoveOffer(Guid offerId) =>
        dbContext.Offers.Remove(new Offer(offerId, Guid.Empty, default, default));

    ///<inheritdoc/>
    public Task<(bool IsStatusActive, bool IsUserOfProvider, IEnumerable<DocumentStatusData> documentStatusDatas)> GetOfferAssignedAppLeadImageDocumentsByIdAsync(Guid offerId, Guid userCompanyId, OfferTypeId offerTypeId) =>
        dbContext.Offers
            .AsNoTracking()
            .Where(offer => offer.Id == offerId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new ValueTuple<bool, bool, IEnumerable<DocumentStatusData>>(
                offer.OfferStatusId == OfferStatusId.ACTIVE,
                offer.ProviderCompanyId == userCompanyId,
                offer.Documents.Where(doc => doc.DocumentTypeId == DocumentTypeId.APP_LEADIMAGE)
                    .Select(doc => new DocumentStatusData(doc.Id, doc.DocumentStatusId))))
            .SingleOrDefaultAsync();

    ///<inheritdoc/>
    public Task<ServiceDetailsData?> GetServiceDetailsByIdAsync(Guid serviceId) =>
        dbContext.Offers
            .AsNoTracking()
            .AsSplitQuery()
            .Where(service => service.Id == serviceId && service.OfferTypeId == OfferTypeId.SERVICE && (service.OfferStatusId == OfferStatusId.IN_REVIEW || service.OfferStatusId == OfferStatusId.ACTIVE))
            .Select(offer => new ServiceDetailsData(
                offer.Id,
                offer.Name,
                offer.ServiceDetails.Select(x => x.ServiceTypeId),
                offer.ProviderCompany!.Name,
                offer.OfferDescriptions.Select(description => new LocalizedDescription(description.LanguageShortName, description.DescriptionLong, description.DescriptionShort)),
                offer.Documents.Select(d => new DocumentTypeData(d.DocumentTypeId, d.Id, d.DocumentName)),
                offer.MarketingUrl,
                offer.ContactEmail,
                offer.ContactNumber,
                offer.OfferStatusId,
                offer.LicenseTypeId,
                offer.TechnicalUserProfiles.Select(tup => new TechnicalUserRoleData(
                    tup.Id,
                    tup.TechnicalUserProfileAssignedUserRoles.Select(ur => ur.UserRole!.UserRoleText)))
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Func<int, int, Task<Pagination.Source<AllOfferStatusData>?>> GetCompanyProvidedServiceStatusDataAsync(IEnumerable<OfferStatusId> offerStatusIds, OfferTypeId offerTypeId, Guid userCompanyId, OfferSorting? sorting, string? offerName) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            dbContext.Offers.AsNoTracking()
                .Where(offer => offer.OfferTypeId == offerTypeId &&
                    offer.ProviderCompanyId == userCompanyId &&
                    offerStatusIds.Contains(offer.OfferStatusId) && (offerName == null || EF.Functions.ILike(offer.Name!, $"%{offerName!.EscapeForILike()}%")))
                .GroupBy(offer => offer.OfferTypeId),
            sorting switch
            {
                OfferSorting.DateAsc => (IEnumerable<Offer> offers) => offers.OrderBy(offer => offer.DateCreated),
                OfferSorting.DateDesc => (IEnumerable<Offer> offers) => offers.OrderByDescending(offer => offer.DateCreated),
                _ => null
            },
            offer => new AllOfferStatusData(
                offer.Id,
                offer.Name,
                offer.Documents.Where(document => document.DocumentTypeId == DocumentTypeId.SERVICE_LEADIMAGE && document.DocumentStatusId != DocumentStatusId.INACTIVE).Select(document => document.Id).FirstOrDefault(),
                offer.ProviderCompany!.Name,
                offer.OfferStatusId,
                offer.DateLastChanged
                ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Func<int, int, Task<Pagination.Source<InReviewServiceData>?>> GetAllInReviewStatusServiceAsync(IEnumerable<OfferStatusId> offerStatusIds, OfferTypeId offerTypeId, OfferSorting? sorting, string? offerName, string languageShortName, string defaultLanguageShortName) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            dbContext.Offers.AsNoTracking()
                .Where(offer => offer.OfferTypeId == offerTypeId && offerStatusIds.Contains(offer.OfferStatusId) && (offerName == null || EF.Functions.ILike(offer.Name!, $"%{offerName!.EscapeForILike()}%")))
                .GroupBy(offer => offer.OfferTypeId),
            sorting switch
            {
                OfferSorting.DateAsc => (IEnumerable<Offer> offers) => offers.OrderBy(offer => offer.DateCreated),
                OfferSorting.DateDesc => (IEnumerable<Offer> offers) => offers.OrderByDescending(offer => offer.DateCreated),
                OfferSorting.NameAsc => (IEnumerable<Offer> offers) => offers.OrderBy(offer => offer.Name),
                OfferSorting.NameDesc => (IEnumerable<Offer> offers) => offers.OrderByDescending(offer => offer.Name),
                _ => null
            },
            offer => new InReviewServiceData(
                offer.Id,
                offer.Name,
                offer.OfferStatusId,
                offer.ProviderCompany!.Name,
                offer.OfferDescriptions.SingleOrDefault(d => d.LanguageShortName == languageShortName)!.DescriptionShort
                           ?? offer.OfferDescriptions.SingleOrDefault(d => d.LanguageShortName == defaultLanguageShortName)!.DescriptionShort
                        ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(OfferStatusId OfferStatus, bool IsUserOfProvidingCompany, AppInstanceSetupTransferData? SetupTransferData, IEnumerable<(Guid AppInstanceId, Guid ClientId, string ClientClientId)> AppInstanceData)> GetOfferWithSetupDataById(Guid offerId, Guid userCompanyId, OfferTypeId offerTypeId) =>
        dbContext.Offers
            .AsNoTracking()
            .Where(x => x.OfferTypeId == offerTypeId && x.Id == offerId)
            .Select(o => new ValueTuple<OfferStatusId, bool, AppInstanceSetupTransferData?, IEnumerable<(Guid, Guid, string)>>(
                o.OfferStatusId,
                o.ProviderCompanyId == userCompanyId,
                o.AppInstanceSetup == null ?
                    null :
                    new AppInstanceSetupTransferData(o.AppInstanceSetup!.AppId, o.AppInstanceSetup!.IsSingleInstance, o.AppInstanceSetup!.InstanceUrl),
                o.AppInstances.Select(x => new ValueTuple<Guid, Guid, string>(x.Id, x.IamClientId, x.IamClient!.ClientClientId))
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public AppInstanceSetup CreateAppInstanceSetup(Guid offerId, Action<AppInstanceSetup>? setOptionalParameter)
    {
        var appInstanceSetup = dbContext.AppInstanceSetups.Add(new AppInstanceSetup(Guid.NewGuid(), offerId)).Entity;
        setOptionalParameter?.Invoke(appInstanceSetup);
        return appInstanceSetup;
    }

    /// <inheritdoc />
    public Task<SingleInstanceOfferData?> GetSingleInstanceOfferData(Guid offerId, OfferTypeId offerTypeId) =>
        dbContext.Offers.Where(o => o.Id == offerId && o.OfferTypeId == offerTypeId)
            .Select(o => new SingleInstanceOfferData(
                o.ProviderCompany!.Id,
                o.Name,
                o.ProviderCompany.BusinessPartnerNumber,
                o.AppInstanceSetup != null && o.AppInstanceSetup.IsSingleInstance,
                o.AppInstances.Select(x => new ValueTuple<Guid, string>(x.Id, x.IamClient!.ClientClientId))
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public void AttachAndModifyAppInstanceSetup(Guid appInstanceSetupId, Guid offerId, Action<AppInstanceSetup> setOptionalParameters, Action<AppInstanceSetup>? initializeParameter = null)
    {
        var entity = new AppInstanceSetup(appInstanceSetupId, offerId);
        initializeParameter?.Invoke(entity);
        var appInstanceSetup = dbContext.Attach(entity).Entity;
        setOptionalParameters.Invoke(appInstanceSetup);
    }

    /// <inheritdoc />
    public Task<(bool IsSingleInstance, IEnumerable<IEnumerable<UserRoleData>> ServiceAccountProfiles, string? OfferName)> GetServiceAccountProfileData(Guid offerId, OfferTypeId offerTypeId) =>
        dbContext.Offers.Where(x => x.Id == offerId && x.OfferTypeId == offerTypeId)
            .Select(o => new ValueTuple<bool, IEnumerable<IEnumerable<UserRoleData>>, string?>(
                o.AppInstanceSetup != null && o.AppInstanceSetup.IsSingleInstance,
                o.TechnicalUserProfiles.Where(x => x.TechnicalUserProfileAssignedUserRoles.Any()).Select(tup => tup.TechnicalUserProfileAssignedUserRoles.Select(ur => new UserRoleData(ur.UserRole!.Id, ur.UserRole.Offer!.AppInstances.First().IamClient!.ClientClientId, ur.UserRole.UserRoleText))),
                o.Name
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(bool IsSingleInstance, IEnumerable<IEnumerable<UserRoleData>> ServiceAccountProfiles, string? OfferName)> GetServiceAccountProfileDataForSubscription(Guid subscriptionId) =>
        dbContext.OfferSubscriptions.Where(x => x.Id == subscriptionId)
            .Select(o => new ValueTuple<bool, IEnumerable<IEnumerable<UserRoleData>>, string?>(
                o.Offer!.AppInstanceSetup != null && o.Offer.AppInstanceSetup.IsSingleInstance,
                o.Offer.TechnicalUserProfiles.Where(x => x.TechnicalUserProfileAssignedUserRoles.Any()).Select(tup => tup.TechnicalUserProfileAssignedUserRoles.Select(ur => new UserRoleData(ur.UserRole!.Id, ur.UserRole.Offer!.AppInstances.First().IamClient!.ClientClientId, ur.UserRole.UserRoleText))),
                o.Offer.Name
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<DocumentTypeData> GetActiveOfferDocumentTypeDataOrderedAsync(Guid offerId, Guid userCompanyId, OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIds) =>
        dbContext.OfferAssignedDocuments
        .Where(oad => oad.OfferId == offerId &&
            oad.Offer!.OfferStatusId == OfferStatusId.ACTIVE &&
            oad.Offer.OfferTypeId == offerTypeId &&
            oad.Offer.ProviderCompanyId == userCompanyId &&
            oad.Document!.DocumentStatusId != DocumentStatusId.INACTIVE &&
            documentTypeIds.Contains(oad.Document!.DocumentTypeId))
        .OrderBy(oad => oad.Document!.DocumentTypeId)
        .Select(oad => new DocumentTypeData(
            oad.Document!.DocumentTypeId,
            oad.Document.Id,
            oad.Document.DocumentName))
        .ToAsyncEnumerable();

    ///<inheritdoc/>
    public Task<(bool IsStatusActive, bool IsUserOfProvider, DocumentTypeId DocumentTypeId, DocumentStatusId DocumentStatusId)> GetOfferAssignedAppDocumentsByIdAsync(Guid offerId, Guid userCompanyId, OfferTypeId offerTypeId, Guid documentId) =>
        dbContext.OfferAssignedDocuments
            .AsNoTracking()
            .Where(oad => oad.OfferId == offerId &&
                oad.DocumentId == documentId &&
                oad.Offer!.OfferTypeId == offerTypeId)
            .Select(oad => new ValueTuple<bool, bool, DocumentTypeId, DocumentStatusId>(
                oad.Offer!.OfferStatusId == OfferStatusId.ACTIVE,
                oad.Offer.ProviderCompanyId == userCompanyId,
                oad.Document!.DocumentTypeId,
                oad.Document.DocumentStatusId))
            .SingleOrDefaultAsync();
}
