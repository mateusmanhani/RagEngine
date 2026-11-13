
using System.Text;
using System.Text.RegularExpressions;
using RagEngine.Application.Interfaces;

namespace RagEngine.Infrastructure.Web;

public sealed class WebContentCleaner : IWebContentCleaner
{
    private static readonly HashSet<string> BoilerplateLines =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Skip to main content",
            "Skip to content",
            "Home",
            "Menu",
            "Search",
            "Sign in",
            "Sign in to ROS",
            "Log in",
            "Log in to ROS",
            "English",
            "Gaeilge",
            "Accessibility",
            "Privacy",
            "Privacy statement",
            "Cookie policy",
            "Cookie settings",
            "Manage cookies",
            "Back to top"
        };

    private static readonly string[] BoilerplatePhrases =
    [
        "accept all cookies",
        "reject optional cookies",
        "manage cookie preferences",
        "we use cookies to",
        "this website uses cookies",
        "your browser is out of date",
        "your browser is not supported",
        "please update your browser",
        "upgrade your browser"
    ];

    public string Clean(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);

        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        var normalized = markdown
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .TrimStart('\uFEFF');

        var output = new StringBuilder();
        var previousLine = string.Empty;

        foreach (var rawLine in normalized.Split('\n'))
        {
            var line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line))
            {
                AppendBlankLine(output);
                previousLine = string.Empty;
                continue;
            }

            if (IsBoilerplate(line))
                continue;

            // Remove consecutive duplicate lines, which can occur in
            // scraped navigation and repeated page chrome.
            if (string.Equals(line, previousLine, StringComparison.Ordinal))
                continue;

            output.AppendLine(line);
            previousLine = line;
        }

        return NormalizeBlankLines(output.ToString()).Trim();
    }

    private static bool IsBoilerplate(string line)
    {
        // Exact matches are deliberately used for navigation labels.
        if (BoilerplateLines.Contains(line))
            return true;

        var normalized = Regex.Replace(line, @"\s+", " ").Trim();
        var lower = normalized.ToLowerInvariant();

        // Remove standalone cookie/browser notices, not paragraphs
        // that discuss cookies or browser requirements as actual guidance.
        if (BoilerplatePhrases.Any(lower.Contains))
        {
            return line.Length < 250;
        }

        return false;
    }

    private static void AppendBlankLine(StringBuilder output)
    {
        if (output.Length == 0)
            return;

        if (output.Length >= 2 &&
            output[^1] == '\n' &&
            output[^2] == '\n')
        {
            return;
        }

        output.AppendLine();
    }

    private static string NormalizeBlankLines(string content)
    {
        return Regex.Replace(content, @"\n{3,}", "\n\n");
    }
}
