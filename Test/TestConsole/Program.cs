namespace TestConsole;

internal class Program
{
    private static Task Main(string[] args)
    {
        LoggerExample.Run();
        DatabaseExample.Run();
        SerializationExample.Run();
        return Task.CompletedTask;
    }
}
