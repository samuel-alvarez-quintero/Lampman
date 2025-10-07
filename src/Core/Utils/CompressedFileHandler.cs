using System.IO.Compression;
using System.Security.Cryptography;

namespace Lampman.Core.Utils;

public class CompressedFileHandler(HttpClient? httpClient = null)
{
    public HttpClient HttpBrowserClient = httpClient ?? new BrowserClient();

    public async Task DownloadAndUnzipFileAsync(string fileUrl, string destinationZipPath, string extractDirectory, Dictionary<string, string>? Checksum = null)
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
                if (Checksum == null || Checksum.Count == 0)
                {
                    Console.WriteLine($"[WARNING] No checksum provided, skipping verification.");

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
                                Console.WriteLine($"[WARNING] Unknown hash algorithm: {hashFunc}");
                                continue;
                        }

                        Console.WriteLine($"[INFO] Verifying checksum for {hashFunc}...");

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
                            Console.WriteLine($"[SUCCESS] Checksum verified: {actualChecksum}");
                            break;
                        }
                        else
                        {
                            Console.WriteLine($"[ERROR] Checksum mismatch: {actualChecksum}");
                        }
                    }
                }
            }

            Console.WriteLine($"[INFO] Downloaded: {destinationZipPath}");

            // 3. Unzip the downloaded file
            ZipFile.ExtractToDirectory(destinationZipPath, extractDirectory, true); // 'true' overwrites existing files
            Console.WriteLine($"[INFO] Unzipped to: {extractDirectory}");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[ERROR] HTTP error during download: {ex.Message}");
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

}