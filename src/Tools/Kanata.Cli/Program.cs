using System.Text;
using Kanata.Build.Commands;
using Kanata.Cli.Commands;

namespace Kanata.Cli;

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
                "create" => await NewGameCommand.RunCreateAsync(commandArgs).ConfigureAwait(false),
                "new" => await NewGameCommand.RunAsync(commandArgs).ConfigureAwait(false),
                "validate" => await ValidateCommand.RunAsync(commandArgs).ConfigureAwait(false),
                "restore" => await RestoreCommand.RunAsync(commandArgs).ConfigureAwait(false),
                "generate" => await GenerateCommand.RunAsync(commandArgs).ConfigureAwait(false),
                "build" => await BuildCommand.RunAsync(commandArgs).ConfigureAwait(false),
                "play" => await PlayCommand.RunAsync(commandArgs).ConfigureAwait(false),
                "engine" => await EngineCommand.RunAsync(commandArgs).ConfigureAwait(false),
                "package" => await PackageCommand.RunAsync(commandArgs).ConfigureAwait(false),
                "tool" => await ToolCommand.RunAsync(commandArgs).ConfigureAwait(false),
                "version" => VersionCommand.Run(),
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
        Console.WriteLine("Kanata");
        Console.WriteLine();
        Console.WriteLine("Bootstrap commands:");
        Console.WriteLine(" kanata package info <file.kpkg>");
        Console.WriteLine(" kanata package verify <file.kpkg> [--fast]");
        Console.WriteLine(" kanata package pack <source-folder> -o <output.kpkg> [--force]");
        Console.WriteLine(" kanata package install <file.kpkg> [--force]");
        Console.WriteLine(" kanata package list");
        Console.WriteLine(" kanata package inspect [package-or-installable-id]");
        Console.WriteLine(" kanata tool list");
        Console.WriteLine(" kanata tool inspect <tool-id>");
        Console.WriteLine(" kanata version");
        Console.WriteLine();
        Console.WriteLine("Project and build commands currently routed directly, later supplied by tool packages:");
        Console.WriteLine(" kanata create <name> [--output <path>] [--id <id>] [--force]");
        Console.WriteLine(" kanata new <template> [--output <path>] [--id <id>] [--force]");
        Console.WriteLine(" kanata new game <name> [--output <path>] [--id <id>] [--force]");
        Console.WriteLine(" kanata validate");
        Console.WriteLine(" kanata restore [target] [configuration] [--force-engine]");
        Console.WriteLine(" kanata generate [target] [configuration] [--force-engine]");
        Console.WriteLine(" kanata build [target] [configuration] [--force-engine]");
        Console.WriteLine(" kanata play [target] [configuration] [--force-engine]");
        Console.WriteLine(" kanata engine build [configuration] [--force]");
        Console.WriteLine(" kanata engine status [configuration]");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine(" kanata package list");
        Console.WriteLine(" kanata package inspect kanata.backend.monogame");
        Console.WriteLine(" kanata tool list");
        Console.WriteLine(" kanata tool inspect example.engineer");
        Console.WriteLine(" kanata create MyGame");
        Console.WriteLine(" cd MyGame");
        Console.WriteLine(" kanata validate");
        Console.WriteLine(" kanata build");
        Console.WriteLine(" kanata play");
    }
}
