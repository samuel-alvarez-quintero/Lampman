using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Lampman.Tests.TestHelpers;

public class FakeFile
{
    public static async Task<string> GetSha256ChecksumAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The file was not found: {filePath}");
        }

        await using (var stream = File.OpenRead(filePath))
        {
            byte[] hash = await SHA256.HashDataAsync(stream);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }

    public static async Task CreateZipServiceFile(string fullZipPath)
    {
        string? zipFileName = Path.GetFileName(fullZipPath);

        if (null == zipFileName)
        {
            throw new Exception("The fullZipPath doesn't contain the file name");
        }

        if (!File.Exists(fullZipPath))
        {
            using var fs = new FileStream(fullZipPath, FileMode.Create, FileAccess.Write);
            using var zip = new ZipArchive(fs, ZipArchiveMode.Create, true);
            var entry = zip.CreateEntry("fake-executable.exe");
            entry.LastWriteTime = new DateTimeOffset(2025, 1, 15, 10, 30, 0, TimeSpan.Zero);
            using var entryStream = entry.Open();
            using var writer = new StreamWriter(entryStream);
            await writer.WriteAsync("This is a fake " + zipFileName);
        }

        return;
    }
}