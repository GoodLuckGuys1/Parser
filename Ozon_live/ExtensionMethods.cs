namespace Ozon_live;

public static class ExtensionMethods
{
    public static Task CancelOnFaulted(this Task task, CancellationTokenSource cts)
    {
        task.ContinueWith(_ => cts.Cancel(), cts.Token, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
        return task;
    }

    public static Task<T> CancelOnFaulted<T>(this Task<T> task, CancellationTokenSource cts)
    {
        task.ContinueWith(_ => cts.Cancel(), cts.Token, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
        return task;
    }
}