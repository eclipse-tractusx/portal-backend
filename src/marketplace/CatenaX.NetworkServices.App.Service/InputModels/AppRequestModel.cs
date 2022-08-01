using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.App.Service.InputModels;

/// <summary>
/// Model for requesting creation of an application.
/// </summary>
public class AppRequestModel
{
    /// <summary>
    /// Private constructor.
    /// </summary>
    private AppRequestModel()
    {
        Provider = string.Empty;
        Price = string.Empty;
        UseCaseIds = new HashSet<Guid>();
        Descriptions = new HashSet<LocalizedDescription>();
        SupportedLanguageCodes = new HashSet<string>();
    }
    
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="provider">Provider of the app.</param>
    /// <param name="price">Price of the app.</param>
    public AppRequestModel(string provider, string price): this()
    {
        Provider = provider;
        Price = price;
    }

    /// <summary>
    /// Title or name of the app.
    /// </summary>
    [MaxLength(255)]
    public string? Title { get; set; }

    /// <summary>
    /// Provider of the app.
    /// </summary>
    [MaxLength(255)]
    public string Provider { get; set; }

    /// <summary>
    /// Uri to app's lead picture.
    /// </summary>
    [MaxLength(255)]
    public string? LeadPictureUri { get; set; }

    /// <summary>
    /// ID of the app's providing company.
    /// </summary>
    public Guid? ProviderCompanyId { get; set; }

    /// <summary>
    /// IDs of app's use cases.
    /// </summary>
    public virtual ICollection<Guid> UseCaseIds { get; set; }

    /// <summary>
    /// Descriptions of the app in different languages.
    /// </summary>
    public virtual ICollection<LocalizedDescription> Descriptions { get; set; }

    /// <summary>
    /// Two character language codes for the app's supported languages.
    /// </summary>
    public ICollection<string> SupportedLanguageCodes { get; set; }

    /// <summary>
    /// Pricing information of the app.
    /// </summary>
    public string Price { get; set; }
}
