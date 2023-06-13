using Registration.Service.Tests.EndToEndTests;
using Xunit;

namespace Registration.Service.Tests.RestAssured.RegistrationEndpointTests;

[TestCaseOrderer("Registration.Service.Tests.RestAssured.AlphabeticalOrderer",
    "Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests")]
public class RegistrationEndpointTestsHappyPathRegistrationWithoutBpn
{
    [Theory]
    [MemberData(nameof(GetDataEntries))]
    public async Task Scenario_HappyPathRegistrationWithoutBpn(TestDataModel testEntry)
    {
        var now = DateTime.Now;
        var userCompanyName = $"{testEntry.companyDetailData.Name}_{now:s}";

        await RegistrationEndpointHelper.ExecuteInvitation(userCompanyName);
        RegistrationEndpointHelper.SetCompanyDetailData(testEntry.companyDetailData);
        Thread.Sleep(3000);
        RegistrationEndpointHelper.SubmitCompanyRoleConsentToAgreements(testEntry.companyRoles);
        Thread.Sleep(3000);
        RegistrationEndpointHelper.UploadDocument_WithEmptyTitle(userCompanyName, testEntry.documentTypeId,
            testEntry.documentPath);
        Thread.Sleep(3000);
        RegistrationEndpointHelper.SubmitRegistration();
        Thread.Sleep(3000);
        RegistrationEndpointHelper.GetApplicationDetails(userCompanyName);
        Thread.Sleep(3000);
        RegistrationEndpointHelper.GetCompanyWithAddress();
    }

    private static IEnumerable<object> GetDataEntries()
    {
        var testDataEntries = TestDataHelper.GetTestData("TestDataHappyPathRegistrationWithoutBpn.json");
        foreach (var t in testDataEntries)
        {
            yield return new object[] { t };
        }
    }
}