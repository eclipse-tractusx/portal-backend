using System.Text.Json;
using System.Text.Json.Serialization;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using PasswordGenerator;
using Xunit;
using static RestAssured.Dsl;

namespace Registration.Service.Tests.RestAssured.RegistrationEndpointTests;

[TestCaseOrderer("Registration.Service.Tests.RestAssured.AlphabeticalOrderer",
    "Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests")]
public class RegistrationEndpointTestsHappyPathRegistrationWithoutBpn
{
    private static RegistrationEndpointHelper _regEndpointHelper;
    private static TestDataHelper _testDataHelper = new TestDataHelper();

    [Theory]
    [MemberData(nameof(GetDataEntries))]
    public async Task Scenario_HappyPathRegistrationWithoutBpn(TestDataModel testEntry)
    {
        var now = DateTime.Now;
        var userCompanyName = testEntry.companyDetailData.Name + now.Month + now.Day + now.Hour + now.Minute + now.Second; 
        _regEndpointHelper = new RegistrationEndpointHelper();

        await _regEndpointHelper.ExecuteInvitation(userCompanyName);
        _regEndpointHelper.SetCompanyDetailData(testEntry.companyDetailData);
        Thread.Sleep(3000);
        _regEndpointHelper.SubmitCompanyRoleConsentToAgreements(testEntry.companyRoles);
        Thread.Sleep(3000);
        _regEndpointHelper.UploadDocument_WithEmptyTitle(userCompanyName, testEntry.documentTypeId, testEntry.documentPath);
        Thread.Sleep(3000);
        _regEndpointHelper.SubmitRegistration();
        Thread.Sleep(3000);
        _regEndpointHelper.GetApplicationDetails(userCompanyName);
        Thread.Sleep(3000);
        _regEndpointHelper.GetCompanyWithAddress();
    }
    
    private static IEnumerable<object> GetDataEntries()
    {
        var testDataEntries = _testDataHelper.GetTestData("TestDataHappyPathRegistrationWithoutBpn.json");
        foreach (var t in testDataEntries)
        {
            yield return new object[] { t };
        }
    }
}