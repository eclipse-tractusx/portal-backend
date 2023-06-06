using Xunit;

namespace Registration.Service.Tests.RestAssured.RegistrationEndpointTests;

[TestCaseOrderer("Registration.Service.Tests.RestAssured.AlphabeticalOrderer",
    "Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests")]
public class HappyPathUpdateCompanyDetailData
{
    [Theory]
    [MemberData(nameof(GetDataEntries))]
    public async Task Scenario_HappyPathUpdateCompanyDetailDataWithoutBpn(TestDataModel testEntry)
    {
        var now = DateTime.Now;
        var userCompanyName = $"{testEntry.companyDetailData.Name}_{now:s}";
        await RegistrationEndpointHelper.ExecuteInvitation(userCompanyName);
        Thread.Sleep(3000);
        RegistrationEndpointHelper.SetCompanyDetailData(testEntry.companyDetailData);
        Thread.Sleep(3000);
        RegistrationEndpointHelper.UpdateCompanyDetailData(testEntry.updateCompanyDetailData);
    }
    
    private static IEnumerable<object> GetDataEntries()
    {
        var testDataEntries = TestDataHelper.GetTestData("TestDataHappyPathUpdateCompanyDetailData.json");
        foreach (var t in testDataEntries)
        {
            yield return new object[] { t };
        }
    }
}