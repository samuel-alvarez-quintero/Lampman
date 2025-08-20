using System.IO.Compression;
using Lampman.Core.Utils;

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

        private static readonly HttpClient httpClient = new HttpClient(new LoggingHandler(new HttpClientHandler()));

        public ServiceManager()
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
            "AppleWebKit/537.36 (KHTML, like Gecko) " +
            "Chrome/122.0 Safari/537.36");

            httpClient.DefaultRequestHeaders.Accept.ParseAdd("*/*");
            httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
        }

        public async Task InstallService(string serviceInput)
        {
            var (name, version, meta) = ServiceResolver.Resolve(serviceInput);
            var url = meta.Url;

            var targetDir = Path.Combine(InstallDir, name, version);
            if (Directory.Exists(targetDir) && Directory.EnumerateFileSystemEntries(targetDir).Any())
            {
                Console.WriteLine($"[Lampman] Service {name}:{version} already installed.");
                return;
            }

            Console.WriteLine($"[Lampman] Installing {name}:{version}...");
            Directory.CreateDirectory(targetDir);

            var zipPath = Path.Combine(targetDir, $"{name}-{version}.zip");
            if (File.Exists(zipPath))
            {
                Console.WriteLine($"[Lampman] Removing existing zip file: {zipPath}");
                File.Delete(zipPath);
            }

            await DownloadAndUnzipFileAsync(url, zipPath, targetDir);

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

        public async Task DownloadAndUnzipFileAsync(string fileUrl, string destinationZipPath, string extractDirectory)
        {
            try
            {
                // 1. Download the ZIP file
                using var response = await httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status is not a success code.

                // 2. Save the downloaded stream to a local file
                await using var contentStream = await response.Content.ReadAsStreamAsync();
                await using var fileStream = new FileStream(destinationZipPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await contentStream.CopyToAsync(fileStream);

                Console.WriteLine($"Downloaded: {destinationZipPath}");

                // 3. Unzip the downloaded file
                ZipFile.ExtractToDirectory(destinationZipPath, extractDirectory, true); // 'true' overwrites existing files
                Console.WriteLine($"Unzipped to: {extractDirectory}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP error during download: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"File I/O error during download or unzip: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
