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
using MimeKit;
using System.Text;
using System.Web;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

public class DevMailApiRequests
{
    private const string MailServiceBaseUrl = "https://developermail.com/api/v1";

    private DevMailboxData? _devMailboxData;

    public async Task<string?> FetchPassword()
    {
        var emails = await CheckMailbox();
        var index = emails.Result.FindIndex(item => item.Value.Contains("Password required"));
        string? password = null;

        if (index < 0)
            throw new Exception("Authentication flow failed: email with password could not be found.");
        var byteArray = Encoding.ASCII.GetBytes(emails.Result[index].Value);
        using var stream = new MemoryStream(byteArray);
        var message = MimeMessage.Load(stream);

        var doc = new HtmlDocument();
        doc.LoadHtml(message.HtmlBody);

        var pNodes = doc.DocumentNode.SelectNodes("//p");

        //search for password in the first paragraph after the paragraph with "Below you can find you password"
        for (var i = 0; i < pNodes.Count - 1; i++)
        {
            if (!pNodes[i].InnerText.Trim().Contains("Below you can find your password"))
                continue;
            password = HttpUtility.HtmlDecode(pNodes[i + 1].InnerText.Trim());
            break;
        }

        DeleteMailbox();

        return password;
    }

    public async Task<DevMailboxData> GenerateRandomEmailAddress()
    {
        const string Endpoint = "/mailbox";
        var data = Given()
            .DisableSslCertificateValidation()
            .When()
            .Put(MailServiceBaseUrl + Endpoint)
            .Then()
            .And()
            .StatusCode(200)
            .Extract()
            .Response();
        _devMailboxData = DataHandleHelper.DeserializeData<DevMailboxData>(await data.Content.ReadAsStringAsync());
        if (_devMailboxData is null)
        {
            throw new Exception($"Could not get mail address from {Endpoint}, response was null/empty.");
        }
        return _devMailboxData;
    }

    private async Task<DevMailboxContent> CheckMailbox()
    {
        var messageIds = GetMessageIds();
        var endpoint = $"/mailbox/{_devMailboxData?.Result.Name}/messages";
        var data = Given()
            .DisableSslCertificateValidation()
            .Header(
                "X-MailboxToken",
                $"{_devMailboxData?.Result.Token}")
            .Body(messageIds)
            .When()
            .Post(MailServiceBaseUrl + endpoint)
            .Then()
            .And()
            .StatusCode(200)
            .Extract()
            .Response();

        var emails = DataHandleHelper.DeserializeData<DevMailboxContent>(await data.Content.ReadAsStringAsync());
        if (emails is null)
        {
            throw new Exception($"Could not mails from {endpoint}, response data was null.");
        }
        return emails;
    }

    private void DeleteMailbox()
    {
        Given()
            .DisableSslCertificateValidation()
            .Header(
                "X-MailboxToken",
                $"{_devMailboxData?.Result.Token}")
            .When()
            .Delete($"{MailServiceBaseUrl}/mailbox/{_devMailboxData?.Result.Name}")
            .Then()
            .And()
            .StatusCode(200)
            .Extract()
            .Body("$.result");
    }

    private async Task<string[]> GetMessageIds()
    {
        var endpoint = $"/mailbox/{_devMailboxData?.Result.Name}";
        var data = Given()
            .DisableSslCertificateValidation()
            .Header(
                "X-MailboxToken",
                $"{_devMailboxData?.Result.Token}")
            .When()
            .Get(MailServiceBaseUrl + endpoint)
            .Then()
            .And()
            .StatusCode(200)
            .Extract()
            .Response();

        var messageIds = DataHandleHelper.DeserializeData<DevMailboxMessageIds>(await data.Content.ReadAsStringAsync());
        if (messageIds is null)
        {
            throw new Exception($"Could not messageIds from {endpoint}, response data was null.");
        }
        return messageIds.Result;
    }
}
