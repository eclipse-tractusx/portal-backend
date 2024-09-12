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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.UserProvisioning.Executor.Tests;

public class UserProvisioningExtensionsTest
{
    [Theory]
    [InlineData(ProcessStepTypeId.DELETE_CENTRAL_USER, ProcessStepTypeId.RETRIGGER_DELETE_CENTRAL_USER)]
    public void GetRetriggerStep_WithValid_ReturnsExpected(ProcessStepTypeId processStep, ProcessStepTypeId expectedStep)
    {
        // Act
        var result = processStep.GetUserProvisioningRetriggerStep();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be(expectedStep);
    }

    [Fact]
    public void GetRetriggerStep_WithInvalidStep_ReturnsNull()
    {
        // Act
        var ex = Assert.Throws<UnexpectedConditionException>(() => ProcessStepTypeId.AWAIT_START_AUTOSETUP.GetUserProvisioningRetriggerStep());

        // Assert
        ex.Message.Should().Be($"ProcessStepTypeId {ProcessStepTypeId.AWAIT_START_AUTOSETUP} is not supported for Process UserProvisioning");
    }
}
