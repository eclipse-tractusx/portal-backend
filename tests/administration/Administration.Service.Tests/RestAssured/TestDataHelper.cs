using System.Text.Json;
using Castle.Core.Internal;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.RestAssured;

public class TestDataHelper
{
    private static readonly string TestDataDirectory = ".." + Path.DirectorySeparatorChar + ".." +
                                                       Path.DirectorySeparatorChar + ".." +
                                                       Path.DirectorySeparatorChar + ".." +
                                                       Path.DirectorySeparatorChar + ".." +
                                                       Path.DirectorySeparatorChar + "shared" +
                                                       Path.DirectorySeparatorChar + "Tests.Shared" +
                                                       Path.DirectorySeparatorChar + "RestAssured" +
                                                       Path.DirectorySeparatorChar + "TestData";

    public static List<string[]>? GetTestData(string fileName)
    {
        var filePath = Path.Combine(TestDataDirectory, fileName);

        var jsonData = File.ReadAllText(filePath);
        var testData = JsonSerializer.Deserialize<List<Dictionary<string, Object>>>(jsonData);
        if (testData.IsNullOrEmpty())
            throw new Exception("Incorrect format of test data for service account scenarios");
        var testDataSet = FetchTestData(testData);
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
}