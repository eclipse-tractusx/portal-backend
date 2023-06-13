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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class DocumentsControllerTests
{
    private const string IamUserId = "4C1A6851-D4E7-4E10-A011-3732CD045E8A";
    private readonly IdentityData _identity = new(IamUserId, Guid.NewGuid(), IdentityTypeId.COMPANY_USER, Guid.NewGuid());
    private readonly IDocumentsBusinessLogic _logic;
    private readonly DocumentsController _controller;
    private readonly Fixture _fixture;

    public DocumentsControllerTests()
    {
        _fixture = new Fixture();
        _logic = A.Fake<IDocumentsBusinessLogic>();
        this._controller = new DocumentsController(_logic);
        _controller.AddControllerContextWithClaim(IamUserId, _identity);
    }

    [Fact]
    public async Task GetDocumentContentFileAsync_WithValidData_ReturnsOk()
    {
        //Arrange
        const string fileName = "test.pdf";
        const string contentType = "application/pdf";
        var id = Guid.NewGuid();
        var content = Encoding.UTF8.GetBytes("This is just test content");
        A.CallTo(() => _logic.GetDocumentAsync(A<Guid>._, A<Guid>._))
            .Returns((fileName, content, contentType));

        //Act
        await this._controller.GetDocumentContentFileAsync(id).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetDocumentAsync(id, _identity.CompanyId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetSelfDescriptionDocumentsAsync_WithValidData_ReturnsOk()
    {
        //Arrange
        const string fileName = "self_description.json";
        var id = Guid.NewGuid();
        var content = Encoding.UTF8.GetBytes("This is just test content");
        A.CallTo(() => _logic.GetSelfDescriptionDocumentAsync(id))
            .ReturnsLazily(() => (fileName, content, "application/json"));

        //Act
        await this._controller.GetSelfDescriptionDocumentsAsync(id).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetSelfDescriptionDocumentAsync(id)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetDocumentSeedData_WithValidId_ReturnsData()
    {
        //Arrange
        var id = Guid.NewGuid();
        var documentData = _fixture.Create<DocumentSeedData>();
        A.CallTo(() => _logic.GetSeedData(id))
            .ReturnsLazily(() => documentData);

        //Act
        await this._controller.GetDocumentSeedData(id).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetSeedData(id)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetFrameDocumentAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var documentId = _fixture.Create<Guid>();
        var content = new byte[7];
        A.CallTo(() => _logic.GetFrameDocumentAsync(documentId))
            .ReturnsLazily(() => new ValueTuple<string, byte[]>("test.json", content));

        //Act
        var result = await this._controller.GetFrameDocumentAsync(documentId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.GetFrameDocumentAsync(documentId)).MustHaveHappenedOnceExactly();
        result.Should().NotBeNull();
    }
}
