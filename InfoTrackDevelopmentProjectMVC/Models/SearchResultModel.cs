using System.ComponentModel;

namespace InfoTrackDevelopmentProjectMVC.Models
{
    public class SearchResultModel
    {
        public string Url { get; set; } = string.Empty;
        [DisplayName("Url Text")]
        public string UrlText { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Rank { get; set; }
    }
}
