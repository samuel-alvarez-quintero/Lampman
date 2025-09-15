using System.Text.Json;

using DotNetEnv;

using Lampman.Cli;
using Lampman.Core;
using Lampman.Tests.Fixtures;
using Lampman.Tests.TestHelpers;

namespace Lampman.Tests.Integration;

[Trait("Category", "Integration"), Trait("Category", "RegistryCommand"), TestCaseOrderer(typeof(PriorityOrderer))]
public class RegistryCommandTests : IClassFixture<MockRegistryFixture>
{
    private readonly string[]? registryUrls;

    private readonly MockRegistryFixture _fixture;

    private readonly LampmanApp App;

    private readonly StringWriter TestingOutputWriter;

    public RegistryCommandTests(MockRegistryFixture fixture)
    {
        Env.TraversePath().Load();

        _fixture = fixture;

        App = new LampmanApp(_fixture.FakeClient);

        string? registrySources = Environment.GetEnvironmentVariable("TESTING_REGISTRY_SOURCES");

        if (!string.IsNullOrEmpty(registrySources))
        {
            registryUrls = registrySources.Split(';', StringSplitOptions.RemoveEmptyEntries);
            registryUrls = [.. registryUrls.Select(_url => _url.Trim())];
        }

        TestingOutputWriter = new();
        Console.SetOut(TestingOutputWriter);
    }

    /** Test the 'lampman registry -h' commands via command line interface **/
    [Fact, Trait("Category", "Command_RegistryHelp"), TestPriority(300)]
    public async Task RegistryHelp_ShouldDisplayHelpInformation()
    {
        var exitCode = await App.RunAsync(["registry", "-h"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("Description", TestingOutputWriter.ToString());
        Assert.Contains("Commands", TestingOutputWriter.ToString());
        Assert.Contains("list", TestingOutputWriter.ToString());
        Assert.Contains("add", TestingOutputWriter.ToString());
        Assert.Contains("remove", TestingOutputWriter.ToString());
        Assert.Contains("update", TestingOutputWriter.ToString());
    }

    /** Test the 'lampman registry list' commands via command line interface **/
    [Fact, Trait("Category", "Command_RegistryList"), TestPriority(301)]
    public async Task RegistryList_ShouldReturnRegistryEntries()
    {
        var exitCode = await App.RunAsync(["registry", "list"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("Configured registries:", TestingOutputWriter.ToString());
    }

    /** Test the 'lampman registry add [URL]' commands via command line interface **/
    [Fact, Trait("Category", "Command_RegistryAdd"), TestPriority(302)]
    public async Task RegistryAdd_ShouldCreateRegistryEntry()
    {
        if (null != registryUrls && registryUrls.Length > 0)
        {
            // Start with a clean registry file
            File.Delete(PathResolver.RegistryFile);

            foreach (var url in registryUrls)
            {
                var exitCode = await App.RunAsync(["registry", "add", url]);

                Assert.Equal(0, exitCode);
            }

            var sources = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(PathResolver.RegistryFile));

            if (null != sources)
                foreach (var url in registryUrls)
                    Assert.Contains(url, sources);
        }
    }

    /** Test the 'lampman registry remove [URL]' commands via command line interface **/
    [Fact, Trait("Category", "Command_RegistryRemove"), TestPriority(303)]
    public async Task RegistryRemove_ShouldDeleteRegistryEntry()
    {
        if (PathResolver.DefaultRegistrySource.Count > 0)
        {
            var removeUrl = PathResolver.DefaultRegistrySource.First();
            var exitCode = await App.RunAsync(["registry", "remove", removeUrl]);

            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(PathResolver.RegistryFile));

            var sources = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(PathResolver.RegistryFile));

            if (null != sources)
                Assert.DoesNotContain(removeUrl, sources);
        }
    }

    /** Test the 'lampman registry update' commands via command line interface **/
    [Fact, Trait("Category", "Command_RegistryUpdate"), TestPriority(304)]
    public async Task RegistryUpdate_ShouldFetchServices()
    {
        var exitCode = await App.RunAsync(["registry", "update"]);

        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(PathResolver.ServicesFile));
        Assert.Contains("ServiceProcess", File.ReadAllText(PathResolver.ServicesFile));
    }
}