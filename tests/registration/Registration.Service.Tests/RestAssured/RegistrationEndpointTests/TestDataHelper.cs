using System.Text.Json;
using System.Text.Json.Serialization;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Registration.Service.Tests.RestAssured.RegistrationEndpointTests;

public class TestDataHelper
{
    private const string _testDataDirectory = "..\\..\\..\\..\\..\\shared\\Tests.Shared\\RestAssured\\TestData";
    private const string _companyDetailPath = "CompanyDetailData.json";
    private const string _companyRolePath = "CompanyRole.json";
    private readonly string jsonCompanyDetailData;
    private readonly string jsonCompanyRoleData;
    
    public TestDataHelper()
    {
        var filePath = Path.Combine(_testDataDirectory, _companyDetailPath);
        jsonCompanyDetailData = File.ReadAllText(filePath);
        
        var companyRoleFilePath = Path.Combine(_testDataDirectory, _companyRolePath);
        jsonCompanyRoleData = File.ReadAllText(companyRoleFilePath);
    }
    
    [Fact]
    public List<TestDataModel> GetTestData()
    {
        var filePath = Path.Combine(_testDataDirectory, "TestDataHappyPathRegistrationWithoutBpn.json");
        var jsonData = File.ReadAllText(filePath);
        var testData = JsonSerializer.Deserialize<List<Dictionary<string, Object>>>(jsonData);

        List<TestDataModel> testDataSet = FetchTestData(testData);
        return testDataSet;
    }
    
    public CompanyDetailData? GetNewCompanyDetailDataFromTestData()
    {
        var testDataCompanyDetailData = DeserializeData<Dictionary<string, CompanyDetailData>>(jsonCompanyDetailData);
        var newCompanyDetailData = testDataCompanyDetailData?["newCompanyDetailData"];
        if (newCompanyDetailData != null) return newCompanyDetailData;
        throw new Exception("Test data with new company detail data was not found");
    }
    
    public List<CompanyRoleId>? GetCompanyRolesFromTestData(int count)
    {
        var testDataCompanyRole = DeserializeData<Dictionary<string, List<CompanyRoleId>>>(jsonCompanyRoleData);
        var companyRoles = testDataCompanyRole?[count.ToString()];
        if (companyRoles != null) return companyRoles;
        throw new Exception("Test data with company roles was not found");
    }
    
    public CompanyDetailData? GetUpdateCompanyDetailDataFromTestData()
    {
        var testDataCompanyDetailData = DeserializeData<Dictionary<string, CompanyDetailData>>(jsonCompanyDetailData);
        var updateCompanyDetailData = testDataCompanyDetailData?["updateCompanyDetailData"];
        if (updateCompanyDetailData != null) return updateCompanyDetailData;
        throw new Exception("Test data with company detail data for update was not found");
    }
    
    private T? DeserializeData<T>(string jsonString)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter()},
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };
        var deserializedData = JsonSerializer.Deserialize<T>(jsonString, options);
        return deserializedData;
    }

    private List<TestDataModel> FetchTestData(List<Dictionary<string, Object>> testData)
    {
        List<TestDataModel> testDataSet = new List<TestDataModel>();
        foreach (var obj in testData)
        {
            CompanyDetailData? companyDetailData = null;
            CompanyDetailData? updateCompanyDetailData = null;
            List<CompanyRoleId>? companyRoles = null;
            string? documentName = null, documentPath = null;
            DocumentTypeId? documentTypeId = null;
            foreach (var pair in obj)
            {
                switch (pair.Key.ToString())
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
                    case "documentName":
                        documentName = JsonSerializer.Deserialize<string>(pair.Value.ToString());
                        break;
                    case "documentTypeId":
                        documentTypeId = JsonSerializer.Deserialize<DocumentTypeId>(pair.Value.ToString());
                        break;
                    case "documentPath":
                        documentPath = JsonSerializer.Deserialize<string>(pair.Value.ToString());
                        break;
                    //throw new Exception("Test data can't be fetched correctly");
                }
            }
            
            testDataSet.Add(new TestDataModel(companyDetailData, updateCompanyDetailData, companyRoles, documentName, documentTypeId, documentPath));
        }

        return testDataSet;
    }
}