using System.IO.Compression;
using System.Security.Cryptography;

namespace Lampman.Core.Utils;

public class CompressedFileHandler(HttpClient? httpClient = null)
{
    public HttpClient HttpBrowserClient = httpClient ?? new BrowserClient();

    public async Task DownloadFileAsync(string fileUrl, string destinationZipPath, Dictionary<string, string>? Checksum = null)
    {
        try
        {
            // 1. Download the ZIP file
            using var response = await HttpBrowserClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status is not a success code.

            // 2. Save the downloaded stream to a local file
            await using (var contentStream = await response.Content.ReadAsStreamAsync())
            await using (var fileStream = new FileStream(destinationZipPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                // Compute checksum and copy to destination
                string? hashFuncSelected = string.Empty;
                HashAlgorithm? algo = null;
                string? checksumSelected = string.Empty;

                if (Checksum != null && Checksum.Count > 0)
                {
                    foreach (var (hashFunc, expectedChecksum) in Checksum)
                    {
                        // Check hashFunc and expectedChecksum is not empty
                        if (string.IsNullOrEmpty(hashFunc) || string.IsNullOrEmpty(expectedChecksum)) continue;

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
                                Console.WriteLine($"[WARNING] Unknown hash algorithm: {hashFunc}");
                                continue;
                        }

                        hashFuncSelected = hashFunc;
                        checksumSelected = expectedChecksum;
                        break; // Use the first valid checksum
                    }
                }

                if (algo is null || string.IsNullOrEmpty(hashFuncSelected) || string.IsNullOrEmpty(checksumSelected))
                {
                    Console.WriteLine($"[WARNING] No checksum provided, skipping verification.");

                    await contentStream.CopyToAsync(fileStream);
                }
                else
                {
                    Console.WriteLine($"[INFO] Verifying checksum for {hashFuncSelected}...");

                    // Wrap file stream with hashing stream
                    using var cryptoStream = new CryptoStream(fileStream, algo, CryptoStreamMode.Write);

                    await contentStream.CopyToAsync(cryptoStream);

                    // Flush all buffers
                    cryptoStream.FlushFinalBlock();

                    // Compute final hash
                    var hashBytes = algo.Hash!;
                    var actualChecksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                    if (string.Equals(actualChecksum, checksumSelected.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"[SUCCESS] Checksum verified: {actualChecksum}");
                    }
                    else
                    {
                        Console.WriteLine($"[ERROR] Checksum mismatch: {actualChecksum}");
                    }
                }
            }

            Console.WriteLine($"[INFO] Downloaded: {destinationZipPath}");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[ERROR] HTTP error during download: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] An unexpected error occurred: {ex.Message}");
        }
    }


    public void UnzipFile(string destinationZipPath, string extractDirectory)
    {
        try
        {
            // Unzip the file
            ZipFile.ExtractToDirectory(destinationZipPath, extractDirectory, true); // 'true' overwrites existing files
            Console.WriteLine($"[INFO] Unzipped to: {extractDirectory}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"[ERROR] File I/O error during download or unzip: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] An unexpected error occurred: {ex.Message}");
        }
    }

    public List<string> GetDirectoryNames(string zipFilePath)
    {
        List<string> directoryNames = [];

        using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                // Directories in a zip file typically have a trailing slash in their FullName
                // and a Name property that is empty.
                if (entry.FullName.EndsWith('/') && string.IsNullOrEmpty(entry.Name))
                {
                    // Extract the directory name from the FullName, removing the trailing slash
                    string? directoryName = Path.GetDirectoryName(entry.FullName);
                    if (!string.IsNullOrEmpty(directoryName))
                    {
                        directoryNames.Add(directoryName);
                    }
                }
            }
        }
        return directoryNames;
    }

}