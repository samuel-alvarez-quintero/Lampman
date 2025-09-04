using System.Diagnostics;
using Lampman.Core;

[assembly: CaptureConsole]
[assembly: CaptureTrace]

namespace Lampman.Tests.Integration;

[Trait("Category", "Integration")]
public class RegistryCommandTests
{
    [Fact]
    public async Task RegistryHelp_ShouldDisplayHelpInformation()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "lampman.dll registry -h",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = PathResolver.RootDir
            }
        };

        process.Start();
        string output = await process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        process.WaitForExit();

        Assert.Equal(0, process.ExitCode);
        Assert.Contains("Description", output);
        Assert.Contains("Commands", output);
        Assert.Contains("list", output);
        Assert.Contains("add", output);
        Assert.Contains("remove", output);
        Assert.Contains("update", output);
    }

    [Fact]
    public async Task RegistryList_ShouldReturnRegistryEntries()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "lampman.dll registry list",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = PathResolver.RootDir
            }
        };

        process.Start();
        string output = await process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        process.WaitForExit();

        Assert.Equal(0, process.ExitCode);
        Assert.Contains("Configured registries:", output);
        Assert.Contains(PathResolver.DefaultRegistrySource.First(), output);
    }

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
        await process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        process.WaitForExit();

        Assert.Equal(0, process.ExitCode);
        Assert.True(File.Exists(PathResolver.RegistryFile));
        Assert.Contains("https://example.com/registry.json", File.ReadAllText(PathResolver.RegistryFile));
    }

    [Fact]
    public async Task RegistryRemove_ShouldDeleteRegistryEntry()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "lampman.dll registry remove https://example.com/registry.json",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = PathResolver.RootDir
            }
        };

        process.Start();
        await process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        process.WaitForExit();

        Assert.Equal(0, process.ExitCode);
        Assert.True(File.Exists(PathResolver.RegistryFile));
        Assert.DoesNotContain("https://example.com/registry.json", File.ReadAllText(PathResolver.RegistryFile));
    }

    [Fact]
    public async Task RegistryUpdate_ShouldFetchServices()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "lampman.dll registry update",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = PathResolver.RootDir
            }
        };

        process.Start();
        await process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        process.WaitForExit();

        Assert.Equal(0, process.ExitCode);
        Assert.True(File.Exists(PathResolver.ServicesFile));
        Assert.Contains("ServiceProcess", File.ReadAllText(PathResolver.ServicesFile));
    }
}