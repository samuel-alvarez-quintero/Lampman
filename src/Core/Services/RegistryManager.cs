using System.Text.Json;
using System.Text.RegularExpressions;

using Lampman.Core.Models;
using Lampman.Core.Utils;

namespace Lampman.Core.Services;

public class RegistryManager
{
    // ANSI escape codes for colors
    const string ANSI_RED = "\u001B[31m";
    const string ANSI_GREEN = "\u001B[32m";
    const string ANSI_BLUE = "\e[0;34m";
    const string ANSI_YELLOW = "\e[0;33m";
    const string ANSI_RESET = "\u001B[0m"; // Resets all formatting

    private static readonly string RegistryConfigFile = PathResolver.RegistryFile;
    private static readonly string ServicesConfigFile = PathResolver.ServicesFile;

    private readonly HttpClient _httpClient;

    public RegistryManager(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new();
    }

    private void EnsureConfig()
    {
        if (!File.Exists(RegistryConfigFile))
            File.WriteAllText(RegistryConfigFile, JsonSerializer.Serialize(PathResolver.DefaultRegistrySource, new JsonSerializerOptions { WriteIndented = true }));
    }

    public void ListRegistries(bool verbose = false)
    {
        EnsureConfig();

        var registries = JsonSerializer.Deserialize<Dictionary<string, RegistryEntry>>(File.ReadAllText(RegistryConfigFile));
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
        EnsureConfig();

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

        var registries = JsonSerializer.Deserialize<Dictionary<string, RegistryEntry>>(File.ReadAllText(RegistryConfigFile)) ?? new();

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

        registries[ns] = new RegistryEntry
        {
            Description = description,
            Url = url,
            Checksum = checksum
        };

        File.WriteAllText(RegistryConfigFile, JsonSerializer.Serialize(registries, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Added registry @{ns}: {url}");
    }

    public void RemoveRegistry(string ns, bool verbose = false)
    {
        EnsureConfig();
        var registries = JsonSerializer.Deserialize<Dictionary<string, RegistryEntry>>(File.ReadAllText(RegistryConfigFile));
        if (registries == null || !registries.Remove(ns))
        {
            Console.WriteLine($"{ANSI_YELLOW}[WARNING] Registry not found: {ns}{ANSI_RESET}");
            return;
        }

        File.WriteAllText(RegistryConfigFile, JsonSerializer.Serialize(registries, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"{ANSI_GREEN}[SUCCESS] Removed registry @{ns}{ANSI_RESET}");
    }

    public async Task UpdateServices(bool verbose = false)
    {
        EnsureConfig();
        var registries = JsonSerializer.Deserialize<Dictionary<string, RegistryEntry>>(File.ReadAllText(RegistryConfigFile));
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
                Console.WriteLine($"{ANSI_BLUE}[INFO] Fetching @{ns} → {entry.Url}{ANSI_RESET}");
                var bytes = await _httpClient.GetByteArrayAsync(entry.Url);

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
                    continue;
                }

                // Deserialize JSON
                var json = System.Text.Encoding.UTF8.GetString(bytes);
                var parsed = JsonSerializer.Deserialize<RegistryServices>(json);
                if (parsed == null)
                {
                    Console.WriteLine($"{ANSI_RED}[ERROR] Invalid registry format: {entry.Url}{ANSI_RESET}");
                    continue;
                }

                merged[ns] = parsed;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ANSI_RED}[ERROR] Failed to fetch {entry.Url}: {ex.Message}{ANSI_RESET}");
            }
        }

        File.WriteAllText(ServicesConfigFile, JsonSerializer.Serialize(merged, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"{ANSI_GREEN}[SUCCESS] Services.json updated → {ServicesConfigFile}{ANSI_RESET}");
    }
}