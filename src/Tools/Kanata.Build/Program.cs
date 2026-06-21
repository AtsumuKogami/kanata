using Kanata.Build.Commands;

return await RunAsync(args);

static async Task<int> RunAsync(string[] args)
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
        "validate" => await new ValidateCommand().RunAsync(commandArgs),
        _ => UnknownCommand(command)
    };
}

static bool IsHelp(string value)
{
    return value is "-h" or "--help" or "help";
}

static int UnknownCommand(string command)
{
    Console.Error.WriteLine($"Unknown command: {command}");
    PrintHelp();
    return 1;
}

static void PrintHelp()
{
    Console.WriteLine("Kanata.Build");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  kanata validate [project-file-or-directory]");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  validate   Validate a .kanata project file.");
}
