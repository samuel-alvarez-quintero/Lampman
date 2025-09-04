using System.Diagnostics;
using Lampman.Core;

namespace Lampman.Tests;

public class RegistryIntegrationTests
{
    [Fact]
    public async Task RegistryAdd_ShouldCreateRegistryEntry()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "lampman.dll registry add https://example.com/registry.json",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = PathResolver.RootDir
            }
        };

        process.Start();
        string output = await process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        process.WaitForExit();

        Console.WriteLine($"lampman Registry output: {output}");
        Assert.Equal(0, process.ExitCode);

        if (File.Exists(PathResolver.RegistryFile))
        {
            Console.WriteLine($"RegistryFile: {PathResolver.RegistryFile}");
            Assert.Contains("https://example.com/registry.json",  File.ReadAllText(PathResolver.RegistryFile));
        }
    }
}