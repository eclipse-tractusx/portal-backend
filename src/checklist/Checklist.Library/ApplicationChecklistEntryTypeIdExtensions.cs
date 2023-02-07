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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;

public static class ApplicationChecklistEntryTypeIdExtensions
{
    public static bool IsAutomated(this ApplicationChecklistEntryTypeId entryTypeId) =>
        entryTypeId switch
        {
            ApplicationChecklistEntryTypeId.CLEARING_HOUSE => true,
            ApplicationChecklistEntryTypeId.IDENTITY_WALLET => true,
            ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP => true,
            _ => false
        };

    public static ProcessStepTypeId? GetManualTriggerProcessStepId(this ApplicationChecklistEntryTypeId entryTypeId) =>
        entryTypeId switch
        {
            ApplicationChecklistEntryTypeId.CLEARING_HOUSE => ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE,
            ApplicationChecklistEntryTypeId.IDENTITY_WALLET => ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET,
            ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP => ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_LP,
            _ => null,
        };
    
    public static ProcessStepTypeId? GetProcessStepForChecklistEntry(this ApplicationChecklistEntryTypeId entryTypeId) =>
        entryTypeId switch
        {
            ApplicationChecklistEntryTypeId.CLEARING_HOUSE => ProcessStepTypeId.START_CLEARING_HOUSE,
            ApplicationChecklistEntryTypeId.IDENTITY_WALLET => ProcessStepTypeId.CREATE_IDENTITY_WALLET,
            ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP => ProcessStepTypeId.CREATE_SELF_DESCRIPTION_LP,
            _ => null,
        };
}
