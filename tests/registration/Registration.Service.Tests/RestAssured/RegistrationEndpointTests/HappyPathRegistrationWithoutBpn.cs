using AutoFixture;
using Microsoft.Extensions.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using RestAssured.Request.Logging;
using Xunit;
using static RestAssured.Dsl;

namespace Registration.Service.Tests.RestAssured.RegistrationEndpointTests;

[TestCaseOrderer("Registration.Service.Tests.RestAssured.AlphabeticalOrderer",
    "Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests")]
public class RegistrationEndpointTestsHappyPathRegistrationWithoutBpn
{
    private readonly string _baseUrl = "https://portal-backend.dev.demo.catena-x.net";
    private readonly string _endPoint = "/api/registration";
    private string _companyToken;
    private static string _applicationId;

    private readonly IFixture _fixture;

    private readonly string _adminEndPoint = "/api/administration";
    private readonly string _operatorToken;
    private static string _companyName;
    private readonly HttpClient _httpClient;
    private static string[] _userEmailAddress;

    public RegistrationEndpointTestsHappyPathRegistrationWithoutBpn()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Secrets>()
            .Build();
        _companyToken = configuration.GetValue<string>("Secrets:CompanyToken");
        //_companyToken = "TestUserToken";

        _operatorToken = configuration.GetValue<string>("Secrets:OperatorToken");
        _fixture = new Fixture();
        _httpClient = new () { BaseAddress = new Uri(_baseUrl) };
        // CreateFilesToUpload();
    }

    #region Happy Path - new registration without BPN

    // POST api/administration/invitation

    /*[Fact]
    public void Test1_ExecuteInvitation_ReturnsExpectedResult()
    {
        DevMailApiRequests devMailApiRequests = new DevMailApiRequests();
        var devUser = devMailApiRequests.GenerateRandomEmailAddress();
        var emailAddress = devUser.Result.Name + "@developermail.com";
        CompanyInvitationData invitationData = new CompanyInvitationData("testuser", "myFirstName", "myLastName",
            emailAddress, "Test-Catena-X-6");
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
        //Thread.Sleep(2000);
        var messageData = devMailApiRequests.FetchPassword();
        
        AuthenticationFlow();
    }*/


    // POST /api/registration/application/{applicationId}/companyDetailsWithAddress

    [Fact]
    public void Test2_SetCompanyDetailData_ReturnsExpectedResult()
    {
        if (GetApplicationStatus() == CompanyApplicationStatusId.CREATED.ToString())
        {
            SetApplicationStatus(CompanyApplicationStatusId.ADD_COMPANY_DATA.ToString());
            CompanyDetailData companyDetailData = GetCompanyDetailData();
            string companyId = companyDetailData.CompanyId.ToString();
            var response = Given()
                .RelaxedHttpsValidation()
                .Header(
                    "authorization",
                    $"Bearer {_companyToken}")
                .ContentType("application/json")
                .When()
                .Body("{\"companyId\":" + "\"" + companyId + "\"" +
                      ",\"name\":" + "\"" + _companyName + "\"" +
                      ",\"city\":\"München\",\"streetName\":\"Street\",\"countryAlpha2Code\":\"DE\",\"bpn\":null, \"shortName\":" +
                      "\"" + _companyName + "\"" +
                      ",\"region\":null,\"streetAdditional\":null,\"streetNumber\":null,\"zipCode\":null,\"countryDe\":\"Deutschland\",\"uniqueIds\":[{\"type\":\"VAT_ID\",\"value\":\"DE123456788\"}]}")
                //.Body(companyDetailData)
                .Post($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyDetailsWithAddress")
                .Then()
                .StatusCode(200);
        }
        else throw new Exception($"Application status is not fitting to the pre-requisite");
    }

    // POST /api/registration/application/{applicationId}/companyRoleAgreementConsents

    [Fact]
    public void Test3_SubmitCompanyRoleConsentToAgreements_ReturnsExpectedResult()
    {
        if(GetApplicationStatus() == CompanyApplicationStatusId.INVITE_USER.ToString()) 
        //if (GetApplicationStatus() == CompanyApplicationStatusId.SELECT_COMPANY_ROLE.ToString())
        {
            SetApplicationStatus(CompanyApplicationStatusId.SELECT_COMPANY_ROLE.ToString());
            Given()
                .RelaxedHttpsValidation()
                .Header(
                    "authorization",
                    $"Bearer {_companyToken}")
                .ContentType("application/json")
                .Body(
                    "{\"companyRoles\": [\"ACTIVE_PARTICIPANT\", \"APP_PROVIDER\", \"SERVICE_PROVIDER\"], \"agreements\": [{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1011\",\"consentStatus\":\"ACTIVE\"},{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1010\",\"consentStatus\":\"ACTIVE\"},{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1090\",\"consentStatus\":\"ACTIVE\"},{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1013\",\"consentStatus\":\"ACTIVE\"},{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1017\",\"consentStatus\":\"ACTIVE\"}]}")
                .When()
                .Post($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyRoleAgreementConsents")
                .Then()
                .StatusCode(200);
        }
        else throw new Exception($"Application status is not fitting to the pre-requisite");
    }
    // POST /api/registration/application/{applicationId}/documentType/{documentTypeId}/documents

    [Fact]
    public void Test4_UploadDocument_WithEmptyTitle_ReturnsExpectedResult()
    {
        if (GetApplicationStatus() == CompanyApplicationStatusId.UPLOAD_DOCUMENTS.ToString())
        {
            string documentTypeId = "COMMERCIAL_REGISTER_EXTRACT";
            File.WriteAllText("testfile.pdf", "Some Text");
            var result = (int)Given()
                .RelaxedHttpsValidation()
                .Header(
                    "authorization",
                    $"Bearer {_companyToken}")
                .ContentType("multipart/form-data")
                .MultiPart(new FileInfo("testfile.pdf"), "document")
                .When()
                .Post($"{_baseUrl}{_endPoint}/application/{_applicationId}/documentType/{documentTypeId}/documents")
                .Then()
                .StatusCode(200)
                .Extract()
                .As(typeof(int));
            Assert.Equal(1, result);
            if (result == 1) SetApplicationStatus(CompanyApplicationStatusId.VERIFY.ToString());
        }
        else throw new Exception($"Application status is not fitting to the pre-requisite");
    }

    // POST /api/registration/application/{applicationId}/submitRegistration

    [Fact]
    public void Test5_SubmitRegistration_ReturnsExpectedResult()
    {
        if (GetApplicationStatus() == CompanyApplicationStatusId.VERIFY.ToString())
        {
            var status = (bool)Given()
                .RelaxedHttpsValidation()
                .Header(
                    "authorization",
                    $"Bearer {_companyToken}")
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

    [Fact]
    public void Test6_GetApplicationDetails_ReturnsExpectedResult()
    {
        var data = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .When()
            .Get(
                $"{_baseUrl}{_adminEndPoint}/registration/applications?companyName={_companyName}&page=0&size=4&companyApplicationStatus=Closed")
            .Then()
            .StatusCode(200)
            .Extract()
            .Body("$.content[0].applicationStatus");
        Assert.Equal("SUBMITTED", data);
    }

    // GET: api/administration/registration/application/{applicationId}/companyDetailsWithAddress
    [Fact]
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

    private string GetFirstApplicationId()
    {
        var applicationIDs = (List<CompanyApplicationData>)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/applications")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(List<CompanyApplicationData>));

        return applicationIDs[0].ApplicationId.ToString();
    }

    private CompanyDetailData GetCompanyDetailData()
    {
        _applicationId = GetFirstApplicationId();
        // Given
        CompanyDetailData companyDetailData = (CompanyDetailData)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .And()
            .StatusCode(200)
            .Extract().As(typeof(CompanyDetailData));

        _companyName = companyDetailData.Name;

        return companyDetailData;
    }

    private void SetApplicationStatus(string applicationStatus)
    {
        _applicationId = GetFirstApplicationId();
        var status = (int)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .ContentType("application/json")
            .When()
            .Put(
                $"{_baseUrl}{_endPoint}/application/{_applicationId}/status?status={applicationStatus}")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(int));
        Assert.Equal(1, status);
    }
    
    private string GetApplicationStatus()
    {
        _applicationId = GetFirstApplicationId();
        var applicationStatus = (string)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .ContentType("application/json")
            .When()
            .Get(
                $"{_baseUrl}{_endPoint}/application/{_applicationId}/status")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(string));
        return applicationStatus;
        // Assert.Equal(0, status);
    }

    /*private string GenerateRandomEmailAddress()
    {
        // Given
        var emailAddress = (string[])Given()
            .RelaxedHttpsValidation()
            .When()
            .Get("https://www.1secmail.com/api/v1/?action=genRandomMailbox&count=1")
            .Then()
            .And()
            .StatusCode(200)
            .Extract().As(typeof(string[]));
        _userEmailAddress = emailAddress[0].Split("@");
        return emailAddress[0];
    }*/

    /*private MailboxData[] CheckMailBox()
    {
        // Given
        var emails = (MailboxData[])Given()
            .RelaxedHttpsValidation()
            .When()
            .Get(
                $"https://www.1secmail.com/api/v1/?action=getMessages&login={_userEmailAddress[0]}&domain={_userEmailAddress[1]}")
            .Then()
            .And()
            .StatusCode(200)
            .Extract().As(typeof(MailboxData[]));
        return emails;
    }*/

    /*private EmailMessageData? FetchPassword()
    {
        // Given
        var emails = CheckMailBox();
        if (emails.Length != 0)
        {
            var passwordMail = emails[0]?.Id;
            var messageData = (EmailMessageData)Given()
                .RelaxedHttpsValidation()
                .When()
                .Get(
                    $"https://www.1secmail.com/api/v1/?action=readMessage&login={_userEmailAddress[0]}&domain={_userEmailAddress[1]}&id={passwordMail}")
                .Then()
                .And()
                .StatusCode(200)
                .Extract().As(typeof(EmailMessageData));
            return messageData;
        }

        return null;
    }*/

    private void AuthenticationFlow()
    {
    }
}