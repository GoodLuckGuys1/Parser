using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json.Linq;

namespace Ozon_live;

public class DataHelper
{
    private IWebDriver _driver;

    public void InitializationDriver()
    {
        var pathToFile = AppDomain.CurrentDomain.BaseDirectory + '\\';

        var options = new ChromeOptions();
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddArgument("--headless");
        options.AddArgument(
            "user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.5563.147 Safari/537.36");

        _driver = new ChromeDriver(pathToFile, options);
    }

    public async Task<bool> ParsePageAsync(int pageIndex, CancellationTokenSource cts)
    {
        cts.Token.ThrowIfCancellationRequested();
        try
        {
            if (pageIndex == 5)
                ;
            Console.WriteLine($"Начало работы с {pageIndex}");

            _driver.Navigate().GoToUrl("https://www.ozon.ru/api/entrypoint-api.bx/page/json/v2?url=" +
                                       "/search/?deny_category_prediction=true&from_global=true&" +
                                       "text=%D0%B2%D0%B0%D0%BA%D1%83%D1%83%D0%BC%D0%B0%D1%82%D0%BE%D1%80" +
                                       "&page_changed=true" +
                                       "&layout_container=categorySearchMegapagination" +
                                       $"&layout_page_index={pageIndex}&page={pageIndex}");

            Thread.Sleep(300);

            var items = ParceStartData();
            if (items == null)
            {
                items = ParceStartData(true, pageIndex);
            }

            foreach (var item in items!)
            {
                var mainState = item["mainState"];
                var itemRoots = mainState?.ToObject<List<Root>>();
                var atoms = itemRoots!
                    .Select(p => p.Atom).ToList();

                var finalPrice = atoms.FirstOrDefault(x => x?.PriceWithTitle?.PriceItem != null)?.PriceWithTitle
                    ?.PriceItem;
                var originalPrice = finalPrice;
                if (finalPrice == null)
                {
                    var price = atoms.FirstOrDefault(x => x?.Price?.PriceItem != null);
                    finalPrice = price?.Price?.PriceItem;
                    originalPrice = price?.Price?.OriginalPrice;
                }

                var name = atoms.FirstOrDefault(x => x?.TextAtom?.Text != null)?.TextAtom?.Text;
                var listInfo = atoms.LastOrDefault(x => x?.LabelList != null)?.LabelList?.Items;

                if (listInfo == null) continue;

                var info = listInfo.Select(x => x.Title);
                var infoString = string.Join(", ", info.ToList()!);
            }

            Console.WriteLine($"Конец работы с {pageIndex}");
            return await Task.Run(() => true);
        }
        catch (NoSuchElementException)
        {
            Console.WriteLine($"Конец работы с {pageIndex} - последняя страница");
            cts.Cancel();
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Конец работы с {pageIndex} c ошибкой");
            Console.WriteLine(e);
            throw;
        }
    }

    private JToken ParceStartData(bool newDriver = false, int pageIndex = 0)
    {
        if (newDriver)
        {
            InitializationDriver();
            _driver.Navigate().GoToUrl("https://www.ozon.ru/api/entrypoint-api.bx/page/json/v2?url=" +
                                       "/search/?deny_category_prediction=true&from_global=true&" +
                                       "text=%D0%B2%D0%B0%D0%BA%D1%83%D1%83%D0%BC%D0%B0%D1%82%D0%BE%D1%80" +
                                       "&page_changed=true" +
                                       "&layout_container=categorySearchMegapagination" +
                                       $"&layout_page_index={pageIndex}&page={pageIndex}");
            Thread.Sleep(500);
        }

        var test = _driver.FindElement(By.TagName("pre"));

        var step1 = JsonConvert.DeserializeObject<JObject>(test.Text);
        var globalJsonObject = JObject.Parse(step1?.ToString()!);
        var widgetState = (JObject) globalJsonObject["widgetStates"]!;
        var searchResultsPropertyName =
            widgetState.Properties().FirstOrDefault(x => x.Name.Contains("searchResultsV2"))?.Name;
        var searchResultsItem = JObject.Parse(widgetState[searchResultsPropertyName!]?.ToString()!);
        return searchResultsItem["items"]!;
    }
}