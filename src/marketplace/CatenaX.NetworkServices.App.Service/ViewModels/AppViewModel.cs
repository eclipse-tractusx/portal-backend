namespace CatenaX.NetworkServices.App.Service.ViewModels
{
    /// <summary>
    /// View model of an application's base data.
    /// </summary>
    public class AppViewModel
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
        /// Short description of the app.
        /// </summary>
        public string ShortDescription { get; set; } = default!;

        /// <summary>
        /// Provider of the app.
        /// </summary>
        public string Provider { get; set; } = default!;

        /// <summary>
        /// Names of the app's use cases.
        /// </summary>
        public IEnumerable<string> UseCases { get; set; } = default!;

        /// <summary>
        /// Pricing information of the app.
        /// </summary>
        public string Price { get; set; } = default!;

        /// <summary>
        /// Uri to app's lead picture.
        /// </summary>
        public string LeadPictureUri { get; set; } = default!;
    }
}
