using Lampman.Core.Models;

namespace Lampman.Core;

public static class PathResolver
{
    public static readonly string RootDir;
    public static readonly Dictionary<string, RegistryNamespace> DefaultRegistryNamespace;
    public static readonly Dictionary<string, RegistrySource> DefaultRegistrySource;

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

        DefaultRegistryNamespace = new Dictionary<string, RegistryNamespace>
        {
            { "official", new RegistryNamespace
                {
                    Version = "0.2.1",
                    Url = "https://github.com/samuel-alvarez-quintero/lampman-official-registry/releases/download/v0.2.1/lamp.json",
                    Description = "Official Windows Lamp services",
                    Checksum = new Dictionary<string, string>{
                        { "SHA256", "953a852b3ad6a5015f2ef0abbe131f91738f2f44d6443ee6b07a2f0bf9b184bf" }
                    }
                }
            },
        };

        DefaultRegistrySource = new Dictionary<string, RegistrySource>
        {
            { "official", new RegistrySource
                {
                    Source = "https://github.com/samuel-alvarez-quintero/lampman-official-registry.git",
                    Branch = "main"
                }
            },
        };
    }

    public static string SourcesFile => Path.Combine(RootDir, "sources.json");
    public static string RegistryFile => Path.Combine(RootDir, "registry.json");
    public static string ServicesFile => Path.Combine(RootDir, "services.json");
    public static string StackFile => Path.Combine(RootDir, "stack.json");

    public static string ServicesInstallDir => Path.Combine(RootDir, "services");

    public static string ServicePath(string serviceName, string version) =>
        Path.Combine(ServicesInstallDir, serviceName, version);

    public static string BinDir => Path.Combine(RootDir, "bin");
}