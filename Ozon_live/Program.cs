// See https://aka.ms/new-console-template for more information

using OpenQA.Selenium;
using Ozon_live;


var counter = 1;
var dataHelper = new DataHelper();
dataHelper.InitializationDriver();
var tasks = new List<Task>();
var cts = new CancellationTokenSource();
while (counter < 130)
{
    tasks.Add(dataHelper.ParsePageAsync(counter, cts).CancelOnFaulted(cts));
    counter++;
}

await Task.WhenAny(tasks);


Console.ReadKey();