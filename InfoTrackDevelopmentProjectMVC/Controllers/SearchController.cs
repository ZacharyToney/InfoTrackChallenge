using InfoTrackDevelopmentProjectMVC.Classes;
using InfoTrackDevelopmentProjectMVC.Interfaces;
using InfoTrackDevelopmentProjectMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace InfoTrackDevelopmentProjectMVC.Controllers
{
    public class SearchController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ISearchQuery _searchQuery;
        public SearchController(ILogger<HomeController> logger,ISearchQuery searchQuery)
        {
            _logger = logger;
            _searchQuery = searchQuery;
        }

        [Route("/search")]
        [HttpGet]
        public IActionResult SearchForm()
        {
            return View();
        }
        [Route("/search")]
        [HttpPost]
        public IActionResult ViewResults(SearchQueryModel searchQuery)
        {
            List<SearchResultModel> searchResults = _searchQuery.GetSearchResultsList(searchQuery.Keywords, searchQuery.Url);
            return View(searchResults);
        }
    }
}
