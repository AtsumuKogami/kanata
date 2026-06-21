using System.Text;

namespace Kanata.Build.Infrastructure;

internal static class TextNaming
{
    public static string ToProjectId(string name)
    {
        var builder = new StringBuilder();
        var previousWasSeparator = false;

        foreach (var character in name.Trim().ToLowerInvariant())
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                builder.Append(character);
                previousWasSeparator = false;
                continue;
            }

            if (!previousWasSeparator && builder.Length > 0)
            {
                builder.Append('-');
                previousWasSeparator = true;
            }
        }

        return builder.ToString().Trim('-') is { Length: > 0 } id ? id : "game";
    }

    public static string ToPascalIdentifier(string name)
    {
        var builder = new StringBuilder();
        var capitalizeNext = true;

        foreach (var character in name)
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                builder.Append(capitalizeNext ? char.ToUpperInvariant(character) : character);
                capitalizeNext = false;
            }
            else
            {
                capitalizeNext = true;
            }
        }

        if (builder.Length == 0)
        {
            builder.Append("Game");
        }

        if (char.IsDigit(builder[0]))
        {
            builder.Insert(0, "Game");
        }

        return builder.ToString();
    }
}
