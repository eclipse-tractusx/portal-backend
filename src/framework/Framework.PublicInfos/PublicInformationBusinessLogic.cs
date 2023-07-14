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

using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using System.Reflection;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.PublicInfos;

public class PublicInformationBusinessLogic : IPublicInformationBusinessLogic
{
    private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
    private readonly IPortalRepositories _portalRepositories;

    /// <summary>
    /// Creates a new instance of <see cref="PublicInformationBusinessLogic"/>
    /// </summary>
    /// <param name="actionDescriptorCollectionProvider">The actionDescriptorCollectionProvider</param>
    /// <param name="portalRepositories"></param>
    public PublicInformationBusinessLogic(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider, IPortalRepositories portalRepositories)
    {
        _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        _portalRepositories = portalRepositories;
    }

    public async Task<IEnumerable<UrlInformation>> GetPublicUrls(Guid companyId)
    {
        var companyRoleIds = await _portalRepositories.GetInstance<ICompanyRepository>().GetOwnCompanyRolesAsync(companyId).ConfigureAwait(false);
        var result = new List<UrlInformation>();
        foreach (var item in _actionDescriptorCollectionProvider.ActionDescriptors.Items
                     .Where(x => x.ActionConstraints != null)
                     .OfType<ControllerActionDescriptor>()
                     .Where(x => x.MethodInfo.GetCustomAttribute<PublicUrlAttribute>() != null))
        {
            var neededCompanyRoles = item.MethodInfo.GetCustomAttribute<PublicUrlAttribute>()!.CompanyRoleIds;
            if (!neededCompanyRoles.Any(x => companyRoleIds.Contains(x)))
            {
                continue;
            }

            var actionConstraintMetadata = item.ActionConstraints!.OfType<HttpMethodActionConstraint>();
            var httpMethods = actionConstraintMetadata.SelectMany(x => x.HttpMethods).Distinct();
            var url = item.AttributeRouteInfo?.Template ?? throw new ConflictException($"There must be an url for {item.DisplayName}");
            result.Add(new UrlInformation(string.Join(", ", httpMethods), url));
        }

        return result;
    }
}
