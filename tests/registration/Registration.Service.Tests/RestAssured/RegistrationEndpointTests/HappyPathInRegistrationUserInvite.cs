using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using PasswordGenerator;
using Xunit;
using static RestAssured.Dsl;

namespace Registration.Service.Tests.RestAssured.RegistrationEndpointTests;

[TestCaseOrderer("Registration.Service.Tests.RestAssured.AlphabeticalOrderer",
    "Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests")]
public class HappyPathInRegistrationUserInvite
{
    private readonly string _baseUrl = "https://portal-backend.dev.demo.catena-x.net";
    private readonly string _endPoint = "/api/registration";
    private string _userCompanyToken;
    private static string? _applicationId;

    private readonly string _adminEndPoint = "/api/administration";
    private string? _operatorToken;
    private readonly string _operatorCompanyName = "CX-Operator";
    private static string _userCompanyName = "Test-Catena-X-C5";
    private static string[] _userEmailAddress;

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
        _applicationId = GetFirstApplicationId();
    }

    // POST /api/registration/application/{applicationId}/inviteNewUser

    [Fact]
    public void Test2_InviteNewUser_ReturnsExpectedResult()
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

        return applicationIDs[0].ApplicationId.ToString();
    }

    private List<InvitedUser> GetInvitedUsers()
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
}