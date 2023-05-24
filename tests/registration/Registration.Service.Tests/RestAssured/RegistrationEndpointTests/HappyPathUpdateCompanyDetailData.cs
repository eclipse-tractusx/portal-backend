using System.Text.Json;
using System.Text.Json.Serialization;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using PasswordGenerator;
using Xunit;
using static RestAssured.Dsl;

namespace Registration.Service.Tests.RestAssured.RegistrationEndpointTests;

[TestCaseOrderer("Registration.Service.Tests.RestAssured.AlphabeticalOrderer",
    "Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests")]
public class HappyPathUpdateCompanyDetailData
{
    private static readonly string _baseUrl = "https://portal-backend.dev.demo.catena-x.net";
    private static readonly string _endPoint = "/api/registration";
    private readonly string _adminEndPoint = "/api/administration";
    private static string _userCompanyToken;
    private static string? _applicationId;
    private static string? _operatorToken;
    private readonly string _operatorCompanyName = "CX-Operator";

    private static string _userCompanyName = "Test-Catena-X-13";
    private static string[] _userEmailAddress;
    private static RegistrationEndpointHelper _regEndpointHelper;
    private TestDataHelper _testDataHelper = new TestDataHelper();

    JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
    
    // POST api/administration/invitation

    [Fact]
    public async Task Test1_ExecuteInvitation_ReturnsExpectedResult()
    {
        DevMailApiRequests devMailApiRequests = new DevMailApiRequests();
        var devUser = devMailApiRequests.GenerateRandomEmailAddress();
        var emailAddress = devUser.Result.Name + "@developermail.com";
        CompanyInvitationData invitationData = new CompanyInvitationData("testuser", "myFirstName", "myLastName",
            emailAddress, _userCompanyName);
        
        Thread.Sleep(20000);
        
        _operatorToken = await new AuthFlow(_operatorCompanyName).GetAccessToken();
        
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .ContentType("application/json")
            .Body(invitationData)
            .When()
            .Post($"{_baseUrl}{_adminEndPoint}/invitation")
            .Then()
            .StatusCode(200);
    
        Thread.Sleep(20000);
    
        var messageData = devMailApiRequests.FetchPassword();
        if (messageData is null)
        {
            throw new Exception("User password could not be fetched.");
        }
    
        var newPassword = new Password().Next();
        _userCompanyToken =
            await new AuthFlow(_userCompanyName).UpdatePasswordAndGetAccessToken(emailAddress, messageData,
                newPassword);
        _regEndpointHelper = new RegistrationEndpointHelper(_userCompanyToken, _operatorToken);
        _applicationId = _regEndpointHelper.GetFirstApplicationId();
    }

    // POST /api/registration/application/{applicationId}/companyDetailsWithAddress

    [Fact]
    public void Test2_SetCompanyDetailData_ReturnsExpectedResult()
    {
        if (_regEndpointHelper.GetApplicationStatus() == CompanyApplicationStatusId.CREATED.ToString())
        {
            _regEndpointHelper.SetApplicationStatus(CompanyApplicationStatusId.ADD_COMPANY_DATA.ToString());
            var companyDetailData = _regEndpointHelper.GetCompanyDetailData();

            var testCompanyDetailData = _testDataHelper.GetNewCompanyDetailDataFromTestData();
            var newCompanyDetailData = testCompanyDetailData with
            {
                CompanyId = companyDetailData.CompanyId
            };
            var body = JsonSerializer.Serialize(newCompanyDetailData, _options);
            var response = Given()
                .RelaxedHttpsValidation()
                .Header(
                    "authorization",
                    $"Bearer {_userCompanyToken}")
                .ContentType("application/json")
                .When()
                .Body(body)
                .Post($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyDetailsWithAddress")
                .Then()
                .StatusCode(200);
            CompanyDetailData storedCompanyDetailData = _regEndpointHelper.GetCompanyDetailData();
            if (!VerifyCompanyDetailDataStorage(storedCompanyDetailData, newCompanyDetailData))
                throw new Exception($"Company detail data was not stored correctly");
        }
        else throw new Exception($"Application status is not fitting to the pre-requisite");
    }
    
    // POST /api/registration/application/{applicationId}/companyDetailsWithAddress

    [Fact]
    public void Test3_UpdateCompanyDetailData_ReturnsExpectedResult()
    {
        var actualStatus = _regEndpointHelper.GetApplicationStatus();
        if (actualStatus != CompanyApplicationStatusId.ADD_COMPANY_DATA.ToString() && actualStatus != CompanyApplicationStatusId.SUBMITTED.ToString())
        {
            var companyDetailData = _regEndpointHelper.GetCompanyDetailData();

            var updateCompanyDetailData = _testDataHelper.GetUpdateCompanyDetailDataFromTestData();
            var newCompanyDetailData = updateCompanyDetailData with
            {
                CompanyId = companyDetailData.CompanyId
            };
            var body = JsonSerializer.Serialize(newCompanyDetailData, _options);
            
            var response = Given()
                .RelaxedHttpsValidation()
                .Header(
                    "authorization",
                    $"Bearer {_userCompanyToken}")
                .ContentType("application/json")
                .When()
                .Body(body)
                .Post($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyDetailsWithAddress")
                .Then()
                .StatusCode(200);
            CompanyDetailData storedCompanyDetailData = _regEndpointHelper.GetCompanyDetailData();
            if (!VerifyCompanyDetailDataStorage(storedCompanyDetailData, newCompanyDetailData))
                throw new Exception($"Company detail data was not updated correctly");
        }
        else throw new Exception($"Application status is not fitting to the pre-requisite");
    }
    
    private bool VerifyCompanyDetailDataStorage(CompanyDetailData storedData, CompanyDetailData postedData)
    {
        bool isEqual = storedData.UniqueIds.SequenceEqual(postedData.UniqueIds);
        if (storedData.CompanyId == postedData.CompanyId && isEqual && storedData.Name == postedData.Name &&
            storedData.StreetName == postedData.StreetName &&
            storedData.CountryAlpha2Code == postedData.CountryAlpha2Code &&
            storedData.BusinessPartnerNumber == postedData.BusinessPartnerNumber &&
            storedData.ShortName == postedData.ShortName &&
            storedData.Region == postedData.Region && storedData.StreetAdditional == postedData.StreetAdditional &&
            storedData.StreetNumber == postedData.StreetNumber &&
            storedData.ZipCode == postedData.ZipCode && storedData.CountryDe == postedData.CountryDe) return true;
        else return false;
    }
}