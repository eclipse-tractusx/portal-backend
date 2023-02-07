/********************************************************************************
 * Copyright (c) 2021,2023 BMW Group AG
 * Copyright (c) 2021,2023 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

public enum ProcessStepTypeId
{
    VERIFY_REGISTRATION = 1,
    CREATE_BUSINESS_PARTNER_NUMBER_PUSH = 2,
    CREATE_BUSINESS_PARTNER_NUMBER_PULL = 3,
    CREATE_BUSINESS_PARTNER_NUMBER_MANUAL = 4,
    CREATE_IDENTITY_WALLET = 5,
    RETRIGGER_IDENTITY_WALLET = 6,
    START_CLEARING_HOUSE = 7,
    RETRIGGER_CLEARING_HOUSE = 8,
    END_CLEARING_HOUSE = 9,
    CREATE_SELF_DESCRIPTION_LP = 10,
    RETRIGGER_SELF_DESCRIPTION_LP = 11,
    ACTIVATE_APPLICATION = 12,
}
