using System.IO.Compression;

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

        private static readonly string InstallDir = PathResolver.ServicesInstallDir;

        public ServiceManager()
        {
        }

        public async Task InstallService(string serviceInput)
        {
            var (name, version, meta) = ServiceResolver.Resolve(serviceInput);
            var url = meta.Url;

            var targetDir = Path.Combine(InstallDir, name, version);
            if (Directory.Exists(targetDir))
            {
                Console.WriteLine($"[Lampman] Service {name}:{version} already installed.");
                return;
            }

            Console.WriteLine($"[Lampman] Installing {name}:{version}...");
            Directory.CreateDirectory(targetDir);

            var zipPath = Path.Combine(targetDir, $"{name}-{version}.zip");
            using var client = new HttpClient();
            var data = await client.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(zipPath, data);

            ZipFile.ExtractToDirectory(zipPath, targetDir, true);
            File.Delete(zipPath);

            Console.WriteLine($"[Lampman] Installed {name}:{version} in {targetDir}");
        }

        public async Task UpdateService(string serviceInput)
        {
            var (name, version, _) = ServiceResolver.Resolve(serviceInput);

            var targetDir = Path.Combine(InstallDir, name, version);
            if (Directory.Exists(targetDir))
            {
                Console.WriteLine($"[Lampman] Updating {name}:{version}...");
                Directory.Delete(targetDir, true);
            }

            await InstallService($"{name}:{version}");
        }

        public void RemoveService(string serviceInput)
        {
            var (name, version, _) = ServiceResolver.Resolve(serviceInput);
            var targetDir = Path.Combine(InstallDir, name, version);

            if (Directory.Exists(targetDir))
            {
                Directory.Delete(targetDir, true);
                Console.WriteLine($"[Lampman] Removed {name}:{version}");
            }
            else
            {
                Console.WriteLine($"[Lampman] Service {name}:{version} is not installed.");
            }
        }
    }
}
