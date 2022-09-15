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

using CatenaX.NetworkServices.Apps.Service.ViewModels;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace CatenaX.NetworkServices.Apps.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IAppReleaseBusinessLogic"/>.
/// </summary>
public class AppReleaseBusinessLogic : IAppReleaseBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly AppsSettings _settings;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories"></param>
    /// <param name="settings"></param>
    public AppReleaseBusinessLogic(IPortalRepositories portalRepositories, IOptions<AppsSettings> settings)
    {
        _portalRepositories = portalRepositories;
        _settings = settings.Value;
    }
    
    /// <inheritdoc/>
    public  Task UpdateAppAsync(Guid appId, AppEditableDetail updateModel, string userId)
    {
        if (appId == Guid.Empty)
        {
            throw new ControllerArgumentException($"AppId must not be empty");
        }
        if (updateModel.Descriptions.Any(item => string.IsNullOrWhiteSpace(item.LanguageCode)))
        {
            throw new ControllerArgumentException("Language Code must not be empty");
        }
        if (updateModel.Images.Any(image => string.IsNullOrWhiteSpace(image)))
        {
            throw new ControllerArgumentException("ImageUrl must not be empty");
        }
        return EditAppAsync(appId, updateModel, userId);
    }

    private async Task EditAppAsync(Guid appId, AppEditableDetail updateModel, string userId)
    {
        var appRepository = _portalRepositories.GetInstance<IOfferRepository>();
        var appResult = await appRepository.GetAppDetailsForUpdateAsync(appId, userId).ConfigureAwait(false);
        if (appResult == default)
        {
            throw new NotFoundException($"app {appId} does not exist");
        }
        if (!appResult.IsProviderUser)
        {
            throw new ForbiddenException($"user {userId} is not eligible to edit app {appId}");
        }
        if (!appResult.IsAppCreated)
        {
            throw new ConflictException($"app {appId} is not in status CREATED");
        }
        _portalRepositories.Attach(new Offer(appId), app =>
        {
            if (appResult.ContactEmail != updateModel.ContactEmail)
            {
                app.ContactEmail = updateModel.ContactEmail;
            }
            if (appResult.ContactNumber != updateModel.ContactNumber)
            {
                app.ContactNumber = updateModel.ContactNumber;
            }
            if (appResult.MarketingUrl != updateModel.ProviderUri)
            {
                app.MarketingUrl = updateModel.ProviderUri;
            }
        });

        UpsertRemoveAppDescription(appId, updateModel.Descriptions, appResult.Descriptions, appRepository);
        UpsertRemoveAppDetailImage(appId, updateModel.Images, appResult.ImageUrls, appRepository);
        
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

   private void UpsertRemoveAppDescription(Guid appId, IEnumerable<Localization> UpdateDescriptions, IEnumerable<(string LanguageShortName, string DescriptionLong, string DescriptionShort)> ExistingDescriptions, IOfferRepository appRepository)
    {
        appRepository.AddOfferDescriptions(
            UpdateDescriptions.ExceptBy(ExistingDescriptions.Select(d => d.LanguageShortName), updateDescription => updateDescription.LanguageCode)
                .Select(updateDescription => new ValueTuple<Guid, string, string, string>(appId, updateDescription.LanguageCode, updateDescription.LongDescription, updateDescription.ShortDescription))
        );

        _portalRepositories.RemoveRange<OfferDescription>(
            ExistingDescriptions.ExceptBy(UpdateDescriptions.Select(d => d.LanguageCode), existingDescription => existingDescription.LanguageShortName)
                .Select(existingDescription => new OfferDescription(appId, existingDescription.LanguageShortName))
        );

        foreach (var (languageCode, longDescription, shortDescription)
            in UpdateDescriptions.IntersectBy(
                ExistingDescriptions.Select(d => d.LanguageShortName), updateDscr => updateDscr.LanguageCode)
                    .Select(updateDscr => (updateDscr.LanguageCode, updateDscr.LongDescription, updateDscr.ShortDescription)))
        {
            var existing = ExistingDescriptions.First(d => d.LanguageShortName == languageCode);
            _portalRepositories.Attach(new OfferDescription(appId, languageCode), appdesc =>
            {
                if (longDescription != existing.DescriptionLong)
                {
                    appdesc.DescriptionLong = longDescription;
                }
                if (shortDescription != existing.DescriptionShort)
                {
                    appdesc.DescriptionShort = shortDescription;
                }
            });
        }
    }

    private void UpsertRemoveAppDetailImage(Guid appId, IEnumerable<string> UpdateUrls, IEnumerable<(Guid Id, string Url)> ExistingImages, IOfferRepository appRepository)
    {
        appRepository.AddAppDetailImages(
            UpdateUrls.Except(ExistingImages.Select(image => image.Url))
                .Select(url => new ValueTuple<Guid,string>(appId, url))
        );

        _portalRepositories.RemoveRange(
            ExistingImages.ExceptBy(UpdateUrls, image => image.Url)
                .Select(image => new OfferDetailImage(image.Id))
        );
    }

    /// <inheritdoc/>
    public Task<int> CreateAppDocumentAsync(Guid appId, DocumentTypeId documentTypeId, IFormFile document, string userId, CancellationToken cancellationToken)
    {
        if (appId == Guid.Empty)
        {
            throw new ArgumentException($"AppId must not be empty");
        }
        if (!_settings.DocumentTypeIds.Contains(documentTypeId))
        {
            throw new ArgumentException($"documentType must be either :{string.Join(",", _settings.DocumentTypeIds)}");
        }
        if (string.IsNullOrEmpty(document.FileName))
        {
            throw new ArgumentException("File name is must not be null");
        }
        // Check if document is a pdf file (also see https://www.rfc-editor.org/rfc/rfc3778.txt)
        if (!document.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnsupportedMediaTypeException("Only .pdf files are allowed.");
        }
        return UploadAppDoc(appId, documentTypeId, document, userId, cancellationToken);
    }

    private async Task<int> UploadAppDoc(Guid appId, DocumentTypeId documentTypeId, IFormFile document, string userId, CancellationToken cancellationToken)
    {
        var companyUserId = await _portalRepositories.GetInstance<IAppReleaseRepository>().GetCompanyUserIdForAppUntrackedAsync(appId, userId).ConfigureAwait(false);
        if (companyUserId == Guid.Empty)
        {
            throw new ForbiddenException($"userId {userId} is not assigned with App {appId}");
        }
        var documentName = document.FileName;
        using (var sha512Hash = SHA512.Create())
        {
            using (var ms = new MemoryStream((int)document.Length))
            {
                await document.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
                var hash = sha512Hash.ComputeHash(ms);
                var documentContent = ms.GetBuffer();
                if (ms.Length != document.Length || documentContent.Length != document.Length)
                {
                    throw new ControllerArgumentException($"document {document.FileName} transmitted length {document.Length} doesn't match actual length {ms.Length}.");
                }
                var doc = _portalRepositories.GetInstance<IDocumentRepository>().CreateDocument(companyUserId, documentName, documentContent, hash, documentTypeId);
                _portalRepositories.GetInstance<IAppReleaseRepository>().CreateOfferAssignedDocument(appId, doc.Id);
                return await _portalRepositories.SaveAsync().ConfigureAwait(false);
            }
        }
    }
    
    /// <inheritdoc/>
    public Task AddAppUserRoleAsync(Guid appId, IEnumerable<AppUserRole> appAssignedDesc, string userId)
    {
        if (appId == Guid.Empty)
        {
            throw new ControllerArgumentException($"AppId must not be empty");
        }
        var descriptions = appAssignedDesc.SelectMany(x => x.descriptions).Where(item => !string.IsNullOrWhiteSpace(item.languageCode)).Distinct();
        if (!descriptions.Any())
        {
            throw new ControllerArgumentException($"Language Code must not be empty");
        }

        return InsertAppUserRoleAsync(appId, appAssignedDesc, userId);
    }

    private async Task InsertAppUserRoleAsync(Guid appId, IEnumerable<AppUserRole> appAssignedDesc, string userId)
    {
        var appReleaseRepository = _portalRepositories.GetInstance<IAppReleaseRepository>();

        if (!await appReleaseRepository.IsProviderCompanyUserAsync(appId, userId).ConfigureAwait(false))
        {
            throw new NotFoundException($"Cannot identify companyId or appId : User CompanyId is not associated with the same company as AppCompanyId");
        }

        foreach (var indexItem in appAssignedDesc)
        {
            var appRole = appReleaseRepository.CreateAppUserRole(appId, indexItem.role);
            foreach (var item in indexItem.descriptions)
            {
                appReleaseRepository.CreateAppUserRoleDescription(appRole.Id, item.languageCode, item.description);
            }
        }
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
    
    /// <inheritdoc/>
    public IAsyncEnumerable<AgreementData> GetOfferAgreementDataAsync()=>
        _portalRepositories.GetInstance<IAppReleaseRepository>().GetAgreements(AgreementCategoryId.APP_CONTRACT);

    /// <inheritdoc/>
    public async Task<OfferAgreementConsent> GetOfferAgreementConsentById(Guid appId, string userId)
    {
        var result = await _portalRepositories.GetInstance<IAppReleaseRepository>().GetOfferAgreementConsentById(appId, userId, AgreementCategoryId.APP_CONTRACT).ConfigureAwait(false);
        if (result == null)
        {
            throw new ForbiddenException($"UserId {userId} is not assigned with Offer {appId}");
        }
        return result;
    }
    
    /// <inheritdoc/>
    public async Task<int> SubmitOfferConsentAsync(Guid appId, OfferAgreementConsent offerAgreementConsent, string userId)
    {
        var appReleaseRepository = _portalRepositories.GetInstance<IAppReleaseRepository>();
        var companyRolesRepository = _portalRepositories.GetInstance<ICompanyRolesRepository>();
        var offerAgreementConsentData = await appReleaseRepository.GetOfferAgreementConsent(appId, userId, OfferStatusId.CREATED, AgreementCategoryId.APP_CONTRACT).ConfigureAwait(false);
        if (offerAgreementConsentData == null)
        {
            throw new NotFoundException($"application {appId} does not exist");
        }
        if (offerAgreementConsentData.companyUserId == Guid.Empty)
        {
            throw new ForbiddenException($"UserId {userId} is not assigned with Offer {appId}");
        }
        var companyId = offerAgreementConsentData.companyId;
        var companyUserId = offerAgreementConsentData.companyUserId;

        foreach (var agreementId in offerAgreementConsents.Agreements.ExceptBy(offerAgreementConsentData.Agreements.Select(d=>d.AgreementId),input=>input.AgreementId)
                .Select(input =>input.AgreementId))
        {
            companyRolesRepository.CreateConsent(agreementId, companyId, companyUserId, ConsentStatusId.ACTIVE);
        }
        foreach (var (agreementId,consentStatus)
            in offerAgreementConsents.Agreements.IntersectBy(
                offerAgreementConsentData.Agreements.Select(d => d.AgreementId), offerConsentInput => offerConsentInput.AgreementId)
                    .Select(offerConsentInput => (offerConsentInput.AgreementId, offerConsentInput.ConsentStatusId)))
        {
            var existing = offerAgreementConsentData.Agreements.First(d => d.AgreementId == agreementId);
            _portalRepositories.Attach(new Consent(existing.ConsentId), consen =>
            {
                if (consentStatus != existing.ConsentStatusId)
                {
                    consen.ConsentStatusId = consentStatus;
                }
            });
            
        }

        return await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}
