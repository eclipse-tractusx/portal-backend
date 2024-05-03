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

using System.Runtime.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.Models;

public enum UseCaseFrameworkId
{
    [EnumMember(Value = "Traceability Framework")]
    TRACEABILITY_FRAMEWORK = 1,

    [EnumMember(Value = "PCF Framework")]
    PCF_FRAMEWORK = 2,

    [EnumMember(Value = "Behavior Twin Framework")]
    BEHAVIOR_TWIN_FRAMEWORK = 3,

    [EnumMember(Value = "Dismantler Certificate")]
    DISMANTLER_CERTIFICATE = 4,

    [EnumMember(Value = "Circular Economy")]
    CIRCULAR_ECONOMY = 5,

    [EnumMember(Value = "BusinessPartnerCredential")]
    BUSINESS_PARTNER_NUMBER = 7,

    [EnumMember(Value = "Demand and Capacity Management")]
    DEMAND_AND_CAPACITY_MANAGEMENT = 8,

    [EnumMember(Value = "Demand and Capacity Management")]
    DEMAND_AND_CAPACITY_MANAGEMENT_PURIS = 9,

    [EnumMember(Value = "Business Partner Data Management")]
    BUSINESS_PARTNER_DATA_MANAGEMENT = 10
}
