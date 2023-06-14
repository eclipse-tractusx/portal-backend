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
using Tests.Shared.EndToEndTests;
using Tests.Shared.RestAssured.AuthFlow;
using Xunit;
using static RestAssured.Dsl;

namespace Registration.Service.Tests.RestAssured.RegistrationEndpointTests;

public static class RegistrationEndpointHelper
{
    private static readonly string BaseUrl = TestResources.BaseUrl;
    private static readonly string EndPoint = "/api/registration";
    private static readonly string AdminEndPoint = "/api/administration";
    private static readonly string OperatorCompanyName = TestResources.OperatorCompanyName;
    private static string? _userCompanyToken;
    private static string? _operatorToken;
    private static string? _applicationId;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new ()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly Secrets Secrets = new ();

    public static CompanyDetailData GetCompanyDetailData()
    {
        // Given
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/application/{_applicationId}/companyDetailsWithAddress")
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

    private static CompanyRoleAgreementConsents GetCompanyRolesAndConsentsForSelectedRoles(
        List<CompanyRoleId> companyRoleIds)
    {
        var availableRolesAndConsents = GetCompanyRolesAndConsents();
        var selectedCompanyRoleIds = new List<CompanyRoleId>();
        var agreementConsentStatusList = new List<AgreementConsentStatus>();
        foreach (var role in availableRolesAndConsents.Where(availableRole =>
                     availableRole.CompanyRolesActive && companyRoleIds.Contains(availableRole.CompanyRoleId)))
        {
            selectedCompanyRoleIds.Add(role.CompanyRoleId);
            agreementConsentStatusList.AddRange(role.Agreements.Select(agreementId =>
                new AgreementConsentStatus(agreementId.AgreementId, ConsentStatusId.ACTIVE)));
        }

        return new CompanyRoleAgreementConsents(selectedCompanyRoleIds, agreementConsentStatusList);
    }

    public static void SetApplicationStatus(string applicationStatus)
    {
        var status = (int)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .ContentType("application/json")
            .When()
            .Put(
                $"{BaseUrl}{EndPoint}/application/{_applicationId}/status?status={applicationStatus}")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(int));
        Assert.Equal(1, status);
    }

    public static string GetApplicationStatus()
    {
        var applicationStatus = (string)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .ContentType("application/json")
            .When()
            .Get(
                $"{BaseUrl}{EndPoint}/application/{_applicationId}/status")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(string));
        return applicationStatus;
    }

    private static List<InvitedUser> GetInvitedUsers()
    {
        var invitedUsers = (List<InvitedUser>)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/application/{_applicationId}/invitedusers")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(List<InvitedUser>));

        return invitedUsers;
    }

    public static async Task<(string?, string?)> ExecuteInvitation(string userCompanyName)
    {
        var emailAddress = "";
        var devMailApiRequests = new DevMailApiRequests();
        (string?, string?) data;
        var tempMailApiRequests = new TempMailApiRequests();
        try
        {
            var devUser = devMailApiRequests.GenerateRandomEmailAddress();
            emailAddress = devUser.Result.Name + "@developermail.com";
        }
        catch (Exception)
        {
            try
            {
                emailAddress = "apitestuser" + tempMailApiRequests.GetDomain();
            }
            catch (Exception)
            {
                throw new Exception("MailApi for sending invitation is not available");
            }
        }
        finally
        {
            Thread.Sleep(20000);
            await ExecuteInvitation(emailAddress, userCompanyName);

            Thread.Sleep(20000);

            var currentPassword = emailAddress.Contains("developermail.com")
                ? devMailApiRequests.FetchPassword()
                : tempMailApiRequests.FetchPassword();

            if (currentPassword is null)
            {
                throw new Exception("User password could not be fetched.");
            }

            var newPassword = new Password().Next();

            data = await GetCompanyTokenAndApplicationId(userCompanyName, emailAddress, currentPassword, newPassword);
        }
        return data;
    }

    // POST /api/registration/application/{applicationId}/companyDetailsWithAddress

    public static CompanyDetailData? SetCompanyDetailData(CompanyDetailData testCompanyDetailData)
    {
        var applicationStatus = GetApplicationStatus();
        if (applicationStatus != CompanyApplicationStatusId.CREATED.ToString()) throw new Exception($"Application status is not fitting to the pre-requisite");
        SetApplicationStatus(CompanyApplicationStatusId.ADD_COMPANY_DATA.ToString());
        var companyDetailData = GetCompanyDetailData();

        var newCompanyDetailData = testCompanyDetailData with
        {
            CompanyId = companyDetailData.CompanyId,
            Name = companyDetailData.Name
        };

        var body = JsonSerializer.Serialize(newCompanyDetailData, JsonSerializerOptions);

        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .ContentType("application/json")
            .When()
            .Body(body)
            .Post($"{BaseUrl}{EndPoint}/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .StatusCode(200);
        var storedCompanyDetailData = GetCompanyDetailData();
        if (!VerifyCompanyDetailDataStorage(storedCompanyDetailData, newCompanyDetailData))
            throw new Exception($"Company detail data was not stored correctly");
        return storedCompanyDetailData;
    }

    public static void UpdateCompanyDetailData(CompanyDetailData updateCompanyDetailData)
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
            var body = JsonSerializer.Serialize(newCompanyDetailData, JsonSerializerOptions);

            Given()
                .RelaxedHttpsValidation()
                .Header(
                    "authorization",
                    $"Bearer {_userCompanyToken}")
                .ContentType("application/json")
                .When()
                .Body(body)
                .Post($"{BaseUrl}{EndPoint}/application/{_applicationId}/companyDetailsWithAddress")
                .Then()
                .StatusCode(200);
            CompanyDetailData storedCompanyDetailData = GetCompanyDetailData();
            if (!VerifyCompanyDetailDataStorage(storedCompanyDetailData, newCompanyDetailData))
                throw new Exception($"Company detail data was not updated correctly");
        }
        else throw new Exception($"Application status is not fitting to the pre-requisite");
    }

    // POST /api/registration/application/{applicationId}/companyRoleAgreementConsents

    public static string? SubmitCompanyRoleConsentToAgreements(List<CompanyRoleId> companyRoles)
    {
        if (GetApplicationStatus() != CompanyApplicationStatusId.INVITE_USER.ToString()) throw new Exception($"Application status is not fitting to the pre-requisite");
        if (companyRoles.IsNullOrEmpty()) throw new Exception($"No company roles were found");
        var companyRoleAgreementConsents = GetCompanyRolesAndConsentsForSelectedRoles(companyRoles);
        var body = JsonSerializer.Serialize(companyRoleAgreementConsents, JsonSerializerOptions);

        SetApplicationStatus(CompanyApplicationStatusId.SELECT_COMPANY_ROLE.ToString());
        var result = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .ContentType("application/json")
            .Body(body)
            .When()
            .Post($"{BaseUrl}{EndPoint}/application/{_applicationId}/companyRoleAgreementConsents")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(int)).ToString();
        return result;
    }

    // POST /api/registration/application/{applicationId}/documentType/{documentTypeId}/documents

    public static int UploadDocument_WithEmptyTitle(string userCompanyName, string? documentTypeId,
        string? documentPath)
    {
        if (documentTypeId == null || !Enum.IsDefined(typeof(DocumentTypeId), documentTypeId))
            documentTypeId = "COMMERCIAL_REGISTER_EXTRACT";
        if (documentPath.IsNullOrEmpty())
        {
            documentPath = userCompanyName + "testfile.pdf";
            File.WriteAllText(documentPath, "Some Text");
        }

        if (GetApplicationStatus() != CompanyApplicationStatusId.UPLOAD_DOCUMENTS.ToString()) throw new Exception($"Application status is not fitting to the pre-requisite");
        var result = (int)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .ContentType("multipart/form-data")
            .MultiPart(new FileInfo(documentPath), "document")
            .When()
            .Post($"{BaseUrl}{EndPoint}/application/{_applicationId}/documentType/{documentTypeId}/documents")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(int));
        
        if (result == 1) SetApplicationStatus(CompanyApplicationStatusId.VERIFY.ToString());
        
        return result;
    }

    // POST /api/registration/application/{applicationId}/submitRegistration

    //[Fact]
    public static bool SubmitRegistration()
    {
        if (GetApplicationStatus() != CompanyApplicationStatusId.VERIFY.ToString()) throw new Exception($"Application status is not fitting to the pre-requisite");
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
        return status;
    }

    // GET: api/administration/registration/applications?companyName={companyName}

    public static CompanyApplicationDetails? GetApplicationDetails(string userCompanyName)
    {
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .When()
            .Get(
                $"{BaseUrl}{AdminEndPoint}/registration/applications?companyName={userCompanyName}&page=0&size=4&companyApplicationStatus=Closed")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();

        var data = DeserializeData<Pagination.Response<CompanyApplicationDetails>>(response.Content.ReadAsStringAsync()
            .Result);
        return data?.Content.First();
    }

    // GET: api/administration/registration/application/{applicationId}/companyDetailsWithAddress
    public static CompanyDetailData? GetCompanyWithAddress()
    {
        // Given
        var data = (CompanyDetailData)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .When()
            .Get($"{BaseUrl}{AdminEndPoint}/registration/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(CompanyDetailData));
        return data;
    }

    public static void InviteNewUser()
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
            .Post($"{BaseUrl}{EndPoint}/application/{_applicationId}/inviteNewUser")
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
    private static async Task ExecuteInvitation(string emailAddress, string userCompanyName)
    {
        CompanyInvitationData invitationData = new CompanyInvitationData("testuser", "myFirstName", "myLastName",
            emailAddress, userCompanyName);

        try
        {
            _operatorToken =
                await new AuthFlow(OperatorCompanyName).GetAccessToken(Secrets.OperatorUserName,
                    Secrets.OperatorUserPassword);

            Given()
                .RelaxedHttpsValidation()
                .Header(
                    "authorization",
                    $"Bearer {_operatorToken}")
                .ContentType("application/json")
                .Body(invitationData)
                .When()
                .Post($"{BaseUrl}{AdminEndPoint}/invitation")
                .Then()
                .StatusCode(200);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static async Task<(string?, string?)> GetCompanyTokenAndApplicationId(string userCompanyName, string emailAddress,
        string currentPassword, string newPassword)
    {
        _userCompanyToken =
            await new AuthFlow(userCompanyName).UpdatePasswordAndGetAccessToken(emailAddress, currentPassword,
                newPassword);
        _applicationId = GetFirstApplicationId();

        return (_userCompanyToken, _applicationId);
    }

    private static bool VerifyCompanyDetailDataStorage(CompanyDetailData storedData, CompanyDetailData postedData)
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

    private static string GetFirstApplicationId()
    {
        var applicationIDs = (List<CompanyApplicationData>)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/applications")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(List<CompanyApplicationData>));

        _applicationId = applicationIDs[0].ApplicationId.ToString();

        return _applicationId;
    }

    private static T? DeserializeData<T>(string jsonString)
    {
        var deserializedData = JsonSerializer.Deserialize<T>(jsonString, JsonSerializerOptions);
        return deserializedData;
    }

    private static List<CompanyRoleConsentViewData> GetCompanyRolesAndConsents()
    {
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .ContentType("application/json")
            .When()
            .Get(
                $"{BaseUrl}{AdminEndPoint}/companydata/companyRolesAndConsents")
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