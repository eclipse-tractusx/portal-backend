/********************************************************************************
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
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.ErrorHandling;
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
                throw ControllerArgumentException.Create(RegistrationErrors.REGISTRATION_INVALID_DID_DOCUMENT);
            }

            var companyRepository = _portalRepositories.GetInstance<ICompanyRepository>();
            var didExists = await companyRepository.IsDidInUse(did).ConfigureAwait(ConfigureAwaitOptions.None);
            if (didExists)
            {
                throw ConflictException.Create(RegistrationErrors.REGISTRATION_CONFLICT_DID_ALREADY_IN_USE);
            }

            return validationResult.DidDocument;
        }

        public async Task SaveCustomerWalletAsync(Guid companyId, string did)
        {
            var companyRepository = _portalRepositories.GetInstance<ICompanyRepository>();
            var companyExists = await companyRepository.IsExistingCompany(companyId).ConfigureAwait(ConfigureAwaitOptions.None);
            if (!companyExists)
            {
                throw NotFoundException.Create(RegistrationErrors.REGISTRATION_CONFLICT_DID_ALREADY_IN_USE);
            }

            if (string.IsNullOrEmpty(did))
            {
                throw ControllerArgumentException.Create(RegistrationErrors.REGISTRATION_INVALID_DID_DOCUMENT);
            }

            var didDocument = await ValidateDid(did, CancellationToken.None).ConfigureAwait(ConfigureAwaitOptions.None);
            var didLocation = CreateDidLocation(didDocument);

            UpdateCompanyDidLocation(companyId, didLocation, companyRepository);
            await companyRepository.CreateCustomerWallet(companyId, did, didDocument).ConfigureAwait(ConfigureAwaitOptions.None);

            await _portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        }

        public async Task<string> getCompanyWalletDidAsync(Guid companyId)
        {
            var companyRepository = _portalRepositories.GetInstance<ICompanyRepository>();
            var companyExists = await companyRepository.IsExistingCompany(companyId).ConfigureAwait(ConfigureAwaitOptions.None);
            if (!companyExists)
            {
                throw NotFoundException.Create(RegistrationErrors.REGISTRATION_COMPANY_ID_NOT_FOUND, [new("companyId", companyId.ToString())]);
            }

            var did = await companyRepository.GetCompanyHolderDidAsync(companyId).ConfigureAwait(ConfigureAwaitOptions.None);
            if (string.IsNullOrEmpty(did))
            {
                throw NotFoundException.Create(RegistrationErrors.REGISTRATION_COMPANY_ID_NOT_FOUND, [new("companyId", companyId.ToString())]);
            }
            return did;
        }
        private static void UpdateCompanyDidLocation(Guid companyId, string didLocation, ICompanyRepository companyRepository) =>
            companyRepository.AttachAndModifyCompany(
                companyId,
                _ => { },
                c =>
                {
                    c.DidDocumentLocation = didLocation;
                }
            );

        private static string CreateDidLocation(JsonDocument didDocument)
        {
            if (!didDocument.RootElement.TryGetProperty("id", out var idProperty))
            {
                throw ControllerArgumentException.Create(RegistrationErrors.REGISTRATION_INVALID_DID_DOCUMENT);

            }

            var did = idProperty.GetString();
            if (string.IsNullOrWhiteSpace(did) || !did.StartsWith("did:", StringComparison.OrdinalIgnoreCase))
            {
                throw ControllerArgumentException.Create(RegistrationErrors.REGISTRATION_INVALID_DID_FORMAT);
            }

            var didParts = did.Split(':', 3);
            if (didParts.Length != 3)
            {
                throw ControllerArgumentException.Create(RegistrationErrors.REGISTRATION_INVALID_DID_FORMAT);
            }

            var method = didParts[1];
            var identifier = didParts[2];

            if (!method.Equals("web", StringComparison.OrdinalIgnoreCase))
            {
                throw ControllerArgumentException.Create(RegistrationErrors.REGISTRATION_UNSUPPORTED_DID_METHOD, [new("method", method)]);
            }

            var hostAndPath = identifier.Replace(":", "/");

            var isBareDomain = !hostAndPath.Contains("/");
            var urlPath = isBareDomain ? "/.well-known/did.json" : "/did.json";

            return $"https://{hostAndPath}{urlPath}";
        }
    }
}
