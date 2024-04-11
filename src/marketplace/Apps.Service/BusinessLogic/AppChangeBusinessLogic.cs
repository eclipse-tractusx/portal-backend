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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Web;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IAppChangeBusinessLogic"/>.
/// </summary>
public class AppChangeBusinessLogic : IAppChangeBusinessLogic
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly IPortalRepositories _portalRepositories;
    private readonly AppsSettings _settings;
    private readonly INotificationService _notificationService;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IOfferService _offerService;
    private readonly IIdentityData _identityData;
    private readonly IOfferDocumentService _offerDocumentService;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">access to the repositories</param>
    /// <param name="notificationService">the notification service</param>
    /// <param name="provisioningManager">The provisioning manager</param>
    /// <param name="identityService">Access to the identityService</param>
    /// <param name="offerService">Offer Servicel</param>
    /// <param name="settings">Settings for the app change bl</param>
    /// <param name="offerDocumentService">document service</param>
    /// <param name="dateTimeProvider">Provider for current DateTime</param>
    public AppChangeBusinessLogic(
        IPortalRepositories portalRepositories,
        INotificationService notificationService,
        IProvisioningManager provisioningManager,
        IOfferService offerService,
        IIdentityService identityService,
        IOptions<AppsSettings> settings,
        IOfferDocumentService offerDocumentService,
        IDateTimeProvider dateTimeProvider)
    {
        _portalRepositories = portalRepositories;
        _notificationService = notificationService;
        _provisioningManager = provisioningManager;
        _settings = settings.Value;
        _offerService = offerService;
        _identityData = identityService.IdentityData;
        _offerDocumentService = offerDocumentService;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc/>
    public Task<IEnumerable<AppRoleData>> AddActiveAppUserRoleAsync(Guid appId, IEnumerable<AppUserRole> appUserRolesDescription)
    {
        AppExtensions.ValidateAppUserRole(appId, appUserRolesDescription);
        return InsertActiveAppUserRoleAsync(appId, appUserRolesDescription);
    }

    private async Task<IEnumerable<AppRoleData>> InsertActiveAppUserRoleAsync(Guid appId, IEnumerable<AppUserRole> userRoles)
    {
        var result = await _portalRepositories.GetInstance<IOfferRepository>().GetInsertActiveAppUserRoleDataAsync(appId, OfferTypeId.APP).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw new NotFoundException($"app {appId} does not exist");
        }

        if (result.ProviderCompanyId == null)
        {
            throw new ConflictException($"App {appId} providing company is not yet set.");
        }

        if (result.ProviderCompanyId.Value != _identityData.CompanyId)
        {
            throw new ForbiddenException($"Company {_identityData.CompanyId} is not the provider company of app {appId}");
        }

        var roleData = AppExtensions.CreateUserRolesWithDescriptions(_portalRepositories.GetInstance<IUserRolesRepository>(), appId, userRoles);
        foreach (var clientId in result.ClientClientIds)
        {
            await _provisioningManager.AddRolesToClientAsync(clientId, userRoles.Select(x => x.Role)).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        _portalRepositories.GetInstance<IOfferRepository>().AttachAndModifyOffer(appId, offer =>
            offer.DateLastChanged = DateTimeOffset.UtcNow);

        var notificationContent = new
        {
            AppName = result.AppName,
            Roles = roleData.Select(x => x.RoleName)
        };
        var serializeNotificationContent = JsonSerializer.Serialize(notificationContent);
        var content = _settings.ActiveAppNotificationTypeIds.Select(typeId => new ValueTuple<string?, NotificationTypeId>(serializeNotificationContent, typeId));
        await _notificationService.CreateNotifications(_settings.ActiveAppCompanyAdminRoles, _identityData.IdentityId, content, result.ProviderCompanyId.Value).AwaitAll().ConfigureAwait(false);
        await _portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        return roleData;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LocalizedDescription>> GetAppUpdateDescriptionByIdAsync(Guid appId)
    {
        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        return await ValidateAndGetAppDescription(appId, offerRepository);
    }

    /// <inheritdoc />
    public async Task CreateOrUpdateAppDescriptionByIdAsync(Guid appId, IEnumerable<LocalizedDescription> offerDescriptionDatas)
    {
        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();

        offerRepository.CreateUpdateDeleteOfferDescriptions(appId,
            await ValidateAndGetAppDescription(appId, offerRepository),
            offerDescriptionDatas.Select(od => new ValueTuple<string, string, string>(od.LanguageCode, od.LongDescription, od.ShortDescription)));

        offerRepository.AttachAndModifyOffer(appId, offer =>
            offer.DateLastChanged = DateTimeOffset.UtcNow);
        await _portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private async Task<IEnumerable<LocalizedDescription>> ValidateAndGetAppDescription(Guid appId, IOfferRepository offerRepository)
    {
        var companyId = _identityData.CompanyId;
        var result = await offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, companyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw new NotFoundException($"App {appId} does not exist.");
        }

        if (!result.IsStatusActive)
        {
            throw new ConflictException($"App {appId} is in InCorrect Status");
        }

        if (!result.IsProviderCompanyUser)
        {
            throw new ForbiddenException($"Company {companyId} is not the provider company of App {appId}");
        }

        if (result.OfferDescriptionDatas == null)
        {
            throw new UnexpectedConditionException("offerDescriptionDatas should never be null here");
        }

        return result.OfferDescriptionDatas;
    }

    /// <inheritdoc />
    public async Task UploadOfferAssignedAppLeadImageDocumentByIdAsync(Guid appId, IFormFile document, CancellationToken cancellationToken)
    {
        var appLeadImageContentTypes = new[] { MediaTypeId.JPEG, MediaTypeId.PNG };
        var documentContentType = document.ContentType.ParseMediaTypeId();
        documentContentType.CheckDocumentContentType(appLeadImageContentTypes);

        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        var result = await offerRepository.GetOfferAssignedAppLeadImageDocumentsByIdAsync(appId, _identityData.CompanyId, OfferTypeId.APP).ConfigureAwait(ConfigureAwaitOptions.None);

        if (result == default)
        {
            throw new NotFoundException($"App {appId} does not exist.");
        }
        if (!result.IsStatusActive)
        {
            throw new ConflictException("offerStatus is in incorrect State");
        }
        if (!result.IsUserOfProvider)
        {
            throw new ForbiddenException($"Company {_identityData.CompanyId} is not the provider company of App {appId}");
        }

        var documentRepository = _portalRepositories.GetInstance<IDocumentRepository>();
        var (documentContent, hash) = await document.GetContentAndHash(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        var doc = documentRepository.CreateDocument(document.FileName, documentContent, hash, documentContentType, DocumentTypeId.APP_LEADIMAGE, x =>
        {
            x.CompanyUserId = _identityData.IdentityId;
            x.DocumentStatusId = DocumentStatusId.LOCKED;
        });
        offerRepository.CreateOfferAssignedDocument(appId, doc.Id);

        offerRepository.RemoveOfferAssignedDocuments(result.documentStatusDatas.Select(data => (appId, data.DocumentId)));
        documentRepository.RemoveDocuments(result.documentStatusDatas.Select(data => data.DocumentId));

        offerRepository.AttachAndModifyOffer(appId, offer =>
            offer.DateLastChanged = DateTimeOffset.UtcNow);
        await _portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public Task DeactivateOfferByAppIdAsync(Guid appId) =>
        _offerService.DeactivateOfferIdAsync(appId, OfferTypeId.APP);

    /// <inheritdoc />
    public Task UpdateTenantUrlAsync(Guid offerId, Guid subscriptionId, UpdateTenantData data)
    {
        data.Url.EnsureValidHttpUrl(() => nameof(data.Url));
        return UpdateTenantUrlAsyncInternal(offerId, subscriptionId, data.Url);
    }

    private async Task UpdateTenantUrlAsyncInternal(Guid offerId, Guid subscriptionId, string url)
    {
        var companyId = _identityData.CompanyId;
        var offerSubscriptionsRepository = _portalRepositories.GetInstance<IOfferSubscriptionsRepository>();
        var result = await offerSubscriptionsRepository.GetUpdateUrlDataAsync(offerId, subscriptionId, companyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == null)
        {
            throw new NotFoundException($"Offer {offerId} or subscription {subscriptionId} do not exists");
        }

        var (offerName, isSingleInstance, isUserOfCompany, requesterId, subscribingCompanyId, offerSubscriptionStatusId, detailData) = result;
        if (isSingleInstance)
        {
            throw new ConflictException("Subscription url of single instance apps can't be changed");
        }

        if (!isUserOfCompany)
        {
            throw new ForbiddenException($"Company {companyId} is not the app's providing company");
        }

        if (offerSubscriptionStatusId != OfferSubscriptionStatusId.ACTIVE)
        {
            throw new ConflictException($"Subscription {subscriptionId} must be in status {OfferSubscriptionStatusId.ACTIVE}");
        }

        if (detailData == null)
        {
            throw new ConflictException($"There is no subscription detail data configured for subscription {subscriptionId}");
        }

        if (url == detailData.SubscriptionUrl)
            return;

        offerSubscriptionsRepository.AttachAndModifyAppSubscriptionDetail(detailData.DetailId, subscriptionId,
            os =>
            {
                os.AppSubscriptionUrl = detailData.SubscriptionUrl;
            },
            os =>
            {
                os.AppSubscriptionUrl = url;
            });

        _portalRepositories.GetInstance<IOfferRepository>().AttachAndModifyOffer(offerId, offer =>
            offer.DateLastChanged = DateTimeOffset.UtcNow);

        if (!string.IsNullOrEmpty(detailData.ClientClientId))
        {
            await _provisioningManager.UpdateClient(detailData.ClientClientId, url, url.AppendToPathEncoded("*")).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        var notificationContent = JsonSerializer.Serialize(new
        {
            AppId = offerId,
            AppName = offerName,
            OldUrl = detailData.SubscriptionUrl,
            NewUrl = url
        }, Options);
        if (requesterId != Guid.Empty)
        {
            _portalRepositories.GetInstance<INotificationRepository>().CreateNotification(requesterId, NotificationTypeId.SUBSCRIPTION_URL_UPDATE, false,
                n =>
                {
                    n.Content = notificationContent;
                });
        }
        else
        {
            await _notificationService.CreateNotifications(_settings.CompanyAdminRoles, null, new (string?, NotificationTypeId)[] { (notificationContent, NotificationTypeId.SUBSCRIPTION_URL_UPDATE) }, subscribingCompanyId).AwaitAll().ConfigureAwait(false);
        }

        await _portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public async Task<ActiveAppDocumentData> GetActiveAppDocumentTypeDataAsync(Guid appId)
    {
        var appDocTypeData = await _portalRepositories.GetInstance<IOfferRepository>()
            .GetActiveOfferDocumentTypeDataOrderedAsync(appId, _identityData.CompanyId, OfferTypeId.APP, _settings.ActiveAppDocumentTypeIds)
            .PreSortedGroupBy(result => result.DocumentTypeId)
            .ToDictionaryAsync(
                group => group.Key,
                group => group.Select(result =>
                    new DocumentData(
                        result.DocumentId,
                        result.DocumentName)))
            .ConfigureAwait(false);
        return new ActiveAppDocumentData(
            _settings.ActiveAppDocumentTypeIds.ToDictionary(
                documentTypeId => documentTypeId,
                documentTypeId => appDocTypeData.TryGetValue(documentTypeId, out var data)
                    ? data
                    : Enumerable.Empty<DocumentData>()));
    }

    /// <inheritdoc />
    public async Task DeleteActiveAppDocumentAsync(Guid appId, Guid documentId)
    {
        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        var documentRepository = _portalRepositories.GetInstance<IDocumentRepository>();
        var result = await offerRepository.GetOfferAssignedAppDocumentsByIdAsync(appId, _identityData.CompanyId, OfferTypeId.APP, documentId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw new NotFoundException($"Document {documentId} for App {appId} does not exist.");
        }
        if (!result.IsStatusActive)
        {
            throw new ConflictException("offerStatus is in incorrect State");
        }
        if (!result.IsUserOfProvider)
        {
            throw new ForbiddenException($"Company {_identityData.CompanyId} is not the provider company of App {appId}");
        }
        if (!_settings.DeleteActiveAppDocumentTypeIds.Contains(result.DocumentTypeId))
        {
            throw new ConflictException($"document {documentId} does not have a valid documentType");
        }
        offerRepository.RemoveOfferAssignedDocument(appId, documentId);
        documentRepository.AttachAndModifyDocument(
            documentId,
            a => { a.DocumentStatusId = result.DocumentStatusId; },
            a => { a.DocumentStatusId = DocumentStatusId.INACTIVE; });
        _portalRepositories.GetInstance<IOfferRepository>().AttachAndModifyOffer(appId, offer =>
            offer.DateLastChanged = _dateTimeProvider.OffsetNow);
        await _portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public async Task CreateActiveAppDocumentAsync(Guid appId, DocumentTypeId documentTypeId, IFormFile document, CancellationToken cancellationToken) =>
        await _offerDocumentService.UploadDocumentAsync(appId, documentTypeId, document, OfferTypeId.APP, _settings.UploadActiveAppDocumentTypeIds, OfferStatusId.ACTIVE, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

    /// <inheritdoc />
    public async Task<IEnumerable<ActiveAppRoleDetails>> GetActiveAppRolesAsync(Guid appId, string? languageShortName)
    {
        var (isValid, isActive, roleDetails) = await _portalRepositories.GetInstance<IUserRolesRepository>().GetActiveAppRolesAsync(appId, OfferTypeId.APP, languageShortName, Constants.DefaultLanguage).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!isValid)
        {
            throw new NotFoundException($"App {appId} does not exist");
        }
        if (!isActive)
        {
            throw new ConflictException($"App {appId} is not Active");
        }
        return roleDetails ?? throw new UnexpectedConditionException("roleDetails should never be null here");
    }
}
