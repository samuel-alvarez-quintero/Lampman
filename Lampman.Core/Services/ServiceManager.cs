using Lampman.Core.Models;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;

namespace Lampman.Core.Services
{
    public class ServiceManager
    {
        // ANSI escape codes for colors
        const string ANSI_RED = "\u001B[31m";
        const string ANSI_GREEN = "\u001B[32m";
        const string ANSI_BlUE = "\e[0;34m";
        const string ANSI_YELLOW = "\e[0;33m";
        const string ANSI_RESET = "\u001B[0m"; // Resets all formatting

        public ServiceManager()
        {
        }

        public async Task InstallService(string serviceSpec)
        {
            var parts = serviceSpec.Split(':');
            var name = parts[0];
            var version = parts.Length > 1 ? parts[1] : "latest";

            var registry = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, ServiceInfo>>>(
                File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "services.json"))
            );

            if (registry is null)
            {
                Console.WriteLine($"[Lampman] Service registry is not available.");
                return;
            }

            if (!registry.ContainsKey(name) || !registry[name].ContainsKey(version))
            {
                Console.WriteLine($"Service {name}:{version} not found in registry.");
                return;
            }

            var info = registry[name][version];
            var url = info.Url;

            var servicesPath = Path.Combine("C:\\Lampman\\services", name, version);
            Directory.CreateDirectory(servicesPath);

            Console.WriteLine($"[Lampman] Downloading {name}:{version}...");
            using var client = new HttpClient();
            var zipPath = Path.Combine(Path.GetTempPath(), $"{name}-{version}.zip");
            var bytes = await client.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(zipPath, bytes);

            Console.WriteLine($"[Lampman] Extracting to {servicesPath}...");
            ZipFile.ExtractToDirectory(zipPath, servicesPath, true);

            Console.WriteLine($"[Lampman] {name}:{version} installed successfully.");
        }

        public async Task UpdateService(string serviceSpec)
        {
            Console.WriteLine($"[Lampman] Updating {serviceSpec}...");
            await InstallService(serviceSpec); // replace install logic
        }

        public void RemoveService(string serviceSpec)
        {
            var parts = serviceSpec.Split(':');
            var name = parts[0];
            var version = parts.Length > 1 ? parts[1] : "latest";

            var servicesPath = Path.Combine("C:\\Lampman\\services", name, version);
            if (Directory.Exists(servicesPath))
            {
                Directory.Delete(servicesPath, true);
                Console.WriteLine($"[Lampman] {name}:{version} removed.");
            }
            else
            {
                Console.WriteLine($"[Lampman] {name}:{version} is not installed.");
            }
        }
    }
}
