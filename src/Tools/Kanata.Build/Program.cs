using System.Text;
using Kanata.Build.Commands;

namespace Kanata.Build;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        try
        {
            if (args.Length == 0 || IsHelp(args[0]))
            {
                PrintHelp();
                return 0;
            }

            var command = args[0].ToLowerInvariant();
            var commandArgs = args.Skip(1).ToArray();

            return command switch
            {
                "new" => await NewGameCommand.RunAsync(commandArgs).ConfigureAwait(false),
                "validate" => await ValidateCommand.RunAsync(commandArgs).ConfigureAwait(false),
                "build" => await BuildCommand.RunAsync(commandArgs).ConfigureAwait(false),
                _ => UnknownCommand(command),
            };
        }
        catch (Exception exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"error: {exception.Message}");
            Console.ResetColor();
            return 1;
        }
    }

    private static bool IsHelp(string value)
    {
        return string.Equals(value, "help", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "--help", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "-h", StringComparison.OrdinalIgnoreCase);
    }

    private static int UnknownCommand(string command)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"Unknown command: {command}");
        Console.ResetColor();
        PrintHelp();
        return 1;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Kanata.Build");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  kanata new game <name> [--output <path>] [--id <id>] [--force]");
        Console.WriteLine("  kanata validate [project-file-or-directory]");
        Console.WriteLine("  kanata build [target] [configuration] [project-file-or-directory]");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  kanata new game MyGame");
        Console.WriteLine("  kanata validate MyGame/MyGame.kanata");
        Console.WriteLine("  kanata build desktop Debug MyGame/MyGame.kanata");
    }
}
