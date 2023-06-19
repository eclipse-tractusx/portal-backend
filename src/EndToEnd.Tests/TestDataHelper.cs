using System.Text.Json;
using System.Text.Json.Serialization;
using Castle.Core.Internal;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Tests.Shared.EndToEndTests;

namespace EndToEnd.Tests;

public class TestDataHelper
{
    private static readonly string TestDataDirectory = Directory.GetParent(TestResources.GetSourceFilePathName()).Parent.Parent.FullName +
                                                       Path.DirectorySeparatorChar + "tests" +
                                                       Path.DirectorySeparatorChar + "shared" +
                                                       Path.DirectorySeparatorChar + "Tests.Shared" +
                                                       Path.DirectorySeparatorChar + "EndToEndTests" +
                                                       Path.DirectorySeparatorChar + "TestData";

    public static List<string[]>? GetTestDataForServiceAccountCUDScenarios(string fileName)
    {
        var filePath = Path.Combine(TestDataDirectory, fileName);

        var jsonData = File.ReadAllText(filePath);
        var testData = JsonSerializer.Deserialize<List<Dictionary<string, Object>>>(jsonData);
        if (testData.IsNullOrEmpty())
            throw new Exception("Incorrect format of test data for service account scenarios");
        var testDataSet = FetchTestData(testData);
        return testDataSet;

    }
    
    public static List<TestDataModel> GetTestDataForRegistrationWithoutBpn(string fileName)
    {
        var filePath = Path.Combine(TestDataDirectory, fileName);

        var jsonData = File.ReadAllText(filePath);
        var testData = JsonSerializer.Deserialize<List<Dictionary<string, Object>>>(jsonData);

        var testDataSet = FetchTestDataForRegistrationWithoutBpn(testData);
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
                    testDataSet.Add(JsonSerializer.Deserialize<string[]>(pair.Value.ToString()));
                    break;
            }
        }
        return testDataSet;
    }
    
    private static List<TestDataModel> FetchTestDataForRegistrationWithoutBpn(List<Dictionary<string, Object>> testData)
    {
        List<TestDataModel> testDataSet = new List<TestDataModel>();
        foreach (var obj in testData)
        {
            CompanyDetailData? companyDetailData = null;
            CompanyDetailData? updateCompanyDetailData = null;
            List<CompanyRoleId>? companyRoles = null;
            string? documentPath = null, documentTypeId = null;
            foreach (var pair in obj)
            {
                switch (pair.Key)
                {
                    case "companyDetailData":
                        companyDetailData = DeserializeData<CompanyDetailData>(pair.Value.ToString());
                        break;
                    case "updateCompanyDetailData":
                        updateCompanyDetailData = DeserializeData<CompanyDetailData>(pair.Value.ToString());
                        break;
                    case "companyRoles":
                        companyRoles = DeserializeData<List<CompanyRoleId>>(pair.Value.ToString());
                        break;
                    case "documentTypeId":
                        documentTypeId = pair.Value.ToString();
                        break;
                    case "documentPath":
                        documentPath = pair.Value.ToString();
                        break;
                }
            }

            testDataSet.Add(new TestDataModel(companyDetailData, updateCompanyDetailData, companyRoles, documentTypeId,
                documentPath));
        }

        return testDataSet;
    }
    
    private static T? DeserializeData<T>(string jsonString)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };
        var deserializedData = JsonSerializer.Deserialize<T>(jsonString, options);
        return deserializedData;
    }
}