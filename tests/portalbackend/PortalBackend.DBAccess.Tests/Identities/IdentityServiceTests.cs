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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Identities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Identities;

public class IdentityServiceTests
{
    private readonly IFixture _fixture;
    private readonly Guid _identityId = Guid.NewGuid();

    private readonly IPortalRepositories _portalRepositories;
    private readonly IIdentityRepository _identityRepository;
    private readonly IdentityService _sut;

    public IdentityServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var identityIdDetermination = A.Fake<IIdentityIdDetermination>();
        A.CallTo(() => identityIdDetermination.IdentityId).Returns(_identityId);

        _portalRepositories = A.Fake<IPortalRepositories>();
        _identityRepository = A.Fake<IIdentityRepository>();
        A.CallTo(() => _portalRepositories.GetInstance<IIdentityRepository>()).Returns(_identityRepository);

        _sut = new IdentityService(_portalRepositories, identityIdDetermination);
    }

    [Fact]
    public void IdentityData_ReturnsExpected()
    {
        // Arrange
        var sub = _fixture.Create<string>();
        var identityType = _fixture.Create<IdentityTypeId>();
        var companyId = Guid.NewGuid();
        A.CallTo(() => _identityRepository.GetIdentityDataByIdentityId(_identityId))
            .Returns(new IdentityData(sub, _identityId, identityType, companyId));

        // Act
        var first = _sut.IdentityData;
        var second = _sut.IdentityData;

        // Assert
        first.Should().NotBeNull()
            .And.BeSameAs(second)
            .And.Match<IdentityData>(x =>
                x.UserEntityId == sub &&
                x.UserId == _identityId &&
                x.IdentityType == identityType &&
                x.CompanyId == companyId);

        A.CallTo(() => _identityRepository.GetIdentityDataByIdentityId(_identityId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void IdentityData_WithNotExistingIdentityId_Throws()
    {
        // Arrange
        A.CallTo(() => _identityRepository.GetIdentityDataByIdentityId(_identityId))
            .Returns(null);

        // Act
        var error = Assert.Throws<ConflictException>(() => _sut.IdentityData);

        // Assert
        error.Message.Should().Be($"Identity {_identityId} could not be found");
    }
}
