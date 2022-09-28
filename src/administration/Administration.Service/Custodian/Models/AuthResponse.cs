/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Custodian.Models;

public class AuthResponse
{
    public string? access_token { get; set; }
    public int expires_in { get; set; }
    public int refresh_expires_in { get; set; }
    public string? refresh_token { get; set; }
    public string? token_type { get; set; }
    public string? id_token { get; set; }
    public int notbeforepolicy { get; set; }
    public string? session_state { get; set; }
    public string? scope { get; set; }
}
