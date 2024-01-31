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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Synchronization.Executor;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.ServiceAccountSync.Executor.Tests;

public class IdentityProviderDisplayNameSyncProcessTypeExecutorTests
{
    private const ProcessStepTypeId ProcessStepTypeId = PortalBackend.PortalEntities.Enums.ProcessStepTypeId.SYNCHRONIZE_IDP_DISPLAY_NAME;
    private readonly IIdentityProviderRepository _identityProviderRepository;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IFixture _fixture;
    private readonly IdentityProviderDisplayNameSyncProcessTypeExecutor _executor;

    public IdentityProviderDisplayNameSyncProcessTypeExecutorTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var portalRepositories = A.Fake<IPortalRepositories>();
        _provisioningManager = A.Fake<IProvisioningManager>();
        _identityProviderRepository = A.Fake<IIdentityProviderRepository>();

        A.CallTo(() => portalRepositories.GetInstance<IIdentityProviderRepository>())
            .Returns(_identityProviderRepository);

        _executor = new IdentityProviderDisplayNameSyncProcessTypeExecutor(portalRepositories, _provisioningManager);
    }

    #region InitializeProcess

    [Fact]
    public async Task InitializeProcess_ValidProcessId_ReturnsExpected()
    {
        // Arrange
        var result = await _executor.InitializeProcess(Guid.NewGuid(), _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

        // Assert
        result.Modified.Should().BeFalse();
        result.ScheduleStepTypeIds.Should().BeNull();
    }

    #endregion

    #region ExecuteProcessStep

    [Fact]
    public async Task ExecuteProcessStep_WithUnrecoverableServiceException_Throws()
    {
        // Arrange
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();
        A.CallTo(() => _provisioningManager.GetIdentityProviderDisplayName("alias1"))
            .ThrowsAsync(new ServiceException("test"));
        A.CallTo(() => _identityProviderRepository.GetNextIdpsWithoutDisplayName())
            .Returns(Enumerable.Repeat(new ValueTuple<Guid, string>(Guid.NewGuid(), "alias1"), 1).ToAsyncEnumerable());

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId, processStepTypeIds, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Modified.Should().BeTrue();
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.FAILED);
        result.ProcessMessage.Should().Be("test");
    }

    [Fact]
    public async Task ExecuteProcessStep_WithConflictException_Throws()
    {
        // Arrange
        A.CallTo(() => _provisioningManager.GetIdentityProviderDisplayName("alias1"))
            .ThrowsAsync(new ConflictException("test"));
        A.CallTo(() => _identityProviderRepository.GetNextIdpsWithoutDisplayName())
            .Returns(Enumerable.Repeat(new ValueTuple<Guid, string>(Guid.NewGuid(), "alias1"), 1).ToAsyncEnumerable());

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId, Enumerable.Repeat(ProcessStepTypeId.SYNCHRONIZE_IDP_DISPLAY_NAME, 1), CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Modified.Should().BeTrue();
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.FAILED);
        result.ProcessMessage.Should().Be("test");
    }

    [Fact]
    public async Task ExecuteProcessStep_WithRecoverableServiceException_Throws()
    {
        // Arrange
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();
        A.CallTo(() => _provisioningManager.GetIdentityProviderDisplayName("alias1"))
            .ThrowsAsync(new ServiceException("test", true));
        A.CallTo(() => _identityProviderRepository.GetNextIdpsWithoutDisplayName())
            .Returns(Enumerable.Repeat(new ValueTuple<Guid, string>(Guid.NewGuid(), "alias1"), 1).ToAsyncEnumerable());

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId, processStepTypeIds, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Modified.Should().BeTrue();
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.TODO);
    }

    [Fact]
    public async Task ExecuteProcessStep_WithNoIamIdp_ExecutesExpected()
    {
        // Arrange
        A.CallTo(() => _identityProviderRepository.GetNextIdpsWithoutDisplayName())
            .Returns(Enumerable.Empty<(Guid, string)>().ToAsyncEnumerable());

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId, new[] { ProcessStepTypeId.SYNCHRONIZE_IDP_DISPLAY_NAME }, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Modified.Should().BeFalse();
        result.ScheduleStepTypeIds.Should().BeNull();
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ProcessMessage.Should().Be("no idps to synchronize found");

        A.CallTo(() => _identityProviderRepository.GetNextIdpsWithoutDisplayName())
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.GetServiceAccountUserId(A<string>._))
            .MustNotHaveHappened();
        A.CallTo(() => _identityProviderRepository.AttachAndModifyIamIdentityProvider(A<string>._, A<Guid>._, A<Action<IamIdentityProvider>>._, A<Action<IamIdentityProvider>>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteProcessStep_WithSingleIdentity_ExecutesExpected()
    {
        // Arrange
        var iamIdentityProvider = new IamIdentityProvider("alias1", string.Empty, Guid.NewGuid());
        const string DisplayName = "test";
        A.CallTo(() => _identityProviderRepository.GetNextIdpsWithoutDisplayName())
            .Returns(new[]
            {
                (iamIdentityProvider.IdentityProviderId, iamIdentityProvider.IamIdpAlias),
            }.ToAsyncEnumerable());
        A.CallTo(() => _provisioningManager.GetIdentityProviderDisplayName(iamIdentityProvider.IamIdpAlias))
            .Returns(DisplayName);
        A.CallTo(() => _identityProviderRepository.AttachAndModifyIamIdentityProvider(A<string>._, A<Guid>._, A<Action<IamIdentityProvider>>._, A<Action<IamIdentityProvider>>._))
            .Invokes((string _, Guid _, Action<IamIdentityProvider>? initialize, Action<IamIdentityProvider> setOptionalFields) =>
            {
                initialize?.Invoke(iamIdentityProvider);
                setOptionalFields.Invoke(iamIdentityProvider);
            });

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId, new[] { ProcessStepTypeId.SYNCHRONIZE_IDP_DISPLAY_NAME }, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Modified.Should().BeTrue();
        result.ScheduleStepTypeIds.Should().BeNull();
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ProcessMessage.Should().Be($"synchronized idp {iamIdentityProvider.IdentityProviderId} with alias {iamIdentityProvider.IamIdpAlias} and display name {iamIdentityProvider.DisplayName}");
        iamIdentityProvider.DisplayName.Should().Be(DisplayName);

        A.CallTo(() => _identityProviderRepository.GetNextIdpsWithoutDisplayName())
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.GetIdentityProviderDisplayName("alias1"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteProcessStep_WithMultipleIdentities_ExecutesExpected()
    {
        // Arrange
        const string DisplayName = "the company name";
        var iamIdp = new IamIdentityProvider("alias1", string.Empty, Guid.NewGuid());
        var iam2Id = Guid.NewGuid();
        A.CallTo(() => _identityProviderRepository.GetNextIdpsWithoutDisplayName())
            .Returns(new[]
            {
                (iamIdp.IdentityProviderId, iamIdp.IamIdpAlias),
                (iam2Id, "alias2")
            }.ToAsyncEnumerable());
        A.CallTo(() => _provisioningManager.GetIdentityProviderDisplayName(A<string>._))
            .Returns(DisplayName);
        A.CallTo(() => _identityProviderRepository.AttachAndModifyIamIdentityProvider(A<string>._, A<Guid>._, A<Action<IamIdentityProvider>>._, A<Action<IamIdentityProvider>>._))
            .Invokes((string _, Guid _, Action<IamIdentityProvider>? initialize, Action<IamIdentityProvider> setOptionalFields) =>
            {
                initialize?.Invoke(iamIdp);
                setOptionalFields.Invoke(iamIdp);
            });

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId, new[] { ProcessStepTypeId.SYNCHRONIZE_IDP_DISPLAY_NAME }, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Modified.Should().BeTrue();
        result.ScheduleStepTypeIds.Should().ContainSingle(x => x == ProcessStepTypeId.SYNCHRONIZE_IDP_DISPLAY_NAME);
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ProcessMessage.Should().Be($"synchronized idp {iamIdp.IdentityProviderId} with alias {iamIdp.IamIdpAlias} and display name {iamIdp.DisplayName}");
        iamIdp.DisplayName.Should().Be(DisplayName);

        A.CallTo(() => _identityProviderRepository.GetNextIdpsWithoutDisplayName())
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.GetIdentityProviderDisplayName("alias1"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.GetServiceAccountUserId("alias2"))
            .MustNotHaveHappened();
        A.CallTo(() => _identityProviderRepository.AttachAndModifyIamIdentityProvider(A<string>._, iamIdp.IdentityProviderId, A<Action<IamIdentityProvider>>._, A<Action<IamIdentityProvider>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _identityProviderRepository.AttachAndModifyIamIdentityProvider(A<string>._, iam2Id, A<Action<IamIdentityProvider>>._, A<Action<IamIdentityProvider>>._))
           .MustNotHaveHappened();
    }

    #endregion

    #region GetProcessTypeId

    [Fact]
    public void GetProcessTypeId_ReturnsExpected()
    {
        // Act
        var result = _executor.GetProcessTypeId();

        // Assert
        result.Should().Be(ProcessTypeId.IDP_DISPLAY_NAME_SYNC);
    }

    #endregion

    #region GetProcessTypeId

    [Fact]
    public async Task IsLockRequested_ReturnsExpected()
    {
        // Act
        var result = await _executor.IsLockRequested(_fixture.Create<ProcessStepTypeId>()).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsExecutableStepTypeId

    [Theory]
    [InlineData(ProcessStepTypeId.SYNCHRONIZE_IDP_DISPLAY_NAME, true)]
    [InlineData(ProcessStepTypeId.OFFERSUBSCRIPTION_CLIENT_CREATION, false)]
    [InlineData(ProcessStepTypeId.OFFERSUBSCRIPTION_TECHNICALUSER_CREATION, false)]
    [InlineData(ProcessStepTypeId.ACTIVATE_SUBSCRIPTION, false)]
    [InlineData(ProcessStepTypeId.TRIGGER_PROVIDER_CALLBACK, false)]
    [InlineData(ProcessStepTypeId.SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION, false)]
    [InlineData(ProcessStepTypeId.START_AUTOSETUP, false)]
    [InlineData(ProcessStepTypeId.END_CLEARING_HOUSE, false)]
    [InlineData(ProcessStepTypeId.START_CLEARING_HOUSE, false)]
    [InlineData(ProcessStepTypeId.DECLINE_APPLICATION, false)]
    [InlineData(ProcessStepTypeId.CREATE_IDENTITY_WALLET, false)]
    [InlineData(ProcessStepTypeId.TRIGGER_ACTIVATE_SUBSCRIPTION, false)]
    public void IsExecutableProcessStep_ReturnsExpected(ProcessStepTypeId processStepTypeId, bool expectedResult)
    {
        // Act
        var result = _executor.IsExecutableStepTypeId(processStepTypeId);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region GetExecutableStepTypeIds

    [Fact]
    public void GetExecutableStepTypeIds_ReturnsExpected()
    {
        //Act
        var result = _executor.GetExecutableStepTypeIds();

        // Assert
        result.Should().HaveCount(1).And.Satisfy(x => x == ProcessStepTypeId.SYNCHRONIZE_IDP_DISPLAY_NAME);
    }

    #endregion
}
