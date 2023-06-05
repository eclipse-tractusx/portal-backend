using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Xunit;
using static RestAssured.Dsl;

namespace Registration.Service.Tests.RestAssured.RegistrationEndpointTests;

[TestCaseOrderer("Notifications.Service.Tests.RestAssured.AlphabeticalOrderer",
    "Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests")]
public class RegistrationEndpointTestsHappyPathRegistrationWithBpn
{
    private static readonly string _baseUrl = "https://portal-backend.dev.demo.catena-x.net";
    private static readonly string _endPoint = "/api/registration";
    private static readonly string _userCompanyToken ="eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJQbFVrV3VuREQ3R3B4cDBYYUlKLWx4b3R5RjgzbHk2REk4SDVjb3I3eHlnIn0.eyJleHAiOjE2ODU5NDU3MTYsImlhdCI6MTY4NTk0NTQxNiwiYXV0aF90aW1lIjoxNjg1OTQ1Mzg5LCJqdGkiOiJiNzgzZjQzYS0yZmUyLTQxZDYtOGVkZS01NDRlMzVlZGI0OTUiLCJpc3MiOiJodHRwczovL2NlbnRyYWxpZHAuZGV2LmRlbW8uY2F0ZW5hLXgubmV0L2F1dGgvcmVhbG1zL0NYLUNlbnRyYWwiLCJhdWQiOlsiQ2wxLUNYLVJlZ2lzdHJhdGlvbiIsIkNsMi1DWC1Qb3J0YWwiLCJyZWFsbS1tYW5hZ2VtZW50IiwiYWNjb3VudCJdLCJzdWIiOiI3M2IxNDY0My1hNDcyLTQzMjQtYmE1Yi1kOTgwNjJlODdjZDgiLCJ0eXAiOiJCZWFyZXIiLCJhenAiOiJDbDItQ1gtUG9ydGFsIiwibm9uY2UiOiJjMDgwMTU4OC1jZDgxLTQzMTQtODk5Ni02MmU2Mzc1NDRlOWUiLCJzZXNzaW9uX3N0YXRlIjoiNzY1ZjFhNmMtMjAzMy00Yzc0LWJiOGQtNWM2ZjFlNjM1YTc4IiwiYWNyIjoiMCIsImFsbG93ZWQtb3JpZ2lucyI6WyJodHRwczovL3BhcnRuZXJzLXBvb2wuZGV2LmRlbW8uY2F0ZW5hLXgubmV0IiwiaHR0cDovL2xvY2FsaG9zdDo4MDgwIiwiaHR0cHM6Ly9wb3J0YWwuZGV2LmRlbW8uY2F0ZW5hLXgubmV0IiwiaHR0cHM6Ly9wb3J0YWwtcmMuZGV2LmRlbW8uY2F0ZW5hLXgubmV0IiwiaHR0cHM6Ly9wYXJ0bmVycy1nYXRlLmRldi5kZW1vLmNhdGVuYS14Lm5ldCIsImh0dHBzOi8vcG9ydGFsLXN3YWdnZXIuZGV2LmRlbW8uY2F0ZW5hLXgubmV0IiwiaHR0cDovL2xvY2FsaG9zdDozMDAwIiwiaHR0cHM6Ly9jYXRlbmF4LWJwZG0tZGV2LmRlbW8uY2F0ZW5hLXgubmV0Il0sInJlYWxtX2FjY2VzcyI6eyJyb2xlcyI6WyJvZmZsaW5lX2FjY2VzcyIsImRlZmF1bHQtcm9sZXMtY2F0ZW5hLXggcmVhbG0iLCJ1bWFfYXV0aG9yaXphdGlvbiJdfSwicmVzb3VyY2VfYWNjZXNzIjp7InJlYWxtLW1hbmFnZW1lbnQiOnsicm9sZXMiOlsibWFuYWdlLXVzZXJzIiwidmlldy1jbGllbnRzIiwicXVlcnktY2xpZW50cyJdfSwiQ2wxLUNYLVJlZ2lzdHJhdGlvbiI6eyJyb2xlcyI6WyJkZWxldGVfZG9jdW1lbnRzIiwiaW52aXRlX3VzZXIiLCJ2aWV3X2RvY3VtZW50cyIsInVwbG9hZF9kb2N1bWVudHMiLCJhZGRfY29tcGFueV9kYXRhIiwic3VibWl0X3JlZ2lzdHJhdGlvbiIsInNpZ25fY29uc2VudCIsInZpZXdfY29tcGFueV9yb2xlcyIsInZpZXdfcmVnaXN0cmF0aW9uIiwiQ29tcGFueSBBZG1pbiJdfSwiYWNjb3VudCI6eyJyb2xlcyI6WyJtYW5hZ2UtYWNjb3VudCIsIm1hbmFnZS1hY2NvdW50LWxpbmtzIiwidmlldy1wcm9maWxlIl19fSwic2NvcGUiOiJvcGVuaWQgY2F0ZW5hIHByb2ZpbGUgZW1haWwiLCJzaWQiOiI3NjVmMWE2Yy0yMDMzLTRjNzQtYmI4ZC01YzZmMWU2MzVhNzgiLCJlbWFpbF92ZXJpZmllZCI6dHJ1ZSwibmFtZSI6IkNhcnN0ZW4gSGF1cGthIiwib3JnYW5pc2F0aW9uIjoiVGVzdEF1dG9tYXRpb25SZWcgMDMiLCJwcmVmZXJyZWRfdXNlcm5hbWUiOiI3MzYyNmVlNy0xYjA1LTQwM2ItODkwZC0xMDhhNDQyMmUzNGUiLCJnaXZlbl9uYW1lIjoiQ2Fyc3RlbiIsImZhbWlseV9uYW1lIjoiSGF1cGthIiwiZW1haWwiOiJjYXJzdGVuLmhhdXBrYUBvZmZpY2UzNjUudG5ndGVjaC5jb20ifQ.JBjDnEwYyPBretdO_kSIEjnjiJwtrA3ELxB8p6TmiMVt-sqBFA2PMRxnr8KU04qEQm2IrRTHRDcPePzRNOZaKnOUtk66f79IvTj9ktXERNop9_8QbM704UtIvslDrw6oiH4mTCbtgGLpAvXObbeKjaD7SHEc8wE-ovKW8TyGloghApHyQbipoL-g1Oyl5-MemNhTBf2DZHKHNzuKuiaQ8uAMzpDuQyn1vx9LvwzuN_XylSuF4lu351WrlB0LtGNIcC4xsg-X9QF2KtzOqp0gMtjn1PIR6LpvFRljLzFwERJ7dyyxWKldlE1kedIJofP7iVv2F4K3B-d67O1eu5oCcw";
    private static string _applicationId;

    private readonly string _adminEndPoint = "/api/administration";
    private static string _operatorToken;
    private static string _companyName = "Test-Catena-X";
    private static string _bpn = "1234";
    private static RegistrationEndpointHelper _regEndpointHelper;


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
            .Get($"{_baseUrl}{_endPoint}/legalEntityAddress/{_bpn}")
            .Then()
            .StatusCode(200)
            .Body("");
    }

    //POST /api/registration/application/{applicationId}/companyDetailsWithAddress

    [Fact]
    public void Test3_SetCompanyDetailData_ReturnsExpectedResult()
    {
        if (_regEndpointHelper.GetApplicationStatus() == CompanyApplicationStatusId.CREATED.ToString())
        {
            _regEndpointHelper.SetApplicationStatus(CompanyApplicationStatusId.ADD_COMPANY_DATA.ToString());
            CompanyDetailData companyDetailData = _regEndpointHelper.GetCompanyDetailData();
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
                .Post($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyDetailsWithAddress")
                .Then()
                .StatusCode(200);
        }
        else throw new Exception($"Application status is not fitting to the pre-requisite");
    }

    // POST /api/registration/application/{applicationId}/companyRoleAgreementConsents

    [Fact]
    public void Test4_SubmitCompanyRoleConsentToAgreements_ReturnsExpectedResult()
    {
        if(_regEndpointHelper.GetApplicationStatus() == CompanyApplicationStatusId.INVITE_USER.ToString()) 
            //if (GetApplicationStatus() == CompanyApplicationStatusId.SELECT_COMPANY_ROLE.ToString())
        {
            _regEndpointHelper.SetApplicationStatus(CompanyApplicationStatusId.SELECT_COMPANY_ROLE.ToString());
            Given()
                .RelaxedHttpsValidation()
                .Header(
                    "authorization",
                    $"Bearer {_userCompanyToken}")
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
    public void Test5_UploadDocument_WithEmptyTitle_ReturnsExpectedResult()
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

    //POST /api/registration/application/{applicationId}/submitRegistration
    
    [Fact]
    public void Test6_SubmitRegistration_ReturnsExpectedResult()
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
    public void Test7_GetApplicationDetails_ReturnsExpectedResult()
    {
        //_companyName = "Test-Catena-X-3";
        // Given
        //var data = (Pagination.Response<CompanyApplicationDetails>)
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
}