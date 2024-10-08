/********************************************************************************
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Tests;

public class ExtensionsTest
{
    private readonly IFixture _fixture;
    public ExtensionsTest()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    #region ProcessStepExtensions

    [Fact]
    public void NoStep_ReturnsExpected()
    {
        var processSteps = Enumerable.Empty<(ProcessStepTypeId, ProcessStepStatusId)>();
        var offerId = Guid.NewGuid();

        var result = processSteps.GetProcessStepTypeId(offerId);

        result.Should().BeNull();
    }

    [Fact]
    public void NoToDoStep_ReturnsExpected()
    {
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(4);
        var processStepStatusIds = new[] { ProcessStepStatusId.DONE, ProcessStepStatusId.DUPLICATE, ProcessStepStatusId.FAILED, ProcessStepStatusId.SKIPPED };
        var processSteps = processStepTypeIds.Zip(processStepStatusIds);
        var offerId = Guid.NewGuid();

        var result = processSteps.GetProcessStepTypeId(offerId);

        result.Should().BeNull();
    }

    [Fact]
    public void SingleToDoStep_ReturnsExpected()
    {
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        var processSteps = new[] { (processStepTypeId, ProcessStepStatusId.TODO) };
        var offerId = Guid.NewGuid();

        var result = processSteps.GetProcessStepTypeId(offerId);

        result.Should().Be(processStepTypeId);
    }

    [Fact]
    public void MultipleToDoSteps_SameType_ReturnsExpected()
    {
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        var processSteps = new[] { (processStepTypeId, ProcessStepStatusId.TODO), (processStepTypeId, ProcessStepStatusId.TODO) };
        var offerId = Guid.NewGuid();

        var result = processSteps.GetProcessStepTypeId(offerId);

        result.Should().Be(processStepTypeId);
    }

    [Fact]
    public void MultipleToDoSteps_DifferentType_Throws()
    {
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();
        var processSteps = processStepTypeIds.Select(processStepTypeId => (processStepTypeId, ProcessStepStatusId.TODO));
        var offerId = Guid.NewGuid();

        var result = Assert.Throws<ConflictException>(() => processSteps.GetProcessStepTypeId(offerId));

        result.Message.Should().Be($"Offers: {offerId} contains more than one process step in todo");
    }

    #endregion
}
