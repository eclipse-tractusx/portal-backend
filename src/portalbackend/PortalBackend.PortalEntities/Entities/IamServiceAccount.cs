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

using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class IamServiceAccount
{
    private IamServiceAccount()
    {
        ClientId = null!;
        ClientClientId = null!;
        UserEntityId = null!;
    }

    public IamServiceAccount(string clientId, string clientClientId, string userEntityId, Guid companyServiceAccountId)
    {
        ClientId = clientId;
        ClientClientId = clientClientId;
        UserEntityId = userEntityId;
        CompanyServiceAccountId = companyServiceAccountId;
    }

    [Key]
    [StringLength(36)]
    public string ClientId { get; private set; }

    [StringLength(255)]
    public string ClientClientId { get; private set; }

    [StringLength(36)]
    public string UserEntityId { get; private set; }

    public Guid CompanyServiceAccountId { get; private set; }

    // Navigation properties
    public virtual CompanyServiceAccount? CompanyServiceAccount { get; private set; }
}
