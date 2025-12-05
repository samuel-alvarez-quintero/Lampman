using System.Reflection;

namespace Lampman.Core.Utils;

public static class AppInfo
{
    private static readonly Assembly? EntryAssembly = Assembly.GetEntryAssembly();

    private static readonly Dictionary<string, string[]> OSDirectories =
    new() {
        { "windows", [ "Win32NT-x64", "Win32NT-x32" ] },
        { "linux", [ "Unix-x64", "Unix-x32" ] },
    };

    public static string? GetName()
    {
        return EntryAssembly?.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
    }
    public static string? GetVersion()
    {
        return EntryAssembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    }

    public static string? GetUserAgent()
    {
        return $"{GetName()}/{GetVersion()}";
    }

    public static string GetOSDirectoryName()
    {
        string? version = Environment.OSVersion.Version.ToString().Split(".").First();
        string? arch = Environment.Is64BitOperatingSystem ? "x64" : "x32";
        string target = $"{Environment.OSVersion.Platform}-{arch}";
        string targetVersion = $"{Environment.OSVersion.Platform}-{version}-{arch}";
        var osDir = OSDirectories.FirstOrDefault(item => Array.Exists(item.Value, platform => target == platform || targetVersion == platform));

        return osDir.Key;
    }
}