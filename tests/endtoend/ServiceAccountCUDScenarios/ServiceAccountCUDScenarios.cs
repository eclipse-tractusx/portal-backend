using Castle.Core.Internal;
using EndToEnd.Tests;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using System.IdentityModel.Tokens.Jwt;
using Xunit;
using Xunit.Abstractions;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

[Trait("Category", "Portal")]
[TestCaseOrderer("Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests.AlphabeticalOrderer",
    "EndToEnd.Tests")]
[Collection("Portal")]
public class ServiceAccountCUDScenarios : EndToEndTestBase
{
    private static readonly string TokenUrl =
        TestResources.BaseCentralIdpUrl + "/auth/realms/CX-Central/protocol/openid-connect/token";

    public ServiceAccountCUDScenarios(ITestOutputHelper output) : base(output)
    {
    }

    [Fact(DisplayName = "Service Account Get Access Token")]
    private async Task GetAccessToken() // in order to just get token once, ensure that method name is alphabetically before other tests cases
    {
        var result = await AdministrationEndpointHelper.GetOperatorToken();
        result.Should().BeTrue("Could not get an access token for technical user.");
    }

    //Scenario - Create a new service account
    [Theory(DisplayName = "Service Account Creation")]
    [MemberData(nameof(GetDataEntries))]
    public void ServiceAccount_Creation(string[] permissions)
    {
        List<CompanyServiceAccountData>? existingServiceAccounts = null;

        // get a snapshot of current existing service accounts
        try
        {
            existingServiceAccounts = AdministrationEndpointHelper.GetServiceAccounts();
        }
        catch (Exception)
        {
            throw new Exception("Get Service Accounts Endpoint failed");
        }
        finally
        {
            //create a new service account
            var newServiceAccount =
                AdministrationEndpointHelper.CreateNewServiceAccount(permissions);

            try
            {
                //check if the new service account is added (validation against the previous taken snapshot)
                var updatedServiceAccounts = AdministrationEndpointHelper.GetServiceAccounts();

                if (!existingServiceAccounts.IsNullOrEmpty())
                {
                    var checkAccountIsNew =
                        existingServiceAccounts!.Where(t =>
                            t.ServiceAccountId == newServiceAccount.ServiceAccountId);
                    checkAccountIsNew.Should().BeEmpty();
                }

                var checkAccountAdded =
                    updatedServiceAccounts.Where(t => t.ServiceAccountId == newServiceAccount.ServiceAccountId);
                checkAccountAdded.Should().NotBeNullOrEmpty("New service account could not be found in the list of service accounts");
            }
            catch (Exception)
            {
                throw new Exception("Get Service Accounts Endpoint failed");
            }
            finally
            {
                //fetch the serviceAccount token and validate if the token includes a attribute "bpn"
                var token = TechTokenRetriever.GetToken(TokenUrl, newServiceAccount.ClientId,
                    newServiceAccount.Secret);
                token.Should().NotBeNullOrEmpty("Token for new technical user could not be fetched correctly");

                var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
                jwtSecurityToken.Payload.Should().ContainKey("bpn", "Attribute BPN in user token was not found");
                AdministrationEndpointHelper.DeleteServiceAccount(
                        newServiceAccount.ServiceAccountId.ToString()).Should().BeTrue("Created service account could not be deleted");
            }
        }
    }

    //Scenario - Create a new service account and update the same
    [Theory(DisplayName = "Service Account Data Update")]
    [MemberData(nameof(GetDataEntries))]
    public void ServiceAccount_DataUpdate(string[] permissions)
    {
        //create a new service account
        var newServiceAccount =
            AdministrationEndpointHelper.CreateNewServiceAccount(permissions);

        //update the previous created service account details by changing "name" and "description"
        var now = DateTime.Now;
        var newTechUserName = $"UpdatedTechUserName_{now:s}";
        var newDescription = "This is an updated description for a technical user via test automation e2e tests";
        AdministrationEndpointHelper.UpdateServiceAccountDetailsById(newServiceAccount.ServiceAccountId.ToString(),
            newTechUserName, newDescription);

        //check if the change of the serviceAccount got successfully saved
        var updatedServiceAccount =
            AdministrationEndpointHelper.GetServiceAccountDetailsById(newServiceAccount.ServiceAccountId.ToString());

        updatedServiceAccount.Name.Should().Be(newTechUserName, "Updated technical user name was not stored correctly.");
        updatedServiceAccount.Description.Should().Be(newDescription, "Updated description of service account was not stored correctly");
        AdministrationEndpointHelper.DeleteServiceAccount(updatedServiceAccount.ServiceAccountId.ToString()).Should().BeTrue("Created service account could not be deleted.");
    }

    //Scenario - Create a new service account and update the credentials
    [Theory(DisplayName = "Service Account credential refresh")]
    [MemberData(nameof(GetDataEntries))]
    public void ServiceAccount_CredentialRefresh(string[] permissions)
    {
        // create a new service account
        var newServiceAccount =
            AdministrationEndpointHelper.CreateNewServiceAccount(permissions);

        //reset service account credentials
        var updatedServiceAccount =
            AdministrationEndpointHelper.ResetServiceAccountCredentialsById(
                newServiceAccount.ServiceAccountId.ToString());
        updatedServiceAccount.Should().NotBeNull();

        //check if the resetup of the credentials was successful
        updatedServiceAccount.Secret.Should().NotBe(newServiceAccount.Secret);

        //get a token with the new credentials to ensure that the reset was really successful
        var token = TechTokenRetriever.GetToken(TokenUrl, updatedServiceAccount.ClientId, updatedServiceAccount.Secret);

        token.Should().NotBeNullOrEmpty("Token for new technical user could not be fetched correctly");
        AdministrationEndpointHelper.DeleteServiceAccount(updatedServiceAccount.ServiceAccountId.ToString()).Should().BeTrue("Created service account could not be deleted");
    }

    //Scenario - Create and delete a new service account
    [Theory(DisplayName = "Service Account Deletion")]
    [MemberData(nameof(GetDataEntries))]
    public void ServiceAccount_Deletion(string[] permissions)
    {
        // create a new service account
        var newServiceAccount =
            AdministrationEndpointHelper.CreateNewServiceAccount(permissions);

        //  check if the new service account is available
        var existingServiceAccounts = AdministrationEndpointHelper.GetServiceAccounts();

        var checkAccountIsAvailable =
            existingServiceAccounts.Where(t => t.ServiceAccountId == newServiceAccount.ServiceAccountId);
        checkAccountIsAvailable.Should().NotBeNullOrEmpty();

        //delete the created service account
        AdministrationEndpointHelper.DeleteServiceAccount(newServiceAccount.ServiceAccountId.ToString());

        //check the endpoint, the deleted service account should not be available anymore
        var updatedServiceAccounts = AdministrationEndpointHelper.GetServiceAccounts();

        var checkAccountDeleted =
            updatedServiceAccounts.Where(t => t.ServiceAccountId == newServiceAccount.ServiceAccountId);
        checkAccountDeleted.Should().BeEmpty();

        updatedServiceAccounts.Where(item => item.ServiceAccountId == newServiceAccount.ServiceAccountId).Should().BeEmpty();
    }

    public static IEnumerable<object[]> GetDataEntries
    {
        get
        {
            var testDataEntries =
                TestDataHelper.GetTestDataForServiceAccountCUDScenarios("TestDataServiceAccountCUDScenarios.json");
            foreach (var t in testDataEntries)
            {
                yield return new object[] { t };
            }
        }
    }
}
