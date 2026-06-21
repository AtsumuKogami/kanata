namespace Kanata.Build.Infrastructure;

internal sealed class CommandLineOptions
{
    private readonly Dictionary<string, string?> _options = new(StringComparer.OrdinalIgnoreCase);

    private CommandLineOptions()
    {
    }

    public IReadOnlyList<string> Positionals { get; private set; } = [];

    public static CommandLineOptions Parse(IReadOnlyList<string> args)
    {
        var result = new CommandLineOptions();
        var positionals = new List<string>();

        for (var index = 0; index < args.Count; index++)
        {
            var arg = args[index];

            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                positionals.Add(arg);
                continue;
            }

            var option = arg[2..];
            var equalsIndex = option.IndexOf('=', StringComparison.Ordinal);

            if (equalsIndex >= 0)
            {
                result._options[option[..equalsIndex]] = option[(equalsIndex + 1)..];
                continue;
            }

            if (index + 1 < args.Count && !args[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                result._options[option] = args[index + 1];
                index++;
            }
            else
            {
                result._options[option] = null;
            }
        }

        result.Positionals = positionals;
        return result;
    }

    public bool HasFlag(string name)
    {
        return _options.ContainsKey(name);
    }

    public string? GetValue(string name)
    {
        return _options.TryGetValue(name, out var value) ? value : null;
    }
}
