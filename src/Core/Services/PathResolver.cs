using Lampman.Core.Models;

namespace Lampman.Core;

public static class PathResolver
{
    public static readonly string RootDir;
    public static readonly Dictionary<string, RegistryNamespace> DefaultRegistrySources;

    static PathResolver()
    {
        // 1. Check for install.json in production path
        string defaultInstallRoot = @"C:/Lampman";
        string installFile = Path.Combine(defaultInstallRoot, "install.json");

        if (File.Exists(installFile))
        {
            // Production mode
            RootDir = defaultInstallRoot;
        }
        else
        {
            // Dev mode → relative to solution/project
            RootDir = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, @"."));
        }

        DefaultRegistrySources = new Dictionary<string, RegistryNamespace>
        {
            { "official", new RegistryNamespace
                {
                    Source = "https://github.com/samuel-alvarez-quintero/lampman-official-registry.git",
                    Branch = "main"
                }
            },
        };
    }

    public static string RegistrySourcesFile => Path.Combine(RootDir, "registrySources.json");
    public static string StackFile => Path.Combine(RootDir, "stack.json");
    public static string RegistriesDir => Path.Combine(RootDir, "registries");
    public static string ServicesInstalledDir => Path.Combine(RootDir, "servicesInstalled");

    public static string ServicePath(string serviceName, string version) =>
        Path.Combine(ServicesInstalledDir, serviceName, version);

    public static string BinDir => Path.Combine(RootDir, "bin");
}