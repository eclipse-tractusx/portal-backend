/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using System.Security.Cryptography;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IAppReleaseBusinessLogic"/>.
/// </summary>
public class AppReleaseBusinessLogic : IAppReleaseBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly AppsSettings _settings;
    private readonly IOfferService _offerService;
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories"></param>
    /// <param name="settings"></param>
    /// <param name="offerService"></param>
    public AppReleaseBusinessLogic(IPortalRepositories portalRepositories, IOptions<AppsSettings> settings, IOfferService offerService)
    {
        _portalRepositories = portalRepositories;
        _settings = settings.Value;
        _offerService = offerService;
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
    public Task<int> CreateAppDocumentAsync(Guid appId, DocumentTypeId documentTypeId, IFormFile document, string iamUserId, CancellationToken cancellationToken)
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
        return UploadAppDoc(appId, documentTypeId, document, iamUserId, cancellationToken);
    }

    private async Task<int> UploadAppDoc(Guid appId, DocumentTypeId documentTypeId, IFormFile document, string iamUserId, CancellationToken cancellationToken)
    {
        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        var result = await offerRepository.GetProviderCompanyUserIdForOfferUntrackedAsync(appId, iamUserId, OfferStatusId.CREATED, OfferTypeId.APP).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"app {appId} does not exist");
        }
        var companyUserId = result.CompanyUserId;
        if (companyUserId == Guid.Empty)
        {
            throw new ForbiddenException($"user {iamUserId} is not a member of the providercompany of app {appId}");
        }
        var documentName = document.FileName;
        using var sha512Hash = SHA512.Create();
        using var ms = new MemoryStream((int)document.Length);

        await document.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
        var hash = sha512Hash.ComputeHash(ms);
        var documentContent = ms.GetBuffer();
        if (ms.Length != document.Length || documentContent.Length != document.Length)
        {
            throw new ControllerArgumentException($"document {document.FileName} transmitted length {document.Length} doesn't match actual length {ms.Length}.");
        }
        
        var doc = _portalRepositories.GetInstance<IDocumentRepository>().CreateDocument(documentName, documentContent, hash, documentTypeId, x =>
        {
            x.CompanyUserId = companyUserId;
        });
        _portalRepositories.GetInstance<IOfferRepository>().CreateOfferAssignedDocument(appId, doc.Id);
        return await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
    
    /// <inheritdoc/>
    public Task<IEnumerable<AppRoleData>> AddAppUserRoleAsync(Guid appId, IEnumerable<AppUserRole> appAssignedDesc, string iamUserId)
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

        return InsertAppUserRoleAsync(appId, appAssignedDesc, iamUserId);
    }

    private async Task<IEnumerable<AppRoleData>> InsertAppUserRoleAsync(Guid appId, IEnumerable<AppUserRole> appAssignedDesc, string iamUserId)
    {
        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();

        var result = await _portalRepositories.GetInstance<IOfferRepository>().IsProviderCompanyUserAsync(appId, iamUserId, OfferTypeId.APP).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"app {appId} does not exist");
        }
        if (!result.IsProviderCompanyUser)
        {
            throw new ForbiddenException($"user {iamUserId} is not a member of the providercompany of app {appId}");
        }
        var roleData = new List<AppRoleData>();
        foreach (var indexItem in appAssignedDesc)
        {
            var appRole = userRolesRepository.CreateAppUserRole(appId, indexItem.role);
            roleData.Add(new AppRoleData(appRole.Id, indexItem.role));
            foreach (var item in indexItem.descriptions)
            {
                userRolesRepository.CreateAppUserRoleDescription(appRole.Id, item.languageCode, item.description);
            }
        }
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return roleData;
    }
    
    /// <inheritdoc/>
    public IAsyncEnumerable<AgreementData> GetOfferAgreementDataAsync()=>
        _offerService.GetOfferTypeAgreementsAsync(OfferTypeId.APP);

    /// <inheritdoc/>
    public async Task<OfferAgreementConsent> GetOfferAgreementConsentById(Guid appId, string userId)
    {
        return await _offerService.GetProviderOfferAgreementConsentById(appId, userId, OfferTypeId.APP).ConfigureAwait(false);
    }
    
    /// <inheritdoc/>
    public Task<int> SubmitOfferConsentAsync(Guid appId, OfferAgreementConsent offerAgreementConsents, string userId)
    {
        if (appId == Guid.Empty)
        {
            throw new ControllerArgumentException($"AppId must not be empty");
        }
        return SubmitOfferConsentInternalAsync(appId, offerAgreementConsents, userId);
    }

    /// <inheritdoc/>
    private Task<int> SubmitOfferConsentInternalAsync(Guid appId, OfferAgreementConsent offerAgreementConsents, string userId) =>
        _offerService.CreaeteOrUpdateProviderOfferAgreementConsent(appId, offerAgreementConsents, userId, OfferTypeId.APP);
    
    /// <inheritdoc/>
    public Task<OfferProviderResponse> GetAppDetailsForStatusAsync(Guid appId, string userId) =>
        _offerService.GetProviderOfferDetailsForStatusAsync(appId, userId, OfferTypeId.APP);
}
