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
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using PasswordGenerator;
using RestAssured.Response.Logging;
using System.Net;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

public static class RegistrationEndpointHelper
{
    private static readonly string BaseUrl = TestResources.BasePortalBackendUrl;
    private static readonly string bpdmBaseUrl = TestResources.BpdmUrl;
    private static readonly string EndPoint = "/api/registration";
    private static readonly string AdminEndPoint = "/api/administration";
    private static readonly string PortalUserCompanyName = TestResources.PortalUserCompanyName;
    private static string? _userCompanyToken;
    private static string? _portalUserToken;
    private static string? _applicationId;

    private static readonly Secrets Secrets = new();

    public static async Task<string> GetBpn()
    {
        const string endpoint = "/companies/test-company/api/catena/input/legal-entities?page=0&size=10";
        _portalUserToken = await new AuthFlow(PortalUserCompanyName)
            .GetAccessToken(Secrets.PortalUserName, Secrets.PortalUserPassword);

        var data = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
                .Post($"{bpdmBaseUrl}{endpoint}")
            .Then()
            .And()
                .StatusCode(200)
                .And()
                .Extract()
                .Response();

        var bpdmLegalEntityDatas = DataHandleHelper.DeserializeData<BpdmPaginationContent>(await data.Content.ReadAsStringAsync().ConfigureAwait(false));
        if (bpdmLegalEntityDatas is null)
        {
            throw new Exception($"Could not get bpn from {endpoint} should not be null.");
        }

        return bpdmLegalEntityDatas.Content.ElementAt(new Random().Next(bpdmLegalEntityDatas.Content.Count())).Bpn;
    }

    //GET /api/registration/legalEntityAddress/{bpn}
    public static async Task<CompanyDetailData> GetCompanyBpdmDetailData(string bpn)
    {
        // Given
        var endpoint = $"{EndPoint}/legalEntityAddress/{bpn}";
        var data = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get(BaseUrl + endpoint)
            .Then()
            .StatusCode(200)
            .And()
            .Extract()
            .Response();
        var bpdmDetailData = DataHandleHelper.DeserializeData<CompanyDetailData>(await data.Content.ReadAsStringAsync().ConfigureAwait(false));
        if (bpdmDetailData is null)
        {
            throw new Exception($"Could not get bpdm detail data from {endpoint} should not be null.");
        }

        return bpdmDetailData;
    }

    public static async Task<CompanyDetailData> GetCompanyDetailData()
    {
        // Given
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .And()
            .StatusCode(200)
            .Extract()
            .Response();
        var data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var companyDetailData = DataHandleHelper.DeserializeData<CompanyDetailData>(data);
        if (companyDetailData is null)
        {
            throw new Exception("Company detail data was not found.");
        }

        return companyDetailData;
    }

    private static async Task<CompanyRoleAgreementConsents> GetCompanyRolesAndConsentsForSelectedRoles(
        List<CompanyRoleId> companyRoleIds)
    {
        var companyRoleAgreementData = await GetCompanyRoleAgreementData().ConfigureAwait(false);
        var selectedCompanyRoleIds = new List<CompanyRoleId>();

        var agreementConsentStatusList = new List<AgreementConsentStatus>();

        foreach (var roleData in companyRoleAgreementData.CompanyRoleData.IntersectBy(companyRoleIds, x => x.CompanyRoleId))
        {
            selectedCompanyRoleIds.Add(roleData.CompanyRoleId);
            agreementConsentStatusList.AddRange(
                roleData.AgreementIds.Select(id => new AgreementConsentStatus(id, ConsentStatusId.ACTIVE)));
        }

        return new CompanyRoleAgreementConsents(selectedCompanyRoleIds, agreementConsentStatusList);
    }

    private static async Task SetApplicationStatus(string applicationStatus)
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .ContentType("application/json")
            .When()
            .Put(
                $"{BaseUrl}{EndPoint}/application/{_applicationId}/status?status={applicationStatus}")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();

        var status = DataHandleHelper.DeserializeData<int>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        status.Should().Be(1);
    }

    private static async Task<string> GetApplicationStatus()
    {
        var endpoint = $"{EndPoint}/application/{_applicationId}/status";
        var data = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .ContentType("application/json")
            .When()
            .Get(BaseUrl + endpoint)
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();
        var applicationStatus = DataHandleHelper.DeserializeData<string>(await data.Content.ReadAsStringAsync().ConfigureAwait(false));
        if (applicationStatus is null)
        {
            throw new Exception($"Could not get application status from {endpoint}, response was null/empty.");
        }
        return applicationStatus;
    }

    private static async Task<List<InvitedUser>> GetInvitedUsers()
    {
        var data = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/application/{_applicationId}/invitedusers")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();

        var invitedUsers = DataHandleHelper.DeserializeData<List<InvitedUser>>(await data.Content.ReadAsStringAsync().ConfigureAwait(false));
        return invitedUsers ?? new List<InvitedUser>();
    }

    public static async Task<(string?, string?)> ExecuteInvitation(string userCompanyName)
    {
        var emailAddress = "";
        var devMailApiRequests = new DevMailApiRequests();
        (string?, string?) data;
        var tempMailApiRequests = new TempMailApiRequests();
        try
        {
            var devUser = await devMailApiRequests.GenerateRandomEmailAddress().ConfigureAwait(false);
            emailAddress = devUser.Result.Name + "@developermail.com";
        }
        catch (Exception)
        {
            try
            {
                emailAddress = "apitestuser" + await tempMailApiRequests.GetDomain().ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw new Exception("MailApi for sending invitation is not available");
            }
        }
        finally
        {
            await Task.Delay(20000).ConfigureAwait(false);
            await ExecuteInvitation(emailAddress, userCompanyName);

            await Task.Delay(60000).ConfigureAwait(false);

            var currentPassword = emailAddress.Contains("developermail.com")
                ? await devMailApiRequests.FetchPassword().ConfigureAwait(false)
                : await tempMailApiRequests.FetchPassword().ConfigureAwait(false);

            if (currentPassword is null)
            {
                throw new Exception("Authentication flow failed: User password could not be fetched.");
            }

            var newPassword = new Password().Next();

            data = await GetCompanyTokenAndApplicationId(userCompanyName, emailAddress, currentPassword, newPassword);
        }

        return data;
    }

    // POST /api/registration/application/{applicationId}/companyDetailsWithAddress

    public static async Task<CompanyDetailData> SetCompanyDetailData(CompanyDetailData? testCompanyDetailData)
    {
        if (testCompanyDetailData is null)
            throw new Exception("Company detail data expected but not provided");
        var applicationStatus = await GetApplicationStatus().ConfigureAwait(false);
        if (applicationStatus != CompanyApplicationStatusId.CREATED.ToString())
            throw new Exception("Application status is not fitting to the pre-requisite");
        await SetApplicationStatus(CompanyApplicationStatusId.ADD_COMPANY_DATA.ToString()).ConfigureAwait(false);
        var companyDetailData = await GetCompanyDetailData().ConfigureAwait(false);

        var newCompanyDetailData = testCompanyDetailData with
        {
            CompanyId = companyDetailData.CompanyId,
            Name = companyDetailData.Name
        };

        var body = DataHandleHelper.SerializeData(newCompanyDetailData);

        Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .ContentType("application/json")
            .When()
            .Body(body)
            .Post($"{BaseUrl}{EndPoint}/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200);
        var storedCompanyDetailData = await GetCompanyDetailData().ConfigureAwait(false);
        if (!VerifyCompanyDetailDataStorage(storedCompanyDetailData, newCompanyDetailData))
            throw new Exception($"Company detail data was not stored correctly");
        return storedCompanyDetailData;
    }

    public static async Task UpdateCompanyDetailData(CompanyDetailData updateCompanyDetailData)
    {
        var actualStatus = await GetApplicationStatus().ConfigureAwait(false);
        if (actualStatus != CompanyApplicationStatusId.ADD_COMPANY_DATA.ToString() &&
            actualStatus != CompanyApplicationStatusId.SUBMITTED.ToString())
        {
            var companyDetailData = await GetCompanyDetailData().ConfigureAwait(false);

            var newCompanyDetailData = updateCompanyDetailData with { CompanyId = companyDetailData.CompanyId };
            var body = DataHandleHelper.SerializeData(newCompanyDetailData);

            Given()
                .DisableSslCertificateValidation()
                .Header(
                    "authorization",
                    $"Bearer {_userCompanyToken}")
                .ContentType("application/json")
                .When()
                .Body(body)
                .Post($"{BaseUrl}{EndPoint}/application/{_applicationId}/companyDetailsWithAddress")
                .Then()
                .Log(ResponseLogLevel.OnError)
                .StatusCode(200);
            var storedCompanyDetailData = await GetCompanyDetailData().ConfigureAwait(false);
            if (!VerifyCompanyDetailDataStorage(storedCompanyDetailData, newCompanyDetailData))
                throw new Exception($"Company detail data was not updated correctly");
        }
        else
        {
            throw new Exception($"Application status is not fitting to the pre-requisite");
        }
    }

    // POST /api/registration/application/{applicationId}/companyRoleAgreementConsents

    public static async Task<string> SubmitCompanyRoleConsentToAgreements(List<CompanyRoleId> companyRoles)
    {
        if ((await GetApplicationStatus().ConfigureAwait(false)) != CompanyApplicationStatusId.INVITE_USER.ToString())
            throw new Exception("Application status is not fitting to the pre-requisite");
        if (companyRoles.IsNullOrEmpty())
            throw new Exception("No company roles were found");
        var companyRoleAgreementConsents = await GetCompanyRolesAndConsentsForSelectedRoles(companyRoles).ConfigureAwait(false);
        var body = DataHandleHelper.SerializeData(companyRoleAgreementConsents);

        await SetApplicationStatus(CompanyApplicationStatusId.SELECT_COMPANY_ROLE.ToString()).ConfigureAwait(false);
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .ContentType("application/json")
            .Body(body)
            .When()
            .Post($"{BaseUrl}{EndPoint}/application/{_applicationId}/companyRoleAgreementConsents")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();

        var result = DataHandleHelper.DeserializeData<int>(await response.Content.ReadAsStringAsync().ConfigureAwait(false)).ToString();
        return result;
    }

    // POST /api/registration/application/{applicationId}/documentType/{documentTypeId}/documents

    public static async Task UploadDocument_WithEmptyTitle(string? documentTypeId, string? documentName)
    {
        if (documentTypeId is null || documentName is null)
            throw new Exception("No document type id or name provided but expected");
        if ((await GetApplicationStatus().ConfigureAwait(false)) != CompanyApplicationStatusId.UPLOAD_DOCUMENTS.ToString())
            throw new Exception($"Application status is not fitting to the pre-requisite");
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .ContentType("multipart/form-data")
            .MultiPart(new FileInfo(documentName), "document")
            .When()
            .Post($"{BaseUrl}{EndPoint}/application/{_applicationId}/documentType/{documentTypeId}/documents")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(204)
            .Extract()
            .Response();

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            await SetApplicationStatus(CompanyApplicationStatusId.VERIFY.ToString()).ConfigureAwait(false);
        }
    }

    // POST /api/registration/application/{applicationId}/submitRegistration

    //[Fact]
    public static async Task<bool> SubmitRegistration()
    {
        if ((await GetApplicationStatus().ConfigureAwait(false)) != CompanyApplicationStatusId.VERIFY.ToString())
            throw new Exception($"Application status is not fitting to the pre-requisite");
        var data = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .ContentType("application/json")
            //.Body("")
            .When()
            .Post(
                $"{BaseUrl}{EndPoint}/application/{_applicationId}/submitRegistration")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();

        var status = DataHandleHelper.DeserializeData<bool>(await data.Content.ReadAsStringAsync().ConfigureAwait(false));
        return status;
    }

    // GET: api/administration/registration/applications?companyName={companyName}

    public static async Task<CompanyApplicationDetails?> GetApplicationDetails(string userCompanyName)
    {
        var endpoint = $"{AdminEndPoint}/registration/applications?companyName={userCompanyName}&page=0&size=4&companyApplicationStatus=Closed";
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get(BaseUrl + endpoint)
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();

        var data = DataHandleHelper.DeserializeData<Pagination.Response<CompanyApplicationDetails>>(await response.Content
            .ReadAsStringAsync().ConfigureAwait(false));
        if (data is null)
        {
            throw new Exception($"Could not application details of first application from {endpoint}, response was null/empty.");
        }
        return data.Content.First();
    }

    // GET: api/administration/registration/application/{applicationId}/companyDetailsWithAddress
    public static async Task<CompanyDetailData?> GetCompanyWithAddress()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get($"{BaseUrl}{AdminEndPoint}/registration/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();

        var data = DataHandleHelper.DeserializeData<CompanyDetailData>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        return data;
    }

    public static async Task InviteNewUser()
    {
        var devMailApiRequests = new DevMailApiRequests();
        var devUser = await devMailApiRequests.GenerateRandomEmailAddress().ConfigureAwait(false);
        var emailAddress = devUser.Result.Name + "@developermail.com";
        var userCreationInfoWithMessage = new UserCreationInfoWithMessage("testuser2", emailAddress, "myFirstName",
            "myLastName", new[] { "Company Admin" }, "testMessage");

        await Task.Delay(20000).ConfigureAwait(false);

        Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .ContentType("application/json")
            .Body(userCreationInfoWithMessage)
            .When()
            .Post($"{BaseUrl}{EndPoint}/application/{_applicationId}/inviteNewUser")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200);

        var invitedUsers = await GetInvitedUsers().ConfigureAwait(false);
        invitedUsers.Count.Should().Be(2, "No invited users were found.");

        var newInvitedUser = invitedUsers.Find(u => u.EmailId!.Equals(userCreationInfoWithMessage.eMail));
        newInvitedUser?.InvitationStatus.Should().Be(InvitationStatusId.CREATED);
        newInvitedUser?.InvitedUserRoles.Should().BeEquivalentTo(userCreationInfoWithMessage.Roles);

        await Task.Delay(20000).ConfigureAwait(false);

        var messageData = await devMailApiRequests.FetchPassword().ConfigureAwait(false);
        messageData.Should().NotBeNullOrEmpty();
    }

    // POST api/administration/invitation
    private static async Task ExecuteInvitation(string emailAddress, string userCompanyName)
    {
        var invitationData = new CompanyInvitationData("testuser", "myFirstName", "myLastName",
            emailAddress, userCompanyName);

        try
        {
            _portalUserToken =
                await new AuthFlow(PortalUserCompanyName).GetAccessToken(Secrets.PortalUserName,
                    Secrets.PortalUserPassword);

            Given()
                .DisableSslCertificateValidation()
                .Header(
                    "authorization",
                    $"Bearer {_portalUserToken}")
                .ContentType("application/json")
                .Body(invitationData)
                .When()
                .Post($"{BaseUrl}{AdminEndPoint}/invitation")
                .Then()
                .Log(ResponseLogLevel.OnError)
                .StatusCode(200);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static async Task<(string?, string?)> GetCompanyTokenAndApplicationId(string userCompanyName,
        string emailAddress,
        string currentPassword, string newPassword)
    {
        _userCompanyToken =
            await new AuthFlow(userCompanyName).UpdatePasswordAndGetAccessToken(emailAddress, currentPassword,
                newPassword);
        _applicationId = await GetFirstApplicationId().ConfigureAwait(false);

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
               storedData.ZipCode == postedData.ZipCode;
    }

    private static async Task<string> GetFirstApplicationId()
    {
        var endpoint = $"{EndPoint}/applications";
        var data = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .When()
            .Get(BaseUrl + endpoint)
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();

        var applicationIds =
            DataHandleHelper.DeserializeData<List<CompanyApplicationWithStatus>>(await data.Content.ReadAsStringAsync().ConfigureAwait(false));
        if (applicationIds.IsNullOrEmpty())
        {
            throw new Exception($"Could not get first application id from {endpoint}, response was null/empty.");
        }
        _applicationId = applicationIds![0].ApplicationId.ToString();
        return _applicationId;
    }

    private static async Task<CompanyRoleAgreementData> GetCompanyRoleAgreementData()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .ContentType("application/json")
            .When()
            .Get(
                $"{BaseUrl}{EndPoint}/companyRoleAgreementData")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .And()
            .Extract()
            .Response();
        var data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var companyRoleAgreementData = DataHandleHelper.DeserializeData<CompanyRoleAgreementData>(data);
        if (companyRoleAgreementData is null)
        {
            throw new Exception("The endpoint GetCompanyRoleAgreementData returned no result.");
        }

        return companyRoleAgreementData;
    }
}
