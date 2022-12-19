using InfoTrackDevelopmentProjectMVC.Classes;
using Xunit;

namespace InfoTrackDevelopmentProjectMVC.UnitTests
{
    public class IntergrationTests
    {
        [Fact]
        public void Check_EmptyResultFromGoogle()
        {
            //thisshouldnotworkasagooglesearchandiwantittoreturnnothing
            var searchQuery = new SearchQuery();
            var results = searchQuery.GetSearchResultsList("thisshouldnotworkasagooglesearchandiwantittoreturnnothing", "https://cars.com");
            Assert.Empty(results);
        }
        [Fact]
        public void Check_OneResultFromCarsSearchIgnoreAds()
        {
            //thisshouldnotworkasagooglesearchandiwantittoreturnnothing
            var searchQuery = new SearchQuery();
            var results = searchQuery.GetSearchResultsList("cars", "https://cars.com");
            Assert.True(results.Count >= 1);
        }
    }
}
