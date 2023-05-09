﻿using System.Net.Http.Headers;
using AutoFixture;
using Microsoft.Extensions.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Xunit;
using static RestAssured.Dsl;

namespace Registration.Service.Tests.RestAssured.RegistrationEndpointTests;

[TestCaseOrderer("Notifications.Service.Tests.RestAssured.AlphabeticalOrderer",
    "Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests")]
public class RegistrationEndpointTestsHappyPathRegistrationWithoutBpn
{
    private readonly string _baseUrl = "https://portal-backend.dev.demo.catena-x.net";
    private readonly string _endPoint = "/api/registration";
    private readonly string _companyToken;
    private static string _applicationId;

    private readonly IFixture _fixture;
    // private readonly string pdfFileName = @"TestDocument.pdf";

    private readonly string _adminEndPoint = "/api/administration";
    private readonly string _operatorToken;
    private static string _companyName = "Test-Catena-X";
    private readonly HttpClient _httpClient;
    private static string[] _userEmailAddress;

    public RegistrationEndpointTestsHappyPathRegistrationWithoutBpn()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Secrets>()
            .Build();
        _companyToken = configuration.GetValue<string>("Secrets:CompanyToken");
        _operatorToken = configuration.GetValue<string>("Secrets:OperatorToken");
        _fixture = new Fixture();
        _httpClient = new () { BaseAddress = new Uri(_baseUrl) };
        // CreateFilesToUpload();
    }

    #region Happy Path - new registration without BPN

    // POST api/administration/invitation

    // [Fact]
    // public void Test1_ExecuteInvitation_ReturnsExpectedResult()
    // {
    //     var emailAddress = GenerateRandomEmailAddress();
    //     CompanyInvitationData invitationData = new CompanyInvitationData("testuser", "myFirstName", "myLastName",
    //         emailAddress, "Test-Catena-X");
    //     Given()
    //         .RelaxedHttpsValidation()
    //         .Header(
    //             "authorization",
    //             $"Bearer {_operatorToken}")
    //         .ContentType("application/json")
    //         .Body(invitationData)
    //         .When()
    //         .Post($"{_baseUrl}{_adminEndPoint}/invitation")
    //         .Then()
    //         .StatusCode(200);
    //     var messageData = FetchPassword();
    //     AuthenticationFlow();
    // }


    // POST /api/registration/application/{applicationId}/companyDetailsWithAddress

    [Fact]
    public void Test2_SetCompanyDetailData_ReturnsExpectedResult()
    {
        CompanyDetailData companyDetailData = GetCompanyDetailData();
        string companyId = companyDetailData.CompanyId.ToString();
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .ContentType("application/json")
            .When()
            .Body("{\"companyId\":" + "\"" + companyId + "\"" +
                  ",\"name\":\"TestAutomationReg 01\",\"city\":\"München\",\"streetName\":\"Streetfgh\",\"countryAlpha2Code\":\"DE\",\"bpn\":null, \"shortName\":\"TestAutomationReg 01\",\"region\":null,\"streetAdditional\":null,\"streetNumber\":null,\"zipCode\":null,\"countryDe\":\"Deutschland\",\"uniqueIds\":[{\"type\":\"VAT_ID\",\"value\":\"DE123456788\"}]}")
            //.Body(companyDetailData)
            .Post($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .StatusCode(200);
    }

    // POST /api/registration/application/{applicationId}/companyRoleAgreementConsents

    [Fact]
    public void Test3_SubmitCompanyRoleConsentToAgreements_ReturnsExpectedResult()
    {
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .ContentType("application/json")
            .Body(
                "{\"companyRoles\": [\"ACTIVE_PARTICIPANT\", \"APP_PROVIDER\", \"SERVICE_PROVIDER\"], \"agreements\": [{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1011\",\"consentStatus\":\"ACTIVE\"},{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1010\",\"consentStatus\":\"ACTIVE\"},{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1090\",\"consentStatus\":\"ACTIVE\"},{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1013\",\"consentStatus\":\"ACTIVE\"},{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1017\",\"consentStatus\":\"ACTIVE\"}]}")
            .When()
            .Post($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyRoleAgreementConsents")
            .Then()
            .StatusCode(200);
    }
    // POST /api/registration/application/{applicationId}/documentType/{documentTypeId}/documents

    [Fact]
    public void Test4_UploadDocument_WithEmptyTitle_ReturnsExpectedResult()
    {
        _applicationId = GetFirstApplicationId();
        string documentTypeId = "COMMERCIAL_REGISTER_EXTRACT";
        var formFile = FormFileHelper.GetFormFile("this is just a test", "testfile.pdf", "application/pdf");
        // File.WriteAllText("testfile.pdf", "Some Text");
        // new FormFile()
        // await CreateFilesToUpload();
        // pdfFileName.Close();
        var formData = new[]
        {
            new KeyValuePair<string, Stream>("document", formFile.OpenReadStream()),
        };
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .ContentType("multipart/form-data")
            // .Body($"{{\"document\": {new MultipartFormDataContent()}")
            // .MultiPart("testfile.pdf")
            // .MultiPart(new FileInfo("testfile.pdf"), "testfile", MediaTypeHeaderValue.Parse("multipart/form-data"))
            // .Body($"{{\"document\": {formFile.OpenReadStream()}")
            // .Body($"\"document\":{formFile.OpenReadStream()}")
            // .MultiPart(new FileInfo("testfile.pdf"))
            // .FormData(formData)
            .When()
            .Post($"{_baseUrl}{_endPoint}/application/{_applicationId}/documentType/{documentTypeId}/documents")
            .Then()
            .Body("")
            .StatusCode(200);
    }

    // [Fact]
    // public async Task Test4_UploadDocument_WithEmptyTitle_ReturnsExpectedResult_HttpClient()
    // {
    //     await File.WriteAllTextAsync("testfile.pdf", "Some Text");
    //     _applicationId = GetFirstApplicationId();
    //     string documentTypeId = "COMMERCIAL_REGISTER_EXTRACT";
    //     await using var stream = File.OpenRead("./testfile.pdf");
    //     using var formContent = new MultipartFormDataContent
    //     {
    //         { new StreamContent(stream), "document", "testfile.pdf" }
    //     };
    //
    //     var requestUri =
    //         new Uri($"{_baseUrl}{_endPoint}/application/{_applicationId}/documentType/{documentTypeId}/documents");
    //     var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
    //     requestMessage.Headers.Add("authorization", $"Bearer {_companyToken}");
    //     requestMessage.Content = formContent;
    //
    //     var response = await _httpClient.SendAsync(requestMessage);
    //     if (!response.IsSuccessStatusCode)
    //     {
    //         var errorMessage = await response.Content.ReadAsStringAsync();
    //         throw new Exception(errorMessage);
    //     }
    // }

    [Fact]
    public async Task Test4_UploadDocument_WithEmptyTitle_ReturnsExpectedResult_HttpClient()
    {
        File.WriteAllText("testfile.pdf", "Some Text");
        _applicationId = GetFirstApplicationId();
        string documentTypeId = "COMMERCIAL_REGISTER_EXTRACT";
        var formFile = FormFileHelper.GetFormFile("this is just a test", "testfile.pdf", "application/pdf");
        var formContent = new MultipartFormDataContent();
        var fileContent = new StreamContent(formFile.OpenReadStream());
        formContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "document",
            FileName = "testfile.pdf",
        };
        formContent.Add(fileContent);

        var requestUri =
            new Uri($"{_baseUrl}{_endPoint}/application/{_applicationId}/documentType/{documentTypeId}/documents");
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
        requestMessage.Headers.Add("authorization", $"Bearer {_companyToken}");
        requestMessage.Content = formContent;

        var response = await _httpClient.SendAsync(requestMessage);
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception(errorMessage);
        }
    }

    // [Fact]
    // public void DownloadDocument_WithEmptyTitle_ReturnsExpectedResult()
    // {
    //     _applicationId = GetFirstApplicationId();
    //     string documentTypeId = "COMMERCIAL_REGISTER_EXTRACT";
    //     Given()
    //         .RelaxedHttpsValidation()
    //         .Header(
    //             "authorization",
    //             $"Bearer {_companyToken}")
    //         .When()
    //         .Get($"{_baseUrl}{_endPoint}/application/{_applicationId}/documentType/{documentTypeId}/documents")
    //         .Then()
    //         .Body("")
    //         .StatusCode(200);
    // }
    //
    // [Fact]
    // public async Task DownloadDocument_WithEmptyTitle_ReturnsExpectedResult_HttpClient()
    // {
    //     var expectedStatusCode = HttpStatusCode.OK;
    //     _applicationId = GetFirstApplicationId();
    //     string documentTypeId = "COMMERCIAL_REGISTER_EXTRACT";
    //     var request = new HttpRequestMessage(HttpMethod.Get,
    //         $"{_baseUrl}{_endPoint}/application/{_applicationId}/companyDetailsWithAddress");
    //     request.Headers.Add("authorization", $"Bearer {_companyToken}");
    //     var response = await _httpClient.SendAsync(request);
    //     var content = await response.Content.ReadAsStringAsync();
    // }

    private async Task CreateFilesToUpload()
    {
        await File.WriteAllLinesAsync("testfile.pdf", new string[] { "Some text" });
    }


    // POST /api/registration/application/{applicationId}/submitRegistration
    // GET: api/administration/registration/applications?companyName={companyName}

    [Fact]
    public void Test6_GetApplicationDetails_ReturnsExpectedResult()
    {
        // Given
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .When()
            .Get(
                $"{_baseUrl}{_adminEndPoint}/registration/applications?companyName={_companyName}/?page=0&size=10&sorting=DateDesc")
            .Then()
            .StatusCode(200);
    }

    // GET: api/administration/registration/application/{applicationId}/companyDetailsWithAddress
    [Fact]
    public void Test7_GetCompanyWithAddress_ReturnsExpectedResult()
    {
        // Given
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .When()
            .Get($"{_baseUrl}{_adminEndPoint}/registration/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .StatusCode(200);
    }

    #endregion
    
    private string GetFirstApplicationId()
    {
        var applicationIDs = (List<CompanyApplicationData>)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/applications")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(List<CompanyApplicationData>));

        return applicationIDs[0].ApplicationId.ToString();
    }

    private CompanyDetailData GetCompanyDetailData()
    {
        _applicationId = GetFirstApplicationId();
        // Given
        CompanyDetailData companyDetailData = (CompanyDetailData)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .And()
            .StatusCode(200)
            .Extract().As(typeof(CompanyDetailData));

        return companyDetailData;
    }

    private string GenerateRandomEmailAddress()
    {
        // Given
        var emailAddress = (string[])Given()
            .RelaxedHttpsValidation()
            .When()
            .Get("https://www.1secmail.com/api/v1/?action=genRandomMailbox&count=1")
            .Then()
            .And()
            .StatusCode(200)
            .Extract().As(typeof(string[]));
        _userEmailAddress = emailAddress[0].Split("@");
        return emailAddress[0];
    }

    private MailboxData[] CheckMailBox()
    {
        // Given
        var emails = (MailboxData[])Given()
            .RelaxedHttpsValidation()
            .When()
            .Get(
                $"https://www.1secmail.com/api/v1/?action=getMessages&login={_userEmailAddress[0]}&domain={_userEmailAddress[1]}")
            .Then()
            .And()
            .StatusCode(200)
            .Extract().As(typeof(MailboxData[]));
        return emails;
    }

    [Fact]
    private EmailMessageData? FetchPassword()
    {
        // Given
        var emails = CheckMailBox();
        if (emails.Length != 0)
        {
            var passwordMail = emails[0]?.Id;
            var messageData = (EmailMessageData)Given()
                .RelaxedHttpsValidation()
                .When()
                .Get(
                    $"https://www.1secmail.com/api/v1/?action=readMessage&login={_userEmailAddress[0]}&domain={_userEmailAddress[1]}&id={passwordMail}")
                .Then()
                .And()
                .StatusCode(200)
                .Extract().As(typeof(EmailMessageData));
            return messageData;
        }

        return null;
    }

    private void AuthenticationFlow()
    {
    }
}