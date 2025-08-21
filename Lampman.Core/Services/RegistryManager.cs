using Lampman.Core.Models;
using Lampman.Core.Utils;
using System.Text.Json;

namespace Lampman.Core.Services
{
    public class RegistryManager
    {
        // ANSI escape codes for colors
        const string ANSI_RED = "\u001B[31m";
        const string ANSI_GREEN = "\u001B[32m";
        const string ANSI_BlUE = "\e[0;34m";
        const string ANSI_YELLOW = "\e[0;33m";
        const string ANSI_RESET = "\u001B[0m"; // Resets all formatting

        private static readonly string RegistryConfigFile = PathResolver.RegistryFile;
        private static readonly string ServicesConfigFile = PathResolver.ServicesFile;

        public RegistryManager()
        {
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
            Console.WriteLine("[Lampman] Configured registries:");

            if (sources == null || sources.Count == 0)
            {
                Console.WriteLine(" - No registries configured.");
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
                sources.Add(url);
                File.WriteAllText(RegistryConfigFile, JsonSerializer.Serialize(sources, new JsonSerializerOptions { WriteIndented = true }));
                Console.WriteLine($"[Lampman] Added registry: {url}");
            }
            else
            {
                Console.WriteLine("[Lampman] Registry already exists.");
            }
        }

        public void RemoveRegistry(string url)
        {
            EnsureConfig();
            var sources = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(RegistryConfigFile));

            if (sources == null)
            {
                Console.WriteLine("[Lampman] No registries configured.");
                return;
            }

            if (sources.Remove(url))
            {
                File.WriteAllText(RegistryConfigFile, JsonSerializer.Serialize(sources, new JsonSerializerOptions { WriteIndented = true }));
                Console.WriteLine($"[Lampman] Removed registry: {url}");
            }
            else
            {
                Console.WriteLine("[Lampman] Registry not found.");
            }
        }

        public async Task UpdateServices()
        {
            EnsureConfig();
            var sources = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(RegistryConfigFile));
            var merged = new Dictionary<string, Dictionary<string, ServiceSource>>();

            using var client = new HttpClient();

            if (sources == null || sources.Count == 0)
            {
                Console.WriteLine("[Lampman] No registries configured.");
                return;
            }

            foreach (var src in sources)
            {
                try
                {
                    Console.WriteLine($"[Lampman] Fetching {src}...");
                    var json = await client.GetStringAsync(src);

                    var services = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, ServiceSource>>>(json);

                    if (services == null)
                    {
                        Console.WriteLine($"[Lampman] Invalid registry format from {src}");
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
                    Console.WriteLine($"{ANSI_RED}[Lampman] Failed to fetch {src}: {ex.Message}{ANSI_RESET}");
                }
            }

            File.WriteAllText(ServicesConfigFile, JsonSerializer.Serialize(merged, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"[Lampman] Registry updated → {ServicesConfigFile}");
        }
    }
}
