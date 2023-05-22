using System.Text.Json;
using System.Text.Json.Serialization;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Xunit;

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
            Converters = { new JsonStringEnumConverter() }
        };
        var deserializedData = JsonSerializer.Deserialize<T>(jsonString, options);
        return deserializedData;
    }
}