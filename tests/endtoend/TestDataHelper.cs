using Castle.Core.Internal;
using EndToEnd.Tests;
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
        var testData = JsonSerializer.Deserialize<List<Dictionary<string, Object>>>(jsonData);
        if (testData.IsNullOrEmpty())
            throw new Exception("Incorrect format of test data for service account scenarios");
        var testDataSet = FetchTestData(testData!);
        return testDataSet;
    }

    public static List<TestDataModelCreateApp> GetTestDataForCreateApp(string fileName)
    {
        var filePath = Path.Combine(GetTestDataDirectory(), "CreateAppScenario" + Path.DirectorySeparatorChar + fileName);

        var jsonData = File.ReadAllText(filePath);
        var testData = JsonSerializer.Deserialize<List<Dictionary<string, Object>>>(jsonData);
        if (testData.IsNullOrEmpty())
            throw new Exception("Incorrect format of test data for service account scenarios");
        var testDataSet = FetchTestDataForCreateApp(testData!);
        return testDataSet;
    }

    private static List<TestDataModelCreateApp> FetchTestDataForCreateApp(List<Dictionary<string, Object>> testData)
    {
        var testDataSet = new List<TestDataModelCreateApp>();
        foreach (var unused in testData)
        {
            List<AppUserRole>? appUserRoles = null;
            AppRequestModel? appRequestModel = null;
            string? documentName = null;
            string? imageName = null;
            foreach (var pair in testData.SelectMany(o => o))
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
                        if (!jsonString.IsNullOrEmpty())
                        {
                            documentName = GetTestDataDirectory() + Path.DirectorySeparatorChar + "CreateAppScenario" +
                                           Path.DirectorySeparatorChar + pair.Value;
                        }
                        else
                        {
                            documentName = "";
                        }

                        break;
                    case "imageName":
                        if (!jsonString.IsNullOrEmpty())
                        {
                            imageName = GetTestDataDirectory() + Path.DirectorySeparatorChar + "CreateAppScenario" +
                                        Path.DirectorySeparatorChar + pair.Value;
                        }
                        else
                        {
                            imageName = "";
                        }

                        break;
                }
            }

            testDataSet.Add(new TestDataModelCreateApp(appRequestModel, appUserRoles!, documentName!, imageName!));
        }

        return testDataSet;
    }

    public static List<TestDataRegistrationModel> GetTestDataForRegistrationWithoutBpn(string fileName)
    {
        var filePath = Path.Combine(GetTestDataDirectory(),
            "RegistrationScenarios" + Path.DirectorySeparatorChar + fileName);

        var jsonData = File.ReadAllText(filePath);
        var testData = DataHandleHelper.DeserializeData<List<Dictionary<string, Object>>>(jsonData);
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
        var testData = DataHandleHelper.DeserializeData<List<Dictionary<string, Object>>>(jsonData);
        if (testData.IsNullOrEmpty())
        {
            throw new Exception($"Could not get testdata from {fileName}, maybe empty.");
        }

        var testDataSet = FetchTestDataForRegistrationScenarios(testData!);
        return testDataSet;
    }

    private static List<string[]> FetchTestData(List<Dictionary<string, Object>> testData)
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
        List<Dictionary<string, Object>> testData)
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
