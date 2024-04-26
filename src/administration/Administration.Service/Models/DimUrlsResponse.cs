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

using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;

public record DimUrlsResponse(
    [property: JsonPropertyName("trusted_issuer")] string IssuerDid,
    [property: JsonPropertyName("participant_id")] string Bpnl,
    [property: JsonPropertyName("iatp_id")] string HolderDid,
    [property: JsonPropertyName("did_resolver")] string BpnDidResolverUrl,
    [property: JsonPropertyName("decentralIdentityManagementAuthUrl")] string DecentralIdentityManagementAuthUrl,
    [property: JsonPropertyName("decentralIdentityManagementServiceUrl")] string DecentralIdentityManagementServiceUrl
);
