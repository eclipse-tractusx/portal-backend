using Registration.Service.Tests.RestAssured.RegistrationEndpointTests;
using Xunit;

namespace Registration.Service.Tests.EndToEndTests;

[TestCaseOrderer("Registration.Service.Tests.EndToEndTests.AlphabeticalOrderer",
    "Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests")]
public class RegistrationScenarios
{
    [Theory]
    [MemberData(nameof(GetDataEntriesForRegistrationWithoutBpn))]
    public async Task Scenario1_HappyPathRegistrationWithoutBpn(TestDataModel testEntry)
    {
        var now = DateTime.Now;
        var userCompanyName = $"{testEntry.companyDetailData.Name}_{now:s}";
        userCompanyName = userCompanyName.Replace(":", "").Replace("_", "");

        var (companyToken, applicationId) = await RegistrationEndpointHelper.ExecuteInvitation(userCompanyName);
        Assert.NotNull(companyToken);
        Assert.NotNull(applicationId);
        
        var companyDetailData = RegistrationEndpointHelper.SetCompanyDetailData(testEntry.companyDetailData);
        Assert.Equal(userCompanyName, companyDetailData?.Name);
        Thread.Sleep(3000);
        
        var roleSubmissionResult = RegistrationEndpointHelper.SubmitCompanyRoleConsentToAgreements(testEntry.companyRoles);
        Assert.True(int.Parse(roleSubmissionResult) > 0);
        Thread.Sleep(3000);
        
        var docUploadResult = RegistrationEndpointHelper.UploadDocument_WithEmptyTitle(userCompanyName, testEntry.documentTypeId,
            testEntry.documentPath);
        Assert.Equal(1, docUploadResult);
        
        Thread.Sleep(3000);
        var status = RegistrationEndpointHelper.SubmitRegistration();
        Assert.True(status);
        Thread.Sleep(3000);
        
        var applicationDetails = RegistrationEndpointHelper.GetApplicationDetails(userCompanyName);
        Assert.NotNull(applicationDetails);
        Assert.Contains("SUBMITTED", applicationDetails.CompanyApplicationStatusId.ToString());
        Assert.Equal(applicationId, applicationDetails.ApplicationId.ToString());
        
        Thread.Sleep(3000);
        var storedCompanyDetailData = RegistrationEndpointHelper.GetCompanyWithAddress();
        Assert.NotNull(storedCompanyDetailData);
    }
    
    [Theory]
    [MemberData(nameof(GetDataEntriesForUpdateCompanyDetailDataWithoutBpn))]
    public async Task Scenario2_HappyPathUpdateCompanyDetailDataWithoutBpn(TestDataModel testEntry)
    {
        var now = DateTime.Now;
        var userCompanyName = $"{testEntry.companyDetailData.Name}_{now:s}";
        await RegistrationEndpointHelper.ExecuteInvitation(userCompanyName);
        Thread.Sleep(3000);
        RegistrationEndpointHelper.SetCompanyDetailData(testEntry.companyDetailData);
        Thread.Sleep(3000);
        RegistrationEndpointHelper.UpdateCompanyDetailData(testEntry.updateCompanyDetailData);
    }
    
    [Fact]
    public async Task Scenario3_HappyPathInRegistrationUserInvite()
    {
        var now = DateTime.Now;
        var userCompanyName = $"Test-Catena-X_{now:s}";
        await RegistrationEndpointHelper.ExecuteInvitation(userCompanyName);
        Thread.Sleep(10000);
        RegistrationEndpointHelper.InviteNewUser();
    }
    
    private static IEnumerable<object> GetDataEntriesForUpdateCompanyDetailDataWithoutBpn()
    {
        var testDataEntries = TestDataHelper.GetTestData("TestDataHappyPathUpdateCompanyDetailData.json");
        foreach (var t in testDataEntries)
        {
            yield return new object[] { t };
        }
    }

    private static IEnumerable<object> GetDataEntriesForRegistrationWithoutBpn()
    {
        var testDataEntries = TestDataHelper.GetTestData("TestDataHappyPathRegistrationWithoutBpn.json");
        foreach (var t in testDataEntries)
        {
            yield return new object[] { t };
        }
    }
}