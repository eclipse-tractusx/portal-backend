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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Security.Cryptography;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IAppReleaseBusinessLogic"/>.
/// </summary>
public class AppReleaseBusinessLogic : IAppReleaseBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly AppsSettings _settings;
    private readonly IOfferService _offerService;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories"></param>
    /// <param name="settings"></param>
    /// <param name="offerService"></param>
    /// <param name="notificationService"></param>
    public AppReleaseBusinessLogic(IPortalRepositories portalRepositories, IOptions<AppsSettings> settings, IOfferService offerService, INotificationService notificationService)
    {
        _portalRepositories = portalRepositories;
        _settings = settings.Value;
        _offerService = offerService;
        _notificationService = notificationService;
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
        appRepository.AttachAndModifyOffer(appId, app =>
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

        _offerService.UpsertRemoveOfferDescription(appId, updateModel.Descriptions, appResult.Descriptions);
        UpsertRemoveAppDetailImage(appId, updateModel.Images, appResult.ImageUrls, appRepository);
        
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private static void UpsertRemoveAppDetailImage(Guid appId, IEnumerable<string> UpdateUrls, IEnumerable<(Guid Id, string Url)> ExistingImages, IOfferRepository appRepository)
    {
        appRepository.AddAppDetailImages(
            UpdateUrls.Except(ExistingImages.Select(image => image.Url))
                .Select(url => new ValueTuple<Guid,string>(appId, url))
        );

        appRepository.RemoveOfferDetailImages(
            ExistingImages.ExceptBy(UpdateUrls, image => image.Url)
                .Select(image => image.Id)
        );
    }

    /// <inheritdoc/>
    public Task<int> CreateAppDocumentAsync(Guid appId, DocumentTypeId documentTypeId, IFormFile document, string iamUserId, CancellationToken cancellationToken)
    {
        if (appId == Guid.Empty)
        {
            throw new ControllerArgumentException($"AppId must not be empty");
        }
        if (!_settings.DocumentTypeIds.Contains(documentTypeId))
        {
            throw new ControllerArgumentException($"documentType must be either: {string.Join(",", _settings.DocumentTypeIds)}");
        }
        if (string.IsNullOrEmpty(document.FileName))
        {
            throw new ControllerArgumentException("File name is must not be null");
        }
        // Check if document is a pdf,jpeg and png file (also see https://www.rfc-editor.org/rfc/rfc3778.txt)
        if (!_settings.ContentTypeSettings.Contains(document.ContentType))
        {
            throw new UnsupportedMediaTypeException($"Document type not supported. File with contentType :{string.Join(",", _settings.ContentTypeSettings)} are allowed.");
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
        ValidateAppUserRole(appId, appAssignedDesc);
        return InsertAppUserRoleAsync(appId, appAssignedDesc, iamUserId);
    }

    private async Task<IEnumerable<AppRoleData>> InsertAppUserRoleAsync(Guid appId, IEnumerable<AppUserRole> appAssignedDesc, string iamUserId)
    {
        var result = await _portalRepositories.GetInstance<IOfferRepository>().IsProviderCompanyUserAsync(appId, iamUserId, OfferTypeId.APP).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"app {appId} does not exist");
        }
        if (!result.IsProviderCompanyUser)
        {
            throw new ForbiddenException($"user {iamUserId} is not a member of the providercompany of app {appId}");
        }
        var roleData = CreateUserRolesWithDescriptions(appId, appAssignedDesc);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return roleData;
    }

    /// <inheritdoc/>
    public Task<IEnumerable<AppRoleData>> AddActiveAppUserRoleAsync(Guid appId, IEnumerable<AppUserRole> appUserRolesDescription, string iamUserId)
    {
        ValidateAppUserRole(appId, appUserRolesDescription);
        return InsertActiveAppUserRoleAsync(appId, appUserRolesDescription, iamUserId);
    }

    private async Task<IEnumerable<AppRoleData>> InsertActiveAppUserRoleAsync(Guid appId, IEnumerable<AppUserRole> appAssignedDesc, string iamUserId)
    {
        var result = await _portalRepositories.GetInstance<IOfferRepository>().GetOfferNameProviderCompanyUserAsync(appId, iamUserId, OfferTypeId.APP).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"app {appId} does not exist");
        }
        if (result.CompanyUserId == Guid.Empty)
        {
            throw new ForbiddenException($"user {iamUserId} is not a member of the providercompany of app {appId}");
        }

        if (result.ProviderCompanyId == null)
        {
            throw new ConflictException($"App {appId} providing company is not yet set.");
        }

        var roleData = CreateUserRolesWithDescriptions(appId, appAssignedDesc);
        var notificationContent = new
        {
            AppName = result.AppName,
            Roles = roleData.Select(x => x.roleName)
        };
        var serializeNotificationContent = JsonSerializer.Serialize(notificationContent);
        var content = _settings.ActiveAppNotificationTypeIds.Select(typeId => new ValueTuple<string?, NotificationTypeId>(serializeNotificationContent, typeId));
        await _notificationService.CreateNotifications(_settings.ActiveAppCompanyAdminRoles, result.CompanyUserId, content, result.ProviderCompanyId.Value).ConfigureAwait(false);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return roleData;
    }

    private static void ValidateAppUserRole(Guid appId, IEnumerable<AppUserRole> appUserRolesDescription)
    {
        if (appId == Guid.Empty)
        {
            throw new ControllerArgumentException($"AppId must not be empty");
        }
        var descriptions = appUserRolesDescription.SelectMany(x => x.descriptions).Where(item => !string.IsNullOrWhiteSpace(item.languageCode)).Distinct();
        if (!descriptions.Any())
        {
            throw new ControllerArgumentException($"Language Code must not be empty");
        }
    }

    private IEnumerable<AppRoleData>CreateUserRolesWithDescriptions(Guid appId, IEnumerable<AppUserRole> appAssignedDesc)
    {
        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
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
        _offerService.CreateOrUpdateProviderOfferAgreementConsent(appId, offerAgreementConsents, userId, OfferTypeId.APP);
    
    /// <inheritdoc/>
    public Task<OfferProviderResponse> GetAppDetailsForStatusAsync(Guid appId, string userId) =>
        _offerService.GetProviderOfferDetailsForStatusAsync(appId, userId, OfferTypeId.APP);

    /// <inheritdoc/>
    public async Task DeleteAppRoleAsync(Guid appId, Guid roleId, string iamUserId)
    {
        var appUserRole = await _portalRepositories.GetInstance<IOfferRepository>().GetAppUserRoleUntrackedAsync(appId, iamUserId, OfferStatusId.CREATED, roleId).ConfigureAwait(false);
        if (!appUserRole.IsProviderCompanyUser)
        {
            throw new ForbiddenException($"user {iamUserId} is not a member of the providercompany of app {appId}");
        }
        if (!appUserRole.OfferStatus)
        {
            throw new ControllerArgumentException($"AppId must be in Created State");
        }
        if (!appUserRole.IsRoleIdExist)
        {
            throw new NotFoundException($"role {roleId} does not exist");
        }
        try
        {
            _portalRepositories.GetInstance<IUserRolesRepository>().DeleteUserRole(roleId);
            await _portalRepositories.SaveAsync().ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new NotFoundException($"role {roleId} does not exist");
        }
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<CompanyUserNameData> GetAppProviderSalesManagersAsync(string iamUserId) =>
       _portalRepositories.GetInstance<IUserRolesRepository>().GetUserDataByAssignedRoles(iamUserId,_settings.SalesManagerRoles);
    
    /// <inheritdoc/>
    public Task<Guid> AddAppAsync(AppRequestModel appRequestModel, string iamUserId)
    {
        var emptyLanguageCodes = appRequestModel.SupportedLanguageCodes.Where(item => String.IsNullOrWhiteSpace(item));
        if (emptyLanguageCodes.Any())
        {
            throw new ControllerArgumentException("Language Codes must not be null or empty", nameof(appRequestModel.SupportedLanguageCodes)); 
        }
        
        var emptyUseCaseIds = appRequestModel.UseCaseIds.Where(item => item == Guid.Empty);
        if (emptyUseCaseIds.Any())
        {
            throw new ControllerArgumentException("Use Case Ids must not be null or empty", nameof(appRequestModel.UseCaseIds));
        }
        
        return this.CreateAppAsync(appRequestModel, iamUserId);
    }

    private async Task<Guid> CreateAppAsync(AppRequestModel appRequestModel, string iamUserId)
    {   
        Guid companyId;
        if(appRequestModel.SalesManagerId.HasValue)
        {
            companyId = await _offerService.ValidateSalesManager(appRequestModel.SalesManagerId.Value, iamUserId, _settings.SalesManagerRoles).ConfigureAwait(false);
        }
        else
        {
            companyId = await _portalRepositories.GetInstance<IUserRepository>()
                .GetOwnCompanyId(iamUserId)
                .ConfigureAwait(false);
            if (companyId == Guid.Empty)
            {
                throw new ControllerArgumentException($"user {iamUserId} is not associated with any company");
            }
        }

        var appRepository = _portalRepositories.GetInstance<IOfferRepository>();
        var appId = appRepository.CreateOffer(appRequestModel.Provider, OfferTypeId.APP, app =>
        {
            app.Name = appRequestModel.Title;
            app.ProviderCompanyId = companyId;
            app.OfferStatusId = OfferStatusId.CREATED;
            if (appRequestModel.SalesManagerId.HasValue)
            {
                app.SalesManagerId = appRequestModel.SalesManagerId;
            }
        }).Id;
        appRepository.AddOfferDescriptions(appRequestModel.Descriptions.Select(d =>
              (appId, d.LanguageCode, d.LongDescription, d.ShortDescription)));
        appRepository.AddAppLanguages(appRequestModel.SupportedLanguageCodes.Select(c =>
              (appId, c)));
        appRepository.AddAppAssignedUseCases(appRequestModel.UseCaseIds.Select(uc =>
              (appId, uc)));
        var licenseId = appRepository.CreateOfferLicenses(appRequestModel.Price).Id;
        appRepository.CreateOfferAssignedLicense(appId, licenseId);

        try
        {
            await _portalRepositories.SaveAsync().ConfigureAwait(false);
            return appId;
        }
        catch(Exception exception)when (exception?.InnerException?.Message.Contains("violates foreign key constraint") ?? false)
        {
            throw new ControllerArgumentException($"invalid language code or UseCaseId specified");
        }
    }

    /// <inheritdoc/>
    public async Task UpdateAppReleaseAsync(Guid appId, AppRequestModel appRequestModel, string iamUserId)
    {
        var appData = await _portalRepositories.GetInstance<IOfferRepository>()
            .GetAppUpdateData(
                appId,
                iamUserId,
                appRequestModel.SupportedLanguageCodes,
                appRequestModel.UseCaseIds)
            .ConfigureAwait(false);
        if (appData is null)
        {
            throw new NotFoundException($"App {appId} does not exists");
        }

        if (appData.OfferState != OfferStatusId.CREATED)
        {
            throw new ConflictException($"Apps in State {appData.OfferState} can't be updated");
        }

        if (!appData.IsUserOfProvider)
        {
            throw new ForbiddenException($"User {iamUserId} is not allowed to change the app.");
        }

        if (appRequestModel.SalesManagerId.HasValue)
        {
            await _offerService.ValidateSalesManager(appRequestModel.SalesManagerId.Value, iamUserId, _settings.SalesManagerRoles).ConfigureAwait(false);
        }

        var newSupportedLanguages = appRequestModel.SupportedLanguageCodes.Except(appData.Languages.Where(x => x.IsMatch).Select(x => x.Shortname));
        var existingLanguageCodes = await _portalRepositories.GetInstance<ILanguageRepository>().GetLanguageCodesUntrackedAsync(newSupportedLanguages).ToListAsync().ConfigureAwait(false);
        if (newSupportedLanguages.Except(existingLanguageCodes).Any())
        {
            throw new ControllerArgumentException($"The language(s) {string.Join(",", newSupportedLanguages.Except(existingLanguageCodes))} do not exist in the database.",
                nameof(appRequestModel.SupportedLanguageCodes));
        }

        var appRepository = _portalRepositories.GetInstance<IOfferRepository>();
        appRepository.AttachAndModifyOffer(
        appId,
        app =>
        {
            app.Name = appRequestModel.Title;
            app.OfferStatusId = OfferStatusId.CREATED;
            app.Provider = appRequestModel.Provider;
            app.SalesManagerId = appRequestModel.SalesManagerId;
        },
        app => {
            app.SalesManagerId = appData.SalesManagerId;
        });

        _offerService.UpsertRemoveOfferDescription(appId, appRequestModel.Descriptions.Select(x => new Localization(x.LanguageCode, x.LongDescription, x.ShortDescription)), appData.OfferDescriptions);
        UpdateAppSupportedLanguages(appId, newSupportedLanguages, appData.Languages.Where(x => !x.IsMatch).Select(x => x.Shortname), appRepository);

        var newUseCases = appRequestModel.UseCaseIds.Except(appData.MatchingUseCases);
        if (newUseCases.Any())
        {
            appRepository.AddAppAssignedUseCases(appRequestModel.UseCaseIds.Select(uc =>
                (appId, uc)));
        }

        _offerService.CreateOrUpdateOfferLicense(appId, appRequestModel.Provider, appData.OfferLicense);
        
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private static void UpdateAppSupportedLanguages(Guid appId, IEnumerable<string> newSupportedLanguages, IEnumerable<string> languagesToRemove, IOfferRepository appRepository)
    {
        appRepository.AddAppLanguages(newSupportedLanguages.Select(language => (appId, language)));
        appRepository.RemoveAppLanguages(languagesToRemove.Select(language => (appId, language)));
    }

    /// <inheritdoc/>
    public Task<Pagination.Response<InReviewAppData>> GetAllInReviewStatusAppsAsync(int page, int size, OfferSorting? sorting, OfferStatusIdFilter? offerStatusIdFilter) =>
        Pagination.CreateResponseAsync(page, size, 15,
            _portalRepositories.GetInstance<IOfferRepository>()
                .GetAllInReviewStatusAppsAsync(GetOfferStatusIds(offerStatusIdFilter), sorting ?? OfferSorting.DateDesc));

    /// <inheritdoc/>
    public Task SubmitAppReleaseRequestAsync(Guid appId, string iamUserId) => 
        _offerService.SubmitOfferAsync(appId, iamUserId, OfferTypeId.APP, _settings.SubmitAppNotificationTypeIds, _settings.CompanyAdminRoles);
    
    /// <inheritdoc/>
    public Task ApproveAppRequestAsync(Guid appId, string iamUserId) =>
        _offerService.ApproveOfferRequestAsync(appId, iamUserId, OfferTypeId.APP, _settings.ApproveAppNotificationTypeIds, (_settings.ApproveAppUserRoles));
    
    private IEnumerable<OfferStatusId> GetOfferStatusIds(OfferStatusIdFilter? offerStatusIdFilter)
    {
        switch(offerStatusIdFilter)
        {
            case OfferStatusIdFilter.InReview :
            {
               return new []{ OfferStatusId.IN_REVIEW };
            }
            default :
            {
                return _settings.OfferStatusIds;
            }
        }       
    }
}
