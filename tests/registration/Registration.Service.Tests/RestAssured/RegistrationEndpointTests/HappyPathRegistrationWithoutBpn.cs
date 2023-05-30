using System.Text.Json;
using System.Text.Json.Serialization;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using PasswordGenerator;
using Xunit;
using static RestAssured.Dsl;

namespace Registration.Service.Tests.RestAssured.RegistrationEndpointTests;

[TestCaseOrderer("Registration.Service.Tests.RestAssured.AlphabeticalOrderer",
    "Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests")]
public class RegistrationEndpointTestsHappyPathRegistrationWithoutBpn
{
    private static readonly string _baseUrl = "https://portal-backend.dev.demo.catena-x.net";
    private static readonly string _endPoint = "/api/registration";
    private static string _userCompanyToken;
    private static string? _applicationId;

    private readonly string _adminEndPoint = "/api/administration";
    private static string? _operatorToken;
    private readonly string _operatorCompanyName = "CX-Operator";
    private static string _userCompanyName = "Test-Catena-X-35";
    private static string[] _userEmailAddress;
    private static RegistrationEndpointHelper _regEndpointHelper;
    private static TestDataHelper _testDataHelper = new TestDataHelper();
    private readonly Secrets _secrets = new ();
    
    JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    #region Happy Path - new registration without BPN
    
    [Theory]
    [MemberData(nameof(GetDataEntries))]
    public async Task Scenario_HappyPathRegistrationWithoutBpn(TestDataModel testEntry)
    //public void Scenario_HappyPathRegistrationWithoutBpn(TestDataModel testEntry)
    {
        _operatorToken = await new AuthFlow(_operatorCompanyName).GetAccessToken(_secrets.OperatorUserName, _secrets.OperatorUserPassword);

        _userCompanyToken = "";
         //await Test1_ExecuteInvitation_ReturnsExpectedResult(testEntry.companyDetailData.Name);
         _regEndpointHelper = new RegistrationEndpointHelper(_userCompanyToken, _operatorToken);
         _applicationId = _regEndpointHelper.GetFirstApplicationId();
         Test2_SetCompanyDetailData_ReturnsExpectedResult(testEntry.companyDetailData);
         Thread.Sleep(5000);
         Test3_SubmitCompanyRoleConsentToAgreements_ReturnsExpectedResult(testEntry.companyRoles);
         Thread.Sleep(5000);
         Test4_UploadDocument_WithEmptyTitle_ReturnsExpectedResult();
         Thread.Sleep(5000);
         Test5_SubmitRegistration_ReturnsExpectedResult();
         Thread.Sleep(5000);
        Test6_GetApplicationDetails_ReturnsExpectedResult();
        Thread.Sleep(5000);
        Test7_GetCompanyWithAddress_ReturnsExpectedResult();
    }
    
    public static IEnumerable<object> GetDataEntries()
    {
        List<TestDataModel> testDataEntries = _testDataHelper.GetTestData();
        for (int i = 0; i < testDataEntries.Count; i++)
        {
            yield return new object[] { testDataEntries[i] };
        }
    }

    // POST api/administration/invitation

    [Fact]
    public async Task Test1_ExecuteInvitation_ReturnsExpectedResult(/*string userCompanyName*/)
    {
        //DevMailApiRequests devMailApiRequests = new DevMailApiRequests();
        //var devUser = devMailApiRequests.GenerateRandomEmailAddress();
        //var emailAddress = devUser.Result.Name + "@developermail.com";
        
        TempMailApiRequests tempMailApiRequests = new TempMailApiRequests();
        var emailAddress = "apitestuser" + tempMailApiRequests.GetDomain();
        
        CompanyInvitationData invitationData = new CompanyInvitationData("testuser", "myFirstName", "myLastName",
            emailAddress, _userCompanyName);
        
        Thread.Sleep(20000);
        
        _operatorToken = await new AuthFlow(_operatorCompanyName).GetAccessToken(_secrets.OperatorUserName, _secrets.OperatorUserPassword);
        
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

        //var messageData = devMailApiRequests.FetchPassword();
        var messageData = tempMailApiRequests.FetchPassword();
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

    //[Fact]
    public void Test2_SetCompanyDetailData_ReturnsExpectedResult(CompanyDetailData testCompanyDetailData)
    {
        if (_regEndpointHelper.GetApplicationStatus() == CompanyApplicationStatusId.CREATED.ToString())
        {
            _regEndpointHelper.SetApplicationStatus(CompanyApplicationStatusId.ADD_COMPANY_DATA.ToString());
            var companyDetailData = _regEndpointHelper.GetCompanyDetailData();

            //var testCompanyDetailData = _testDataHelper.GetNewCompanyDetailDataFromTestData();
            var newCompanyDetailData = testCompanyDetailData with
            {
                CompanyId = companyDetailData.CompanyId
            };
            _userCompanyName = newCompanyDetailData.Name;
            
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

    // POST /api/registration/application/{applicationId}/companyRoleAgreementConsents

    //[Fact]
    public void Test3_SubmitCompanyRoleConsentToAgreements_ReturnsExpectedResult(List<CompanyRoleId> companyRoles)
    {
        if (_regEndpointHelper.GetApplicationStatus() == CompanyApplicationStatusId.INVITE_USER.ToString())
        {
            //var companyRoles = _testDataHelper.GetCompanyRolesFromTestData(3);
            if (companyRoles != null)
            {
                var companyRoleAgreementConsents =
                    _regEndpointHelper.GetCompanyRolesAndConsentsForSelectedRoles(companyRoles);
                var body = JsonSerializer.Serialize(companyRoleAgreementConsents, _options);
                
                _regEndpointHelper.SetApplicationStatus(CompanyApplicationStatusId.SELECT_COMPANY_ROLE.ToString());
                Given()
                    .RelaxedHttpsValidation()
                    .Header(
                        "authorization",
                        $"Bearer {_userCompanyToken}")
                    .ContentType("application/json")
                    .Body(body)                
                    .When()
                    .Post($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyRoleAgreementConsents")
                    .Then()
                    .StatusCode(200);
            }
        }
        else throw new Exception($"Application status is not fitting to the pre-requisite");
    }
    
    // POST /api/registration/application/{applicationId}/documentType/{documentTypeId}/documents

    //[Fact]
    public void Test4_UploadDocument_WithEmptyTitle_ReturnsExpectedResult()
    {
        if (_regEndpointHelper.GetApplicationStatus() == CompanyApplicationStatusId.UPLOAD_DOCUMENTS.ToString())
        {
            string documentTypeId = "COMMERCIAL_REGISTER_EXTRACT";
            File.WriteAllText("testfile.pdf", "Some Text");
            var result = (int)Given()
                .RelaxedHttpsValidation()
                .Header(
                    "authorization",
                    $"Bearer {_userCompanyToken}")
                .ContentType("multipart/form-data")
                .MultiPart(new FileInfo("testfile.pdf"), "document")
                .When()
                .Post($"{_baseUrl}{_endPoint}/application/{_applicationId}/documentType/{documentTypeId}/documents")
                .Then()
                .StatusCode(200)
                .Extract()
                .As(typeof(int));
            Assert.Equal(1, result);
            if (result == 1) _regEndpointHelper.SetApplicationStatus(CompanyApplicationStatusId.VERIFY.ToString());
        }
        else throw new Exception($"Application status is not fitting to the pre-requisite");
    }

    // POST /api/registration/application/{applicationId}/submitRegistration

    //[Fact]
    public void Test5_SubmitRegistration_ReturnsExpectedResult()
    {
        if (_regEndpointHelper.GetApplicationStatus() == CompanyApplicationStatusId.VERIFY.ToString())
        {
            var status = (bool)Given()
                .RelaxedHttpsValidation()
                .Header(
                    "authorization",
                    $"Bearer {_userCompanyToken}")
                .ContentType("application/json")
                //.Body("")
                .When()
                .Post(
                    $"{_baseUrl}{_endPoint}/application/{_applicationId}/submitRegistration")
                .Then()
                .StatusCode(200)
                .Extract()
                .As(typeof(bool));
            Assert.True(status);
        }
        else throw new Exception($"Application status is not fitting to the pre-requisite");
    }

    // GET: api/administration/registration/applications?companyName={companyName}

    //[Fact]
    public void Test6_GetApplicationDetails_ReturnsExpectedResult()
    {
        _userCompanyName = "Test-Catena-X";
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .When()
            .Get(
                $"{_baseUrl}{_adminEndPoint}/registration/applications?companyName={_userCompanyName}&page=0&size=4&companyApplicationStatus=Closed")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();

        var data = DeserializeData<Pagination.Response<CompanyApplicationDetails>>(response.Content.ReadAsStringAsync()
            .Result);
        Assert.Contains("SUBMITTED", data.Content.First().CompanyApplicationStatusId.ToString());
        Assert.Equal(_applicationId.ToString(), data.Content.First().ApplicationId.ToString());
        
    }

    // GET: api/administration/registration/application/{applicationId}/companyDetailsWithAddress
    //[Fact]
    public void Test7_GetCompanyWithAddress_ReturnsExpectedResult()
    {
        // Given
        var data = (CompanyDetailData)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .When()
            .Get($"{_baseUrl}{_adminEndPoint}/registration/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(CompanyDetailData));
        Assert.NotNull(data);
    }

    #endregion

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
    
    private T? DeserializeData<T>(string jsonString)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
        var deserializedData = JsonSerializer.Deserialize<T>(jsonString, options);
        return deserializedData;
    }
}