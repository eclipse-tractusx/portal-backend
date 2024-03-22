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
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using RestAssured;
using RestAssured.Response.Logging;
using Xunit;
using Xunit.Abstractions;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestCollectionOrderer(
    ordererTypeName: "Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests.DisplayNameOrderer",
    ordererAssemblyName: "EndToEnd.Tests")]

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

using static Dsl;

[Trait("Category", "Portal")]
[Collection("Portal")]
public class CreateNewTestAppScenario : EndToEndTestBase
{
    private static readonly string BaseUrl = TestResources.BasePortalBackendUrl;
    private static readonly string EndPoint = "/api/apps";
    private static readonly string AdminEndPoint = "/api/administration";
    private readonly string _portalUserCompanyName = TestResources.PortalUserCompanyName;
    private string? _portalUserToken;

    private static readonly Secrets Secrets = new();

    public CreateNewTestAppScenario(ITestOutputHelper output) : base(output)
    {
    }

    [Theory(DisplayName = "App Release Request Creation")]
    [MemberData(nameof(GetDataEntries))]
    public async Task CreateAppRequest(TestDataModelCreateApp testEntry)
    {
        _portalUserToken = await new AuthFlow(_portalUserCompanyName).GetAccessToken(Secrets.PortalUserName,
            Secrets.PortalUserPassword);
        var newAppRequest = await GetAppRequestModel(testEntry.AppRequestModel);

        var appId = await CreateNewAppInStatusCreated(newAppRequest);
        appId.Should().NotBeEmpty();

        var appDetailWithStatus = await GetAppDetailWithStatus(appId);
        ValidateStorageOfAppDetailData(newAppRequest, appDetailWithStatus).Should().BeTrue();

        var agreementData = await GetAgreementData();
        var agreementDataIds = agreementData.Select(t => t.AgreementId).ToList();

        var signedConsentStatusData = await SignConsentAgreement(appId, agreementDataIds);

        signedConsentStatusData.Should().AllSatisfy(x =>
        {
            x.ConsentStatus.Should().Be(ConsentStatusId.ACTIVE);
            agreementDataIds.Should().Contain(x.AgreementId);
        });

        var uploadDocumentResult = await UploadAppDocumentOrImage(appId, DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS, testEntry.DocumentName);
        uploadDocumentResult.Should().BeEmpty();

        var uploadImageResult = await UploadAppDocumentOrImage(appId, DocumentTypeId.APP_IMAGE, testEntry.ImageName);
        uploadImageResult.Should().BeEmpty();

        var uploadLeadImageResult = await UploadAppDocumentOrImage(appId, DocumentTypeId.APP_LEADIMAGE, testEntry.ImageName);
        uploadLeadImageResult.Should().BeEmpty();

        var appRoleDatas = await AddRoleAndRoleDescriptionForApp(appId, testEntry.AppUserRoles);
        var appRoleDataNames = appRoleDatas.Select(data => data.RoleName);

        testEntry.AppUserRoles.Should().AllSatisfy(t =>
        {
            appRoleDataNames.Should().Contain(t.Role);
        });

        var technicalUserProfiles = await GetTechnicalUserProfiles();

        var userRoleIds = technicalUserProfiles.Select(t => t.UserRoleId).ToList();

        CreateAndUpdateTechnicalUserProfile(appId, userRoleIds);

        SubmitApp(appId);
    }

    public static IEnumerable<object[]> GetDataEntries
    {
        get
        {
            var testDataEntries =
                TestDataHelper.GetTestDataForCreateApp("TestDataCreateApp.json");
            foreach (var t in testDataEntries)
            {
                yield return new object[] { t };
            }
        }
    }

    private static bool ValidateStorageOfAppDetailData(AppRequestModel newAppRequest,
        AppProviderResponse? appDetailWithStatus)
    {
        if (appDetailWithStatus is null)
        {
            return false;
        }
        return newAppRequest.Title == appDetailWithStatus.Title &&
               newAppRequest.Provider == appDetailWithStatus.Provider &&
               newAppRequest.SalesManagerId == appDetailWithStatus.SalesManagerId &&
               newAppRequest.UseCaseIds.SequenceEqual(appDetailWithStatus.UseCase.Select(t => t.Id)) &&
               newAppRequest.Descriptions.SequenceEqual(appDetailWithStatus.Descriptions) &&
               newAppRequest.SupportedLanguageCodes.SequenceEqual(appDetailWithStatus.SupportedLanguageCodes) &&
               newAppRequest.Price == appDetailWithStatus.Price &&
               newAppRequest.PrivacyPolicies.SequenceEqual(appDetailWithStatus.PrivacyPolicies) &&
               newAppRequest.ProviderUri == appDetailWithStatus.ProviderUri &&
               newAppRequest.ContactEmail == appDetailWithStatus.ContactEmail &&
               newAppRequest.ContactNumber == appDetailWithStatus.ContactNumber;
    }

    #region Scenario functions

    //POST: /api/apps/appreleaseprocess/createapp
    private async Task<string> CreateNewAppInStatusCreated(AppRequestModel newAppRequest)
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Body(DataHandleHelper.SerializeData(newAppRequest))
            .Post($"{BaseUrl}{EndPoint}/appreleaseprocess/createapp")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(201)
            .Extract()
            .Response();

        return (await response.Content.ReadAsStringAsync()).Replace("\"", "");
    }

    private async Task<AppRequestModel> GetAppRequestModel(AppRequestModel? testData)
    {
        if (testData is null)
        {
            throw new Exception("No app request model in test data provided. Please check.");
        }
        var privacyPolicies = await GetAvailablePrivacyPolicies();
        var languageTags = await GetAppLanguageTags();
        var useCases = await GetUseCases();

        if (languageTags.IsNullOrEmpty() || useCases.IsNullOrEmpty())
        {
            throw new Exception(
                "Cannot create app request model as language tags or use cases are empty.");
        }

        var privacyPolicyIds = new List<PrivacyPolicyId> { privacyPolicies.PrivacyPolicies.First() };

        var supportedLanguageCodes = languageTags.Select(l => l.LanguageShortName);

        var useCaseIds = new List<Guid> { useCases.First().Id };

        var appRequestModel = testData with
        {
            SalesManagerId = null, //explicitly set to null
            PrivacyPolicies = privacyPolicyIds.ToArray(),
            SupportedLanguageCodes = supportedLanguageCodes.ToArray(),
            UseCaseIds = useCaseIds.ToArray()
        };
        return appRequestModel;
    }

    //GET: /api/apps/appreleaseprocess/{appId}/appStatus
    private async Task<AppProviderResponse?> GetAppDetailWithStatus(string appId)
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/appreleaseprocess/{appId}/appStatus")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();
        return DataHandleHelper.DeserializeData<AppProviderResponse>(await response.Content.ReadAsStringAsync());
    }

    //POST: /api/apps/appreleaseprocess/consent/{appId}/agreementConsents - sign the agreements

    private async Task<List<ConsentStatusData>> SignConsentAgreement(string appId, List<Guid> consentAgreementIds)
    {
        var agreementConsentStatuses = consentAgreementIds
            .Select(agr => new AgreementConsentStatus(agr, ConsentStatusId.ACTIVE)).ToList();
        var offerAgreementConsent = new OfferAgreementConsent(agreementConsentStatuses);

        var endpoint = $"{EndPoint}/appreleaseprocess/consent/{appId}/agreementConsents";
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Body(DataHandleHelper.SerializeData(offerAgreementConsent))
            .Post(BaseUrl + endpoint)
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();
        var signConsentAgreement = DataHandleHelper.DeserializeData<List<ConsentStatusData>>(await response.Content.ReadAsStringAsync());
        if (signConsentAgreement == null)
            throw new Exception($"Could not fetch consent status data from {endpoint}");
        return signConsentAgreement;
    }

    //PUT: /api/apps/AppReleaseProcess/updateappdoc/{appId}/documentType/{documentType}/documents

    private Task<string> UploadAppDocumentOrImage(string appId, DocumentTypeId documentType, string documentOrImageName)
    {
        documentOrImageName.Should().NotBeNullOrEmpty($"No document with type {documentType} provided, but required for tests case.");

        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .ContentType("multipart/form-data")
            .MultiPart(new FileInfo(documentOrImageName), "document")
            .Put($"{BaseUrl}{EndPoint}/AppReleaseProcess/updateappdoc/{appId}/documentType/{documentType}/documents")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(204)
            .Extract()
            .Response();
        return response.Content.ReadAsStringAsync();
    }

    //POST: /api/apps/appreleaseprocess/{appId}/role

    private async Task<List<AppRoleData>> AddRoleAndRoleDescriptionForApp(string appId, List<AppUserRole> appUserRoles)
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Body(DataHandleHelper.SerializeData(appUserRoles))
            .Post($"{BaseUrl}{EndPoint}/appreleaseprocess/{appId}/role")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();
        return DataHandleHelper.DeserializeData<List<AppRoleData>>(await response.Content.ReadAsStringAsync()) ?? new List<AppRoleData>();
    }

    //PUT: /api/apps/appreleaseprocess/{appId}/technical-user-profiles - select the technical user profile - important keep the uuid of the technical user inside the request body empty

    private void CreateAndUpdateTechnicalUserProfile(string appId, List<Guid> userRoleIds)
    {
        var technicalUserProfileData =
            new List<TechnicalUserProfileData> { new TechnicalUserProfileData(null, userRoleIds) };

        Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Body(DataHandleHelper.SerializeData(technicalUserProfileData))
            .Put($"{BaseUrl}{EndPoint}/appreleaseprocess/{appId}/technical-user-profiles")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(204)
            .Extract()
            .Response();
    }

    //PUT: /api/apps/appreleaseprocess/{appId}/submit - submit the app
    private void SubmitApp(string appId)
    {
        Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Put($"{BaseUrl}{EndPoint}/appreleaseprocess/{appId}/submit")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(204)
            .Extract()
            .Response();
    }

    #endregion

    #region Fetch the relevant metadata for the test execution run

    //GET: /api/apps/appreleaseprocess/privacyPolicies - get a list of available privacyPolicies (needed for the POST call /createapp)

    private async Task<PrivacyPolicyData> GetAvailablePrivacyPolicies()
    {
        var endpoint = $"{EndPoint}/appreleaseprocess/privacyPolicies";
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
        var availablePrivacyPolicies = DataHandleHelper.DeserializeData<PrivacyPolicyData>(await response.Content.ReadAsStringAsync());
        if (availablePrivacyPolicies is null)
        {
            throw new Exception($"Cannot create app request model as privacy policies cannot be fetched from {endpoint}.");
        }
        return availablePrivacyPolicies;
    }

    //GET: /api/administration/staticdata/languagetags - get a list of available language tags (needed for the POST call /createapp)

    private async Task<List<LanguageData>> GetAppLanguageTags()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get($"{BaseUrl}{AdminEndPoint}/staticdata/languagetags")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();

        return DataHandleHelper.DeserializeData<List<LanguageData>>(await response.Content.ReadAsStringAsync()) ?? new List<LanguageData>();
    }

    //GET: /api/administration/staticdata/usecases - get a list of available language tags (needed for the POST call /createapp)
    private async Task<List<UseCaseData>> GetUseCases()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get($"{BaseUrl}{AdminEndPoint}/staticdata/usecases")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();

        return DataHandleHelper.DeserializeData<List<UseCaseData>>(await response.Content.ReadAsStringAsync()) ?? new List<UseCaseData>();
    }

    //GET: /api/apps/appreleaseprocess/agreementData - get a list of agreements (need to get signed as part of the POST /agreementConsents call)

    private async Task<List<AgreementDocumentData>> GetAgreementData()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/appreleaseprocess/agreementData")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();
        return DataHandleHelper.DeserializeData<List<AgreementDocumentData>>(await response.Content.ReadAsStringAsync()) ?? new List<AgreementDocumentData>();
    }

    //GET: api/administration/serviceaccount/user/roles - get a list of technical user profiles (needed for the POST call /technical-user-profiles)
    private async Task<List<UserRoleWithDescription>> GetTechnicalUserProfiles()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get($"{BaseUrl}{AdminEndPoint}/serviceaccount/user/roles")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();

        return DataHandleHelper.DeserializeData<List<UserRoleWithDescription>>(await response.Content.ReadAsStringAsync()) ?? new List<UserRoleWithDescription>();
    }

    #endregion
}
