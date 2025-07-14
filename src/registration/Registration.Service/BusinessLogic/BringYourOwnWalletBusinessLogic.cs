/********************************************************************************
 * Copyright (c) 2025 Cofinity-X GmbH
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.UniversalDidResolver.Library;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic
{
    public class BringYourOwnWalletBusinessLogic : IBringYourOwnWalletBusinessLogic
    {
        private readonly IUniversalDidResolverService _universalResolverService;
        private readonly IPortalRepositories _portalRepositories;

        public BringYourOwnWalletBusinessLogic(IUniversalDidResolverService universalResolverService, IPortalRepositories portalRepositories)
        {
            _universalResolverService = universalResolverService;
            _portalRepositories = portalRepositories;

        }
        public async Task<JsonDocument> ValidateDid(string did, CancellationToken cancellationToken)
        {
            var validationResult = await _universalResolverService.ValidateDid(did, cancellationToken);
            var isSchemaValid = await _universalResolverService.ValidateSchema(
                validationResult.DidDocument,
                cancellationToken
            ).ConfigureAwait(false);

            if (!isSchemaValid)
            {
                throw new UnsupportedMediaTypeException("DID validation failed. DID Document is not valid.");
            }
            return validationResult.DidDocument;
        }

        public async Task SaveCustomerWalletAsync(Guid companyId, string did)
        {
            if (string.IsNullOrEmpty(did))
            {
                throw new ForbiddenException("Invalid DID. DID cannot be empty or NULL.");
            }

            var companyRepository = _portalRepositories.GetInstance<ICompanyRepository>();
            var didDocument = await ValidateDid(did, CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.None);
            UpdateDidLocation(companyId, did, companyRepository);
            companyRepository.CreateCustomerWallet(companyId, did, didDocument);

            await _portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        }

        public async Task<string> getCompanyWalletDidAsync(Guid companyId)
        {
            var companyRepository = _portalRepositories.GetInstance<ICompanyRepository>();
            var did = await companyRepository.GetCompanyHolderDidAsync(companyId).ConfigureAwait(ConfigureAwaitOptions.None);
            if (string.IsNullOrEmpty(did))
            {
                throw new NotFoundException("Company wallet DID not found for the given company ID.");
            }
            return did;
        }
        private static void UpdateDidLocation(Guid companyId, string didLocation, ICompanyRepository companyRepository) =>
            companyRepository.AttachAndModifyCompany(
                companyId,
                _ => { },
                c => { c.DidDocumentLocation = didLocation; }
            );
    }
}
