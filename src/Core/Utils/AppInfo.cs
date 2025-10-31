using System.Reflection;

namespace Lampman.Core.Utils;

public static class AppInfo
{
    private static readonly Assembly? EntryAssembly;

    static AppInfo()
    {
        EntryAssembly = Assembly.GetEntryAssembly();
    }

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
}