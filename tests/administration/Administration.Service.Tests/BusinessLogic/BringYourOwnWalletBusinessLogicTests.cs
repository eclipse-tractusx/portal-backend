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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class BringYourOwnWalletBusinessLogicTests
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly ICompanyRepository _companyRepository;
    private readonly IOptions<BringYourOwnWalletSettings> _options;
    private readonly BringYourOwnWalletBusinessLogic _sut;
    private readonly Guid _companyId = Guid.NewGuid();
    private readonly Guid _applicationId = Guid.NewGuid();
    private readonly Guid _excludedRoleId = Guid.NewGuid();

    public BringYourOwnWalletBusinessLogicTests()
    {
        _portalRepositories = A.Fake<IPortalRepositories>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _options = Options.Create(new BringYourOwnWalletSettings
        {
            NonApplicableUserRoles = new List<Guid> { _excludedRoleId }
        });
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        _sut = new BringYourOwnWalletBusinessLogic(_portalRepositories, _options);
    }

    [Fact]
    public void GetExcludedUserRoles_ReturnsConfiguredRoles()
    {
        // Act
        var result = _sut.GetExcludedUserRoles();

        // Assert
        result.Should().ContainSingle().Which.Should().Be(_excludedRoleId);
    }

    [Fact]
    public async Task IsBringYourOwnWallet_ReturnsExpectedValue()
    {
        // Arrange
        A.CallTo(() => _companyRepository.GetApplicationIdByCompanyId(_companyId)).Returns(_applicationId);
        A.CallTo(() => _companyRepository.IsBringYourOwnWallet(_applicationId)).Returns(true);

        // Act
        var result = await _sut.IsBringYourOwnWallet(_companyId);

        // Assert
        result.Should().BeTrue();
    }
}
