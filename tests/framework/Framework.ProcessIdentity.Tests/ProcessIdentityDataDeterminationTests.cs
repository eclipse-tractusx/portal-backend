/********************************************************************************
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ProcessIdentity;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Framework.ProcessIdentity.Tests;

public class ProcessIdentityDataDeterminationTests
{
    private readonly IFixture _fixture;
    private readonly IIdentityRepository _identityRepository;
    private readonly IProcessIdentityDataBuilder _processIdentityDataBuilder;
    private readonly ProcessIdentityDataDetermination _sut;

    public ProcessIdentityDataDeterminationTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _identityRepository = A.Fake<IIdentityRepository>();
        _processIdentityDataBuilder = A.Fake<IProcessIdentityDataBuilder>();

        var portalRepositories = A.Fake<IPortalRepositories>();
        A.CallTo(() => portalRepositories.GetInstance<IIdentityRepository>()).Returns(_identityRepository);

        _sut = new ProcessIdentityDataDetermination(portalRepositories, _processIdentityDataBuilder);
    }

    [Fact]
    public async Task GetIdentityData_ReturnsExpected()
    {
        // Arrange
        var identityId = Guid.NewGuid();
        var identityType = _fixture.Create<IdentityTypeId>();
        var companyId = Guid.NewGuid();
        A.CallTo(() => _processIdentityDataBuilder.IdentityId).Returns(identityId);
        A.CallTo(() => _identityRepository.GetActiveIdentityDataByIdentityId(A<Guid>._))
            .Returns(new ValueTuple<IdentityTypeId, Guid>(identityType, companyId));

        // Act
        await _sut.GetIdentityData().ConfigureAwait(false);

        // Assert
        A.CallTo(() => _identityRepository.GetActiveIdentityDataByIdentityId(identityId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _processIdentityDataBuilder.AddIdentityData(identityType, companyId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetIdentityData_WithNotExistingIdentityId_Throws()
    {
        // Arrange
        var identityId = Guid.NewGuid();
        A.CallTo(() => _processIdentityDataBuilder.IdentityId).Returns(identityId);
        A.CallTo(() => _identityRepository.GetActiveIdentityDataByIdentityId(A<Guid>._))
            .Returns<(IdentityTypeId, Guid)>(default);

        // Act
        var error = await Assert.ThrowsAsync<ConflictException>(async () => await _sut.GetIdentityData().ConfigureAwait(false)).ConfigureAwait(false);

        // Assert
        error.Message.Should().Be($"Identity {identityId} could not be found");
        A.CallTo(() => _identityRepository.GetActiveIdentityDataByIdentityId(identityId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _processIdentityDataBuilder.AddIdentityData(A<IdentityTypeId>._, A<Guid>._)).MustNotHaveHappened();
    }
}
