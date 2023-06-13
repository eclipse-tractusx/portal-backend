using System.Text.Json;
using System.Text.Json.Serialization;
using Castle.Core.Internal;
using Notifications.Service.Tests.RestAssured;
using Tests.Shared.EndToEndTests;

namespace Notifications.Service.Tests.EndToEndTests;

public static class TestDataHelper
{
    private static readonly string TestDataDirectory = Directory.GetParent(TestResources.GetSourceFilePathName()).Parent.Parent.Parent.FullName +
                                                       Path.DirectorySeparatorChar + "shared" +
                                                       Path.DirectorySeparatorChar + "Tests.Shared" +
                                                       Path.DirectorySeparatorChar + "EndToEndTests" +
                                                       Path.DirectorySeparatorChar + "TestData";

    public static List<TestDataModel>? GetTestData()
    {
        var filePath = Path.Combine(TestDataDirectory, "HappyPathModifyCoreUserRoles.json");
        var jsonData = File.ReadAllText(filePath);
        var testData = JsonSerializer.Deserialize<List<Dictionary<string, Object>>>(jsonData);
        return testData.IsNullOrEmpty() ? null : FetchTestData(testData);
    }

    private static List<TestDataModel> FetchTestData(List<Dictionary<string, Object>> testData)
    {
        var testDataSet = new List<TestDataModel>();
        foreach (var obj in testData)
        {
            List<string> rolesToAssign = null, rolesToUnassign = null;
            foreach (var pair in obj)
            {
                switch (pair.Key)
                {
                    case "rolesToAssign":
                        rolesToAssign = DeserializeData<List<string>>(pair.Value.ToString());
                        break;
                    case "rolesToUnAssign":
                        rolesToUnassign = DeserializeData<List<string>>(pair.Value.ToString());
                        break;
                }
            }

            testDataSet.Add(new TestDataModel(rolesToAssign, rolesToUnassign));
        }

        return testDataSet;
    }

    private static T? DeserializeData<T>(string jsonString)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
        var deserializedData = JsonSerializer.Deserialize<T>(jsonString, options);
        return deserializedData;
    }
}