using System.Text;
using FakeItEasy;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class DocumentsControllerTests
{
    private const string IamUserId = "4C1A6851-D4E7-4E10-A011-3732CD045E8A";
    private readonly IDocumentsBusinessLogic _logic;
    private readonly DocumentsController _controller;

    public DocumentsControllerTests()
    {
        _logic = A.Fake<IDocumentsBusinessLogic>();
        this._controller = new DocumentsController(_logic);
        _controller.AddControllerContextWithClaim(IamUserId);
    }

    [Fact]
    public async Task CreateServiceProviderCompanyDetail_WithValidData_ReturnsOk()
    {
        //Arrange
        const string fileName = "test.pdf";
        var id = Guid.NewGuid();
        var content = Encoding.UTF8.GetBytes("This is just test content");
        A.CallTo(() => _logic.GetDocumentAsync(id, IamUserId))
            .ReturnsLazily(() => (fileName, content));

        //Act
        await this._controller.GetDocumentContentFileAsync(id).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetDocumentAsync(id, IamUserId)).MustHaveHappenedOnceExactly();
    }
}