using System.ComponentModel.DataAnnotations;

namespace InfoTrackDevelopmentProjectMVC.Models
{
    public class SearchQueryModel
    {
        [Required]
        [MinLength(1)]
        public string Keywords { get; set; }
        [Required]
        [MinLength(1)]
        [Url]
        public string Url { get; set; }
    }
}
