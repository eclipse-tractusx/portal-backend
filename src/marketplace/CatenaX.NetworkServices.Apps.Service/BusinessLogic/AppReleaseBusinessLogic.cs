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
using System.Security.Cryptography;

namespace CatenaX.NetworkServices.Apps.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IAppReleaseBusinessLogic"/>.
/// </summary>
public class AppReleaseBusinessLogic : IAppReleaseBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories"></param>
    public AppReleaseBusinessLogic(IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
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
        if (udateModel.Images.Any(image => string.IsNullOrWhiteSpace(image)))
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
            app.ContactEmail = updateModel.ContactEmail;
            app.ContactNumber = updateModel.ContactNumber;
            app.MarketingUrl = updateModel.ProviderUri;
        });

        UpsertRemoveAppDescription(appId, updateModel.Descriptions, appResult.LanguageShortNames, appRepository);
        UpsertRemoveAppDetailImage(appId, updateModel.Images, appResult.ImageUrls, appRepository);
        
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

   private void UpsertRemoveAppDescription(Guid appId, IEnumerable<Localization> Descriptions, IEnumerable<string> LanguageShortNames, IOfferRepository appRepository)
    {
        var lstToAdd = new List<Localization>();
        foreach (var item in Descriptions)
        {
            foreach (var languageShortName in LanguageShortNames)
            {
                if (item.LanguageCode == languageShortName)
                {
                    if (string.IsNullOrWhiteSpace(item.LongDescription) && string.IsNullOrWhiteSpace(item.ShortDescription) && LanguageShortNames.Any())
                    {
                        _portalRepositories.Remove(new OfferDescription(appId, languageShortName));
                    }

                    else
                        _portalRepositories.Attach(new OfferDescription(appId, item.LanguageCode!), appdesc =>
                        {
                            appdesc.DescriptionLong = item.LongDescription!;
                            appdesc.DescriptionShort = item.ShortDescription!;
                        });
                }
                
            }
            if (!LanguageShortNames.Contains(item.LanguageCode) && (!string.IsNullOrWhiteSpace(item.LongDescription) || !string.IsNullOrWhiteSpace(item.ShortDescription)))
            {
                lstToAdd.Add(new Localization(item.LanguageCode, item.LongDescription, item.ShortDescription));
            }
        }

        if (lstToAdd.Count > 0)
        {
            appRepository.AddOfferDescriptions(lstToAdd.AsEnumerable().Select(d =>
               (appId, d.LanguageCode!, d.LongDescription!, d.ShortDescription!)));
        }
    }

    private void UpsertRemoveAppDetailImage(Guid appId, IEnumerable<string> Images, IEnumerable<(Guid Id, string Url)> ImageUrls, IOfferRepository appRepository)
    {
        appRepository.AddAppDetailImages(Images.Except(ImageUrls.Select(iu => iu.Url)).Select(image => new ValueTuple<Guid,string>(appId,image)));
        foreach (var (imageId, imageUrl) in ImageUrls.ExceptBy(Images,iu => iu.Url).Select(iu => (iu.Id, iu.Url)))
        {
            _portalRepositories.Remove<OfferDetailImage>(new OfferDetailImage(imageId));
        }
        
    }

    /// <inheritdoc/>
    public Task<int> CreateAppDocumentAsync(Guid appId, DocumentTypeId documentTypeId, IFormFile document, string userId, CancellationToken cancellationToken)
    {
        if (appId == Guid.Empty)
        {
            throw new ArgumentException($"AppId must not be empty");
        }
        if (!(documentTypeId == DocumentTypeId.APP_CONTRACT || documentTypeId == DocumentTypeId.DATA_CONTRACT))
        {
            throw new ArgumentException($"documentType must  be either APP_CONTRACT or DATA_CONTRACT");
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
}
