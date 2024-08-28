namespace TvMazeScrapper.Models.ViewModels
{
    public class ShowsListViewModel
    {
        public List<ShowViewModel> Shows { get; set; } = new List<ShowViewModel>();
    }

    public class CastMemberViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? Birthday { get; set; }
    }

    public class ShowViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<CastMemberViewModel> Cast { get; set; }
    }
}
