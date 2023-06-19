using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Xunit;
using static RestAssured.Dsl;

namespace EndToEnd.Tests;

[TestCaseOrderer("EndToEnd.Tests.AlphabeticalOrderer",
    "EndToEnd.Tests")]
public class RegistrationEndpointTestsHappyPathRegistrationWithBpn
{
    private static readonly string BaseUrl = "https://portal-backend.dev.demo.catena-x.net";
    private static readonly string EndPoint = "/api/registration";
    private static readonly string AdminEndPoint = "/api/administration";
    private static string? _userCompanyToken;
    private static string? _portalUserToken;
    private static string? _applicationId;

    private static string _companyName = "Test-Catena-X";
    private static string _bpn = "1234";

    /*
    #region Happy Path - new registration with BPN

    [Fact]
    public void Test0_GetBpn_ReturnsExpectedResult()
    {
        // Given
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .When()
            .Get($"https://partners-pool.dev.demo.catena-x.net/api/catena/legal-entities?page=0&size=20")
            .Then()
            .StatusCode(200)
            .Body("");
    }

    // POST api/administration/invitation

    // [Fact]
    // public void Test1_ExecuteInvitation_ReturnsExpectedResult()
    // {
    //     CompanyInvitationData invitationData = new CompanyInvitationData("user", "myFirstName", "myLastName",
    //         "myEmail", "Test-Catena-X");
    //      Given()
    //         .RelaxedHttpsValidation()
    //         .Header(
    //             "authorization",
    //             $"Bearer {_operatorToken}")
    //         .ContentType("application/json")
    //         .Body(invitationData)
    //         .When()
    //         .Post($"{_baseUrl}{_adminEndPoint}/invitation")
    //         .Then()
    //         .StatusCode(200);
    // }

    //GET /api/registration/legalEntityAddress/{bpn}

    [Fact]
    public void Test2_GetCompanyBpdmDetailData_ReturnsExpectedResult()
    {
        // Given
        _bpn = "BPNL000000000001";
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/legalEntityAddress/{_bpn}")
            .Then()
            .StatusCode(200)
            .Body("");
    }

    //POST /api/registration/application/{applicationId}/companyDetailsWithAddress

    [Fact]
    public void Test3_SetCompanyDetailData_ReturnsExpectedResult()
    {
        if (RegistrationEndpointHelper.GetApplicationStatus() == CompanyApplicationStatusId.CREATED.ToString())
        {
            RegistrationEndpointHelper.SetApplicationStatus(CompanyApplicationStatusId.ADD_COMPANY_DATA.ToString());
            CompanyDetailData companyDetailData = RegistrationEndpointHelper.GetCompanyDetailData();
            _companyName = companyDetailData.Name;
            string companyId = companyDetailData.CompanyId.ToString();
            var response = Given()
                .RelaxedHttpsValidation()
                .Header(
                    "authorization",
                    $"Bearer {_userCompanyToken}")
                .ContentType("application/json")
                .When()
                .Body("{\"companyId\":" + "\"" + companyId + "\"" +
                      ",\"name\":" + "\"" + _companyName + "\"" +
                      ",\"city\":\"München\",\"streetName\":\"Street\",\"countryAlpha2Code\":\"DE\",\"bpn\":null, \"shortName\":" +
                      "\"" + _companyName + "\"" +
                      ",\"region\":null,\"streetAdditional\":null,\"streetNumber\":null,\"zipCode\":null,\"countryDe\":\"Deutschland\",\"uniqueIds\":[{\"type\":\"VAT_ID\",\"value\":\"DE123456788\"}]}")
                //.Body(companyDetailData)
                .Post($"{BaseUrl}{EndPoint}/application/{_applicationId}/companyDetailsWithAddress")
                .Then()
                .StatusCode(200);
        }
        else throw new Exception($"Application status is not fitting to the pre-requisite");
    }

    // POST /api/registration/application/{applicationId}/companyRoleAgreementConsents

    [Fact]
    public void Test4_SubmitCompanyRoleConsentToAgreements_ReturnsExpectedResult()
    {
        if(RegistrationEndpointHelper.GetApplicationStatus() == CompanyApplicationStatusId.INVITE_USER.ToString()) 
            //if (GetApplicationStatus() == CompanyApplicationStatusId.SELECT_COMPANY_ROLE.ToString())
        {
            RegistrationEndpointHelper.SetApplicationStatus(CompanyApplicationStatusId.SELECT_COMPANY_ROLE.ToString());
            Given()
                .RelaxedHttpsValidation()
                .Header(
                    "authorization",
                    $"Bearer {_userCompanyToken}")
                .ContentType("application/json")
                .Body(
                    "{\"companyRoles\": [\"ACTIVE_PARTICIPANT\", \"APP_PROVIDER\", \"SERVICE_PROVIDER\"], \"agreements\": [{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1011\",\"consentStatus\":\"ACTIVE\"},{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1010\",\"consentStatus\":\"ACTIVE\"},{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1090\",\"consentStatus\":\"ACTIVE\"},{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1013\",\"consentStatus\":\"ACTIVE\"},{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1017\",\"consentStatus\":\"ACTIVE\"}]}")
                .When()
                .Post($"{BaseUrl}{EndPoint}/application/{_applicationId}/companyRoleAgreementConsents")
                .Then()
                .StatusCode(200);
        }
        else throw new Exception($"Application status is not fitting to the pre-requisite");
    }

    // POST /api/registration/application/{applicationId}/documentType/{documentTypeId}/documents

    [Fact]
    public void Test5_UploadDocument_WithEmptyTitle_ReturnsExpectedResult()
    {
        if (RegistrationEndpointHelper.GetApplicationStatus() == CompanyApplicationStatusId.UPLOAD_DOCUMENTS.ToString())
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
                .Post($"{BaseUrl}{EndPoint}/application/{_applicationId}/documentType/{documentTypeId}/documents")
                .Then()
                .StatusCode(200)
                .Extract()
                .As(typeof(int));
            Assert.Equal(1, result);
            if (result == 1) RegistrationEndpointHelper.SetApplicationStatus(CompanyApplicationStatusId.VERIFY.ToString());
        }
        else throw new Exception($"Application status is not fitting to the pre-requisite");
    }

    //POST /api/registration/application/{applicationId}/submitRegistration
    
    [Fact]
    public void Test6_SubmitRegistration_ReturnsExpectedResult()
    {
        if (RegistrationEndpointHelper.GetApplicationStatus() == CompanyApplicationStatusId.VERIFY.ToString())
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
                    $"{BaseUrl}{EndPoint}/application/{_applicationId}/submitRegistration")
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
    public void Test7_GetApplicationDetails_ReturnsExpectedResult()
    {
        //_companyName = "Test-Catena-X-3";
        // Given
        //var data = (Pagination.Response<CompanyApplicationDetails>)
        var data = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get(
                $"{BaseUrl}{AdminEndPoint}/registration/applications?companyName={_companyName}&page=0&size=4&companyApplicationStatus=Closed")
            .Then()
            .StatusCode(200)
            .Extract()
            .Body("$.content[0].applicationStatus");
        Assert.Equal("SUBMITTED", data);
        //.As(typeof(Pagination.Response<CompanyApplicationDetails>));
        //Assert.Contains(_companyName, data.Content.Select(content => content.CompanyName.ToString()));
    }

    // GET: api/administration/registration/application/{applicationId}/companyDetailsWithAddress

    [Fact]
    public void Test8_GetCompanyWithAddress_ReturnsExpectedResult()
    {
        // Given
        var data = (CompanyDetailData)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get($"{BaseUrl}{AdminEndPoint}/registration/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(CompanyDetailData));
        
        Assert.NotNull(data);
    }

    #endregion*/
}