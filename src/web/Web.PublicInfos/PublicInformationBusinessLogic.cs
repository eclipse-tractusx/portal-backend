/********************************************************************************
 * Copyright (c) 2024 BMW Group AG
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using System.Reflection;

namespace Org.Eclipse.TractusX.Portal.Backend.Web.PublicInfos;

public class PublicInformationBusinessLogic : IPublicInformationBusinessLogic
{
    private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IIdentityData _identityData;

    /// <summary>
    /// Creates a new instance of <see cref="PublicInformationBusinessLogic"/>
    /// </summary>
    /// <param name="actionDescriptorCollectionProvider">The actionDescriptorCollectionProvider</param>
    /// <param name="portalRepositories"></param>
    /// <param name="identityService"></param>
    public PublicInformationBusinessLogic(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider, IPortalRepositories portalRepositories, IIdentityService identityService)
    {
        _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        _portalRepositories = portalRepositories;
        _identityData = identityService.IdentityData;
    }

    public async Task<IEnumerable<UrlInformation>> GetPublicUrls()
    {
        var companyRoleIds = await _portalRepositories.GetInstance<ICompanyRepository>().GetOwnCompanyRolesAsync(_identityData.CompanyId).ToArrayAsync().ConfigureAwait(false);
        return _actionDescriptorCollectionProvider.ActionDescriptors.Items
            .Where(item => item.ActionConstraints != null && item.ActionConstraints.OfType<HttpMethodActionConstraint>().Any())
            .OfType<ControllerActionDescriptor>()
            .Where(item => item.MethodInfo.GetCustomAttribute<PublicUrlAttribute>()?.CompanyRoleIds.Intersect(companyRoleIds).Any() ?? false)
            .Select(item =>
                new UrlInformation(
                    string.Join(", ", item.ActionConstraints!.OfType<HttpMethodActionConstraint>().SelectMany(x => x.HttpMethods).Distinct()),
                    (item.AttributeRouteInfo?.Template ?? throw new ConflictException($"There must be an url for {item.DisplayName}")).ToLower()));
    }
}
