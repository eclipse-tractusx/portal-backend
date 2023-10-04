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

using HtmlAgilityPack;
using Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

public class TempMailApiRequests
{
    private const string BaseUrl = "https://api.apilayer.com";
    private const string EndPoint = "/temp_mail";
    private const string ApiTestUsername = "apitestuser";

    private readonly Secrets _secrets = new();

    public string? FetchPassword()
    {
        var passwordMessage = GetPasswordMessage();
        string? password = null;

        if (passwordMessage == null)
            throw new Exception("Authentication flow failed: email with password could not be found.");

        var doc = new HtmlDocument();
        doc.LoadHtml(passwordMessage.MailHtml);

        var pNodes = doc.DocumentNode.SelectNodes("//p");

        //search for password in the first paragraph after the paragraph with "Below you can find you password"
        for (var i = 0; i < pNodes.Count - 1; i++)
        {
            if (!pNodes[i].InnerText.Trim().Contains("Below you can find your password"))
                continue;
            password = HttpUtility.HtmlDecode(pNodes[i + 1].InnerText.Trim());
            break;
        }

        DeletePasswordMessage(passwordMessage.MailId);
        return password;
    }

    private TempMailMessageData GetPasswordMessage()
    {
        var hashedEmailAddress = CreateMd5();
        var endpoint = $"{EndPoint}/mail/id/{hashedEmailAddress}";
        var data = Given()
            .DisableSslCertificateValidation()
            .Header(
                "apikey",
                $"{_secrets.TempMailApiKey}")
            .When()
            .Get(BaseUrl + endpoint)
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();
        var messages =
            DataHandleHelper.DeserializeData<List<TempMailMessageData>>(data.Content.ReadAsStringAsync().Result);
        if (messages is null)
        {
            throw new Exception($"Could not get password message from {endpoint}, response was null/empty.");
        }
        var passwordMessage = messages.First(item => item.MailSubject.Contains("Password required"));
        return passwordMessage;
    }

    public string GetDomain()
    {
        var endpoint = $"{EndPoint}/domains";
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "apikey",
                $"{_secrets.TempMailApiKey}")
            .When()
            .Get(BaseUrl + endpoint)
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();

        var data = DataHandleHelper.DeserializeData<string[]>(response.Content.ReadAsStringAsync().Result);
        if (data is null || data.Length < 1)
        {
            throw new Exception($"Could not get domain from {endpoint}, response data was null.");
        }
        return data![0];
    }

    //https://stackoverflow.com/questions/11454004/calculate-a-md5-hash-from-a-string
    private string CreateMd5()
    {
        var emailAddress = ApiTestUsername + GetDomain();
        using (var md5 = MD5.Create())
        {
            var inputBytes = Encoding.ASCII.GetBytes(emailAddress);
            var hashBytes = md5.ComputeHash(inputBytes);

            return Convert.ToHexString(hashBytes);
        }
    }

    private void DeletePasswordMessage(string mailId)
    {
        Given()
            .DisableSslCertificateValidation()
            .Header(
                "apikey",
                $"{_secrets.TempMailApiKey}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/delete/id/{mailId}")
            .Then()
            .StatusCode(200);
    }
}
