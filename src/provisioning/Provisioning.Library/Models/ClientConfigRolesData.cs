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

using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;

public class ClientConfigRolesData
{
    public ClientConfigRolesData(string name, string description, IamClientAuthMethod iamClientAuthMethod, IDictionary<string, IEnumerable<string>> clientRoles)
    {
        Name = name;
        Description = description;
        IamClientAuthMethod = iamClientAuthMethod;
        ClientRoles = clientRoles;
    }

    public string Name { get; set; }
    public string Description { get; set; }
    public IamClientAuthMethod IamClientAuthMethod { get; set; }
    public IDictionary<string,IEnumerable<string>> ClientRoles { get; set; }
}
