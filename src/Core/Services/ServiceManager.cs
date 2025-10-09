using Lampman.Core.Utils;

namespace Lampman.Core.Services;

public class ServiceManager
{
    // ANSI escape codes for colors
    const string ANSI_RED = "\u001B[31m";
    const string ANSI_GREEN = "\u001B[32m";
    const string ANSI_BLUE = "\e[0;34m";
    const string ANSI_YELLOW = "\e[0;33m";
    const string ANSI_RESET = "\u001B[0m"; // Resets all formatting

    private static readonly string InstallDir = PathResolver.ServicesInstalledDir;

    public HttpClient HttpBrowserClient;

    private readonly CompressedFileHandler _compressFileHandler;

    public ServiceManager(HttpClient? httpClient = null)
    {
        HttpBrowserClient = httpClient ?? new BrowserClient();
        _compressFileHandler = new CompressedFileHandler(HttpBrowserClient);
    }

    public async Task InstallService(string serviceInput)
    {
        try
        {
            if (!File.Exists(PathResolver.ServicesInstalledDir))
                throw new Exception("Local services registry not found. Run `lampman registry update` first.");

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

            await _compressFileHandler.DownloadFileAsync(url, zipPath, meta.Checksum);
            _compressFileHandler.UnzipFile(zipPath, targetDir);

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
}