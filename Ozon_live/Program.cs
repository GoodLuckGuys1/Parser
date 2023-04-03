// See https://aka.ms/new-console-template for more information

using OpenQA.Selenium;
using Ozon_live;

Console.WriteLine("Enter good for search:");
var nameRequest = Console.ReadLine();
while (string.IsNullOrEmpty(nameRequest))
{
    Console.WriteLine("Enter good for search:");
    nameRequest = Console.ReadLine();
}

Console.WriteLine("Enter max page number (default 100):");
var maxNumber = Console.ReadLine();

if (string.IsNullOrEmpty(maxNumber))
    maxNumber = "100";

int maxNumberInt;

while (!int.TryParse(maxNumber, out maxNumberInt))
{
    Console.WriteLine("Enter max page number (default 100). Press Enter for the set default value:");
}

Console.WriteLine("Enter max count reloaded driver (default 10). Press Enter for the set default value:");
var maxNumberReloadDriver = Console.ReadLine();

if (string.IsNullOrEmpty(maxNumberReloadDriver))
    maxNumberReloadDriver = "10";

int maxNumberReloadDriverInt;

while (!int.TryParse(maxNumberReloadDriver, out maxNumberReloadDriverInt))
{
    Console.WriteLine("Enter max count reloaded driver (default 10). Press Enter for the set default value:");
}

var counter = 1;
var dataHelper = new DataHelper();
dataHelper.InitializationDriver();
var tasks = new List<Task>();
var cts = new CancellationTokenSource();
while (counter <= maxNumberInt)
{
    tasks.Add(dataHelper.ParsePageAsync(counter, nameRequest, maxNumberReloadDriverInt, cts).CancelOnFaulted(cts));
    counter++;
}

await Task.WhenAny(tasks);

Console.ReadKey();