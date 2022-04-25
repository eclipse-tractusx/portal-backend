namespace CatenaX.NetworkServices.App.Service.ViewModels
{
    /// <summary>
    /// View model of an application's detailed data.
    /// </summary>
    public class AppDetailsViewModel
    {
        /// <summary>
        /// ID of the app.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Title or name of the app.
        /// </summary>
        public string Title { get; set; } = default!;

        /// <summary>
        /// Uri to app's lead picture.
        /// </summary>
        public string LeadPictureUri { get; set; } = default!;

        /// <summary>
        /// List of URIs to app's secondary pictures.
        /// </summary>
        public IEnumerable<string> DetailPictureUris { get; set; } = default!;

        /// <summary>
        /// Uri to provider's marketing presence.
        /// </summary>
        public string ProviderUri { get; set; } = default!;

        /// <summary>
        /// Provider of the app.
        /// </summary>
        public string Provider { get; set; } = default!;

        /// <summary>
        /// Email address of the app's primary contact.
        /// </summary>
        public string ContactEmail { get; set; } = default!;

        /// <summary>
        /// Phone number of the app's primary contact.
        /// </summary>
        public string ContactNumber { get; set; } = default!;

        /// <summary>
        /// Names of the app's use cases.
        /// </summary>
        public IEnumerable<string> UseCases { get; set; } = default!;

        /// <summary>
        /// Long description of the app.
        /// </summary>
        public string LongDescription { get; set; } = default!;

        /// <summary>
        /// Pricing information of the app.
        /// </summary>
        public string Price { get; set; } = default!;

        /// <summary>
        /// Tags assigned to application.
        /// </summary>
        public IEnumerable<string> Tags { get; set; } = default!;

        /// <summary>
        /// Whether app has been purchased by the user's company.
        /// </summary>
        public bool IsPurchased { get; set; }
    }
}
