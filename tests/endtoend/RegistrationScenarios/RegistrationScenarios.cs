/********************************************************************************
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit;
using Xunit.Abstractions;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

[Trait("Category", "Registration")]
[TestCaseOrderer("Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests.AlphabeticalOrderer",
    "EndToEnd.Tests")]
[Collection("Registration")]
public class RegistrationScenarios : EndToEndTestBase
{
    public RegistrationScenarios(ITestOutputHelper output) : base(output)
    {
    }

    [Theory(DisplayName = "Company Registration with manual data input")]
    [MemberData(nameof(GetDataEntriesForRegistrationWithoutBpn))]
    public async Task CompanyRegistration_WithManualDataInput(TestDataRegistrationModel testEntry)
    {
        if (testEntry == null)
            throw new ArgumentNullException(nameof(testEntry));
        var now = DateTime.Now;
        var userCompanyName = $"{testEntry.CompanyDetailData?.Name}_{now:s}";
        userCompanyName = userCompanyName.Replace(":", "").Replace("_", "");

        var (companyToken, applicationId) = await RegistrationEndpointHelper.ExecuteInvitation(userCompanyName);
        companyToken.Should().NotBeNull();
        applicationId.Should().NotBeNull();

        var companyDetailData = await RegistrationEndpointHelper.SetCompanyDetailData(testEntry.CompanyDetailData);
        companyDetailData.Name.Should().Be(userCompanyName);
        await Task.Delay(3000);

        var roleSubmissionResult =
            await RegistrationEndpointHelper.SubmitCompanyRoleConsentToAgreements(testEntry.CompanyRoles);
        int.Parse(roleSubmissionResult).Should().BeGreaterThan(0);
        await Task.Delay(3000);

        await RegistrationEndpointHelper.UploadDocument_WithEmptyTitle(testEntry.DocumentTypeId, testEntry.DocumentName);

        await Task.Delay(3000);
        var status = await RegistrationEndpointHelper.SubmitRegistration();
        status.Should().BeTrue();
        await Task.Delay(3000);

        var applicationDetails = await RegistrationEndpointHelper.GetApplicationDetails(userCompanyName);
        applicationDetails.Should().NotBeNull();
        applicationDetails!.CompanyApplicationStatusId.Should().Be(CompanyApplicationStatusId.SUBMITTED);
        applicationDetails.ApplicationId.ToString().Should().Be(applicationId);

        await Task.Delay(3000);
        var storedCompanyDetailData = await RegistrationEndpointHelper.GetCompanyWithAddress();
        storedCompanyDetailData.Should().NotBeNull();
    }

    [Theory(DisplayName = "Company Registration - add company data and update company data")]
    [MemberData(nameof(GetDataEntriesForUpdateCompanyDetailDataWithoutBpn))]
    public async Task CompanyRegistration_AddAndUpdateCompanyData(TestDataRegistrationModel testEntry)
    {
        if (testEntry.CompanyDetailData is null || testEntry.UpdateCompanyDetailData is null)
        {
            throw new Exception("Test data must provide company detail data and company detail data to update.");
        }

        var now = DateTime.Now;
        var userCompanyName = $"{testEntry.CompanyDetailData.Name}_{now:s}";
        userCompanyName = userCompanyName.Replace(":", "").Replace("_", "");
        await RegistrationEndpointHelper.ExecuteInvitation(userCompanyName);
        await Task.Delay(3000);
        await RegistrationEndpointHelper.SetCompanyDetailData(testEntry.CompanyDetailData);
        await RegistrationEndpointHelper.UpdateCompanyDetailData(testEntry.UpdateCompanyDetailData);
    }

    [Fact(DisplayName = "Company Registration with manual data input & additional user invite")]
    public async Task CompanyRegistration_WithManualDataInputAndAdditionalUserInvite()
    {
        var now = DateTime.Now;
        var userCompanyName = $"Test-Catena-X_{now:s}";
        userCompanyName = userCompanyName.Replace(":", "").Replace("_", "");
        await RegistrationEndpointHelper.ExecuteInvitation(userCompanyName);
        await Task.Delay(10000);
        await RegistrationEndpointHelper.InviteNewUser();
    }

    [Theory(DisplayName = "Company Registration by BPN")]
    [MemberData(nameof(GetDataEntriesForRegistrationWithBpn))]
    public async Task CompanyRegistration_ByBpn(TestDataRegistrationModel testEntry)
    {
        var bpn = await RegistrationEndpointHelper.GetBpn();
        var bpdmCompanyDetailData = await RegistrationEndpointHelper.GetCompanyBpdmDetailData(bpn);

        var now = DateTime.Now;
        var userCompanyName = bpdmCompanyDetailData.Name + $"{now:s}";
        userCompanyName = userCompanyName.Replace(":", "").Replace("_", "");
        var (companyToken, applicationId) =
            await RegistrationEndpointHelper.ExecuteInvitation(userCompanyName);
        companyToken.Should().NotBeNullOrEmpty();
        applicationId.Should().NotBeNullOrEmpty();

        var companyDetailData = await RegistrationEndpointHelper.SetCompanyDetailData(bpdmCompanyDetailData);
        companyDetailData.Name.Should().Be(userCompanyName);
        await Task.Delay(3000);

        var roleSubmissionResult =
            await RegistrationEndpointHelper.SubmitCompanyRoleConsentToAgreements(testEntry.CompanyRoles);
        int.Parse(roleSubmissionResult).Should().BeGreaterThan(0);
        await Task.Delay(3000);

        await RegistrationEndpointHelper.UploadDocument_WithEmptyTitle(testEntry.DocumentTypeId, testEntry.DocumentName);

        await Task.Delay(3000);
        var status = await RegistrationEndpointHelper.SubmitRegistration();
        status.Should().BeTrue();
        await Task.Delay(3000);

        var applicationDetails = await RegistrationEndpointHelper.GetApplicationDetails(userCompanyName);
        applicationDetails.Should().NotBeNull();
        applicationDetails?.CompanyApplicationStatusId.Should().Be(CompanyApplicationStatusId.SUBMITTED);
        applicationDetails?.ApplicationId.ToString().Should().Be(applicationId);

        await Task.Delay(3000);
        var storedCompanyDetailData = await RegistrationEndpointHelper.GetCompanyWithAddress();
        storedCompanyDetailData.Should().NotBeNull();
    }

    public static IEnumerable<object[]> GetDataEntriesForUpdateCompanyDetailDataWithoutBpn
    {
        get => TestDataHelper.GetTestDataForRegistrationWithoutBpn("TestDataHappyPathUpdateCompanyDetailData.json").Select(t => new object[] { t });
    }

    public static IEnumerable<object[]> GetDataEntriesForRegistrationWithoutBpn
    {
        get => TestDataHelper.GetTestDataForRegistrationWithoutBpn("TestDataHappyPathRegistrationWithoutBpn.json").Select(t => new object[] { t });
    }

    public static IEnumerable<object[]> GetDataEntriesForRegistrationWithBpn
    {
        get => TestDataHelper.GetTestDataForRegistrationWithBpn("TestDataHappyPathRegistrationWithBpn.json").Select(t => new object[] { t });
    }
}
