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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IAppChangeBusinessLogic"/>.
/// </summary>
public class AppChangeBusinessLogic : IAppChangeBusinessLogic
{
    private static readonly JsonSerializerOptions Options = new (){ PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly IPortalRepositories _portalRepositories;
    private readonly AppsSettings _settings;
    private readonly INotificationService _notificationService;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IOfferService _offerService;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">access to the repositories</param>
    /// <param name="notificationService">the notification service</param>
    /// <param name="provisioningManager">The provisioning manager</param>
    /// <param name="settings">Settings for the app change bl</param>
    /// <param name="offerService">Offer Servicel</param>
    public AppChangeBusinessLogic(IPortalRepositories portalRepositories, INotificationService notificationService, IProvisioningManager provisioningManager, IOfferService offerService, IOptions<AppsSettings> settings)
    {
        _portalRepositories = portalRepositories;
        _notificationService = notificationService;
        _provisioningManager = provisioningManager;
        _settings = settings.Value;
        _offerService = offerService;
    }

    /// <inheritdoc/>
    public Task<IEnumerable<AppRoleData>> AddActiveAppUserRoleAsync(Guid appId, IEnumerable<AppUserRole> appUserRolesDescription, string iamUserId)
    {
        AppExtensions.ValidateAppUserRole(appId, appUserRolesDescription);
        return InsertActiveAppUserRoleAsync(appId, appUserRolesDescription, iamUserId);
    }

    private async Task<IEnumerable<AppRoleData>> InsertActiveAppUserRoleAsync(Guid appId, IEnumerable<AppUserRole> userRoles, string iamUserId)
    {
        var result = await _portalRepositories.GetInstance<IOfferRepository>().GetInsertActiveAppUserRoleDataAsync(appId, iamUserId, OfferTypeId.APP).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"app {appId} does not exist");
        }
       
        if (result.CompanyUserId == Guid.Empty)
        {
            throw new ForbiddenException($"user {iamUserId} is not a member of the provider company of app {appId}");
        }

        if (result.ProviderCompanyId == null)
        {
            throw new ConflictException($"App {appId} providing company is not yet set.");
        }

        var roleData = AppExtensions.CreateUserRolesWithDescriptions(_portalRepositories.GetInstance<IUserRolesRepository>(), appId, userRoles);
        foreach (var clientId in result.ClientClientIds)
        {
            await _provisioningManager.AddRolesToClientAsync(clientId, userRoles.Select(x => x.Role)).ConfigureAwait(false);
        }

        var notificationContent = new
        {
            AppName = result.AppName,
            Roles = roleData.Select(x => x.RoleName)
        };
        var serializeNotificationContent = JsonSerializer.Serialize(notificationContent);
        var content = _settings.ActiveAppNotificationTypeIds.Select(typeId => new ValueTuple<string?, NotificationTypeId>(serializeNotificationContent, typeId));
        await _notificationService.CreateNotifications(_settings.ActiveAppCompanyAdminRoles, result.CompanyUserId, content, result.ProviderCompanyId.Value).AwaitAll().ConfigureAwait(false);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return roleData;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LocalizedDescription>> GetAppUpdateDescriptionByIdAsync(Guid appId, string iamUserId)
    {        
        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        return await ValidateAndGetAppDescription(appId, iamUserId, offerRepository);        
    }
    
    /// <inheritdoc />
    public async Task CreateOrUpdateAppDescriptionByIdAsync(Guid appId, string iamUserId, IEnumerable<LocalizedDescription> offerDescriptionDatas)
    {
        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();

        offerRepository.CreateUpdateDeleteOfferDescriptions(appId,
            await ValidateAndGetAppDescription(appId, iamUserId, offerRepository),
            offerDescriptionDatas.Select(od => new ValueTuple<string,string, string>(od.LanguageCode,od.LongDescription,od.ShortDescription)));

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private static async Task<IEnumerable<LocalizedDescription>> ValidateAndGetAppDescription(Guid appId, string iamUserId, IOfferRepository offerRepository)
    {
        var result = await offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, iamUserId).ConfigureAwait(false);
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
            throw new ForbiddenException($"user {iamUserId} is not a member of the providercompany of App {appId}");
        }

        if (result.OfferDescriptionDatas == null)
        {
            throw new UnexpectedConditionException("offerDescriptionDatas should never be null here");
        }

        return result.OfferDescriptionDatas;
    }

  /// <inheritdoc />
    public async Task UploadOfferAssignedAppLeadImageDocumentByIdAsync(Guid appId, string iamUserId, IFormFile document, CancellationToken cancellationToken)
    {
        var appLeadImageContentTypes = new []{ MediaTypeId.JPEG, MediaTypeId.PNG };
        var documentContentType = ContentTypeMapperExtensions.ParseMediaTypeId(document.ContentType);
        if (!appLeadImageContentTypes.Contains(documentContentType))
        {
            throw new UnsupportedMediaTypeException($"Document type not supported. File with contentType :{string.Join(",", appLeadImageContentTypes)} are allowed.");
        }

        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        var result = await offerRepository.GetOfferAssignedAppLeadImageDocumentsByIdAsync(appId, iamUserId, OfferTypeId.APP).ConfigureAwait(false);

        if(result == default)
        {
            throw new NotFoundException($"App {appId} does not exist.");
        }
        if (!result.IsStatusActive)
        {
            throw new ConflictException("offerStatus is in incorrect State");
        }
        var companyUserId = result.CompanyUserId;
        if (companyUserId == Guid.Empty)
        {
            throw new ForbiddenException($"user {iamUserId} is not a member of the provider company of App {appId}");
        }

        var documentRepository = _portalRepositories.GetInstance<IDocumentRepository>();
        var (documentContent, hash) = await document.GetContentAndHash(cancellationToken).ConfigureAwait(false);
        var doc = documentRepository.CreateDocument(document.FileName, documentContent, hash, documentContentType, DocumentTypeId.APP_LEADIMAGE, x =>
        {
            x.CompanyUserId = companyUserId;
            x.DocumentStatusId = DocumentStatusId.LOCKED;
        });
        offerRepository.CreateOfferAssignedDocument(appId, doc.Id);

        offerRepository.RemoveOfferAssignedDocuments(result.documentStatusDatas.Select(data => (appId, data.DocumentId)));
        documentRepository.RemoveDocuments(result.documentStatusDatas.Select(data => data.DocumentId));

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task DeactivateOfferByAppIdAsync(Guid appId, string iamUserId) =>
        _offerService.DeactivateOfferIdAsync(appId, iamUserId, OfferTypeId.APP);

    /// <inheritdoc />
    public Task UpdateTenantUrlAsync(Guid offerId, Guid subscriptionId, UpdateTenantData data, string iamUserId)
    {
        data.Url.EnsureValidHttpUrl(() => nameof(data.Url));
        return UpdateTenantUrlAsyncInternal(offerId, subscriptionId, data.Url, iamUserId);
    }

    private async Task UpdateTenantUrlAsyncInternal(Guid offerId, Guid subscriptionId, string url, string iamUserId)
    {
        var offerSubscriptionsRepository = _portalRepositories.GetInstance<IOfferSubscriptionsRepository>();
        var result = await offerSubscriptionsRepository.GetUpdateUrlDataAsync(offerId, subscriptionId, iamUserId).ConfigureAwait(false);
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
            throw new ForbiddenException($"User {iamUserId} is not part of the app's providing company");
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

        if (!string.IsNullOrEmpty(detailData.ClientClientId))
        {
            await _provisioningManager.UpdateClient(detailData.ClientClientId, url, url.AppendToPathEncoded("*")).ConfigureAwait(false);
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
            await _notificationService.CreateNotifications(_settings.CompanyAdminRoles, null, new[] {((string?)notificationContent, NotificationTypeId.SUBSCRIPTION_URL_UPDATE)}, subscribingCompanyId).AwaitAll().ConfigureAwait(false);
        }

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}
