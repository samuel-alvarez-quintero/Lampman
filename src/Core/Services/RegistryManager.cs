using System.Text.Json;
using System.Text.RegularExpressions;

using Lampman.Core.Models;
using Lampman.Core.Utils;

using Microsoft.Win32;

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

    public RegistryManager(HttpClient? httpClient = null)
    {
        HttpBrowserClient = httpClient ?? new BrowserClient();
        _compressFileHandler = new CompressedFileHandler(HttpBrowserClient);
    }

    private static void EnsureDefaultConfig()
    {
        if (!File.Exists(PathResolver.RegistrySourcesFile))
            File.WriteAllText(PathResolver.RegistrySourcesFile, JsonSerializer.Serialize(PathResolver.DefaultRegistrySources, new JsonSerializerOptions { WriteIndented = true }));
    }

    public void ListRegistrySources(bool verbose = false)
    {
        EnsureDefaultConfig();

        var registries = JsonSerializer.Deserialize<Dictionary<string, RegistryNamespace>>(File.ReadAllText(PathResolver.RegistrySourcesFile));
        Console.WriteLine($"{ANSI_BLUE}[INFO] Configured registries:{ANSI_RESET}");

        if (registries == null || registries.Count == 0)
        {
            Console.WriteLine($"{ANSI_YELLOW}[WARNING] No registries configured.{ANSI_RESET}");
            return;
        }

        foreach (var kv in registries)
        {
            var ns = kv.Key;
            var entry = kv.Value;

            Console.WriteLine($"{ANSI_BLUE}[INFO] {ns}:{entry.Source}{ANSI_RESET}");

            if (verbose)
            {
                Console.WriteLine($"{ANSI_BLUE}[INFO] Tag: {entry.Tag ?? "(none)"}{ANSI_RESET}");
                Console.WriteLine($"{ANSI_BLUE}[INFO] Description: {entry.Description ?? "(none)"}{ANSI_RESET}");
                Console.WriteLine($"{ANSI_BLUE}[INFO] Last Request: {entry.LastRequest?.ToString() ?? "(none)"}{ANSI_RESET}");
            }
        }
    }

    public void AddRegistrySource(string ns, string sourceURL, string branch, bool verbose = false)
    {
        EnsureDefaultConfig();

        if (!Regex.IsMatch(ns, "^[a-z0-9\\-]+$"))
        {
            Console.WriteLine($"Invalid namespace format: {ns}");
            return;
        }

        if (!Uri.IsWellFormedUriString(sourceURL, UriKind.Absolute))
        {
            Console.WriteLine($"Invalid URL: {sourceURL}");
            return;
        }

        var registries = JsonSerializer.Deserialize<Dictionary<string, RegistryNamespace>>(File.ReadAllText(PathResolver.RegistrySourcesFile)) ?? new();

        if (registries.ContainsKey(ns))
        {
            Console.WriteLine($"Namespace already exists: {ns}");
            return;
        }

        registries[ns] = new RegistryNamespace
        {
            Source = sourceURL,
            Branch = branch,
            LastRequest = null
        };

        File.WriteAllText(PathResolver.RegistrySourcesFile, JsonSerializer.Serialize(registries, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Added registry @{ns}: {sourceURL}");
    }

    public void RemoveRegistrySource(string ns, bool verbose = false)
    {
        EnsureDefaultConfig();

        var registries = JsonSerializer.Deserialize<Dictionary<string, RegistryNamespace>>(File.ReadAllText(PathResolver.RegistrySourcesFile));
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

        var sources = JsonSerializer.Deserialize<Dictionary<string, RegistryNamespace>>(File.ReadAllText(PathResolver.RegistrySourcesFile));
        if (sources == null || sources.Count == 0)
        {
            Console.WriteLine($"{ANSI_YELLOW}[WARNING] No sources configured.{ANSI_RESET}");
            return;
        }

        var merged = new Dictionary<string, RegistryNamespace>();

        foreach (var (ns, entry) in sources)
        {
            try
            {
                Console.WriteLine($"{ANSI_BLUE}[INFO] Fetching source @{ns} -> {entry.Source}{ANSI_RESET}");

                Uri sourceURI = new(entry.Source);
                GitHubClient gitHubClient = new(new ProductHeaderValue("Lampman"));

                var uriPathParts = sourceURI.AbsolutePath.Split("/");

                if (null == uriPathParts || uriPathParts.Length < 0)
                {
                    throw new Exception($"Invalid Source: {entry.Source}");
                }

                string owner = uriPathParts[^2];
                string repoName = uriPathParts[^1].Replace(".git", "");
                var latestRelease = await gitHubClient.Repository.Release.GetLatest(owner, repoName);
                var tags = await gitHubClient.Repository.GetAllTags(owner, repoName);

                if (null != latestRelease)
                {
                    string releaseTagName = latestRelease.TagName;

                    entry.Description = latestRelease.Body;

                    if (null != tags)
                    {
                        var tag = tags.First(_t => _t.Name == releaseTagName);
                        if (null != tag)
                        {
                            entry.Tag = tag.Name;

                            Console.WriteLine($"{ANSI_BLUE}[INFO] Found tag: {tag.Name}{ANSI_RESET}");

                            var tempZipPath = Path.Combine(PathResolver.RegistriesDir, "tmp");

                            if (!Directory.Exists(tempZipPath))
                            {
                                Console.WriteLine($"{ANSI_BLUE}[INFO] Creating temporary directory: {tempZipPath}{ANSI_RESET}");
                                Directory.CreateDirectory(tempZipPath);
                            }

                            tempZipPath = Path.Combine(tempZipPath, $"{ns}-{tag.Name}.zip");

                            if (File.Exists(tempZipPath))
                            {
                                Console.WriteLine($"{ANSI_YELLOW}[WARNING] Removing existing zip file: {tempZipPath}{ANSI_RESET}");
                                File.Delete(tempZipPath);
                            }

                            var targetDir = Path.Combine(PathResolver.RegistriesDir, "namespaces");
                            if (Directory.Exists(targetDir) && Directory.EnumerateFileSystemEntries(targetDir).Any())
                            {
                                Directory.Delete(targetDir, true);
                                Directory.CreateDirectory(targetDir);
                            }
                            else if (!Directory.Exists(targetDir))
                            {
                                Directory.CreateDirectory(targetDir);
                            }

                            Console.WriteLine($"{ANSI_BLUE}[INFO] Download and extract {tag.ZipballUrl}...{ANSI_RESET}");
                            Directory.CreateDirectory(targetDir);

                            await _compressFileHandler.DownloadAndUnzipFileAsync(tag.ZipballUrl, tempZipPath, targetDir);

                            entry.LastRequest = DateTime.Now;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ANSI_RED}[ERROR] Failed to fetch {entry.Source}: {ex.Message}{ANSI_RESET}");
            }
        }

        File.WriteAllText(PathResolver.RegistrySourcesFile, JsonSerializer.Serialize(sources, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"{ANSI_GREEN}[SUCCESS] {PathResolver.RegistrySourcesFile} -> refreshed {ANSI_RESET}");
    }
}