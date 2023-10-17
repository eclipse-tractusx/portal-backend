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

using Castle.Core.Internal;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using PasswordGenerator;
using RestAssured.Response.Logging;
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

        var bpdmLegalEntityDatas = DataHandleHelper.DeserializeData<BpdmPaginationContent>(data.Content.ReadAsStringAsync().Result);
        if (bpdmLegalEntityDatas is null)
        {
            throw new Exception($"Could not get bpn from {endpoint} should not be null.");
        }

        return bpdmLegalEntityDatas.Content.ElementAt(new Random().Next(bpdmLegalEntityDatas.Content.Count())).Bpn;
    }

    //GET /api/registration/legalEntityAddress/{bpn}
    public static CompanyDetailData GetCompanyBpdmDetailData(string bpn)
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
        var bpdmDetailData = DataHandleHelper.DeserializeData<CompanyDetailData>(data.Content.ReadAsStringAsync().Result);
        if (bpdmDetailData is null)
        {
            throw new Exception($"Could not get bpdm detail data from {endpoint} should not be null.");
        }

        return bpdmDetailData;
    }

    public static CompanyDetailData GetCompanyDetailData()
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
        var data = response.Content.ReadAsStringAsync().Result;
        var companyDetailData = DataHandleHelper.DeserializeData<CompanyDetailData>(data);
        if (companyDetailData is null)
        {
            throw new Exception("Company detail data was not found.");
        }

        return companyDetailData;
    }

    private static CompanyRoleAgreementConsents GetCompanyRolesAndConsentsForSelectedRoles(
        List<CompanyRoleId> companyRoleIds)
    {
        var companyRoleAgreementData = GetCompanyRoleAgreementData();
        var selectedCompanyRoleIds = new List<CompanyRoleId>();

        var agreementConsentStatusList = new List<AgreementConsentStatus>();

        foreach (var roleData in companyRoleAgreementData.CompanyRoleData)
        {
            if (!companyRoleIds.Contains(roleData.CompanyRoleId))
                continue;
            selectedCompanyRoleIds.Add(roleData.CompanyRoleId);
            agreementConsentStatusList.AddRange(
                roleData.AgreementIds.Select(id => new AgreementConsentStatus(id, ConsentStatusId.ACTIVE)));
        }

        return new CompanyRoleAgreementConsents(selectedCompanyRoleIds, agreementConsentStatusList);
    }

    private static void SetApplicationStatus(string applicationStatus)
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

        var status = DataHandleHelper.DeserializeData<int>(response.Content.ReadAsStringAsync().Result);
        status.Should().Be(1);
    }

    private static string GetApplicationStatus()
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
        var applicationStatus = DataHandleHelper.DeserializeData<string>(data.Content.ReadAsStringAsync().Result);
        if (applicationStatus is null)
        {
            throw new Exception($"Could not get application status from {endpoint}, response was null/empty.");
        }
        return applicationStatus;
    }

    private static List<InvitedUser> GetInvitedUsers()
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

        var invitedUsers = DataHandleHelper.DeserializeData<List<InvitedUser>>(data.Content.ReadAsStringAsync().Result);
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
            var devUser = devMailApiRequests.GenerateRandomEmailAddress();
            emailAddress = devUser.Result.Name + "@developermail.com";
        }
        catch (Exception)
        {
            try
            {
                emailAddress = "apitestuser" + tempMailApiRequests.GetDomain();
            }
            catch (Exception)
            {
                throw new Exception("MailApi for sending invitation is not available");
            }
        }
        finally
        {
            Thread.Sleep(20000);
            await ExecuteInvitation(emailAddress, userCompanyName);

            Thread.Sleep(60000);

            var currentPassword = emailAddress.Contains("developermail.com")
                ? devMailApiRequests.FetchPassword()
                : tempMailApiRequests.FetchPassword();

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

    public static CompanyDetailData SetCompanyDetailData(CompanyDetailData? testCompanyDetailData)
    {
        if (testCompanyDetailData is null)
            throw new Exception("Company detail data expected but not provided");
        var applicationStatus = GetApplicationStatus();
        if (applicationStatus != CompanyApplicationStatusId.CREATED.ToString())
            throw new Exception("Application status is not fitting to the pre-requisite");
        SetApplicationStatus(CompanyApplicationStatusId.ADD_COMPANY_DATA.ToString());
        var companyDetailData = GetCompanyDetailData();

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
        var storedCompanyDetailData = GetCompanyDetailData();
        if (!VerifyCompanyDetailDataStorage(storedCompanyDetailData, newCompanyDetailData))
            throw new Exception($"Company detail data was not stored correctly");
        return storedCompanyDetailData;
    }

    public static void UpdateCompanyDetailData(CompanyDetailData updateCompanyDetailData)
    {
        var actualStatus = GetApplicationStatus();
        if (actualStatus != CompanyApplicationStatusId.ADD_COMPANY_DATA.ToString() &&
            actualStatus != CompanyApplicationStatusId.SUBMITTED.ToString())
        {
            var companyDetailData = GetCompanyDetailData();

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
            var storedCompanyDetailData = GetCompanyDetailData();
            if (!VerifyCompanyDetailDataStorage(storedCompanyDetailData, newCompanyDetailData))
                throw new Exception($"Company detail data was not updated correctly");
        }
        else
        {
            throw new Exception($"Application status is not fitting to the pre-requisite");
        }
    }

    // POST /api/registration/application/{applicationId}/companyRoleAgreementConsents

    public static string SubmitCompanyRoleConsentToAgreements(List<CompanyRoleId> companyRoles)
    {
        if (GetApplicationStatus() != CompanyApplicationStatusId.INVITE_USER.ToString())
            throw new Exception("Application status is not fitting to the pre-requisite");
        if (companyRoles.IsNullOrEmpty())
            throw new Exception("No company roles were found");
        var companyRoleAgreementConsents = GetCompanyRolesAndConsentsForSelectedRoles(companyRoles);
        var body = DataHandleHelper.SerializeData(companyRoleAgreementConsents);

        SetApplicationStatus(CompanyApplicationStatusId.SELECT_COMPANY_ROLE.ToString());
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

        var result = DataHandleHelper.DeserializeData<int>(response.Content.ReadAsStringAsync().Result).ToString();
        return result;
    }

    // POST /api/registration/application/{applicationId}/documentType/{documentTypeId}/documents

    public static int UploadDocument_WithEmptyTitle(string? documentTypeId,
        string? documentName)
    {
        if (documentTypeId is null || documentName is null)
            throw new Exception("No document type id or name provided but expected");
        if (GetApplicationStatus() != CompanyApplicationStatusId.UPLOAD_DOCUMENTS.ToString())
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
            .StatusCode(200)
            .Extract()
            .Response();

        var result = DataHandleHelper.DeserializeData<int>(response.Content.ReadAsStringAsync().Result);

        if (result == 1)
            SetApplicationStatus(CompanyApplicationStatusId.VERIFY.ToString());

        return result;
    }

    // POST /api/registration/application/{applicationId}/submitRegistration

    //[Fact]
    public static bool SubmitRegistration()
    {
        if (GetApplicationStatus() != CompanyApplicationStatusId.VERIFY.ToString())
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

        var status = DataHandleHelper.DeserializeData<bool>(data.Content.ReadAsStringAsync().Result);
        return status;
    }

    // GET: api/administration/registration/applications?companyName={companyName}

    public static CompanyApplicationDetails? GetApplicationDetails(string userCompanyName)
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

        var data = DataHandleHelper.DeserializeData<Pagination.Response<CompanyApplicationDetails>>(response.Content
            .ReadAsStringAsync()
            .Result);
        if (data is null)
        {
            throw new Exception($"Could not application details of first application from {endpoint}, response was null/empty.");
        }
        return data.Content.First();
    }

    // GET: api/administration/registration/application/{applicationId}/companyDetailsWithAddress
    public static CompanyDetailData? GetCompanyWithAddress()
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

        var data = DataHandleHelper.DeserializeData<CompanyDetailData>(response.Content.ReadAsStringAsync().Result);
        return data;
    }

    public static void InviteNewUser()
    {
        var devMailApiRequests = new DevMailApiRequests();
        var devUser = devMailApiRequests.GenerateRandomEmailAddress();
        var emailAddress = devUser.Result.Name + "@developermail.com";
        var userCreationInfoWithMessage = new UserCreationInfoWithMessage("testuser2", emailAddress, "myFirstName",
            "myLastName", new[] { "Company Admin" }, "testMessage");

        Thread.Sleep(20000);

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

        var invitedUsers = GetInvitedUsers();
        invitedUsers.Count.Should().Be(2, "No invited users were found.");

        var newInvitedUser = invitedUsers.Find(u => u.EmailId!.Equals(userCreationInfoWithMessage.eMail));
        newInvitedUser?.InvitationStatus.Should().Be(InvitationStatusId.CREATED);
        newInvitedUser?.InvitedUserRoles.Should().BeEquivalentTo(userCreationInfoWithMessage.Roles);

        Thread.Sleep(20000);

        var messageData = devMailApiRequests.FetchPassword();
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
        _applicationId = GetFirstApplicationId();

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

    private static string GetFirstApplicationId()
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
            DataHandleHelper.DeserializeData<List<CompanyApplicationWithStatus>>(data.Content.ReadAsStringAsync().Result);
        if (applicationIds.IsNullOrEmpty())
        {
            throw new Exception($"Could not get first application id from {endpoint}, response was null/empty.");
        }
        _applicationId = applicationIds![0].ApplicationId.ToString();
        return _applicationId;
    }

    private static CompanyRoleAgreementData GetCompanyRoleAgreementData()
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
        var data = response.Content.ReadAsStringAsync().Result;
        var companyRoleAgreementData = DataHandleHelper.DeserializeData<CompanyRoleAgreementData>(data);
        if (companyRoleAgreementData is null)
        {
            throw new Exception("The endpoint GetCompanyRoleAgreementData returned no result.");
        }

        return companyRoleAgreementData;
    }
}
