using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.RestAssured;

public class AdministrationEndpointTests
{
    private readonly string _baseUrl = "https://portal-backend.dev.demo.catena-x.net";
    private readonly string _endPoint = "/api/administration";
    private readonly string _operatorToken;
    private static string _applicationId;
    private static string _companyName = "Catena-X";

    [Fact]
    public void GetApplicationDetails_ReturnsExpectedResult()
    {
        // Given
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/registration/applications?companyName={_companyName}/?page=0&size=10&sorting=DateDesc")
            .Then()
            .StatusCode(200);
    }
    
    [Fact]
    public void GetCompanyWithAddress_ReturnsExpectedResult()
    {
        // Given
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/registration/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .StatusCode(200);
    }
}