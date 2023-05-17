using System.Text.Json;
using System.Text.Json.Serialization;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
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
    private string? _operatorToken;
    private readonly string _operatorCompanyName = "CX-Operator";
    private static string _userCompanyName = "Test-Catena-X-13";
    private static string[] _userEmailAddress;
    private readonly RegistrationEndpointHelper _regEndpointHelper = new RegistrationEndpointHelper(_userCompanyToken, _baseUrl, _endPoint);
    JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    #region Happy Path - new registration without BPN

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
        _applicationId = _regEndpointHelper.GetFirstApplicationId();
    }

    // POST /api/registration/application/{applicationId}/companyDetailsWithAddress

    [Fact]
    public void Test2_SetCompanyDetailData_ReturnsExpectedResult()
    {
        if (_regEndpointHelper.GetApplicationStatus() == CompanyApplicationStatusId.CREATED.ToString())
        {
            _regEndpointHelper.SetApplicationStatus(CompanyApplicationStatusId.ADD_COMPANY_DATA.ToString());
            CompanyDetailData companyDetailData = _regEndpointHelper.GetCompanyDetailData();
            CompanyUniqueIdData newCompanyUniqueIdData =
                new CompanyUniqueIdData(UniqueIdentifierId.VAT_ID, "DE123456789");
            CompanyDetailData newCompanyDetailData = companyDetailData with
            {
                City = "Augsburg", CountryAlpha2Code = "DE", StreetName = "Hauptstrasse", ZipCode = "86199",
                UniqueIds = new List<CompanyUniqueIdData> { newCompanyUniqueIdData }
            };
            var body = JsonSerializer.Serialize(newCompanyDetailData, _options);
            _userCompanyName = companyDetailData.Name;
            string companyId = companyDetailData.CompanyId.ToString();
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

    [Fact]
    public void Test3_SubmitCompanyRoleConsentToAgreements_ReturnsExpectedResult()
    {
        if (_regEndpointHelper.GetApplicationStatus() == CompanyApplicationStatusId.INVITE_USER.ToString())
        {
            List<CompanyRoleConsentViewData> availableRolesAndConsents = _regEndpointHelper.GetCompanyRolesAndConsents();
            List<CompanyRoleId> availableCompanyRoleIds = new List<CompanyRoleId>();
            List<AgreementConsentStatus> agreementConsentStatusList = new List<AgreementConsentStatus>();
            foreach (var role in availableRolesAndConsents)
            {
                if (role.CompanyRolesActive)
                {
                    availableCompanyRoleIds.Add(role.CompanyRoleId);
                    foreach (var agreementId in role.Agreements)
                    {
                        AgreementConsentStatus agreementConsentStatus =
                            new AgreementConsentStatus(agreementId.AgreementId, ConsentStatusId.ACTIVE);
                        agreementConsentStatusList.Add(agreementConsentStatus);
                    }
                }
            }
            CompanyRoleAgreementConsents companyRoleAgreementConsents =
                new CompanyRoleAgreementConsents(availableCompanyRoleIds, agreementConsentStatusList);
            
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
        else throw new Exception($"Application status is not fitting to the pre-requisite");
    }
    
    // POST /api/registration/application/{applicationId}/documentType/{documentTypeId}/documents

    [Fact]
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

    [Fact]
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
                $"{_baseUrl}{_adminEndPoint}/registration/applications?companyName={_userCompanyName}&page=0&size=4&companyApplicationStatus=Closed")
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