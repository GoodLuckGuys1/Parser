using OpenQA.Selenium;
using OzonLive;

try
{
    while (true)
    {
        Console.WriteLine("Введите ключевые слова для поиска::");
        var nameRequest = Console.ReadLine();
        while (string.IsNullOrEmpty(nameRequest))
        {
            Console.WriteLine("Введите ключевые слова для поиска:");
            nameRequest = Console.ReadLine();
        }

        Console.WriteLine("Введите номер максимальной страницы (default - до последней). Нажмите Enter для значения по умолчанию:");
        var maxNumber = Console.ReadLine();

        if (string.IsNullOrEmpty(maxNumber))
            maxNumber = "0";

        int maxNumberInt;

        while (!int.TryParse(maxNumber, out maxNumberInt))
        {
            Console.WriteLine("Введите номер максимальной страницы (default - до последней). Нажмите Enter для значения по умолчанию:");
            maxNumber = Console.ReadLine();
            if (string.IsNullOrEmpty(maxNumber)) maxNumber = "0";
        }

        Console.WriteLine("Введите максимальное количество попыток запроса (default - 10). Нажмите Enter для значения по умолчанию:");
        var maxNumberReloadDriver = Console.ReadLine();

        if (string.IsNullOrEmpty(maxNumberReloadDriver))
            maxNumberReloadDriver = "10";

        int maxNumberReloadDriverInt;

        while (!int.TryParse(maxNumberReloadDriver, out maxNumberReloadDriverInt))
        {
            Console.WriteLine("Введите максимальное количество попыток запроса (default - 10). Нажмите Enter для значения по умолчанию:");
            maxNumberReloadDriver = Console.ReadLine();
            if (string.IsNullOrEmpty(maxNumberReloadDriver)) maxNumberReloadDriver = "10";
        }

        var counter = 1;
        var dataHelper = new DataHelper();
        dataHelper.InitializationDriver(false);
        var tasks = new List<Task>();
        var cts = new CancellationTokenSource();
        if (maxNumberInt == 0)
        {
            while (!cts.IsCancellationRequested)
            {
                tasks.Add(dataHelper.ParsePageAsync(counter, nameRequest, maxNumberReloadDriverInt, cts)
                    .CancelOnFaulted(cts));
                counter++;
            }
        }
        else
        {
            while (counter <= maxNumberInt)
            {
                tasks.Add(dataHelper.ParsePageAsync(counter, nameRequest, maxNumberReloadDriverInt, cts)
                    .CancelOnFaulted(cts));
                counter++;
            }
        }
        
        await Task.WhenAll(tasks);
        
        Console.WriteLine("Программа завершила работу");
        Console.WriteLine();
        Console.WriteLine("Для нового поиска нажмите Enter");
        Console.WriteLine();
        Console.ReadKey();
    }
}
catch (NoSuchElementException)
{
    Console.WriteLine("Программа завершила работу с последней страницей");
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine("Для нового поиска нажмите Enter");
    Console.WriteLine();
    Console.ReadKey();
}


