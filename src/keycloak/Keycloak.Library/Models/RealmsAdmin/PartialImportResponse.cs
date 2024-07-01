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

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.RealmsAdmin;

public class PartialImportResponse
{
    public int? Overwritten { get; set; }
    public int? Added { get; set; }
    public int? Skipped { get; set; }
    public IEnumerable<PartialImportResult>? Results { get; set; }
}

public class PartialImportResult
{
    public string? Action { get; set; }
    public string? ResourceType { get; set; }
    public string? ResourceName { get; set; }
    public string? Id { get; set; }
}
