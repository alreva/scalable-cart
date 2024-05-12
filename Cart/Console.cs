#define VERBOSE

namespace Cart;

public static class Console
{
    public static void WriteLine(string message)
    {
#if VERBOSE
        WriteLineAnyways(message);
#endif
    }

    public static void WriteLineAnyways(string message)
    {
        System.Console.WriteLine($"{DateTimeOffset.Now:O}: {message}");
    }

    public static string? ReadLine()
    {
        return System.Console.ReadLine();
    }
}