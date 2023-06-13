namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.RestAssured;

public class ServiceAccountCUDScenarios
{
    //Scenario - Create a new service account
    [Fact]
    public async Task Scenario_HappyPathCreateServiceAccount()
    {
        await AdministrationEndpointHelper.GetOperatorToken();
        
        var now = DateTime.Now;
        var techUserName = $"NewTechUserName_{now:s}";

        // get a snapshot of current existing service accounts
        var existingServiceAccounts = AdministrationEndpointHelper.GetServiceAccounts();

        //create a new service account
        var newServiceAccount =
            AdministrationEndpointHelper.CreateNewServiceAccount(techUserName, "This is a new test technical user");

        //check if the new service account is added (validation against the previous taken snapshot)
        var updatedServiceAccounts = AdministrationEndpointHelper.GetServiceAccounts();

        //fetch the serviceAccount token and validate if the token includes a attribute "bpn"
    }

    //Scenario - Create a new service account and update the same
    [Fact]
    public async Task Scenario_HappyPathCreateAndUpdateServiceAccount()
    {
        await AdministrationEndpointHelper.GetOperatorToken();
        var now = DateTime.Now;
        var techUserName = $"NewTechUserName_{now:s}";

        //create a new service account
        var newServiceAccount =
            AdministrationEndpointHelper.CreateNewServiceAccount(techUserName, "This is a new test technical user");

        //update the previous created service account details by changing "name" and "description"
        var newTechUserName = $"UpdatedTechUserName_{now:s}";
        var newDescription = "This is an updated description";
        AdministrationEndpointHelper.UpdateServiceAccountDetailsById(newServiceAccount.ServiceAccountId.ToString(),
            newTechUserName, newDescription);

        //check if the change of the serviceAccount got successfully saved
        var updatedServiceAccount =
            AdministrationEndpointHelper.GetServiceAccountDetailsById(newServiceAccount.ServiceAccountId.ToString());

        Assert.True(updatedServiceAccount.Name == newTechUserName);
        Assert.True(updatedServiceAccount.Description == newDescription);
    }

    //Scenario - Create a new service account and update the credentials
    [Fact]
    public async Task Scenario_HappyPathCreateServiceAccountAndUpdateCredentials()
    {
        await AdministrationEndpointHelper.GetOperatorToken();
        
        var now = DateTime.Now;
        var techUserName = $"NewTechUserName_{now:s}";

        // create a new service account
        var newServiceAccount =
            AdministrationEndpointHelper.CreateNewServiceAccount(techUserName, "This is a new test technical user");

        //reset service account credentials
        var updatedServiceAccount =
            AdministrationEndpointHelper.ResetServiceAccountCredentialsById(
                newServiceAccount.ServiceAccountId.ToString());

        //check if the resetup of the credentials was successful
        Assert.True(newServiceAccount.Secret != updatedServiceAccount.Secret);

        //get a token with the new credentials to ensure that the reset was really successful
    }

    //Scenario - Create and delete a new service account
    [Fact]
    public async Task Scenario_HappyPathCreateAndDeleteServiceAccount()
    {
        await AdministrationEndpointHelper.GetOperatorToken();
        var now = DateTime.Now;
        var techUserName = $"NewTechUserName_{now:s}";

        // create a new service account
        var newServiceAccount =
            AdministrationEndpointHelper.CreateNewServiceAccount(techUserName, "This is a new test technical user");

        //  check if the new service account is available
        var existingServiceAccounts = AdministrationEndpointHelper.GetServiceAccounts();

        //delete the created service account
        AdministrationEndpointHelper.DeleteServiceAccount(newServiceAccount.ServiceAccountId.ToString());

        //check the endpoint, the deleted service account should not be available anymore
        var updatedServiceAccounts = AdministrationEndpointHelper.GetServiceAccounts();

        Assert.Empty(updatedServiceAccounts.Where(item => item.ServiceAccountId == newServiceAccount.ServiceAccountId));
    }
}