using System.Text.Json;
using System.Text.Json.Serialization;
using Castle.Core.Internal;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using PasswordGenerator;
using Xunit;
using static RestAssured.Dsl;

namespace Registration.Service.Tests.RestAssured.RegistrationEndpointTests;

public class RegistrationEndpointHelper
{
    private readonly string _baseUrl = "https://portal-backend.dev.demo.catena-x.net";
    private readonly string _endPoint = "/api/registration";
    private readonly string _adminEndPoint = "/api/administration";
    private static string? _userCompanyToken;
    private static string? _operatorToken;
    private static string? _applicationId;

    readonly JsonSerializerOptions _jsonSerializerOptions = new ()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _operatorCompanyName = "CX-Operator";

    private readonly Secrets _secrets = new ();

    public CompanyDetailData GetCompanyDetailData()
    {
        // Given
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .And()
            .StatusCode(200)
            .Extract()
            .Response();
        var data = response.Content.ReadAsStringAsync().Result;
        var companyDetailData = DeserializeData<CompanyDetailData>(data);
        if (companyDetailData is null)
        {
            throw new Exception("Company detail data was not found.");
        }

        return companyDetailData;
    }

    public CompanyRoleAgreementConsents GetCompanyRolesAndConsentsForSelectedRoles(List<CompanyRoleId> companyRoleIds)
    {
        var availableRolesAndConsents = GetCompanyRolesAndConsents();
        var selectedCompanyRoleIds = new List<CompanyRoleId>();
        var agreementConsentStatusList = new List<AgreementConsentStatus>();
        foreach (var role in availableRolesAndConsents.Where(role =>
                     role.CompanyRolesActive && companyRoleIds.Contains(role.CompanyRoleId)))
        {
            selectedCompanyRoleIds.Add(role.CompanyRoleId);
            agreementConsentStatusList.AddRange(role.Agreements.Select(agreementId =>
                new AgreementConsentStatus(agreementId.AgreementId, ConsentStatusId.ACTIVE)));
        }

        return new CompanyRoleAgreementConsents(selectedCompanyRoleIds, agreementConsentStatusList);
    }

    public void SetApplicationStatus(string applicationStatus)
    {
        var status = (int)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
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

    public string GetApplicationStatus()
    {
        var applicationStatus = (string)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .ContentType("application/json")
            .When()
            .Get(
                $"{_baseUrl}{_endPoint}/application/{_applicationId}/status")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(string));
        return applicationStatus;
    }

    public List<InvitedUser> GetInvitedUsers()
    {
        var invitedUsers = (List<InvitedUser>)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/application/{_applicationId}/invitedusers")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(List<InvitedUser>));

        return invitedUsers;
    }

    public async Task ExecuteInvitation(string userCompanyName)
    {
        string? emailAddress = "", currentPassword = "";
        var devMailApiRequests = new DevMailApiRequests();
        var tempMailApiRequests = new TempMailApiRequests();
        try
        {
            var devUser = devMailApiRequests.GenerateRandomEmailAddress();
            emailAddress = devUser.Result.Name + "@developermail.com";
        }
        catch (Exception e)
        {
            try
            {
                emailAddress = "apitestuser" + tempMailApiRequests.GetDomain();
            }
            catch (Exception exception)
            {
                throw new Exception("MailApi for sending invitation is not available");
            }
        }
        finally
        {
            Thread.Sleep(20000);
            await ExecuteInvitation(emailAddress, userCompanyName);

            Thread.Sleep(20000);

            currentPassword = emailAddress.Contains("developermail.com")
                ? devMailApiRequests.FetchPassword()
                : tempMailApiRequests.FetchPassword();

            if (currentPassword is null)
            {
                throw new Exception("User password could not be fetched.");
            }

            var newPassword = new Password().Next();

            await SetCompanyTokenAndApplicationId(userCompanyName, emailAddress, currentPassword, newPassword);
        }
    }

    // POST /api/registration/application/{applicationId}/companyDetailsWithAddress

    public void SetCompanyDetailData(CompanyDetailData testCompanyDetailData)
    {
        var applicationStatus = GetApplicationStatus();
        if (applicationStatus == CompanyApplicationStatusId.CREATED.ToString())
        {
            SetApplicationStatus(CompanyApplicationStatusId.ADD_COMPANY_DATA.ToString());
            var companyDetailData = GetCompanyDetailData();

            var newCompanyDetailData = testCompanyDetailData with
            {
                CompanyId = companyDetailData.CompanyId,
                Name = companyDetailData.Name
            };

            var body = JsonSerializer.Serialize(newCompanyDetailData, _jsonSerializerOptions);

            Given()
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
            var storedCompanyDetailData = GetCompanyDetailData();
            if (!VerifyCompanyDetailDataStorage(storedCompanyDetailData, newCompanyDetailData))
                throw new Exception($"Company detail data was not stored correctly");
        }
        else throw new Exception($"Application status is not fitting to the pre-requisite");
    }

    public void UpdateCompanyDetailData(CompanyDetailData updateCompanyDetailData)
    {
        var actualStatus = GetApplicationStatus();
        if (actualStatus != CompanyApplicationStatusId.ADD_COMPANY_DATA.ToString() &&
            actualStatus != CompanyApplicationStatusId.SUBMITTED.ToString())
        {
            var companyDetailData = GetCompanyDetailData();

            var newCompanyDetailData = updateCompanyDetailData with
            {
                CompanyId = companyDetailData.CompanyId
            };
            var body = JsonSerializer.Serialize(newCompanyDetailData, _jsonSerializerOptions);

            Given()
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
            CompanyDetailData storedCompanyDetailData = GetCompanyDetailData();
            if (!VerifyCompanyDetailDataStorage(storedCompanyDetailData, newCompanyDetailData))
                throw new Exception($"Company detail data was not updated correctly");
        }
        else throw new Exception($"Application status is not fitting to the pre-requisite");
    }

    // POST /api/registration/application/{applicationId}/companyRoleAgreementConsents

    public void SubmitCompanyRoleConsentToAgreements(List<CompanyRoleId> companyRoles)
    {
        if (GetApplicationStatus() == CompanyApplicationStatusId.INVITE_USER.ToString())
        {
            if (companyRoles.IsNullOrEmpty()) throw new Exception($"No company roles were found");
            var companyRoleAgreementConsents = GetCompanyRolesAndConsentsForSelectedRoles(companyRoles);
            var body = JsonSerializer.Serialize(companyRoleAgreementConsents, _jsonSerializerOptions);

            SetApplicationStatus(CompanyApplicationStatusId.SELECT_COMPANY_ROLE.ToString());
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

    public void UploadDocument_WithEmptyTitle(string userCompanyName, string? documentTypeId, string? documentPath)
    {
        if (documentTypeId == null || !Enum.IsDefined(typeof(DocumentTypeId), documentTypeId))
            documentTypeId = "COMMERCIAL_REGISTER_EXTRACT";
        if (documentPath.IsNullOrEmpty())
        {
            documentPath = userCompanyName + "testfile.pdf";
            File.WriteAllText(documentPath, "Some Text");
        }

        if (GetApplicationStatus() == CompanyApplicationStatusId.UPLOAD_DOCUMENTS.ToString())
        {
            var result = (int)Given()
                .RelaxedHttpsValidation()
                .Header(
                    "authorization",
                    $"Bearer {_userCompanyToken}")
                .ContentType("multipart/form-data")
                .MultiPart(new FileInfo(documentPath), "document")
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

    //[Fact]
    public void SubmitRegistration()
    {
        if (GetApplicationStatus() == CompanyApplicationStatusId.VERIFY.ToString())
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

    public void GetApplicationDetails(string userCompanyName)
    {
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .When()
            .Get(
                $"{_baseUrl}{_adminEndPoint}/registration/applications?companyName={userCompanyName}&page=0&size=4&companyApplicationStatus=Closed")
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
    public void GetCompanyWithAddress()
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

    public void InviteNewUser()
    {
        DevMailApiRequests devMailApiRequests = new DevMailApiRequests();
        var devUser = devMailApiRequests.GenerateRandomEmailAddress();
        var emailAddress = devUser.Result.Name + "@developermail.com";
        var userCreationInfoWithMessage = new UserCreationInfoWithMessage("testuser2", emailAddress, "myFirstName",
            "myLastName", new[] { "Company Admin" }, "testMessage");

        Thread.Sleep(20000);

        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .ContentType("application/json")
            .Body(userCreationInfoWithMessage)
            .When()
            .Post($"{_baseUrl}{_endPoint}/application/{_applicationId}/inviteNewUser")
            .Then()
            .StatusCode(200);

        var invitedUsers = GetInvitedUsers();
        if (invitedUsers.Count != 2)
        {
            throw new Exception("No invited users were found.");
        }

        var newInvitedUser = invitedUsers.Find(u => u.EmailId!.Equals(userCreationInfoWithMessage.eMail));
        Assert.Equal(InvitationStatusId.CREATED, newInvitedUser?.InvitationStatus);
        Assert.Equal(userCreationInfoWithMessage.Roles, newInvitedUser?.InvitedUserRoles);

        Thread.Sleep(20000);

        var messageData = devMailApiRequests.FetchPassword();
        Assert.NotNull(messageData);
        Assert.NotEmpty(messageData);
    }

    // POST api/administration/invitation
    private async Task ExecuteInvitation(string emailAddress, string userCompanyName)
    {
        CompanyInvitationData invitationData = new CompanyInvitationData("testuser", "myFirstName", "myLastName",
            emailAddress, userCompanyName);

        try
        {
            _operatorToken =
                await new AuthFlow(_operatorCompanyName).GetAccessToken(_secrets.OperatorUserName,
                    _secrets.OperatorUserPassword);

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
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task SetCompanyTokenAndApplicationId(string userCompanyName, string emailAddress,
        string currentPassword, string newPassword)
    {
        _userCompanyToken =
            await new AuthFlow(userCompanyName).UpdatePasswordAndGetAccessToken(emailAddress, currentPassword,
                newPassword);
        _applicationId = GetFirstApplicationId();
    }

    private bool VerifyCompanyDetailDataStorage(CompanyDetailData storedData, CompanyDetailData postedData)
    {
        var isEqual = storedData.UniqueIds.SequenceEqual(postedData.UniqueIds);
        return storedData.CompanyId == postedData.CompanyId && isEqual && storedData.Name == postedData.Name &&
               storedData.StreetName == postedData.StreetName &&
               storedData.CountryAlpha2Code == postedData.CountryAlpha2Code &&
               storedData.BusinessPartnerNumber == postedData.BusinessPartnerNumber &&
               storedData.ShortName == postedData.ShortName &&
               storedData.Region == postedData.Region &&
               storedData.StreetAdditional == postedData.StreetAdditional &&
               storedData.StreetNumber == postedData.StreetNumber &&
               storedData.ZipCode == postedData.ZipCode &&
               storedData.CountryDe == postedData.CountryDe;
    }

    private string GetFirstApplicationId()
    {
        var applicationIDs = (List<CompanyApplicationData>)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/applications")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(List<CompanyApplicationData>));

        _applicationId = applicationIDs[0].ApplicationId.ToString();

        return _applicationId;
    }

    private T? DeserializeData<T>(string jsonString)
    {
        // var options = new JsonSerializerOptions
        // {
        //     PropertyNameCaseInsensitive = true,
        //     Converters = { new JsonStringEnumConverter() }
        // };
        var deserializedData = JsonSerializer.Deserialize<T>(jsonString, _jsonSerializerOptions);
        return deserializedData;
    }

    private List<CompanyRoleConsentViewData> GetCompanyRolesAndConsents()
    {
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .ContentType("application/json")
            .When()
            .Get(
                $"{_baseUrl}{_adminEndPoint}/companydata/companyRolesAndConsents")
            .Then()
            .StatusCode(200)
            .And()
            .Extract()
            .Response();
        var data = response.Content.ReadAsStringAsync().Result;
        var companyRolesAndConsents = DeserializeData<List<CompanyRoleConsentViewData>>(data);
        if (companyRolesAndConsents is null)
        {
            throw new Exception("Company roles and consents were not found.");
        }

        return companyRolesAndConsents;
    }
}