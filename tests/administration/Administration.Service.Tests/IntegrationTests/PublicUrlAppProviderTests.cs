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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers.V1;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.IntegrationTests;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.TestSeeds;
using System.Diagnostics.CodeAnalysis;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.IntegrationTests;

public class PublicUrlAppProviderTests : BasePublicUrlTests<CompanyDataController, AppProviderSeeding>
{
    public PublicUrlAppProviderTests(IntegrationTestFactory<CompanyDataController, AppProviderSeeding> factory)
        : base(factory)
    { }

    [Fact]
    [SuppressMessage("SonarLint", "S2699", Justification = "Ignored because the assert is done in OpenInformationController_ReturnsCorrectAmount")]
    public async Task OpenInformationController_WithAppProvider_ReturnsCorrectAmount()
    {
        await OpenInformationController_ReturnsCorrectAmount(8,
            x => x.HttpMethods == "GET" && x.Url == "api/v{version:apiversion}/administration/connectors/managed",
            x => x.HttpMethods == "POST" && x.Url == "api/v{version:apiversion}/administration/connectors/managed",
            x => x.HttpMethods == "POST" && x.Url == "api/v{version:apiversion}/administration/connectors/discovery",
            x => x.HttpMethods == "GET" && x.Url == "api/v{version:apiversion}/administration/documents/selfdescription/{documentid}",
            x => x.HttpMethods == "GET" && x.Url == "api/v{version:apiversion}/administration/partnernetwork/membercompanies",
            x => x.HttpMethods == "GET" && x.Url == "api/v{version:apiversion}/administration/subscriptionconfiguration/owncompany",
            x => x.HttpMethods == "GET" && x.Url == "api/v{version:apiversion}/administration/subscriptionconfiguration/process/offer-subscription/{offersubscriptionid}",
            x => x.HttpMethods == "POST" && x.Url == "api/v{version:apiversion}/administration/subscriptionconfiguration/process/offer-subscription/{offersubscriptionid}/retrigger-provider-callback")
            .ConfigureAwait(false);
    }
}
