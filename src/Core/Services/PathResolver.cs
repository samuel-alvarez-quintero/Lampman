using Lampman.Core.Models;

namespace Lampman.Core;

public static class PathResolver
{
    public static readonly string RootDir;
    public static readonly Dictionary<string, GitHubRegistryNamespace> DefaultGitHubRegistrySources;

    static PathResolver()
    {
        RootDir = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, @"."));

        DefaultGitHubRegistrySources = new Dictionary<string, GitHubRegistryNamespace>
        {
            { "official", new GitHubRegistryNamespace
                {
                    Owner = "samuel-alvarez-quintero",
                    RepoName = "lampman-official-registry",
                    Branch = "main"
                }
            },
        };
    }

    public static string RegistrySourcesFile => Path.Combine(RootDir, "registrySources.json");
    public static string StackFile => Path.Combine(RootDir, "stack.json");
    public static string RegistriesDir => Path.Combine(RootDir, "registries");
    public static string ServicesInstalledDir => Path.Combine(RootDir, "bin");

    public static string ServicePath(string serviceName, string version) =>
        Path.Combine(ServicesInstalledDir, serviceName, version);

    public static string BinDir => Path.Combine(RootDir, "bin");
}