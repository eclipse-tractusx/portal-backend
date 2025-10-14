/********************************************************************************
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.MultiSharedIdp.Executor.Tests;

public class MultiSharedIdpExtensionsTest
{
    [Theory]
    [InlineData(ProcessStepTypeId.SYNC_MULTI_SHARED_IDP, ProcessStepTypeId.SYNC_MULTI_SHARED_IDP)]
    public void GetRetriggerStep_WithValid_ReturnsExpected(ProcessStepTypeId processStep, ProcessStepTypeId expectedStep)
    {
        // Act
        var result = processStep.GetMultiSharedIdpRetriggerStep();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be(expectedStep);
    }

    [Fact]
    public void GetRetriggerStep_WithInvalidStep_ReturnsNull()
    {
        // Act
        var ex = Assert.Throws<UnexpectedConditionException>(() => ProcessStepTypeId.AWAIT_START_AUTOSETUP.GetMultiSharedIdpRetriggerStep());

        // Assert
        ex.Message.Should().Be($"ProcessStepTypeId {ProcessStepTypeId.AWAIT_START_AUTOSETUP} is not supported for Process SYNC_MULTI_SHARED_IDP");
    }
}
