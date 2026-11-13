
using RagEngine.Infrastructure.Web;

namespace RagEngine.Tests.Infrastructure.DataIngestion;

public sealed class WebContentCleanerTests
{
    private readonly WebContentCleaner _cleaner = new();

    [Fact]
    public void Clean_RemovesCookieBanner()
    {
        // Arrange
        const string markdown = """
            # Income Tax

            We use cookies to improve your experience on our website.

            Accept all cookies
            Reject optional cookies
            Manage cookie preferences

            Income Tax is charged on your income.
            """;

        // Act
        var result = _cleaner.Clean(markdown);

        // Assert
        Assert.Contains("# Income Tax", result);
        Assert.Contains("Income Tax is charged on your income.", result);

        Assert.DoesNotContain("We use cookies", result);
        Assert.DoesNotContain("Accept all cookies", result);
        Assert.DoesNotContain("Reject optional cookies", result);
        Assert.DoesNotContain("Manage cookie preferences", result);
    }

    [Fact]
    public void Clean_RemovesStandaloneNavigationItems()
    {
        // Arrange
        const string markdown = """
            Home
            Menu
            Search
            Sign in
            English
            Gaeilge

            # Tax credits

            Tax credits reduce the amount of tax you pay.
            """;

        // Act
        var result = _cleaner.Clean(markdown);

        // Assert
        Assert.Contains("# Tax credits", result);
        Assert.Contains("Tax credits reduce the amount of tax you pay.", result);

        Assert.DoesNotContain("Home", result);
        Assert.DoesNotContain("Menu", result);
        Assert.DoesNotContain("Search", result);
        Assert.DoesNotContain("Sign in", result);
        Assert.DoesNotContain("English", result);
        Assert.DoesNotContain("Gaeilge", result);
    }

    [Fact]
    public void Clean_RemovesBrowserUpdateMessages()
    {
        // Arrange
        const string markdown = """
            # Revenue Online Service

            Your browser is out of date. Please update your browser.

            You can manage your tax affairs online.
            """;

        // Act
        var result = _cleaner.Clean(markdown);

        // Assert
        Assert.Contains("# Revenue Online Service", result);
        Assert.Contains("You can manage your tax affairs online.", result);
        Assert.DoesNotContain("Your browser is out of date", result);
    }

    [Fact]
    public void Clean_PreservesHeadingsAndParagraphs()
    {
        // Arrange
        const string markdown = """
            # Income Tax

            ## How income tax works

            Income tax is charged on your taxable income.

            ### Tax rates

            Different rates apply depending on your income.
            """;

        // Act
        var result = _cleaner.Clean(markdown);

        // Assert
        Assert.Contains("# Income Tax", result);
        Assert.Contains("## How income tax works", result);
        Assert.Contains("### Tax rates", result);
        Assert.Contains("Income tax is charged on your taxable income.", result);
        Assert.Contains("Different rates apply depending on your income.", result);
    }

    [Fact]
    public void Clean_PreservesMarkdownTables()
    {
        // Arrange
        const string markdown = """
            # Tax rates

            | Income band | Rate |
            |---|---:|
            | First €40,000 | 20% |
            | Balance | 40% |
            """;

        // Act
        var result = _cleaner.Clean(markdown);

        // Assert
        Assert.Contains("| Income band | Rate |", result);
        Assert.Contains("| First €40,000 | 20% |", result);
        Assert.Contains("| Balance | 40% |", result);
    }

    [Fact]
    public void Clean_PreservesMarkdownLinks()
    {
        // Arrange
        const string markdown = """
            # Tax credits

            Read more about [tax credits](https://www.revenue.ie/en/personal-tax-credits-reliefs/index.aspx).
            """;

        // Act
        var result = _cleaner.Clean(markdown);

        // Assert
        Assert.Contains(
            "[tax credits](https://www.revenue.ie/en/personal-tax-credits-reliefs/index.aspx)",
            result);
    }

    [Fact]
    public void Clean_PreservesLegitimateCookieAndBrowserReferences()
    {
        // Arrange
        const string markdown = """
            # Website requirements

            Your browser must support modern security features.

            Cookie records may be retained in accordance with the applicable legislation.
            """;

        // Act
        var result = _cleaner.Clean(markdown);

        // Assert
        Assert.Contains("Your browser must support modern security features.", result);
        Assert.Contains("Cookie records may be retained", result);
    }

    [Fact]
    public void Clean_RemovesExcessiveBlankLines()
    {
        // Arrange
        const string markdown = """
            # Income Tax



            Income tax is charged on income.
            """;

        // Act
        var result = _cleaner.Clean(markdown);

        // Assert
        Assert.DoesNotContain("\n\n\n", result);
        Assert.Contains("# Income Tax", result);
        Assert.Contains("Income tax is charged on income.", result);
    }

    [Fact]
    public void Clean_RemovesConsecutiveDuplicateLines()
    {
        // Arrange
        const string markdown = """
            # Income Tax
            # Income Tax

            Income tax is charged on income.
            """;

        // Act
        var result = _cleaner.Clean(markdown);

        // Assert
        Assert.Equal(1, CountOccurrences(result, "# Income Tax"));
    }

    [Fact]
    public void Clean_ReturnsEmptyString_WhenInputIsEmpty()
    {
        Assert.Equal(string.Empty, _cleaner.Clean(string.Empty));
        Assert.Equal(string.Empty, _cleaner.Clean("   "));
    }

    [Fact]
    public void Clean_ThrowsArgumentNullException_WhenInputIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _cleaner.Clean(null!));
    }

    private static int CountOccurrences(string text, string value)
    {
        return text.Split(value).Length - 1;
    }
}
