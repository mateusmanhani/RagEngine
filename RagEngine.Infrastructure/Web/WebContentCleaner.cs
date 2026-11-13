
using System.Text;
using System.Text.RegularExpressions;
using RagEngine.Application.Interfaces;

namespace RagEngine.Infrastructure.Web;

public sealed class WebContentCleaner : IWebContentCleaner
{
    private static readonly Regex ArticleNavigation = new(
        @"^\s*\[(?:Next|Previous):",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex RelatedTopics = new(
        @"^\s*[-*]\s*\[Related topics\]",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex Published = new(
        @"^\s*_Published:",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly string[] FooterMarkers =
    [
        "[Back to homepage]",
        "[Back to top]"
    ];

    public string Clean(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);

        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        var lines = markdown
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .TrimStart('\uFEFF')
            .Split('\n');

        // Locate the first H1: the article starts here.
        var start = Array.FindIndex(
            lines,
            line => Regex.IsMatch(line.Trim(), @"^#\s+\S"));

        if (start < 0)
            return Normalize(markdown);

        var output = new StringBuilder();

        for (var i = start; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            // Stop before page-level navigation and footer content.
            if (ArticleNavigation.IsMatch(line) ||
                RelatedTopics.IsMatch(line) ||
                Published.IsMatch(line) ||
                FooterMarkers.Any(marker =>
                    line.StartsWith(marker, StringComparison.OrdinalIgnoreCase)))
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                AppendBlankLine(output);
                continue;
            }

            output.AppendLine(line);
        }

        return Normalize(output.ToString()).Trim();
    }

    private static void AppendBlankLine(StringBuilder output)
    {
        if (output.Length == 0)
            return;

        if (output.Length >= 2 &&
            output[^1] == '\n' &&
            output[^2] == '\n')
            return;

        output.AppendLine();
    }

    private static string Normalize(string content)
    {
        return Regex.Replace(
            content.Replace("\r\n", "\n").Replace('\r', '\n'),
            @"\n{3,}",
            "\n\n");
    }
}
