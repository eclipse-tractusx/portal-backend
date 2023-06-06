using System.Text.Json;
using System.Text.Json.Serialization;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;

namespace Registration.Service.Tests.RestAssured.RegistrationEndpointTests;

public static class TestDataHelper
{
    // private const string TestDataDirectory = "..\\..\\..\\..\\..\\shared\\Tests.Shared\\RestAssured\\TestData";
    private static readonly string TestDataDirectory = ".." + Path.DirectorySeparatorChar + ".." +
                                                       Path.DirectorySeparatorChar + ".." +
                                                       Path.DirectorySeparatorChar + ".." +
                                                       Path.DirectorySeparatorChar + ".." +
                                                       Path.DirectorySeparatorChar + "shared" +
                                                       Path.DirectorySeparatorChar + "Tests.Shared" +
                                                       Path.DirectorySeparatorChar + "RestAssured" +
                                                       Path.DirectorySeparatorChar + "TestData";

    public static List<TestDataModel> GetTestData(string fileName)
    {
        var filePath = Path.Combine(TestDataDirectory, fileName);

        var jsonData = File.ReadAllText(filePath);
        var testData = JsonSerializer.Deserialize<List<Dictionary<string, Object>>>(jsonData);

        var testDataSet = FetchTestData(testData);
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

    private static List<TestDataModel> FetchTestData(List<Dictionary<string, Object>> testData)
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
}