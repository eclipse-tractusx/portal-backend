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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Library.Tests;

public class ChecklistCreationServiceTests
{
    private static readonly Guid ApplicationWithoutBpnId = new ("0a9bd7b1-e692-483e-8128-dbf52759c7a5");
    private static readonly Guid ApplicationWithBpnId = new ("c244f79a-7faf-4c59-bb85-fbfdf72ce46f");
    private readonly IPortalRepositories _portalRepositories;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IApplicationChecklistRepository _applicationChecklistRepository;
    private readonly ChecklistCreationService _service;

    public ChecklistCreationServiceTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        
        _applicationRepository = A.Fake<IApplicationRepository>();
        _applicationChecklistRepository = A.Fake<IApplicationChecklistRepository>();

        _service = new ChecklistCreationService(_portalRepositories);
    }
    
    #region CreateInitialChecklistAsync

    [Fact]
    public async Task CreateInitialChecklistAsync_WithBpnSet_CreatesExpectedResult()
    {
        // Arrange
        SetupFakesForCreate();
        
        // Act
        await _service.CreateInitialChecklistAsync(ApplicationWithBpnId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _applicationChecklistRepository.CreateChecklistForApplication(
            ApplicationWithBpnId,
            A<IEnumerable<(ChecklistEntryTypeId TypeId, ChecklistEntryStatusId StatusId)>>
                .That
                .Matches(x => 
                    x.Count(y => y.TypeId == ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER && y.StatusId == ChecklistEntryStatusId.DONE) == 1)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateInitialChecklistAsync_WithoutBpnSet_CreatesExpectedResult()
    {
        // Arrange
        SetupFakesForCreate();
        
        // Act
        await _service.CreateInitialChecklistAsync(ApplicationWithoutBpnId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _applicationChecklistRepository.CreateChecklistForApplication(
                ApplicationWithoutBpnId,
                A<IEnumerable<(ChecklistEntryTypeId TypeId, ChecklistEntryStatusId StatusId)>>
                    .That
                    .Matches(x => x.All(y => y.StatusId == ChecklistEntryStatusId.TO_DO))))
            .MustHaveHappenedOnceExactly();
    }

    #endregion
    
    #region Setup

    private void SetupFakesForCreate()
    {
        A.CallTo(() => _applicationRepository.GetBpnForApplicationIdAsync(ApplicationWithBpnId)).ReturnsLazily(() => "testbpn");
        A.CallTo(() => _applicationRepository.GetBpnForApplicationIdAsync(ApplicationWithoutBpnId)).ReturnsLazily(() => (string?)null);

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationChecklistRepository>()).Returns(_applicationChecklistRepository);
    }

    #endregion
}
