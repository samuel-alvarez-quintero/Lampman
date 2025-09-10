using System.Diagnostics;
using System.Text.Json;
using DotNetEnv;
using Lampman.Core;
using Lampman.Tests.Fixtures;
using Lampman.Tests.TestHelpers;

namespace Lampman.Tests.Integration;

[Trait("Category", "Integration"), Trait("Category", "RegistryCommand"), TestCaseOrderer(typeof(PriorityOrderer))]
public class RegistryCommandTests : IClassFixture<MockRegistryFixture>
{
    private readonly string[]? registryUrls;

    private readonly MockRegistryFixture _fixture;

    public RegistryCommandTests(MockRegistryFixture fixture)
    {
        Env.TraversePath().Load();

        _fixture = fixture;

        string? registrySources = Environment.GetEnvironmentVariable("TESTING_REGISTRY_SOURCES");

        if (!string.IsNullOrEmpty(registrySources))
        {
            registryUrls = registrySources.Split(';', StringSplitOptions.RemoveEmptyEntries);
        }
    }

    /** Test the 'lampman registry -h' commands via command line interface **/
    [Fact, Trait("Category", "Command_RegistryHelp"), TestPriority(100)]
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

    /** Test the 'lampman registry list' commands via command line interface **/
    [Fact, Trait("Category", "Command_RegistryList"), TestPriority(101)]
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

    /** Test the 'lampman registry add' commands via command line interface **/
    [Fact, Trait("Category", "Command_RegistryAdd"), TestPriority(102)]
    public async Task RegistryAdd_ShouldCreateRegistryEntry()
    {
        if (null != registryUrls && registryUrls.Length > 0)
        {
            // Start with a clean registry file
            File.Delete(PathResolver.RegistryFile);

            foreach (var url in registryUrls)
            {
                Console.WriteLine($"Using registry URL: {url} -- End");
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"lampman.dll registry add {url}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WorkingDirectory = PathResolver.RootDir
                    }
                };

                process.Start();
                await process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
                process.WaitForExit();

                Assert.Equal(0, process.ExitCode);
            }

            var sources = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(PathResolver.RegistryFile));

            if (null != sources)
                foreach (var url in registryUrls)
                    Assert.Contains(url, sources);
        }
    }

    /** Test the 'lampman registry remove' commands via command line interface **/
    [Fact, Trait("Category", "Command_RegistryRemove"), TestPriority(103)]
    public async Task RegistryRemove_ShouldDeleteRegistryEntry()
    {
        if (null != registryUrls && registryUrls.Length > 0)
        {
            var removeUrl = registryUrls.First();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"lampman.dll registry remove {removeUrl}",
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

            var sources = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(PathResolver.RegistryFile));

            if (null != sources)
                Assert.DoesNotContain(removeUrl, sources);
        }
    }

    /** Test the 'lampman registry update' commands via command line interface **/
    [Fact, Trait("Category", "Command_RegistryUpdate"), TestPriority(104)]
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