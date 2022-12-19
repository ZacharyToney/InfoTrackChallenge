using InfoTrackDevelopmentProjectMVC.Models;

namespace InfoTrackDevelopmentProjectMVC.Interfaces
{
    public interface ISearchQuery
    {
        public List<SearchResultModel> GetSearchResultsList(string keywords, string url);
    }
}
