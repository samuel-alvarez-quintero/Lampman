using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

using Lampman.Core.Models;
using Lampman.Core.Utils;

using Octokit;

namespace Lampman.Core.Services;

public class RegistryManager
{
    // ANSI escape codes for colors
    const string ANSI_RED = "\u001B[31m";
    const string ANSI_GREEN = "\u001B[32m";
    const string ANSI_BLUE = "\e[0;34m";
    const string ANSI_YELLOW = "\e[0;33m";
    const string ANSI_RESET = "\u001B[0m"; // Resets all formatting

    public HttpClient HttpBrowserClient;

    private readonly CompressedFileHandler _compressFileHandler;
    private readonly GitHubClient _gitHubClient;

    public RegistryManager(HttpClient? httpClient = null)
    {
        HttpBrowserClient = httpClient ?? new BrowserClient();
        _compressFileHandler = new CompressedFileHandler(HttpBrowserClient);

        string? version = Assembly.GetExecutingAssembly()
                      .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                      .InformationalVersion;

        _gitHubClient = new GitHubClient(new Connection(
            productInformation: new ProductHeaderValue("Lampman", version),
            baseAddress: GitHubClient.GitHubApiUrl
        ));
    }

    private static void EnsureDefaultConfig()
    {
        if (!File.Exists(PathResolver.RegistrySourcesFile))
            File.WriteAllText(
                PathResolver.RegistrySourcesFile,
                JsonSerializer.Serialize(PathResolver.DefaultGitHubRegistrySources, new JsonSerializerOptions { WriteIndented = true })
            );
    }

    public void ListRegistrySources(bool verbose = false)
    {
        EnsureDefaultConfig();

        var registries = JsonSerializer.Deserialize<Dictionary<string, GitHubRegistryNamespace>>(File.ReadAllText(PathResolver.RegistrySourcesFile));
        Console.WriteLine($"{ANSI_BLUE}[INFO] Configured registries:{ANSI_RESET}");

        if (registries == null || registries.Count == 0)
        {
            Console.WriteLine($"{ANSI_YELLOW}[WARNING] No registries configured.{ANSI_RESET}");
            return;
        }

        foreach (var (ns, entry) in registries)
        {
            Console.WriteLine($"{ANSI_BLUE}[INFO] {ns}:{GitHubClient.GitHubApiUrl}repos/{entry.Owner}/{entry.RepoName}{ANSI_RESET}");

            if (verbose)
            {
                Console.WriteLine($"{ANSI_BLUE}[INFO] Tag: {entry.Tag ?? "(none)"}{ANSI_RESET}");
                Console.WriteLine($"{ANSI_BLUE}[INFO] Description: {entry.Description ?? "(none)"}{ANSI_RESET}");
                Console.WriteLine($"{ANSI_BLUE}[INFO] Last Request: {entry.LastRequest?.ToString() ?? "(none)"}{ANSI_RESET}");
            }
        }
    }

    public void AddRegistrySource(string ns, string owner, string repoName, string branch, bool verbose = false)
    {
        EnsureDefaultConfig();

        if (!Regex.IsMatch(ns, "^[a-z0-9\\-]+$"))
        {
            Console.WriteLine($"Invalid namespace format: {ns}");
            return;
        }

        var registries = JsonSerializer.Deserialize<Dictionary<string, GitHubRegistryNamespace>>(File.ReadAllText(PathResolver.RegistrySourcesFile)) ?? new();

        if (registries.ContainsKey(ns))
        {
            Console.WriteLine($"Namespace already exists: {ns}");
            return;
        }

        registries[ns] = new GitHubRegistryNamespace
        {
            Owner = owner,
            RepoName = repoName,
            Branch = branch,
            LastRequest = null
        };

        File.WriteAllText(PathResolver.RegistrySourcesFile, JsonSerializer.Serialize(registries, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Added registry @{ns}:{owner}/{repoName}");
    }

    public void RemoveRegistrySource(string ns, bool verbose = false)
    {
        EnsureDefaultConfig();

        var registries = JsonSerializer.Deserialize<Dictionary<string, GitHubRegistryNamespace>>(File.ReadAllText(PathResolver.RegistrySourcesFile));
        if (registries == null || !registries.Remove(ns))
        {
            Console.WriteLine($"{ANSI_YELLOW}[WARNING] Registry not found: {ns}{ANSI_RESET}");
            return;
        }

        File.WriteAllText(PathResolver.RegistrySourcesFile, JsonSerializer.Serialize(registries, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"{ANSI_GREEN}[SUCCESS] Removed registry @{ns}{ANSI_RESET}");
    }

    public async Task FetchRegistrySources(bool force = false, bool verbose = false)
    {
        EnsureDefaultConfig();

        // Load existing sources
        var sources = JsonSerializer.Deserialize<Dictionary<string, GitHubRegistryNamespace>>(File.ReadAllText(PathResolver.RegistrySourcesFile));
        if (sources == null || sources.Count == 0)
        {
            Console.WriteLine($"{ANSI_YELLOW}[WARNING] No sources configured.{ANSI_RESET}");
            return;
        }

        foreach (var (ns, entry) in sources)
        {
            try
            {
                Console.WriteLine($"{ANSI_BLUE}[INFO] Fetching source @{ns}:{entry.Owner}/{entry.RepoName}{ANSI_RESET}");

                // Get latest release and tags
                var latestRelease = await _gitHubClient.Repository.Release.GetLatest(entry.Owner, entry.RepoName);
                var tags = await _gitHubClient.Repository.GetAllTags(entry.Owner, entry.RepoName);

                if (null != latestRelease)
                {
                    // Update registry description with latest info
                    entry.Description = latestRelease.Body;

                    string releaseTagName = latestRelease.TagName;

                    if (null != tags)
                    {
                        var tag = tags.First(_t => _t.Name == releaseTagName);
                        if (null != tag)
                        {
                            // Update tag info
                            entry.Tag = tag.Name;

                            Console.WriteLine($"{ANSI_BLUE}[INFO] Found tag: {tag.Name}{ANSI_RESET}");

                            // Create cache zip directory if doesn't exists
                            string cacheTargetDir = Path.Combine(PathResolver.RegistriesDir, "cache");
                            if (!Directory.Exists(cacheTargetDir))
                            {
                                Console.WriteLine($"{ANSI_BLUE}[INFO] Creating cache directory: {cacheTargetDir}{ANSI_RESET}");
                                Directory.CreateDirectory(cacheTargetDir);
                            }

                            string cacheZipPath = Path.Combine(cacheTargetDir, $"{ns}-{tag.Name}.zip");

                            // Download and unzip if not already cached
                            if (!File.Exists(cacheZipPath))
                            {
                                Console.WriteLine($"{ANSI_BLUE}[INFO] Download and extract {tag.ZipballUrl}...{ANSI_RESET}");
                                await _compressFileHandler.DownloadFileAsync(tag.ZipballUrl, cacheZipPath);
                                _compressFileHandler.UnzipFile(cacheZipPath, cacheTargetDir);
                            }
                            else
                            {
                                Console.WriteLine($"{ANSI_YELLOW}[WARNING] Using cached zip: {cacheZipPath}{ANSI_RESET}");
                                _compressFileHandler.UnzipFile(cacheZipPath, cacheTargetDir);
                            }

                            List<string>? directoryNames = _compressFileHandler.GetDirectoryNames(cacheZipPath);
                            if (null != directoryNames && directoryNames.Count > 0)
                            {
                                // Clear existing target directory
                                string targetDir = Path.Combine(PathResolver.RegistriesDir, "namespaces", ns);
                                if (Directory.Exists(targetDir) && Directory.EnumerateFileSystemEntries(targetDir).Any())
                                {
                                    Directory.Delete(targetDir, true);
                                    Directory.CreateDirectory(targetDir);
                                }
                                else if (!Directory.Exists(targetDir))
                                {
                                    Directory.CreateDirectory(targetDir);
                                }

                                // Move extracted directory to target
                                foreach (var dirName in directoryNames)
                                {
                                    string sourceDir = Path.Combine(cacheTargetDir, dirName);
                                    if (Directory.Exists(sourceDir))
                                    {
                                        // Move all contents from sourceDir to targetDir
                                        foreach (var dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
                                            Directory.CreateDirectory(dirPath.Replace(sourceDir, targetDir));

                                        foreach (var newPath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
                                            File.Copy(newPath, newPath.Replace(sourceDir, targetDir), true);

                                        // Clean up extracted source directory
                                        Directory.Delete(sourceDir, true);
                                    }
                                    else
                                    {
                                        throw new Exception($"Expected directory not found after extraction: {sourceDir}");
                                    }
                                }
                            }
                            else
                            {
                                throw new Exception("No directories found in the zip file.");
                            }

                            // Update the last request date
                            entry.LastRequest = DateTime.Now;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ANSI_RED}[ERROR] Failed to fetch @{ns}:{entry.Owner}/{entry.RepoName}: {ex.Message}{ANSI_RESET}");
            }
        }

        File.WriteAllText(PathResolver.RegistrySourcesFile, JsonSerializer.Serialize(sources, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"{ANSI_GREEN}[SUCCESS] {PathResolver.RegistrySourcesFile} -> refreshed {ANSI_RESET}");
    }
}