// See https://aka.ms/new-console-template for more information

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Ozon_live;

using var httpClient = new HttpClient();

var pathToFile = AppDomain.CurrentDomain.BaseDirectory + '\\';

ChromeOptions options = new ChromeOptions();
options.AddArgument("--disable-blink-features=AutomationControlled");
options.AddArgument("--headless");
options.AddArgument(
    "user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.5563.147 Safari/537.36");

IWebDriver driver = new ChromeDriver(pathToFile, options);

driver.Navigate().GoToUrl("https://www.ozon.ru/api/entrypoint-api.bx/page/json/v2?url=" +
                          "/search/?deny_category_prediction=true&from_global=true&" +
                          "text=%D0%B2%D0%B0%D0%BA%D1%83%D1%83%D0%BC%D0%B0%D1%82%D0%BE%D1%80" +
                          "&page_changed=true" +
                          "&layout_container=categorySearchMegapagination" +
                          "&layout_page_index=2&page=2");

Thread.Sleep(1000);
var test = driver.FindElement(By.TagName("pre"));

var step1 = JsonConvert.DeserializeObject<JObject>(test.Text);
var globalJsonObject = JObject.Parse(step1?.ToString()!);
var widgetState = (JObject) globalJsonObject["widgetStates"]!;
var searchResultsPropertyName = widgetState.Properties().FirstOrDefault(x => x.Name.Contains("searchResultsV2"))?.Name;
var searchResultsItem = JObject.Parse(widgetState[searchResultsPropertyName!]?.ToString()!);
var items = searchResultsItem["items"];
foreach (var item in items!)
{
    var mainState = item["mainState"];
    var itemRoots = mainState?.ToObject<List<Root>>();
    var atoms = itemRoots!
        .Select(p => p.Atom).ToList();

    var finalPrice = atoms.FirstOrDefault(x => x?.PriceWithTitle?.PriceItem != null)?.PriceWithTitle?.PriceItem;
    var originalPrice = finalPrice;
    if (finalPrice == null)
    {
        var price = atoms.FirstOrDefault(x => x?.Price?.PriceItem != null);
        finalPrice = price?.Price?.PriceItem;
        originalPrice = price?.Price?.OriginalPrice;
    }
    
    var name = atoms.FirstOrDefault(x => x?.TextAtom?.Text != null)?.TextAtom?.Text;
    var info = atoms.FirstOrDefault(x => x?.LabelList != null)?.LabelList?.Items?.Select(x => x.Title);
    var infoString = string.Join(", ", info?.Select(x => x?.Trim()).ToList()!);
    
}

;

Console.WriteLine();

Console.ReadKey();

/*System.Net.ServicePointManager.SecurityProtocol =
    System.Net.SecurityProtocolType.Tls12
    | System.Net.SecurityProtocolType.Tls
    | System.Net.SecurityProtocolType.Tls11;

try
{
    var resultat = Data.GetData(url: "https://www.banki.ru/products/currency/usd/");
    if (resultat != null)
    {
        Console.WriteLine(string.Join("\t", resultat));
    }

    Console.ReadKey();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}*/