﻿using Registration.Service.Tests.EndToEndTests;
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

    private static IEnumerable<object> GetDataEntries()
    {
        var testDataEntries = TestDataHelper.GetTestData("TestDataHappyPathRegistrationWithoutBpn.json");
        foreach (var t in testDataEntries)
        {
            yield return new object[] { t };
        }
    }
}