﻿using System.Text;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class DocumentsControllerTests
{
    private const string IamUserId = "4C1A6851-D4E7-4E10-A011-3732CD045E8A";
    private readonly IDocumentsBusinessLogic _logic;
    private readonly DocumentsController _controller;
    private readonly Fixture _fixture;

    public DocumentsControllerTests()
    {
        _fixture = new Fixture();
        _logic = A.Fake<IDocumentsBusinessLogic>();
        this._controller = new DocumentsController(_logic);
        _controller.AddControllerContextWithClaim(IamUserId);
    }

    [Fact]
    public async Task GetDocumentContentFileAsync_WithValidData_ReturnsOk()
    {
        //Arrange
        const string fileName = "test.pdf";
        const string contentType = "application/pdf";
        var id = Guid.NewGuid();
        var content = Encoding.UTF8.GetBytes("This is just test content");
        A.CallTo(() => _logic.GetDocumentAsync(id, IamUserId))
            .ReturnsLazily(() => (fileName, content, contentType));

        //Act
        await this._controller.GetDocumentContentFileAsync(id).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetDocumentAsync(id, IamUserId)).MustHaveHappenedOnceExactly();
    }
    
    [Fact]
    public async Task GetSelfDescriptionDocumentsAsync_WithValidData_ReturnsOk()
    {
        //Arrange
        const string fileName = "self_description.json";
        var id = Guid.NewGuid();
        var content = Encoding.UTF8.GetBytes("This is just test content");
        A.CallTo(() => _logic.GetSelfDescriptionDocumentAsync(id))
            .ReturnsLazily(() => (fileName, content));

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
}