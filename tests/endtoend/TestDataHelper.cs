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
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

public class TestDataHelper
{
    private static string GetTestDataDirectory()
    {
        var projectRoot = Directory.GetParent(TestResources.GetSourceFilePathName())?.FullName;
        if (projectRoot is null)
        {
            throw new Exception("Could not determine project root directory");
        }
        return projectRoot + Path.DirectorySeparatorChar + "TestData";
    }

    public static List<string[]> GetTestDataForServiceAccountCUDScenarios(string fileName)
    {
        var filePath = Path.Combine(GetTestDataDirectory(),
            "ServiceAccountsCUDScenarios" + Path.DirectorySeparatorChar + fileName);

        var jsonData = File.ReadAllText(filePath);
        var testData = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonData);
        if (testData.IsNullOrEmpty())
            throw new Exception("Incorrect format of test data for service account scenarios");
        var testDataSet = FetchTestData(testData!);
        return testDataSet;
    }

    public static List<TestDataModelCreateApp> GetTestDataForCreateApp(string fileName)
    {
        var filePath = Path.Combine(GetTestDataDirectory(), "CreateAppScenario" + Path.DirectorySeparatorChar + fileName);

        var jsonData = File.ReadAllText(filePath);
        var testData = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonData);
        if (testData.IsNullOrEmpty())
            throw new Exception("Incorrect format of test data for service account scenarios");
        var testDataSet = FetchTestDataForCreateApp(testData!);
        return testDataSet;
    }

    private static List<TestDataModelCreateApp> FetchTestDataForCreateApp(List<Dictionary<string, object>> testData)
    {
        var testDataSet = new List<TestDataModelCreateApp>();
        foreach (var data in testData)
        {
            List<AppUserRole>? appUserRoles = null;
            AppRequestModel? appRequestModel = null;
            string? documentName = null;
            string? imageName = null;
            foreach (var pair in data)
            {
                var jsonString = pair.Value.ToString();
                switch (pair.Key)
                {
                    case "requestModel":
                        appRequestModel = jsonString is null ? null : DataHandleHelper.DeserializeData<AppRequestModel>(jsonString);
                        break;
                    case "appUserRoles":
                        appUserRoles = DataHandleHelper.DeserializeData<List<AppUserRole>>(jsonString ?? "[]");
                        break;
                    case "documentName":
                        documentName = jsonString.IsNullOrEmpty()
                            ? ""
                            : GetTestDataDirectory() + Path.DirectorySeparatorChar + "CreateAppScenario" + Path.DirectorySeparatorChar + pair.Value;
                        break;
                    case "imageName":
                        imageName = jsonString.IsNullOrEmpty()
                            ? ""
                            : GetTestDataDirectory() + Path.DirectorySeparatorChar + "CreateAppScenario" + Path.DirectorySeparatorChar + pair.Value;
                        break;
                }
            }

            testDataSet.Add(new TestDataModelCreateApp(appRequestModel, appUserRoles ?? throw new Exception("invalid testdata, appUserRoles must not be null"), documentName ?? throw new Exception("invalid testdata, documentName must not be null"), imageName ?? throw new Exception("invalid testdata, imageName must not be null")));
        }

        return testDataSet;
    }

    public static List<TestDataRegistrationModel> GetTestDataForRegistrationWithoutBpn(string fileName)
    {
        var filePath = Path.Combine(GetTestDataDirectory(),
            "RegistrationScenarios" + Path.DirectorySeparatorChar + fileName);

        var jsonData = File.ReadAllText(filePath);
        var testData = DataHandleHelper.DeserializeData<List<Dictionary<string, object>>>(jsonData);
        if (testData.IsNullOrEmpty())
        {
            throw new Exception($"Could not get testdata from {fileName}, maybe empty.");
        }

        var testDataSet = FetchTestDataForRegistrationScenarios(testData!);
        return testDataSet;
    }

    public static List<TestDataRegistrationModel> GetTestDataForRegistrationWithBpn(string fileName)
    {
        var filePath = Path.Combine(GetTestDataDirectory(),
            "RegistrationScenarios" + Path.DirectorySeparatorChar + fileName);

        var jsonData = File.ReadAllText(filePath);
        var testData = DataHandleHelper.DeserializeData<List<Dictionary<string, object>>>(jsonData);
        if (testData.IsNullOrEmpty())
        {
            throw new Exception($"Could not get testdata from {fileName}, maybe empty.");
        }

        var testDataSet = FetchTestDataForRegistrationScenarios(testData!);
        return testDataSet;
    }

    private static List<string[]> FetchTestData(List<Dictionary<string, object>> testData)
    {
        var testDataSet = new List<string[]>();
        foreach (var pair in testData.SelectMany(obj => obj))
        {
            switch (pair.Key)
            {
                case "permissions":
                    var deserializeData = DataHandleHelper.DeserializeData<string[]>(pair.Value.ToString() ?? "[]");
                    if (!deserializeData.IsNullOrEmpty())
                    {
                        testDataSet.Add(deserializeData!);
                    }
                    break;
            }
        }
        return testDataSet;
    }

    private static List<TestDataRegistrationModel> FetchTestDataForRegistrationScenarios(
        List<Dictionary<string, object>> testData)
    {
        var testDataSet = new List<TestDataRegistrationModel>();
        foreach (var obj in testData)
        {
            CompanyDetailData? companyDetailData = null;
            CompanyDetailData? updateCompanyDetailData = null;
            var companyRoles = new List<CompanyRoleId>();
            string? documentName = null, documentTypeId = null;
            foreach (var pair in obj)
            {
                var jsonString = pair.Value.ToString();
                switch (pair.Key)
                {
                    case "companyDetailData":
                        companyDetailData = jsonString is null ? null : DataHandleHelper.DeserializeData<CompanyDetailData>(jsonString);
                        break;
                    case "updateCompanyDetailData":
                        updateCompanyDetailData = jsonString is null ? null : DataHandleHelper.DeserializeData<CompanyDetailData>(jsonString);
                        break;
                    case "companyRoles":
                        companyRoles = DataHandleHelper.DeserializeData<List<CompanyRoleId>>(jsonString ?? "[]");
                        break;
                    case "documentTypeId":
                        documentTypeId = jsonString;
                        break;
                    case "documentName":
                        documentName = GetTestDataDirectory() + Path.DirectorySeparatorChar + "RegistrationScenarios" +
                                       Path.DirectorySeparatorChar + pair.Value;
                        break;
                }
            }

            testDataSet.Add(new TestDataRegistrationModel(companyDetailData, updateCompanyDetailData, companyRoles!,
                documentTypeId, documentName));
        }

        return testDataSet;
    }
}
