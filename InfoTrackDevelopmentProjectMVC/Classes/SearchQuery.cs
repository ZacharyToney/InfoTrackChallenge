using InfoTrackDevelopmentProjectMVC.Interfaces;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System.Web;
using OpenQA.Selenium.Chrome;
using InfoTrackDevelopmentProjectMVC.Models;
using WebElementCollection = System.Collections.ObjectModel.ReadOnlyCollection<OpenQA.Selenium.IWebElement>;

namespace InfoTrackDevelopmentProjectMVC.Classes
{
    public class SearchQuery : ISearchQuery
    {
        private bool alreadyCheckedFeaturedResult;
        private WebElementCollection? allSearchResults;
        public List<SearchResultModel> GetSearchResultsList(string keywords, string url)
        {

            string hostToLookFor = GetHostFromUrl(url);
            IWebDriver driver = DriverSetup();
            string keywordsEncoded = DoesSpellingCorrectionExist(driver, HttpUtility.UrlPathEncode(keywords));
            allSearchResults = FindAllSearchResults(driver, keywordsEncoded);
            return CheckIfSearchResultsListIsEmpty(driver, hostToLookFor);    
        }

        private List<SearchResultModel> CheckIfSearchResultsListIsEmpty(IWebDriver driver, string hostToLookFor)
        {
            if (allSearchResults is not null && allSearchResults.Count > 0)
            {
                List<SearchResultModel> searchResultsList = IterateThroughSearchResults(hostToLookFor, new List<SearchResultModel>());
                driver.Quit();
                return searchResultsList;

            }
            else
            {
                driver.Quit();
                return new List<SearchResultModel>();
            }
        }

        private void DoesRecaptchaShow(IWebDriver driver)
        {
            try
            {
                driver.FindElement(By.Id("captcha-form"));
                throw new Exception("Looks like a captcha form showed up.");
            }
            catch (NoSuchElementException)
            {
                //All is good keep moving
            }
        }

        private string GetHostFromUrl(string url)
        {
            url = HttpUtility.UrlDecode(url);
            string hostToLookFor = new Uri(url).Host;
            hostToLookFor = hostToLookFor.ToLower();
            hostToLookFor = hostToLookFor.Replace("www.", "");
            return hostToLookFor;
        }
        private IWebDriver DriverSetup()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("-headless");
            options.AddArguments("window-size=1920,1080");
            IWebDriver driver = new ChromeDriver(options);
            return driver;
        }
        private string DoesSpellingCorrectionExist(IWebDriver driver,string keywordsEncoded)
        {
            try
            {
                driver.Navigate().GoToUrl($"https://www.google.com/search?num=100&q={keywordsEncoded}");
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                DoesRecaptchaShow(driver);
                wait.Until(drv => drv.FindElement(By.Id("search")));
                var href = driver.FindElement(By.XPath("//*[@id=\"taw\"]/div/p/a")).GetAttribute("href");
                var parseHref = HttpUtility.ParseQueryString(href);
                keywordsEncoded = parseHref["q"]!;
                keywordsEncoded = HttpUtility.UrlPathEncode(keywordsEncoded);
                return keywordsEncoded;
            }
            catch (NoSuchElementException)
            {
                return keywordsEncoded;
            }
        }

        private WebElementCollection? FindAllSearchResults(IWebDriver driver, string keywordsEncoded)
        {
            try
            {
                var searchResultsFromSelenium = driver.FindElement(By.XPath($"//div[@data-async-context=\"query:{keywordsEncoded}\"]"));
                allSearchResults = searchResultsFromSelenium.FindElements(By.XPath("div"));
                return allSearchResults;
            }
            catch (NoSuchElementException)
            {
                driver.Quit();
                return null;
            }
            catch(WebDriverTimeoutException)
            {
                driver.Quit();
                throw new Exception("This error is most likely due to a slow internet connection");
            }
        }

        private List<SearchResultModel> IterateThroughSearchResults(string hostToLookFor,List<SearchResultModel> searchResultsList)
        {
            for (int currentSearchElement = 0; currentSearchElement <= (allSearchResults.Count + 1); currentSearchElement++)
            {
                searchResultsList = AddSearchResultToList(currentSearchElement, hostToLookFor,searchResultsList);
            }
            return searchResultsList;
        }

        private List<SearchResultModel> AddSearchResultToList(int currentSearchElement, string hostToLookFor, List<SearchResultModel> searchResultsList)
        {
            try
            {
                if (!DoesGSectionExist(currentSearchElement))
                {
                    try
                    {
                        if (CheckIfUrlMatchesBeforeGettingOtherData(currentSearchElement,hostToLookFor))
                        {
                            var searchResult = GetSearchResult(currentSearchElement);
                            searchResultsList.Add(searchResult);
                        }
                        
                    }
                    catch (NoSuchElementException)
                    {
                        //Was not able to get data from elements (most likely a section from google)
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message.ToString());
                //Would most likely log to somewhere skipping for brevity of assignment
            }
            return searchResultsList;
        }

        private SearchResultModel GetSearchResult(int currentSearchElement)
        {
            string searchResultDescription;
            try
            {
                if (alreadyCheckedFeaturedResult)
                {
                    searchResultDescription = GetSearchResultDescription(currentSearchElement);
                }
                else
                {
                    alreadyCheckedFeaturedResult = true;
                    var featuredResult = allSearchResults[currentSearchElement].FindElement(By.TagName("block-component"));
                    searchResultDescription = featuredResult.FindElement(By.XPath("//div[@data-attrid='wa:/description']")).Text;
                    
                }

            }
            catch (NoSuchElementException)
            {
                searchResultDescription = GetSearchResultDescription(currentSearchElement);
            }

            var searchResult = new SearchResultModel
            {
                Url = allSearchResults[currentSearchElement].FindElement(By.TagName("a")).GetAttribute("href"),
                UrlText = allSearchResults[currentSearchElement].FindElement(By.TagName("cite")).Text,
                Title = allSearchResults[currentSearchElement].FindElement(By.TagName("h3")).Text,
                Rank = (currentSearchElement + 1),
                Description = searchResultDescription
            };
            return searchResult;
        }

        private bool CheckIfUrlMatchesBeforeGettingOtherData(int currentSearchElement, string hostToLookFor)
        {
            var checkHost = GetHostFromUrl(allSearchResults[currentSearchElement].FindElement(By.TagName("a")).GetAttribute("href").ToLower());
            return checkHost.Equals(hostToLookFor);
        }

        private string GetSearchResultDescription(int currentSearchElement)
        {
            string searchResultDescription;
            try
            {
                //see if it is a regular section
                searchResultDescription = allSearchResults[currentSearchElement].FindElement(By.XPath($"//*[@id=\"rso\"]/div[{currentSearchElement + 1}]/div/div/div[2]")).Text;

            }
            catch (NoSuchElementException)
            {
                //special sections before g sections or mid roll check
                try
                {
                    searchResultDescription = allSearchResults[currentSearchElement].FindElement(By.XPath($"//*[@id=\"rso\"]/div[{currentSearchElement + 1}]/div/div/div/div[2]")).Text;
                }
                catch (NoSuchElementException)
                {
                    searchResultDescription = allSearchResults[currentSearchElement].FindElement(By.XPath($"//div[{currentSearchElement+1}]/div/div[1]")).Text;
                }
                
            }
            return searchResultDescription;
        }

        private bool DoesGSectionExist(int currentSearchElement)
        {
            try
            {
                //g-section-with-header
                allSearchResults[currentSearchElement].FindElement(By.TagName("g-section-with-header"));
                return true;
            }
            catch(NoSuchElementException)
            {
                return false;
            }
        }
    }
}
