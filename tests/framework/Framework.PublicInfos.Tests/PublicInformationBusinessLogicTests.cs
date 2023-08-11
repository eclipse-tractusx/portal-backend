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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using System.Reflection;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.PublicInfos.Tests;

public class PublicInformationBusinessLogicTests
{
    private readonly Guid _participantCompany = Guid.NewGuid();
    private readonly Guid _appProviderCompany = Guid.NewGuid();
    private readonly IIdentityService _identityService;
    private readonly IPublicInformationBusinessLogic _sut;

    public PublicInformationBusinessLogicTests()
    {
        _identityService = A.Fake<IIdentityService>();
        var companyRepository = A.Fake<ICompanyRepository>();
        var portalRepositories = A.Fake<IPortalRepositories>();

        var actionDescriptorCollectionProvider = A.Fake<IActionDescriptorCollectionProvider>();
        SetupActionDescriptorCollectionProvider(actionDescriptorCollectionProvider);
        SetupCompanyRepository(companyRepository);
        A.CallTo(() => portalRepositories.GetInstance<ICompanyRepository>()).Returns(companyRepository);

        _sut = new PublicInformationBusinessLogic(actionDescriptorCollectionProvider, portalRepositories, _identityService);
    }

    [Fact]
    public async Task GetPublicUrls_ForParticipant_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _identityService.IdentityData).Returns(new IdentityData("4C1A6851-D4E7-4E10-A011-3732CD045E8A", Guid.NewGuid(), IdentityTypeId.COMPANY_USER, _participantCompany));

        // Act
        var result = await _sut.GetPublicUrls().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(1).And.Satisfy(x => x.HttpMethods == "GET" && x.Url == "all");
    }

    [Fact]
    public async Task GetPublicUrls_ForAppProvider_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _identityService.IdentityData).Returns(new IdentityData("4C1A6851-D4E7-4E10-A011-3732CD045E8A", Guid.NewGuid(), IdentityTypeId.COMPANY_USER, _appProviderCompany));

        // Act
        var result = await _sut.GetPublicUrls().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(3).And.Satisfy(
            x => x.HttpMethods == "GET" && x.Url == "all",
            x => x.HttpMethods == "GET" && x.Url == "participant",
            x => x.HttpMethods == "POST" && x.Url == "participant"
        );
    }

    #region Setup

    private void SetupCompanyRepository(ICompanyRepository companyRepository)
    {
        A.CallTo(() => companyRepository.GetOwnCompanyRolesAsync(_appProviderCompany)).Returns(new[] { CompanyRoleId.ACTIVE_PARTICIPANT, CompanyRoleId.APP_PROVIDER }.ToAsyncEnumerable());
        A.CallTo(() => companyRepository.GetOwnCompanyRolesAsync(_participantCompany)).Returns(new[] { CompanyRoleId.ACTIVE_PARTICIPANT }.ToAsyncEnumerable());
    }

    private static void SetupActionDescriptorCollectionProvider(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
    {
        var methods = new[]
        {
            nameof(TestController.OnlyAppProvider),
            nameof(TestController.All),
            nameof(TestController.OnlyAppProviderPost)
        };

        var actionDescriptors = typeof(TestController).GetMethods().Where(x => methods.Contains(x.Name)).Select(x => new ControllerActionDescriptor
        {
            MethodInfo = x,
            ActionConstraints = new List<IActionConstraintMetadata>
            {
                new HttpMethodActionConstraint(x.GetCustomAttribute<HttpMethodAttribute>()!.HttpMethods)
            },
            AttributeRouteInfo = new AttributeRouteInfo
            {
                Template = x.GetCustomAttribute<RouteAttribute>()!.Template
            }
        }).ToList<ActionDescriptor>();

        A.CallTo(() => actionDescriptorCollectionProvider.ActionDescriptors).Returns(new ActionDescriptorCollection(actionDescriptors, 1));
    }

    #endregion
}

[ApiController]
internal class TestController : ControllerBase
{
    [PublicUrl(CompanyRoleId.ACTIVE_PARTICIPANT, CompanyRoleId.APP_PROVIDER, CompanyRoleId.SERVICE_PROVIDER)]
    [HttpGet]
    [Route("all")]
#pragma warning disable CA1822
    public void All()
#pragma warning restore CA1822
    {
    }

    [PublicUrl(CompanyRoleId.APP_PROVIDER)]
    [HttpGet]
    [Route("participant")]
#pragma warning disable CA1822
    public void OnlyAppProvider()
#pragma warning restore CA1822
    {
    }

    [PublicUrl(CompanyRoleId.APP_PROVIDER)]
    [HttpPost]
    [Route("participant")]
#pragma warning disable CA1822
    public void OnlyAppProviderPost()
#pragma warning restore CA1822
    {
    }

    [HttpGet]
    [Route("none")]
#pragma warning disable CA1822
    public void NoRole()
#pragma warning restore CA1822
    {
    }
}
