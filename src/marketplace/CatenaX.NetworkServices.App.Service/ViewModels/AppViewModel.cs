namespace CatenaX.NetworkServices.App.Service.ViewModels
{
    public class AppViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = default!;

        public string ShortDescription { get; set; } = default!;

        public string Provider { get; set; } = default!;

        public ICollection<string> UseCases { get; set; } = new HashSet<string>();

        public string Price { get; set; } = default!;

        public string LeadPictureUri { get; set; } = default!;
    }
}
