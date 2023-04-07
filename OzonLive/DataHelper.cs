using System.Diagnostics;
using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using UndetectedChromeDriver = SeleniumUndetectedChromeDriver.UndetectedChromeDriver;
using xNet;

namespace OzonLive;

public class DataHelper
{
    private IWebDriver _driver = null!;
    private readonly Guid _guidSession = Guid.NewGuid();
    private bool _isProxy = false;
    private readonly List<string> _proxies = new List<string>();
    private readonly string _pathToFile = AppDomain.CurrentDomain.BaseDirectory + '\\';
    private int _counterProxy;

    public DataHelper()
    {
        ReadProxyFromFile();
        Console.WriteLine($"Create session with GUID - {_guidSession}");
    }

    public void InitializationDriver(bool isNewDriver = false)
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--disable-blink-features=AutomationControlled");

        if (_isProxy && !isNewDriver)
        {
            Console.WriteLine($"Проверяем {_proxies[_counterProxy]}");

            while (!HttpCheckerWorker(_proxies[_counterProxy]))
            {
                Console.WriteLine($"Proxy {_proxies[_counterProxy]} не работает");
                if (_counterProxy == _proxies.Count - 1)
                {
                    _counterProxy = 0;
                }

                _counterProxy++;
                
                Console.WriteLine($"Переключаем прокси на {_proxies[_counterProxy]} и проверяем");
            }

            Console.WriteLine($"Работаем на proxy {_proxies[_counterProxy]}");
            options.AddArgument($"--proxy-server={_proxies[_counterProxy]}");
            options.PageLoadStrategy = PageLoadStrategy.Normal;

            _counterProxy++;
        }

        options.AddArgument(
            "user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.5563.147 Safari/537.36");

        _driver = UndetectedChromeDriver.Create(
            driverExecutablePath: $"{_pathToFile}\\chromedriver.exe",
            configureService: service =>
            {
                service.EnableVerboseLogging = false;
                service.SuppressInitialDiagnosticInformation = true;
                service.HideCommandPromptWindow = true;
            },
            headless: true,
            options: options
        );
    }


    public async Task<bool> ParsePageAsync(int pageIndex, string nameRequest, int maxCountReloadDriver,
        CancellationTokenSource cts)
    {
        cts.Token.ThrowIfCancellationRequested();
        try
        {
            Console.WriteLine($"Начало работы со страницей {pageIndex}");

            var items = ParceStartData(false, pageIndex, nameRequest);

            if (pageIndex % 10 == 0)
            {
                _isProxy = true;
                _driver.Close();
                _driver.Dispose();
                InitializationDriver();
            }

            var counterReload = 1;
            while (items == null && counterReload <= maxCountReloadDriver)
            {
                Thread.Sleep(new Random().Next(1000, 3000));
                Console.WriteLine(
                    $"Задержка загрузки данных со страницы {pageIndex}, перезагрузка драйвера. Попытка {counterReload} из {maxCountReloadDriver}");
                items = ParceStartData(true, pageIndex, nameRequest);
                counterReload++;
            }

            if (items == null)
            {
                Console.WriteLine(
                    $"Не удалось получить запрос с {maxCountReloadDriver} попыток от сервера, страница {pageIndex} не обработана");
                return false;
            }

            var outputData = new List<OutputData>();
            var numberSearchResult = pageIndex == 1 ? 1 : (pageIndex - 1) * 36 - 1;
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

            if (outputData.Count == 0)
            {
                Console.WriteLine($"Программа завершила работу с последней страницей - {pageIndex}");
                cts.Cancel();
                _driver.Close();
                _driver.Dispose();
                return await Task.Run(() => true);
            }
            
            await new Writer().WriteToExcel($"{nameRequest}_{_guidSession}", $"page_{pageIndex}", outputData);
            Console.WriteLine($"Конец работы со страницей {pageIndex}");
            return await Task.Run(() => true);
        }
        catch (NoSuchElementException)
        {
            Console.WriteLine("Проверка cloudFlare не пройдена");
            Console.WriteLine("Переключаем proxy");
            _isProxy = true;
            _driver.Close();
            _driver.Dispose();
            InitializationDriver();
            await ParsePageAsync(pageIndex, nameRequest, maxCountReloadDriver, cts);
        }

        catch (Exception e)
        {
            Console.WriteLine($"Конец работы со страницей {pageIndex} c ошибкой");
            Console.WriteLine(e);
            throw;
        }

        return await Task.Run(() => true);
    }

    private JToken? ParceStartData(bool isNewDriver, int pageIndex = 0, string nameRequest = "")
    {
        if (isNewDriver)
        {
            _driver.Close();
            _driver.Dispose();
            InitializationDriver(isNewDriver);
        }

        _driver.Navigate().GoToUrl("https://www.ozon.ru/api/entrypoint-api.bx/page/json/v2?url=" +
                                   "/search/?deny_category_prediction=true&from_global=true&" +
                                   $"text={nameRequest}" +
                                   "&page_changed=true" +
                                   "&layout_container=categorySearchMegapagination" +
                                   $"&layout_page_index={pageIndex}&page={pageIndex}");

        Thread.Sleep(new Random().Next(3000, 5000));

        var test = _driver.FindElement(By.TagName("pre"));
        try
        {
            var step1 = JsonConvert.DeserializeObject<JObject>(test.Text);
            var globalJsonObject = JObject.Parse(step1?.ToString()!);
            var widgetState = (JObject) globalJsonObject["widgetStates"]!;
            var searchResultsPropertyName =
                widgetState.Properties().FirstOrDefault(x => x.Name.Contains("searchResultsV2"))?.Name;
            var searchResultsItem = JObject.Parse(widgetState[searchResultsPropertyName!]?.ToString()!);
            if (!searchResultsItem.HasValues && _isProxy)
            {
                Console.WriteLine("С этого proxy не приходят данные");
                return ParceStartData(true, pageIndex, nameRequest);
            }

            return searchResultsItem["items"];
        }
        catch
        {
            Console.WriteLine("С этого proxy не приходят нужные данные");
            return ParceStartData(true, pageIndex, nameRequest);
        }
    }

    private void ReadProxyFromFile()
    {
        try
        {
            using var reader = new StreamReader($"{_pathToFile}\\proxiesList.txt");

            var proxy = reader.ReadLine();
            while (proxy != null)
            {
                _proxies.Add(proxy);
                proxy = reader.ReadLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private bool HttpCheckerWorker(string proxy) // Чекер HTTP прокси
    {
        try
        {
            var checkerHttpRequest = new HttpRequest(); // Создаю запрос
            checkerHttpRequest.UserAgent = Http.ChromeUserAgent(); // Задаю параметр UserAgent
            checkerHttpRequest.KeepAlive = false;
            checkerHttpRequest.ConnectTimeout =
                1000; // Задаю Timeout подключения к ресурсу делёный на 2, так как таймаут подключения к прокси отдельный
            checkerHttpRequest.Proxy = HttpProxyClient.Parse(proxy); // Задаю прокси
            checkerHttpRequest.Proxy.ConnectTimeout = 1000; // Задаю Timeout подключения к прокси
            checkerHttpRequest.IgnoreProtocolErrors = true;
            try
            {
                checkerHttpRequest.Get("https://www.google.com");
                checkerHttpRequest.Close();
                checkerHttpRequest.Dispose();
                return checkerHttpRequest.Response.IsOK;
            }
            catch (Exception ex) // Отлавливаю остальные исключения
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }
}