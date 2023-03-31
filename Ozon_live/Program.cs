// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Ozon_live;

Console.WriteLine("Hello, World!");

using var httpClient = new HttpClient();

var pathToFile = AppDomain.CurrentDomain.BaseDirectory + '\\';
IWebDriver driver = new ChromeDriver(pathToFile);

driver.Navigate().GoToUrl("https://www.ozon.ru/api/entrypoint-api.bx/page/json/v2?url=" +
                          "/search/?deny_category_prediction=true&from_global=true&" +
                          "text=%D0%B2%D0%B0%D0%BA%D1%83%D1%83%D0%BC%D0%B0%D1%82%D0%BE%D1%80" +
                          "&page_changed=true" +
                          "&layout_container=categorySearchMegapagination" +
                          "&layout_page_index=7&page=7");

var test = driver.FindElement(By.TagName("pre"));


var step1 = JsonConvert.DeserializeObject<JObject>(test.Text);
var globalJsonObject = JObject.Parse(step1.ToString());
var widgetState = (JObject)globalJsonObject["widgetStates"];
var props = widgetState.Properties().FirstOrDefault(x=>x.Name.Contains("searchResultsV2")).Name;
var catalog = JObject.Parse(widgetState[props].ToString());
var items = catalog["items"];
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