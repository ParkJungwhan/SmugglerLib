namespace TestConsole;

internal class Program
{
    private static Task Main(string[] args)
    {
        LoggerExample.Run();
        return Task.CompletedTask;
    }
}
