using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json.Linq;
using SeleniumUndetectedChromeDriver;

namespace OzonLive;

public class DataHelper
{
    private IWebDriver _driver = null!;
    private readonly Guid _guidSession = Guid.NewGuid();

    public DataHelper()
    {
        Console.WriteLine($"Create session with GUID - {_guidSession}");
    }

    public IWebDriver InitializationDriver()
    {
        
        var pathToFile = AppDomain.CurrentDomain.BaseDirectory + '\\';

        var options = new ChromeOptions();
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddArgument("--headless");
        options.AddArgument(
            "user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.5563.147 Safari/537.36");
        using var driver = UndetectedChromeDriver.Create(
            browserExecutablePath: pathToFile,
            options: options
        );
        return driver;
    }

    public async Task<bool> ParsePageAsync(int pageIndex, string nameRequest, int maxCountReloadDriver,
        CancellationTokenSource cts)
    {
        cts.Token.ThrowIfCancellationRequested();
        try
        {
            Console.WriteLine($"Начало работы со страницей {pageIndex}");
            
            var items = ParceStartData(pageIndex, nameRequest);

            var counterReload = 1;
            while (items == null && counterReload <= maxCountReloadDriver)
            {
                Thread.Sleep(500);
                Console.WriteLine(
                    $"Задержка загрузки данных со страницы {pageIndex}, перезагрузка драйвера. Попытка {counterReload} из {maxCountReloadDriver}");
                items = ParceStartData(pageIndex, nameRequest);
                counterReload++;
            }

            if (items == null)
            {
                Console.WriteLine(
                    $"Не удалось получить запрос с {maxCountReloadDriver} попыток от сервера, страница {pageIndex} не обработана");
                return false;
            }

            var outputData = new List<OutputData>();
            var numberSearchResult = pageIndex == 1 ? 1 : (pageIndex - 1) * 36;
            foreach (var item in items!)
            {
                var output = new OutputData();

                var mainState = item["mainState"];
                var idGood = item["multiButton"]?["ozonButton"]?["addToCartButtonWithQuantity"]?["action"]?["id"]
                                 ?.ToString() ??
                             item["multiButton"]?["expressButton"]?["addToCartButtonWithQuantity"]?["action"]?["id"]
                                 ?.ToString();
                output.IdGood = idGood!;
                var itemRoots = mainState?.ToObject<List<Root>>();
                var atoms = itemRoots!
                    .Select(p => p.Atom).ToList();


                var finalPrice = atoms.FirstOrDefault(x => x?.PriceWithTitle?.PriceItem != null)?.PriceWithTitle
                    ?.PriceItem;

                if (finalPrice == null)
                {
                    var price = atoms.FirstOrDefault(x => x?.Price?.PriceItem != null);
                    output.CurrentPrice = price?.Price?.PriceItem!;
                    output.OriginalPrice = price?.Price?.OriginalPrice!;
                }
                else
                {
                    output.OriginalPrice = output.CurrentPrice = finalPrice;
                }

                output.NamePosition = atoms.FirstOrDefault(x => x?.TextAtom?.Text != null)?.TextAtom?.Text?
                    .Replace("&#x2;", "")
                    .Replace("&#34;", "\"")
                    .Replace("&amp;", "&")
                    .Replace("&#x2F;", "/")
                    .Replace("&#39;", "'")!;

                var listInfo = atoms.LastOrDefault(x => x?.LabelList != null)?.LabelList?.Items;

                if (listInfo != null)
                {
                    var info = listInfo.Select(x => x.Title);
                    output.Discription = string.Join(", ", info.ToList()!);
                }

                output.NumberSearchResult = numberSearchResult.ToString();
                outputData.Add(output);
                numberSearchResult++;
            }

            await new Writer().WriteToExcel($"{nameRequest}_{_guidSession}", $"page_{pageIndex}", outputData);
            Console.WriteLine($"Конец работы со страницей {pageIndex}");
            return await Task.Run(() => true);
        }
        catch (NoSuchElementException)
        {
            Console.WriteLine($"Конец работы со страницей {pageIndex} - последняя страница");
            cts.Cancel();
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Конец работы со страницей {pageIndex} c ошибкой");
            Console.WriteLine(e);
            throw;
        }
    }

    private JToken? ParceStartData(int pageIndex = 0, string nameRequest = "")
    {
        using var driver = InitializationDriver();
        
        driver.Navigate().GoToUrl("https://www.ozon.ru/api/entrypoint-api.bx/page/json/v2?url=" +
                                  "/search/?deny_category_prediction=true&from_global=true&" +
                                  $"text={nameRequest}" +
                                  "&page_changed=true" +
                                  "&layout_container=categorySearchMegapagination" +
                                  $"&layout_page_index={pageIndex}&page={pageIndex}");
        
        Thread.Sleep(100);
        
        var test = driver.FindElement(By.TagName("pre"));

        var step1 = JsonConvert.DeserializeObject<JObject>(test.Text);
        var globalJsonObject = JObject.Parse(step1?.ToString()!);
        var widgetState = (JObject) globalJsonObject["widgetStates"]!;
        var searchResultsPropertyName =
            widgetState.Properties().FirstOrDefault(x => x.Name.Contains("searchResultsV2"))?.Name;
        var searchResultsItem = JObject.Parse(widgetState[searchResultsPropertyName!]?.ToString()!);
        driver.Close();
        return searchResultsItem["items"];
    }
}