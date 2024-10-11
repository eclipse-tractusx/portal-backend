/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library;
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic.Tests;

public class PartnerNetworkBusinessLogicTests
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IBpnAccess _bpnAccess;
    private readonly ICompanyRepository _companyRepository;
    private readonly IPartnerNetworkBusinessLogic _sut;
    private readonly IFixture _fixture;

    public PartnerNetworkBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _bpnAccess = A.Fake<IBpnAccess>();
        _companyRepository = A.Fake<ICompanyRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);

        _sut = new PartnerNetworkBusinessLogic(_portalRepositories, _bpnAccess);
    }

    [Fact]
    public async Task GetAllMemberCompaniesBPNAsync_ReturnsExpected()
    {
        //Arrange
        var bpnIds = _fixture.CreateMany<string>(3);
        A.CallTo(() => _companyRepository.GetAllMemberCompaniesBPNAsync(A<IEnumerable<string>>._))
            .Returns(_fixture.CreateMany<string>(2).ToAsyncEnumerable());
        var uppercaseIds = bpnIds.Select(x => x.ToUpper());

        // Act
        var result = await _sut.GetAllMemberCompaniesBPNAsync(bpnIds).ToListAsync();

        // Assert
        result.Should().NotBeNull().And.HaveCount(2);
        A.CallTo(() => _companyRepository.GetAllMemberCompaniesBPNAsync(A<IEnumerable<string>>.That.IsSameSequenceAs(uppercaseIds))).MustHaveHappenedOnceExactly();
    }

    #region GetPartnerNetworkDataAsync

    [Fact]
    public async Task GetPartnerNetworkDataAsync_List_ReturnsExpected()
    {
        // Arrange
        var token = _fixture.Create<string>();
        var totalElements = 30;
        var request = new PartnerNetworkRequest(_fixture.CreateMany<string>(totalElements), "");
        var page = 0;
        var size = 10;
        var totalPages = totalElements / size;
        var legalEntities = _fixture.CreateMany<BpdmLegalEntityDto>(size);

        var responseDto = _fixture.Build<BpdmPartnerNetworkData>()
            .With(x => x.Content, legalEntities)
            .With(x => x.ContentSize, size)
            .With(x => x.TotalElements, totalElements)
            .With(x => x.TotalPages, totalPages)
            .With(x => x.Page, page)
            .Create();

        A.CallTo(() => _bpnAccess.FetchPartnerNetworkData(page, size, request.Bpnls, request.LegalName, token, A<CancellationToken>._))
            .Returns(responseDto);

        // Act
        var result = await _sut
            .GetPartnerNetworkDataAsync(page, size, request, token, CancellationToken.None);

        A.CallTo(() => _bpnAccess.FetchPartnerNetworkData(page, size, request.Bpnls, request.LegalName, token, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.Should().NotBeNull();
        result.Content.Should().HaveCount(size);
        result.TotalElements.Should().Be(totalElements);
        result.Page.Should().Be(page);
        result.TotalPages.Should().Be(totalPages);
    }

    [Fact]
    public async Task GetPartnerNetworkDataAsync_Selected_ReturnsExpected()
    {
        // Arrange
        var token = _fixture.Create<string>();
        var totalElements = 1;
        var businessPartnerNumber = "THISBPNISVALID12";
        var bpnls = new string[] { businessPartnerNumber }.AsEnumerable();
        var request = new PartnerNetworkRequest(bpnls, "");
        var page = 0;
        var size = 1;
        var totalPages = totalElements / size;

        var bpdmAddress = _fixture.Build<BpdmLegalEntityAddress>()
            .With(x => x.Bpna, businessPartnerNumber)
            .Create();

        var legalEntity = _fixture.Build<BpdmLegalEntityDto>()
            .With(x => x.Bpn, businessPartnerNumber)
            .With(x => x.LegalEntityAddress, bpdmAddress)
            .Create();

        var legalEntities = new BpdmLegalEntityDto[] { legalEntity }.AsEnumerable();

        var responseDto = _fixture.Build<BpdmPartnerNetworkData>()
            .With(x => x.Content, legalEntities)
            .With(x => x.ContentSize, size)
            .With(x => x.TotalElements, totalElements)
            .With(x => x.TotalPages, totalPages)
            .With(x => x.Page, page)
            .Create();

        A.CallTo(() => _bpnAccess.FetchPartnerNetworkData(page, size, request.Bpnls, request.LegalName, token, A<CancellationToken>._))
            .Returns(responseDto);

        // Act
        var result = await _sut
            .GetPartnerNetworkDataAsync(page, size, request, token, CancellationToken.None);

        A.CallTo(() => _bpnAccess.FetchPartnerNetworkData(page, size, request.Bpnls, request.LegalName, token, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.Should().NotBeNull();
        result.Content.First().Bpn.Should().Be(businessPartnerNumber);
        result.Content.Should().HaveCount(size);
        result.TotalElements.Should().Be(totalElements);
        result.Page.Should().Be(page);
        result.TotalPages.Should().Be(totalPages);
    }

    #endregion
}
