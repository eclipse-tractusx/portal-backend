using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

[ApiController]
[Route("api/info")]
public class OpenInformationController : ControllerBase
{
    private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

    /// <summary>
    /// Creates a new instance of <see cref="OpenInformationController"/>
    /// </summary>
    /// <param name="actionDescriptorCollectionProvider">The actionDescriptorCollectionProvider</param>
    public OpenInformationController(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
    {
        _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
    }
    
}
