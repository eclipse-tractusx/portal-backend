/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models
{
    public class CompanyRoleAgreementConsentData
    {
        public CompanyRoleAgreementConsentData(Guid companyUserId, Guid companyId, CompanyApplication companyApplication, IEnumerable<CompanyAssignedRole> companyAssignedRoles, IEnumerable<Consent> consents)
        {
            CompanyUserId = companyUserId;
            CompanyId = companyId;
            CompanyApplication = companyApplication;
            CompanyAssignedRoles = companyAssignedRoles;
            Consents = consents;
        }
        public Guid CompanyUserId { get; }
        public Guid CompanyId { get; }
        public CompanyApplication CompanyApplication { get; }
        public IEnumerable<CompanyAssignedRole> CompanyAssignedRoles { get; }
        public IEnumerable<Consent> Consents { get; }
    }
}
