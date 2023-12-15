/********************************************************************************
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
    // ApplicationChecklist Process
    VERIFY_REGISTRATION = 1,
    CREATE_BUSINESS_PARTNER_NUMBER_PUSH = 2,
    CREATE_BUSINESS_PARTNER_NUMBER_PULL = 3,
    CREATE_BUSINESS_PARTNER_NUMBER_MANUAL = 4,
    CREATE_IDENTITY_WALLET = 5,
    RETRIGGER_IDENTITY_WALLET = 6,
    START_CLEARING_HOUSE = 7,
    RETRIGGER_CLEARING_HOUSE = 8,
    END_CLEARING_HOUSE = 9,
    START_SELF_DESCRIPTION_LP = 10,
    RETRIGGER_SELF_DESCRIPTION_LP = 11,
    ACTIVATE_APPLICATION = 12,
    RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH = 13,
    RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL = 14,
    OVERRIDE_BUSINESS_PARTNER_NUMBER = 15,
    TRIGGER_OVERRIDE_CLEARING_HOUSE = 16,
    START_OVERRIDE_CLEARING_HOUSE = 17,
    FINISH_SELF_DESCRIPTION_LP = 18,
    DECLINE_APPLICATION = 19,

    // OfferSubscriptionProcess
    TRIGGER_PROVIDER = 100,
    START_AUTOSETUP = 101,
    OFFERSUBSCRIPTION_CLIENT_CREATION = 102,
    SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION = 103,
    OFFERSUBSCRIPTION_TECHNICALUSER_CREATION = 104,
    ACTIVATE_SUBSCRIPTION = 105,
    TRIGGER_PROVIDER_CALLBACK = 106,
    RETRIGGER_PROVIDER = 107,
    RETRIGGER_OFFERSUBSCRIPTION_CLIENT_CREATION = 108,
    RETRIGGER_OFFERSUBSCRIPTION_TECHNICALUSER_CREATION = 109,
    RETRIGGER_PROVIDER_CALLBACK = 110,
    TRIGGER_ACTIVATE_SUBSCRIPTION = 111,

    // NetworkRegistration
    SYNCHRONIZE_USER = 200,
    RETRIGGER_SYNCHRONIZE_USER = 201,
    TRIGGER_CALLBACK_OSP_SUBMITTED = 202,
    TRIGGER_CALLBACK_OSP_APPROVED = 203,
    TRIGGER_CALLBACK_OSP_DECLINED = 204,
    RETRIGGER_CALLBACK_OSP_SUBMITTED = 205,
    RETRIGGER_CALLBACK_OSP_APPROVED = 206,
    RETRIGGER_CALLBACK_OSP_DECLINED = 207,
}
