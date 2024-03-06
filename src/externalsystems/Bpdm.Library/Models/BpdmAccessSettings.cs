using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;

public class BpdmAccessSettings
{
    [Required(AllowEmptyStrings = false)]
    public string BaseUrl { get; set; } = null!;
}
