using System.Text.Json;

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

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
        "AppleWebKit/537.36 (KHTML, like Gecko) " +
        "Chrome/122.0 Safari/537.36");

        _httpClient.DefaultRequestHeaders.Accept.ParseAdd("*/*");
        _httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
    }

    private void EnsureConfig()
    {
        if (!File.Exists(RegistryConfigFile))
        {

            File.WriteAllText(RegistryConfigFile, JsonSerializer.Serialize(PathResolver.DefaultRegistrySource, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    public void ListRegistries()
    {
        EnsureConfig();
        var sources = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(RegistryConfigFile));
        Console.WriteLine($"{ANSI_BLUE}[INFO] Configured registries:{ANSI_RESET}");

        if (sources == null || sources.Count == 0)
        {
            Console.WriteLine($"{ANSI_YELLOW}[WARNING] No registries configured.{ANSI_RESET}");
            return;
        }

        foreach (var src in sources)
            Console.WriteLine($" - {src}");
    }

    public void AddRegistry(string url)
    {
        EnsureConfig();
        var sources = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(RegistryConfigFile));

        if (sources == null)
        {
            sources = new List<string>();
        }

        if (!sources.Contains(url))
        {
            // Verificate URL format
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                Console.WriteLine($"{ANSI_RED}[ERROR] Invalid URL format: {url}{ANSI_RESET}");
                return;
            }

            sources.Add(url);
            File.WriteAllText(RegistryConfigFile, JsonSerializer.Serialize(sources, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"{ANSI_GREEN}[SUCCESS] Added registry: {url}{ANSI_RESET}");
        }
        else
        {
            Console.WriteLine($"{ANSI_YELLOW}[WARNING] Registry already exists.{ANSI_RESET}");
        }
    }

    public void RemoveRegistry(string url)
    {
        EnsureConfig();
        var sources = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(RegistryConfigFile));

        if (sources == null)
        {
            Console.WriteLine($"{ANSI_YELLOW}[WARNING] No registries configured.{ANSI_RESET}");
            return;
        }

        if (sources.Remove(url))
        {
            File.WriteAllText(RegistryConfigFile, JsonSerializer.Serialize(sources, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"{ANSI_GREEN}[SUCCESS] Removed registry: {url}{ANSI_RESET}");
        }
        else
        {
            Console.WriteLine($"{ANSI_YELLOW}[WARNING] Registry not found.{ANSI_RESET}");
        }
    }

    public async Task UpdateServices()
    {
        EnsureConfig();
        var sources = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(RegistryConfigFile));
        var merged = new Dictionary<string, Dictionary<string, ServiceSource>>();

        if (sources == null || sources.Count == 0)
        {
            Console.WriteLine($"{ANSI_YELLOW}[WARNING] No registries configured.{ANSI_RESET}");
            return;
        }

        foreach (var src in sources)
        {
            try
            {
                Console.WriteLine($"{ANSI_BLUE}[INFO] Fetching {src}...{ANSI_RESET}");
                var json = await _httpClient.GetStringAsync(src);

                var services = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, ServiceSource>>>(json);

                if (services == null)
                {
                    Console.WriteLine($"{ANSI_RED}[ERROR] Invalid registry format from {src}{ANSI_RESET}");
                    continue;
                }

                foreach (var service in services)
                {
                    string serviceName = SlugHelper.GenerateSlug(service.Key);

                    if (!merged.ContainsKey(serviceName))
                        merged[serviceName] = new Dictionary<string, ServiceSource>();

                    foreach (var ver in service.Value)
                    {
                        string versionSlug = SlugHelper.GenerateSlug(ver.Key, true);

                        if (!merged[serviceName].ContainsKey(versionSlug))
                            merged[serviceName][versionSlug] = ver.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ANSI_RED}[ERROR] Failed to fetch {src}: {ex.Message}{ANSI_RESET}");
            }
        }

        File.WriteAllText(ServicesConfigFile, JsonSerializer.Serialize(merged, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"{ANSI_GREEN}[SUCCESS] Registry updated → {ServicesConfigFile}{ANSI_RESET}");
    }
}