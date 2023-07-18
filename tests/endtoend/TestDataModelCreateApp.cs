using Newtonsoft.Json;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

public record TestDataModelCreateApp(
    AppRequestModel? AppRequestModel,
    List<AppUserRole> AppUserRoles,
    string DocumentName,
    string ImageName
)
{
    public override string ToString()
    {
        return $"Title={AppRequestModel?.Title}";
    }
};
