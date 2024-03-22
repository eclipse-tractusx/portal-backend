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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.NetworkRegistration.Executor.Tests;

public class NetworkRegistrationExtensisonsTests
{
    [Theory]
    [InlineData(ProcessStepTypeId.SYNCHRONIZE_USER, ProcessStepTypeId.RETRIGGER_SYNCHRONIZE_USER)]
    [InlineData(ProcessStepTypeId.REMOVE_KEYCLOAK_USERS, ProcessStepTypeId.RETRIGGER_REMOVE_KEYCLOAK_USERS)]
    [InlineData(ProcessStepTypeId.TRIGGER_CALLBACK_OSP_SUBMITTED, ProcessStepTypeId.RETRIGGER_CALLBACK_OSP_SUBMITTED)]
    [InlineData(ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED, ProcessStepTypeId.RETRIGGER_CALLBACK_OSP_DECLINED)]
    [InlineData(ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED, ProcessStepTypeId.RETRIGGER_CALLBACK_OSP_APPROVED)]
    public void GetRetriggerStep_WithValid_ReturnsExpected(ProcessStepTypeId processStep, ProcessStepTypeId expectedStep)
    {
        // Act
        var result = processStep.GetNetworkRetriggerStep();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be(expectedStep);
    }

    [Fact]
    public void GetRetriggerStep_WithInvalidStep_ReturnsNull()
    {
        // Act
        var ex = Assert.Throws<UnexpectedConditionException>(() => ProcessStepTypeId.START_AUTOSETUP.GetNetworkRetriggerStep());

        // Assert
        ex.Message.Should().Be("ProcessStepTypeId START_AUTOSETUP is not supported for Process NetworkRegistration");
    }
}
