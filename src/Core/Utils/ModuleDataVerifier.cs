using System.Text.RegularExpressions;

namespace Lampman.Core.Utils;

public static partial class ModuleDataVerifier
{
    // Verify GitHub URI source
    [GeneratedRegex(
        @"^(?:https?|git|ssh):\/\/(?:www\.)?github\.com\/[a-zA-Z0-9-]+\/[a-zA-Z0-9-._]+(?:\.git)?(?:\/.*)?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        matchTimeoutMilliseconds: 1000
    )]
    private static partial Regex UriGitHubSource();

    /// <summary>
    /// Verify GitHub URI source
    /// </summary>
    public static bool IsValidGitHubSource(string uri)
    {
        return UriGitHubSource().IsMatch(uri);
    }
}