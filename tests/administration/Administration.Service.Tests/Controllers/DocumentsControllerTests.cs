/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class DocumentsControllerTests
{
    private readonly IIdentityData _identity;
    private readonly IDocumentsBusinessLogic _logic;
    private readonly DocumentsController _controller;
    private readonly Fixture _fixture;

    public DocumentsControllerTests()
    {
        _fixture = new Fixture();
        _identity = A.Fake<IIdentityData>();
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(Guid.NewGuid());
        _logic = A.Fake<IDocumentsBusinessLogic>();
        _controller = new DocumentsController(_logic);
        _controller.AddControllerContextWithClaim(_identity);
    }

    [Fact]
    public async Task GetDocumentContentFileAsync_WithValidData_ReturnsOk()
    {
        //Arrange
        const string fileName = "test.pdf";
        const string contentType = "application/pdf";
        var id = Guid.NewGuid();
        var content = Encoding.UTF8.GetBytes("This is just test content");
        A.CallTo(() => _logic.GetDocumentAsync(A<Guid>._))
            .Returns((fileName, content, contentType));

        //Act
        await _controller.GetDocumentContentFileAsync(id);

        //Assert
        A.CallTo(() => _logic.GetDocumentAsync(id)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetSelfDescriptionDocumentsAsync_WithValidData_ReturnsOk()
    {
        //Arrange
        const string fileName = "self_description.json";
        var id = Guid.NewGuid();
        var content = Encoding.UTF8.GetBytes("This is just test content");
        A.CallTo(() => _logic.GetSelfDescriptionDocumentAsync(id))
            .Returns((fileName, content, "application/json"));

        //Act
        await _controller.GetSelfDescriptionDocumentsAsync(id);

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
            .Returns(documentData);

        //Act
        await _controller.GetDocumentSeedData(id);

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
            .Returns(("test.json", content));

        //Act
        var result = await _controller.GetFrameDocumentAsync(documentId);

        // Assert
        A.CallTo(() => _logic.GetFrameDocumentAsync(documentId)).MustHaveHappenedOnceExactly();
        result.Should().NotBeNull();
    }
}
