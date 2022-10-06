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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using Org.CatenaX.Ng.Portal.Backend.Apps.Service.BusinessLogic;
using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.Offers.Library.Service;
using Xunit;
using Org.CatenaX.Ng.Portal.Backend.Apps.Service.ViewModels;
using Microsoft.Extensions.Options;

namespace Org.CatenaX.Ng.Portal.Backend.Apps.Service.Tests;

public class AppReleaseBusinessLogicTest
{
    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferService _offerService;
    private readonly IOfferRepository _offerRepository;
    private readonly IAppReleaseRepository _appReleaseRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly AppReleaseBusinessLogic _logic;
    private readonly IOptions<AppsSettings> _options;
    public AppReleaseBusinessLogicTest()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _offerService = A.Fake<IOfferService>();
        _offerRepository = A.Fake<IOfferRepository>();
        _appReleaseRepository = A.Fake<IAppReleaseRepository>();
        _documentRepository = A.Fake<IDocumentRepository>();
        _options = A.Fake<IOptions<AppsSettings>>();
        A.CallTo(() => _portalRepositories.GetInstance<IAppReleaseRepository>()).Returns(_appReleaseRepository);
         _logic = new AppReleaseBusinessLogic(_portalRepositories, _options, _offerService);
    }

    [Fact]
    public async Task CreateServiceOffering_WithValidDataAndEmptyDescriptions_ReturnsCorrectDetails()
    {
        // Arrange
        Guid appId = new Guid("5cf74ef8-e0b7-4984-a872-474828beb5d2");
        Guid userId = new Guid("7eab8e16-8298-4b41-953b-515745423658");
        var appUserRoleDescription = new List<AppUserRoleDescription>();
        appUserRoleDescription.Add(new AppUserRoleDescription("en","this is test1"));
        appUserRoleDescription.Add(new AppUserRoleDescription("de","this is test2"));
        appUserRoleDescription.Add(new AppUserRoleDescription("fr","this is test3"));
        var appUserRoles = new List<AppUserRole>();
        appUserRoles.Add(new AppUserRole("IT Admin",appUserRoleDescription));
       
        
        A.CallTo(() => _appReleaseRepository.IsProviderCompanyUserAsync(appId,userId.ToString())).ReturnsLazily(() => true);

        // Act
        var result = await _logic.AddAppUserRoleAsync(appId, appUserRoles, userId.ToString()).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _appReleaseRepository.IsProviderCompanyUserAsync(A<Guid>._, A<string>._)).MustHaveHappened();
        foreach(var appRole in appUserRoles)
        {
            A.CallTo(() => _appReleaseRepository.CreateAppUserRole(A<Guid>._, A<string>._)).MustHaveHappened();
            foreach(var item in appRole.descriptions)
            {
                A.CallTo(() => _appReleaseRepository.CreateAppUserRoleDescription(A<Guid>._, A<string>._, A<string>._)).MustHaveHappened();
            }
        }
        
        Assert.NotNull(result);
        Assert.IsType<List<AppRoleData>>(result);
        
    }
}
