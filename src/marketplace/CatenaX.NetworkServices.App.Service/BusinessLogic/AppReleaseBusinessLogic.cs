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

using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.App.Service.InputModels;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CatenaX.NetworkServices.App.Service.BusinessLogic;

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
            throw new ArgumentException($"AppId must not be empty");
        }
        var descriptions = updateModel.Descriptions.Where(item => !String.IsNullOrWhiteSpace(item.LanguageCode)).Distinct();
        if (descriptions.Count() == 0)
        {
            throw new ArgumentException($"Language Code must not be empty");
        }

        return EditAppAsync(appId, updateModel, userId);
    }

    private async Task EditAppAsync(Guid appId, AppEditableDetail updateModel, string userId)
    {
         var result = await _portalRepositories.GetInstance<IAppRepository>().GetAppByIdAsync(appId, userId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"Cannot identify companyId or appId : User CompanyId is not associated with the same company as AppCompanyId:app status incorrect");
        }
        var (description, images) = result;
        var newApp = _portalRepositories.Attach(new CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.App(appId), app =>
        {
            app.ContactEmail = updateModel.ContactEmail;
            app.ContactNumber = updateModel.ContactNumber;
            app.MarketingUrl = updateModel.ProviderUri;
        });
        int currentIndex=0;
        foreach (var item in updateModel.Descriptions)
        {
            newApp.AppDescriptions.Add(new AppDescription(appId, item.LanguageCode, item.LongDescription, description.Where(x => x.AppId == appId).Select(x => x.DescriptionShort).ElementAt(currentIndex)));
            currentIndex++;
        }
        foreach (var record in updateModel.Images)
        {
            newApp.AppDetailImages.Add(new AppDetailImage(appId, record));
        }
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}

