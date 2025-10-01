using System.Text.Json;
using System.Text.RegularExpressions;

using Lampman.Core.Models;
using Lampman.Core.Utils;

using Octokit;

namespace Lampman.Core.Services;

public class RegistryManager(HttpClient? httpClient = null)
{
    // ANSI escape codes for colors
    const string ANSI_RED = "\u001B[31m";
    const string ANSI_GREEN = "\u001B[32m";
    const string ANSI_BLUE = "\e[0;34m";
    const string ANSI_YELLOW = "\e[0;33m";
    const string ANSI_RESET = "\u001B[0m"; // Resets all formatting

    private static readonly string SourcesConfigFile = PathResolver.SourcesFile;
    private static readonly string RegistryConfigFile = PathResolver.RegistryFile;
    private static readonly string ServicesConfigFile = PathResolver.ServicesFile;

    public HttpClient HttpBrowserClient = httpClient ?? new BrowserClient();

    private static void EnsureDefaultConfig()
    {
        if (!File.Exists(SourcesConfigFile))
            File.WriteAllText(SourcesConfigFile, JsonSerializer.Serialize(PathResolver.DefaultRegistrySource, new JsonSerializerOptions { WriteIndented = true }));

        if (!File.Exists(RegistryConfigFile))
            File.WriteAllText(RegistryConfigFile, JsonSerializer.Serialize(PathResolver.DefaultRegistryNamespace, new JsonSerializerOptions { WriteIndented = true }));
    }

    public void ListRegistries(bool verbose = false)
    {
        EnsureDefaultConfig();

        var registries = JsonSerializer.Deserialize<Dictionary<string, RegistryNamespace>>(File.ReadAllText(RegistryConfigFile));
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

            Console.WriteLine($"{ANSI_BLUE}[INFO] {ns}:{entry.Url}{ANSI_RESET}");

            if (verbose)
            {
                Console.WriteLine($"{ANSI_BLUE}[INFO] Description: {entry.Description ?? "(none)"}{ANSI_RESET}");
                Console.WriteLine($"{ANSI_BLUE}[INFO] Checksum: {entry.Checksum?.FirstOrDefault().Value ?? "(none)"}{ANSI_RESET}");
            }
        }
    }

    public void AddRegistry(string ns, string url, string? description = null, string? hashFunc = null, string? hashValue = null, bool verbose = false)
    {
        EnsureDefaultConfig();

        if (!Regex.IsMatch(ns, "^[a-z0-9\\-]+$"))
        {
            Console.WriteLine($"Invalid namespace format: {ns}");
            return;
        }

        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            Console.WriteLine($"Invalid URL: {url}");
            return;
        }

        var registries = JsonSerializer.Deserialize<Dictionary<string, RegistryNamespace>>(File.ReadAllText(RegistryConfigFile)) ?? new();

        if (registries.ContainsKey(ns))
        {
            Console.WriteLine($"Namespace already exists: {ns}");
            return;
        }

        Dictionary<string, string>? checksum = null;
        if (!string.IsNullOrWhiteSpace(hashFunc) && !string.IsNullOrWhiteSpace(hashValue))
        {
            checksum = new Dictionary<string, string>
        {
            { hashFunc.ToUpperInvariant(), hashValue.ToLowerInvariant() }
        };
        }

        registries[ns] = new RegistryNamespace
        {
            Description = description,
            Url = url,
            Checksum = checksum,
            LastRequest = null
        };

        File.WriteAllText(RegistryConfigFile, JsonSerializer.Serialize(registries, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Added registry @{ns}: {url}");
    }

    public void RemoveRegistry(string ns, bool verbose = false)
    {
        EnsureDefaultConfig();

        var registries = JsonSerializer.Deserialize<Dictionary<string, RegistryNamespace>>(File.ReadAllText(RegistryConfigFile));
        if (registries == null || !registries.Remove(ns))
        {
            Console.WriteLine($"{ANSI_YELLOW}[WARNING] Registry not found: {ns}{ANSI_RESET}");
            return;
        }

        File.WriteAllText(RegistryConfigFile, JsonSerializer.Serialize(registries, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"{ANSI_GREEN}[SUCCESS] Removed registry @{ns}{ANSI_RESET}");
    }

    public async Task FetchServices(bool verbose = false)
    {
        EnsureDefaultConfig();

        var registries = JsonSerializer.Deserialize<Dictionary<string, RegistryNamespace>>(File.ReadAllText(RegistryConfigFile));
        if (registries == null || registries.Count == 0)
        {
            Console.WriteLine($"{ANSI_YELLOW}[WARNING] No registries configured.{ANSI_RESET}");
            return;
        }

        var merged = new Dictionary<string, RegistryServices>();

        foreach (var (ns, entry) in registries)
        {
            try
            {
                Console.WriteLine($"{ANSI_BLUE}[INFO] Fetching @{ns} -> {entry.Url}{ANSI_RESET}");
                var bytes = await HttpBrowserClient.GetByteArrayAsync(entry.Url);

                // Validate checksum
                if (entry.Checksum != null && entry.Checksum.Count > 0)
                {
                    bool checksumFailed = true;
                    foreach (var (hashFunc, expected) in entry.Checksum)
                    {
                        checksumFailed = !ChecksumVerifier.Verify(bytes, hashFunc, expected);
                        if (!checksumFailed)
                        {
                            Console.WriteLine($"[SUCCESS] {hashFunc} checksum verified for @{ns}");
                            break;
                        }
                    }

                    if (checksumFailed)
                    {
                        Console.WriteLine($"{ANSI_RED}[ERROR] checksum mismatch for @{ns}{ANSI_RESET}");
                        continue;
                    }
                }
                else
                {
                    Console.WriteLine($"{ANSI_YELLOW}[WARNING] No checksum defined for @{ns}, skipping validation.{ANSI_RESET}");
                }

                // Deserialize JSON
                var json = System.Text.Encoding.UTF8.GetString(bytes);
                var parsed = JsonSerializer.Deserialize<RegistryServices>(json);
                if (parsed == null)
                {
                    Console.WriteLine($"{ANSI_RED}[ERROR] Invalid registry format: {entry.Url}{ANSI_RESET}");
                    continue;
                }

                parsed.Version = entry.Version;
                parsed.LastRequest = DateTime.Now;

                merged[ns] = parsed;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ANSI_RED}[ERROR] Failed to fetch {entry.Url}: {ex.Message}{ANSI_RESET}");
            }
        }

        File.WriteAllText(ServicesConfigFile, JsonSerializer.Serialize(merged, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"{ANSI_GREEN}[SUCCESS] services.json refreshed -> {ServicesConfigFile}{ANSI_RESET}");
    }

    public void ListSources(bool verbose = false)
    {
        EnsureDefaultConfig();

        var sources = JsonSerializer.Deserialize<Dictionary<string, RegistrySource>>(File.ReadAllText(SourcesConfigFile));
        Console.WriteLine($"{ANSI_BLUE}[INFO] Configured sources:{ANSI_RESET}");

        if (sources == null || sources.Count == 0)
        {
            Console.WriteLine($"{ANSI_YELLOW}[WARNING] No sources configured.{ANSI_RESET}");
            return;
        }

        foreach (var kv in sources)
        {
            var ns = kv.Key;
            var entry = kv.Value;

            Console.WriteLine($"{ANSI_BLUE}[INFO] {ns}:{entry.Source}{ANSI_RESET}");
        }
    }

    public void AddSource(string ns, string url, string? branch = null, bool verbose = false)
    {
        EnsureDefaultConfig();

        if (!Regex.IsMatch(ns, "^[a-z0-9\\-]+$"))
        {
            Console.WriteLine($"Invalid namespace format: {ns}");
            return;
        }

        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            Console.WriteLine($"Invalid URL: {url}");
            return;
        }

        var sources = JsonSerializer.Deserialize<Dictionary<string, RegistrySource>>(File.ReadAllText(SourcesConfigFile)) ?? new();

        if (sources.ContainsKey(ns))
        {
            Console.WriteLine($"Namespace already exists: {ns}");
            return;
        }

        sources[ns] = new RegistrySource
        {
            Source = url,
            Branch = branch
        };

        File.WriteAllText(SourcesConfigFile, JsonSerializer.Serialize(sources, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Added source @{ns}: {url}");
    }

    public void RemoveSource(string ns, bool verbose = false)
    {
        EnsureDefaultConfig();

        var sources = JsonSerializer.Deserialize<Dictionary<string, RegistrySource>>(File.ReadAllText(SourcesConfigFile));
        if (sources == null || !sources.Remove(ns))
        {
            Console.WriteLine($"{ANSI_YELLOW}[WARNING] Source not found: {ns}{ANSI_RESET}");
            return;
        }

        File.WriteAllText(SourcesConfigFile, JsonSerializer.Serialize(sources, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"{ANSI_GREEN}[SUCCESS] Removed source @{ns}{ANSI_RESET}");
    }

    public async Task FetchRegistries(bool verbose = false)
    {
        EnsureDefaultConfig();

        var sources = JsonSerializer.Deserialize<Dictionary<string, RegistrySource>>(File.ReadAllText(SourcesConfigFile));
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

                if (null != latestRelease)
                {
                    var asset = latestRelease.Assets.First(_a => _a.ContentType == "application/json");

                    merged[ns] = new RegistryNamespace
                    {
                        Url = asset.BrowserDownloadUrl,
                        Version = latestRelease.TagName,
                        Description = latestRelease.Body,
                        LastRequest = DateTime.Now
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ANSI_RED}[ERROR] Failed to fetch {entry.Source}: {ex.Message}{ANSI_RESET}");
            }
        }

        var registries = JsonSerializer.Deserialize<Dictionary<string, RegistryNamespace>>(File.ReadAllText(RegistryConfigFile));
        if (registries == null || registries.Count == 0)
        {
            Console.WriteLine($"{ANSI_YELLOW}[WARNING] No registries configured.{ANSI_RESET}");
            return;
        }

        foreach (var kv in merged)
        {
            registries[kv.Key] = kv.Value;
        }

        File.WriteAllText(RegistryConfigFile, JsonSerializer.Serialize(registries, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"{ANSI_GREEN}[SUCCESS] registry.json refreshed -> {RegistryConfigFile}{ANSI_RESET}");
    }
}