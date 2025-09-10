using System.IO.Compression;
using System.Security.Cryptography;

namespace Lampman.Core.Services
{
    public class ServiceManager
    {
        // ANSI escape codes for colors
        const string ANSI_RED = "\u001B[31m";
        const string ANSI_GREEN = "\u001B[32m";
        const string ANSI_BLUE = "\e[0;34m";
        const string ANSI_YELLOW = "\e[0;33m";
        const string ANSI_RESET = "\u001B[0m"; // Resets all formatting

        private static readonly string InstallDir = PathResolver.ServicesInstallDir;

        private readonly HttpClient _httpClient;

        public ServiceManager(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new();

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
            "AppleWebKit/537.36 (KHTML, like Gecko) " +
            "Chrome/122.0 Safari/537.36");

            _httpClient.DefaultRequestHeaders.Accept.ParseAdd("*/*");
            _httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
        }

        public async Task InstallService(string serviceInput)
        {
            try
            {
                var (serviceName, version, meta) = ServiceResolver.Resolve(serviceInput);
                var url = meta.Url;

                var targetDir = Path.Combine(InstallDir, serviceName, version);
                if (Directory.Exists(targetDir) && Directory.EnumerateFileSystemEntries(targetDir).Any())
                {
                    Console.WriteLine($"{ANSI_YELLOW}[WARNING] Service {serviceName}:{version} already installed.{ANSI_RESET}");
                    return;
                }

                Console.WriteLine($"{ANSI_BLUE}[INFO] Installing {serviceName}:{version}...{ANSI_RESET}");
                Directory.CreateDirectory(targetDir);

                var zipPath = Path.Combine(InstallDir, serviceName, "tmp");

                if (!Directory.Exists(zipPath))
                {
                    Console.WriteLine($"{ANSI_BLUE}[INFO] Creating temporary directory: {zipPath}{ANSI_RESET}");
                    Directory.CreateDirectory(zipPath);
                }

                zipPath = Path.Combine(zipPath, $"{serviceName}-{version}.zip");

                if (File.Exists(zipPath))
                {
                    Console.WriteLine($"{ANSI_YELLOW}[WARNING] Removing existing zip file: {zipPath}{ANSI_RESET}");
                    File.Delete(zipPath);
                }

                await DownloadAndUnzipFileAsync(url, zipPath, targetDir, meta.Checksum);

                Console.WriteLine($"{ANSI_GREEN}[SUCCESS] Installed {serviceName}:{version} in {targetDir}{ANSI_RESET}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ANSI_RED}[ERROR] Failed to install - {ex.Message}{ANSI_RESET}");
                throw;
            }
        }

        public async Task UpdateService(string serviceInput)
        {
            var (serviceName, version, _) = ServiceResolver.Resolve(serviceInput);

            var targetDir = Path.Combine(InstallDir, serviceName, version);
            if (Directory.Exists(targetDir))
            {
                Console.WriteLine($"{ANSI_BLUE}[INFO] Updating {serviceName}:{version}...{ANSI_RESET}");
                Directory.Delete(targetDir, true);
            }

            await InstallService($"{serviceName}:{version}");
        }

        public void RemoveService(string serviceInput)
        {
            var (serviceName, version, _) = ServiceResolver.Resolve(serviceInput);
            var targetDir = Path.Combine(InstallDir, serviceName, version);

            if (Directory.Exists(targetDir))
            {
                Directory.Delete(targetDir, true);
                Console.WriteLine($"{ANSI_GREEN}[SUCCESS] Removed {serviceName}:{version}{ANSI_RESET}");
            }
            else
            {
                Console.WriteLine($"{ANSI_YELLOW}[WARNING] Service {serviceName}:{version} is not installed.{ANSI_RESET}");
            }
        }

        private async Task DownloadAndUnzipFileAsync(string fileUrl, string destinationZipPath, string extractDirectory, Dictionary<string, string>? Checksum)
        {
            try
            {
                // 1. Download the ZIP file
                using var response = await _httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status is not a success code.

                // 2. Save the downloaded stream to a local file
                await using (var contentStream = await response.Content.ReadAsStreamAsync())
                await using (var fileStream = new FileStream(destinationZipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    // Compute checksum and copy to destination
                    if (Checksum == null || Checksum.Count == 0)
                    {
                        Console.WriteLine($"{ANSI_YELLOW}[WARNING] No checksum provided, skipping verification.{ANSI_RESET}");

                        await contentStream.CopyToAsync(fileStream);
                    }
                    else
                    {
                        foreach (var (hashFunc, expectedChecksum) in Checksum)
                        {
                            HashAlgorithm algo;

                            switch (hashFunc)
                            {
                                case "SHA512":
                                    algo = SHA512.Create();
                                    break;
                                case "SHA384":
                                    algo = SHA384.Create();
                                    break;
                                case "SHA256":
                                    algo = SHA256.Create();
                                    break;
                                case "SHA1":
                                    algo = SHA1.Create();
                                    break;

                                default:
                                    Console.WriteLine($"{ANSI_YELLOW}[WARNING] Unknown hash algorithm: {hashFunc}{ANSI_RESET}");
                                    continue;
                            }

                            Console.WriteLine($"{ANSI_BLUE}[INFO] Verifying checksum for {hashFunc}...{ANSI_RESET}");

                            // Wrap file stream with hashing stream
                            using var cryptoStream = new CryptoStream(fileStream, algo, CryptoStreamMode.Write);

                            await contentStream.CopyToAsync(cryptoStream);

                            // Flush all buffers
                            cryptoStream.FlushFinalBlock();

                            // Compute final hash
                            var hashBytes = algo.Hash!;
                            var actualChecksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                            if (string.Equals(actualChecksum, expectedChecksum.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"{ANSI_GREEN}[SUCCESS] Checksum verified: {actualChecksum}{ANSI_RESET}");
                                break;
                            }
                            else
                            {
                                Console.WriteLine($"{ANSI_RED}[ERROR] Checksum mismatch: {actualChecksum}{ANSI_RESET}");
                            }
                        }
                    }
                }

                Console.WriteLine($"{ANSI_BLUE}[INFO] Downloaded: {destinationZipPath}{ANSI_RESET}");

                // 3. Unzip the downloaded file
                ZipFile.ExtractToDirectory(destinationZipPath, extractDirectory, true); // 'true' overwrites existing files
                Console.WriteLine($"{ANSI_BLUE}[INFO] Unzipped to: {extractDirectory}{ANSI_RESET}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"{ANSI_RED}[ERROR] HTTP error during download: {ex.Message}{ANSI_RESET}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"{ANSI_RED}[ERROR] File I/O error during download or unzip: {ex.Message}{ANSI_RESET}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ANSI_RED}[ERROR] An unexpected error occurred: {ex.Message}{ANSI_RESET}");
            }
        }
    }
}
